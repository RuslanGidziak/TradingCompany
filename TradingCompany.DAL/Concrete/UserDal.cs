using System.Data;
using Microsoft.Data.SqlClient;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DTO;

namespace TradingCompany.DAL.Concrete;

public class UserDal(string connectionString) : IUserDal
{
    private readonly string _connectionString = connectionString;

    private const string SelectColumns =
        "SELECT UserId, Login, Email, FirstName, LastName, PasswordHash, Role, IsActive, CreatedAt FROM dbo.Users";

    // ---------- READ: усі користувачі ----------
    public List<UserDto> GetAll()
    {
        var users = new List<UserDto>();
        const string sql = SelectColumns + " ORDER BY UserId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(Map(reader));
        }
        return users;
    }

    // ---------- READ: один користувач за Id ----------
    public UserDto? GetById(int id)
    {
        const string sql = SelectColumns + " WHERE UserId = @UserId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", id);

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    // ---------- READ: один користувач за логіном (для входу) ----------
    public UserDto? GetByLogin(string login)
    {
        const string sql = SelectColumns + " WHERE Login = @Login";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Login", login);

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    // ---------- READ: пошук + сортування (для адмін-сторінки) ----------
    public List<UserDto> Search(string? text, UserSortField sortBy, bool descending)
    {
        // Назва колонки береться тільки з цього списку — SQL-ін'єкція неможлива
        var column = sortBy switch
        {
            UserSortField.Login => "Login",
            UserSortField.LastName => "LastName",
            UserSortField.Email => "Email",
            UserSortField.Role => "Role",
            UserSortField.IsActive => "IsActive",
            UserSortField.CreatedAt => "CreatedAt",
            _ => "UserId"
        };
        var direction = descending ? "DESC" : "ASC";
        var orderBy = column == "UserId" ? $"UserId {direction}" : $"{column} {direction}, UserId";

        var sql = SelectColumns + @"
            WHERE @Pattern IS NULL
               OR Login LIKE @Pattern OR Email LIKE @Pattern
               OR FirstName LIKE @Pattern OR LastName LIKE @Pattern
            ORDER BY " + orderBy;

        var users = new List<UserDto>();
        var pattern = string.IsNullOrWhiteSpace(text) ? null : $"%{text.Trim()}%";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Pattern", SqlDbType.NVarChar, 102).Value = (object?)pattern ?? DBNull.Value;

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(Map(reader));
        }
        return users;
    }

    // ---------- CREATE ----------
    public UserDto Create(UserDto user, string password)
    {
        user.PasswordHash = PasswordHasher.Hash(password);

        const string sql = @"
            INSERT INTO dbo.Users (Login, Email, FirstName, LastName, PasswordHash, Role, IsActive)
            OUTPUT INSERTED.UserId, INSERTED.CreatedAt
            VALUES (@Login, @Email, @FirstName, @LastName, @PasswordHash, @Role, @IsActive)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, user);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

        connection.Open();
        using var reader = command.ExecuteReader();
        reader.Read();
        user.UserId = reader.GetInt32(0);
        user.CreatedAt = reader.GetDateTime(1);
        return user;
    }

    // ---------- UPDATE ----------
    public bool Update(UserDto user)
    {
        const string sql = @"
            UPDATE dbo.Users
            SET Login = @Login, Email = @Email, FirstName = @FirstName, LastName = @LastName,
                Role = @Role, IsActive = @IsActive
            WHERE UserId = @UserId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", user.UserId);
        AddParameters(command, user);

        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    // ---------- UPDATE: блокування / активація ----------
    public bool SetActive(int userId, bool isActive)
    {
        const string sql = "UPDATE dbo.Users SET IsActive = @IsActive WHERE UserId = @UserId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@IsActive", isActive);

        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    // ---------- DELETE ----------
    public bool Delete(int id)
    {
        const string sql = "DELETE FROM dbo.Users WHERE UserId = @UserId";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", id);

        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    // ---------- Кількість активних адмінів (правило «хоча б один адмін») ----------
    public int CountActiveAdmins()
    {
        const string sql = "SELECT COUNT(*) FROM dbo.Users WHERE Role = N'Administrator' AND IsActive = 1";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // ---------- Допоміжні методи ----------
    private static void AddParameters(SqlCommand command, UserDto user)
    {
        command.Parameters.AddWithValue("@Login", user.Login);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@FirstName", user.FirstName);
        command.Parameters.AddWithValue("@LastName", user.LastName);
        command.Parameters.AddWithValue("@Role", user.Role);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
    }

    private static UserDto Map(SqlDataReader reader) => new()
    {
        UserId = reader.GetInt32(0),
        Login = reader.GetString(1),
        Email = reader.GetString(2),
        FirstName = reader.GetString(3),
        LastName = reader.GetString(4),
        PasswordHash = reader.GetString(5),
        Role = reader.GetString(6),
        IsActive = reader.GetBoolean(7),
        CreatedAt = reader.GetDateTime(8)
    };
}