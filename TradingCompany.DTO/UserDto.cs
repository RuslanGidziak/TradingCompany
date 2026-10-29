namespace TradingCompany.DTO;

// Можливі ролі в системі (як у CHECK-обмеженні таблиці Users)
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string User = "User";
}

// Відповідає таблиці dbo.Users
public class UserDto
{
    public int UserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}