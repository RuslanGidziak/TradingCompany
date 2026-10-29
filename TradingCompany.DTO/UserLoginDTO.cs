namespace TradingCompany.DTO;

// Відповідає таблиці dbo.UserLogins (історія входів)
public class UserLoginDto
{
    public int UserLoginId { get; set; }
    public int UserId { get; set; }
    public DateTime LoginAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? Device { get; set; }

    public override string ToString() =>
        $"#{UserLoginId,-4} користувач #{UserId,-3} {LoginAt:dd.MM.yyyy HH:mm}  {(IsSuccessful ? "успішно" : "НЕВДАЛО"),-8} {Device}";
}