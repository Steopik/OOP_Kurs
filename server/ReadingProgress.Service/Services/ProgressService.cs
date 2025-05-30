using Microsoft.EntityFrameworkCore;
using ReadingProgress.Service.DTOs;
using ReadingProgress.Service.Models;

namespace ReadingProgress.Service.Services;

public class ProgressService
{
    private readonly ProgressDbContext _context;
    private readonly ExternalApiService _externalApiService;

    public ProgressService(ProgressDbContext context, ExternalApiService externalApiService)
    {
        _context = context;
        _externalApiService = externalApiService;
    }

    // Обновление прогресса и статуса
    public async Task<bool> UpdateProgress(UpdateProgressDto dto)
    {
        var userExists = await _externalApiService.UserExists(dto.UserId);
        var bookExists = await _externalApiService.BookExists(dto.BookId);

        if (!userExists || !bookExists)
            return false;

        var existing = await _context.Progresses.FindAsync(dto.UserId, dto.BookId);

        if (existing == null)
        {
            await _context.Progresses.AddAsync(new ReadingProgress.Service.Models.ReadingProgress
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                CurrentPage = dto.Page,
                Status = dto.Status
            });
        }
        else
        {
            existing.CurrentPage = dto.Page;
            existing.Status = dto.Status;
            existing.LastReadAt = DateTime.UtcNow;
        }

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ReadingProgress.Service.Models.ReadingProgress?> GetProgress(Guid userId, Guid bookId) =>
        await _context.Progresses.FindAsync(userId, bookId);

    public async Task<List<ReadingProgress.Service.Models.ReadingProgress>> GetUserProgressByStatus(Guid userId, ReadingStatus status) =>
        await _context.Progresses
            .Where(p => p.UserId == userId && p.Status == status)
            .ToListAsync();

    public async Task<bool> ResetProgress(Guid userId, Guid bookId, bool isUserAdmin)
    {
        var existing = await _context.Progresses.FindAsync(userId, bookId);
        if (existing == null) return false;

        if (existing.UserId != userId && !isUserAdmin)
            return false;

        _context.Progresses.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}