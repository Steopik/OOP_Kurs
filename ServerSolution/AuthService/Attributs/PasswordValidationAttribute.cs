using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class PasswordValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        var password = value as string;
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        // Латинские буквы (любые), цифры, спецсимволы, минимум 8 символов
        var regex = new Regex(@"^[A-Za-z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?`~]{8,}$");
        return regex.IsMatch(password);
    }

    public override string FormatErrorMessage(string name)
    {
        return "Password must be at least 8 characters long and contain only English letters, numbers, and special characters.";
    }
}
