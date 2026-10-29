using NUnit.Framework;
using System.Transactions;
using TradingCompany.DAL.Concrete;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DTO;

namespace TradingCompany.DAL.Tests;

// Базовий клас: кожен тест виконується в транзакції, яка потім відкочується
public abstract class TransactionalTestBase
{
    protected const string ConnectionString = DbConfig.ConnectionString;
    private TransactionScope? _scope;

    [SetUp]
    public void BeginTransaction()
    {
        _scope = new TransactionScope();
    }

    [TearDown]
    public void RollbackTransaction()
    {
        _scope?.Dispose(); // Complete() не викликаємо -> ROLLBACK
    }

    protected static string Unique(string prefix) => prefix + Guid.NewGuid().ToString("N")[..8];

    protected static UserDto CreateTestUser(string role = Roles.User)
    {
        var login = Unique("u");
        return new UserDal(ConnectionString).Create(new UserDto
        {
            Login = login,
            Email = login + "@test.ua",
            FirstName = "Тест",
            LastName = "Користувач",
            Role = role
        }, "Secret1!");
    }
}