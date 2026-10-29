using Microsoft.Data.SqlClient;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DTO;

namespace TradingCompany.DAL.Concrete;

public class UserLoginDal(string connectionString) : IUserLoginDal
{
    private readonly string _connectionString = connectionString;

    private const string SelectColumns =
        "SELECT UserLoginId, UserId, LoginAt, IsSuccessful, Device FROM dbo.UserLogins";

    public List<UserLoginDto> GetAll()
    {
        var logins = new List<UserLoginDto>();
        const string sql = SelectColumns + " ORDER BY LoginAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            logins.Add(Map(reader));
        }
        return logins;
    }

    public List<UserLoginDto> GetByUser(int userId)
    {
        var logins = new List<UserLoginDto>();
        const string sql = SelectColumns + " WHERE UserId = @UserId ORDER BY LoginAt DESC";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            logins.Add(Map(reader));
        }
        return logins;
    }

    public UserLoginDto? GetById(int id)
    {
        const string sql = SelectColumns + " WHERE UserLoginId = @UserLoginId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserLoginId", id);

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public UserLoginDto Create(UserLoginDto login)
    {
        if (login.LoginAt == default) login.LoginAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO dbo.UserLogins (UserId, LoginAt, IsSuccessful, Device)
            OUTPUT INSERTED.UserLoginId
            VALUES (@UserId, @LoginAt, @IsSuccessful, @Device)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, login);

        connection.Open();
        login.UserLoginId = Convert.ToInt32(command.ExecuteScalar());
        return login;
    }

    public bool Update(UserLoginDto login)
    {
        const string sql = @"
            UPDATE dbo.UserLogins
            SET UserId = @UserId, LoginAt = @LoginAt, IsSuccessful = @IsSuccessful, Device = @Device
            WHERE UserLoginId = @UserLoginId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserLoginId", login.UserLoginId);
        AddParameters(command, login);

        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        const string sql = "DELETE FROM dbo.UserLogins WHERE UserLoginId = @UserLoginId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserLoginId", id);

        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static void AddParameters(SqlCommand command, UserLoginDto login)
    {
        command.Parameters.AddWithValue("@UserId", login.UserId);
        command.Parameters.AddWithValue("@LoginAt", login.LoginAt);
        command.Parameters.AddWithValue("@IsSuccessful", login.IsSuccessful);
        command.Parameters.AddWithValue("@Device", (object?)login.Device ?? DBNull.Value);
    }

    private static UserLoginDto Map(SqlDataReader reader) => new()
    {
        UserLoginId = reader.GetInt32(0),
        UserId = reader.GetInt32(1),
        LoginAt = reader.GetDateTime(2),
        IsSuccessful = reader.GetBoolean(3),
        Device = reader.IsDBNull(4) ? null : reader.GetString(4)
    };
}