using System.Security.Claims;
using Auth.Service.Models;
using Auth.Service.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Auth.Service.Services;

public class AuthService
{
    private readonly AuthDbContext _context;
    private readonly TokenService _tokenService;
    private readonly EmailService _emailService;
    private readonly RedisVerificationService _redisVerification;
    private readonly TokenBlacklistService _blacklist;
    private readonly IConfiguration _config;

    private readonly Random _random = new();

    public AuthService(
        AuthDbContext context,
        TokenService tokenService,
        EmailService emailService,
        RedisVerificationService redisVerification,
        TokenBlacklistService blacklist,
        IConfiguration config)
    {
        _context = context;
        _tokenService = tokenService;
        _emailService = emailService;
        _redisVerification = redisVerification;
        _blacklist = blacklist;
        _config = config;
    }

    public async Task<bool> Register(RegisterRequestDto dto)
    {
        // Проверяем, существует ли пользователь с таким логином
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return false;

        // Хэшируем пароль
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Создаём пользователя
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            IsEmailConfirmed = false
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Генерируем код подтверждения
        var code = GenerateVerificationCode();

        // Сохраняем в Redis на 10 минут
        await _redisVerification.SaveCodeAsync(user.Id, code);

        // Отправляем на почту
        await _emailService.SendVerificationCode(dto.Email, code);

        return true;
    }

    public async Task<string?> VerifyEmail(string username, string code)
    {
        // Находим пользователя
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || user.IsEmailConfirmed)
            return null;

        // Проверяем код через Redis
        if (!await _redisVerification.ValidateCodeAsync(user.Id, code))
            return null;

        // Подтверждаем email
        user.IsEmailConfirmed = true;
        await _context.SaveChangesAsync();

        // Возвращаем токен
        return _tokenService.GenerateAccessToken(user);
    }

    public async Task<string?> Login(LoginRequestDto dto)
    {
        // Ищем пользователя
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !user.IsEmailConfirmed ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        // Возвращаем токен
        return _tokenService.GenerateAccessToken(user);
    }

    private string GenerateVerificationCode() =>
        _random.Next(100000, 999999).ToString();


    public async Task<bool> DeleteUser(string username, string requesterUsername)
    {
        var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        var requester = await _context.Users.FirstOrDefaultAsync(u => u.Username == requesterUsername);

        if (userToDelete == null || requester == null)
            return false;

        if (requester.IsSuperUser)
            return true; // Суперюзер может удалить любого

        if (requester.IsAdmin)
        {
            if (userToDelete.IsAdmin || userToDelete.IsSuperUser)
                return false; // Админ не может удалять других админов или суперюзеров

            return true;
        }

        // Обычный пользователь может удалить только себя
        return requester.Username == username;
    }


    public async Task<bool> UpdateUser(string username, UpdateUserRequestDto dto, string requesterUsername)
    {
        var userToEdit = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        var requester = await _context.Users.FirstOrDefaultAsync(u => u.Username == requesterUsername);

        if (userToEdit == null || requester == null)
            return false;

        // SuperUser может всё, кроме редактирования других SuperUser'ов
        if (requester.IsSuperUser)
        {
            if (userToEdit.IsSuperUser && !dto.IsSuperUser.HasValue)
                return false; // Нельзя редактировать других SuperUser'ов

            // Изменение имени
            if (!string.IsNullOrEmpty(dto.NewUsername))
            {
                if (await _context.Users.AnyAsync(u => u.Username == dto.NewUsername && u.Username != username))
                    return false; // Такой ник уже занят

                userToEdit.Username = dto.NewUsername;
            }

            // Изменение пароля
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                userToEdit.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                userToEdit.TokenVersion++; // ← увеличиваем версию токена
            }

            // Изменение email
            if (!string.IsNullOrEmpty(dto.NewEmail))
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.NewEmail && u.Id != userToEdit.Id))
                    return false; // Email уже используется

                var code = GenerateVerificationCode();
                await _redisVerification.SaveCodeAsync(userToEdit.Id, code);
                await _emailService.SendVerificationCode(dto.NewEmail, code);

                userToEdit.Email = dto.NewEmail;
                userToEdit.IsEmailConfirmed = false;
            }

            // Изменение статуса админа
            if (dto.IsAdmin.HasValue)
                userToEdit.IsAdmin = dto.IsAdmin.Value;

            // Изменение статуса суперпользователя
            if (dto.IsSuperUser.HasValue)
                userToEdit.IsSuperUser = dto.IsSuperUser.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        // Администратор может:
        // - Редактировать только обычных пользователей
        // - Только себя делать админом/суперюзером
        if (requester.IsAdmin)
        {
            // Не может редактировать других админов или суперюзеров
            if (userToEdit.IsAdmin || userToEdit.IsSuperUser)
                return false;

            // Не может менять статус суперюзера
            if (dto.IsSuperUser.HasValue)
                return false;

            // Может редактировать только себя
            if (requester.Username != username)
                return false;

            // Изменение имени
            if (!string.IsNullOrEmpty(dto.NewUsername))
            {
                if (await _context.Users.AnyAsync(u => u.Username == dto.NewUsername && u.Username != username))
                    return false;

                userToEdit.Username = dto.NewUsername;
            }

            // Изменение пароля
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                userToEdit.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                userToEdit.TokenVersion++;
            }

            // Изменение email
            if (!string.IsNullOrEmpty(dto.NewEmail))
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.NewEmail && u.Id != userToEdit.Id))
                    return false;

                var code = GenerateVerificationCode();
                await _redisVerification.SaveCodeAsync(userToEdit.Id, code);
                await _emailService.SendVerificationCode(dto.NewEmail, code);

                userToEdit.Email = dto.NewEmail;
                userToEdit.IsEmailConfirmed = false;
            }

            // Изменение статуса админа (только себе)
            if (dto.IsAdmin.HasValue)
                userToEdit.IsAdmin = dto.IsAdmin.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        // Обычный пользователь может менять только себя
        if (requester.Username != username)
            return false;

        // Изменение имени
        if (!string.IsNullOrEmpty(dto.NewUsername))
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.NewUsername && u.Username != username))
                return false;

            userToEdit.Username = dto.NewUsername;
        }

        // Изменение пароля
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            userToEdit.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            userToEdit.TokenVersion++;
        }

        // Изменение email
        if (!string.IsNullOrEmpty(dto.NewEmail))
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.NewEmail && u.Id != userToEdit.Id))
                return false;

            var code = GenerateVerificationCode();
            await _redisVerification.SaveCodeAsync(userToEdit.Id, code);
            await _emailService.SendVerificationCode(dto.NewEmail, code);

            userToEdit.Email = dto.NewEmail;
            userToEdit.IsEmailConfirmed = false;
        }

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<(bool Success, string ErrorMessage)> AdminCreateUser(AdminCreateUserDto dto, string requesterUsername)
    {
        var requester = await _context.Users.FirstOrDefaultAsync(u => u.Username == requesterUsername);
        if (requester == null || (!requester.IsAdmin && !requester.IsSuperUser))
            return (false, "Нет прав");

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return (false, "Пользователь с таким ником уже существует");

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return (false, "Email уже используется");

        // Суперпользователь может создать любого
        if (!requester.IsSuperUser && dto.IsAdmin)
            return (false, "Только суперпользователь может создавать админов");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsEmailConfirmed = true, // Админ создаёт пользователя как подтверждённого
            IsAdmin = dto.IsAdmin,
            IsSuperUser = dto.IsSuperUser
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }


    public async Task<List<UserResponseDto>> GetAllUsers(string requesterUsername)
    {
        var requester = await _context.Users.FirstOrDefaultAsync(u => u.Username == requesterUsername);
        if (requester == null || (!requester.IsAdmin && !requester.IsSuperUser))
            throw new UnauthorizedAccessException("Нет прав на просмотр списка пользователей");

        var users = await _context.Users
            .Select(u => new UserResponseDto
            {
                Username = u.Username,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                IsSuperUser = u.IsSuperUser
            })
            .ToListAsync();

        return users;
    }


    public async Task<UserResponseDto?> GetUser(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
            IsSuperUser = user.IsSuperUser
        };
    }

    public async Task<Guid?> GetUserByName(string name)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);

        if (user == null)
            return null;

        return user.Id;
    }

    public async Task<(bool Success, AuthResponseDto? Response)> RefreshToken(RefreshTokenDto dto)
    {
        // Проверяем, не в бане ли access token
        if (await _blacklist.IsInBlacklist(dto.AccessToken))
            return (false, null);

        // Получаем данные из истёкшего access token
        var accessTokenPrincipal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
        var accessTokenUsername = accessTokenPrincipal.Identity?.Name;

        if (string.IsNullOrEmpty(accessTokenUsername))
            return (false, null);

        // Получаем данные из refresh token
        var refreshTokenPrincipal = _tokenService.GetPrincipalFromExpiredToken(dto.RefreshToken);
        var refreshTokenUserId = refreshTokenPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(refreshTokenUserId))
            return (false, null);

        // Находим пользователя
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == accessTokenUsername);
        if (user == null || user.Id.ToString() != refreshTokenUserId)
            return (false, null);

        // Генерируем новые токены
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

        // Добавляем старый refresh token в черный список
        var refreshTokenExpiry = _tokenService.GetExpiryTimeFromToken(dto.RefreshToken);
        var refreshTokenTtl = refreshTokenExpiry - DateTime.UtcNow;
        await _blacklist.AddToBlacklist(dto.RefreshToken, refreshTokenTtl);

        return (true, new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:AccessExpirationMinutes"]))
        });
    }




}