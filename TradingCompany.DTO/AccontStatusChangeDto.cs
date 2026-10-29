namespace TradingCompany.DTO;

// Відповідає таблиці dbo.AccountStatusChanges (журнал блокувань/активацій)
public class AccountStatusChangeDto
{
    public int AccountStatusChangeId { get; set; }
    public int UserId { get; set; }
    public int ChangedByUserId { get; set; }
    public bool NewIsActive { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; }

    public override string ToString() =>
        $"#{AccountStatusChangeId,-4} {ChangedAt:dd.MM.yyyy HH:mm}  користувача #{UserId,-3} " +
        $"{(NewIsActive ? "АКТИВОВАНО" : "ЗАБЛОКОВАНО"),-12} адміном #{ChangedByUserId,-3} {Reason}";
}