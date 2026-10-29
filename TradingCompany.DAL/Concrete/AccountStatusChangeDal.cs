using Microsoft.Data.SqlClient;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DTO;

namespace TradingCompany.DAL.Concrete;

public class AccountStatusChangeDal(string connectionString) : IAccountStatusChangeDal
{
    private readonly string _connectionString = connectionString;

    private const string SelectColumns =
        "SELECT AccountStatusChangeId, UserId, ChangedByUserId, NewIsActive, Reason, ChangedAt FROM dbo.AccountStatusChanges";

    public List<AccountStatusChangeDto> GetAll()
    {
        var changes = new List<AccountStatusChangeDto>();
        const string sql = SelectColumns + " ORDER BY ChangedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            changes.Add(Map(reader));
        }
        return changes;
    }

    public List<AccountStatusChangeDto> GetByUser(int userId)
    {
        var changes = new List<AccountStatusChangeDto>();
        const string sql = SelectColumns + " WHERE UserId = @UserId ORDER BY ChangedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            changes.Add(Map(reader));
        }
        return changes;
    }

    public AccountStatusChangeDto? GetById(int id)
    {
        const string sql = SelectColumns + " WHERE AccountStatusChangeId = @Id";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public AccountStatusChangeDto Create(AccountStatusChangeDto change)
    {
        if (change.ChangedAt == default) change.ChangedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO dbo.AccountStatusChanges (UserId, ChangedByUserId, NewIsActive, Reason, ChangedAt)
            OUTPUT INSERTED.AccountStatusChangeId
            VALUES (@UserId, @ChangedByUserId, @NewIsActive, @Reason, @ChangedAt)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, change);

        connection.Open();
        change.AccountStatusChangeId = Convert.ToInt32(command.ExecuteScalar());
        return change;
    }

    public bool Update(AccountStatusChangeDto change)
    {
        const string sql = @"
            UPDATE dbo.AccountStatusChanges
            SET UserId = @UserId, ChangedByUserId = @ChangedByUserId, NewIsActive = @NewIsActive,
                Reason = @Reason, ChangedAt = @ChangedAt
            WHERE AccountStatusChangeId = @Id";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", change.AccountStatusChangeId);
        AddParameters(command, change);

        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        const string sql = "DELETE FROM dbo.AccountStatusChanges WHERE AccountStatusChangeId = @Id";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static void AddParameters(SqlCommand command, AccountStatusChangeDto change)
    {
        command.Parameters.AddWithValue("@UserId", change.UserId);
        command.Parameters.AddWithValue("@ChangedByUserId", change.ChangedByUserId);
        command.Parameters.AddWithValue("@NewIsActive", change.NewIsActive);
        command.Parameters.AddWithValue("@Reason", (object?)change.Reason ?? DBNull.Value);
        command.Parameters.AddWithValue("@ChangedAt", change.ChangedAt);
    }

    private static AccountStatusChangeDto Map(SqlDataReader reader) => new()
    {
        AccountStatusChangeId = reader.GetInt32(0),
        UserId = reader.GetInt32(1),
        ChangedByUserId = reader.GetInt32(2),
        NewIsActive = reader.GetBoolean(3),
        Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
        ChangedAt = reader.GetDateTime(5)
    };
}