using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class UsernameValidationAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        var username = value as string;
        if (string.IsNullOrWhiteSpace(username))
            return false;

        // Не начинается с цифры, содержит только латинские буквы и цифры, и не короче 3 символов
        var regex = new Regex(@"^[A-Za-z][A-Za-z0-9]{2,}$");
        return regex.IsMatch(username);
    }

    public override string FormatErrorMessage(string name)
    {
        return "Username must be at least 3 characters long, start with a letter, and contain only English letters and digits.";
    }
}
