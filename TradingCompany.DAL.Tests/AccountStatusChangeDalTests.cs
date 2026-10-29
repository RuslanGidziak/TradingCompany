using Microsoft.Data.SqlClient;
using NUnit.Framework;
using TradingCompany.DAL.Concrete;
using TradingCompany.DTO;

namespace TradingCompany.DAL.Tests;

[TestFixture]
public class AccountStatusChangeDalTests : TransactionalTestBase
{
    private AccountStatusChangeDal _dal = null!;

    [SetUp]
    public void SetUp()
    {
        _dal = new AccountStatusChangeDal(ConnectionString);
    }

    private AccountStatusChangeDto CreateTestChange() => _dal.Create(new AccountStatusChangeDto
    {
        UserId = CreateTestUser().UserId,
        ChangedByUserId = CreateTestUser(Roles.Administrator).UserId,
        NewIsActive = false,
        Reason = "Тест"
    });

    [Test]
    public void GetAll_WhenSeeded_ReturnsAtLeast20Records()
    {
        Assert.That(_dal.GetAll(), Has.Count.GreaterThanOrEqualTo(20));
    }

    [Test]
    public void Create_NewChange_CanBeReadById()
    {
        var change = CreateTestChange();

        var fromDb = _dal.GetById(change.AccountStatusChangeId);

        Assert.That(fromDb, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fromDb!.UserId, Is.EqualTo(change.UserId));
            Assert.That(fromDb.ChangedByUserId, Is.EqualTo(change.ChangedByUserId));
            Assert.That(fromDb.NewIsActive, Is.False);
            Assert.That(fromDb.Reason, Is.EqualTo("Тест"));
        });
    }

    [Test]
    public void Update_ChangedReason_IsSaved()
    {
        var change = CreateTestChange();
        change.Reason = "Нова причина";
        change.NewIsActive = true;

        Assert.That(_dal.Update(change), Is.True);

        var fromDb = _dal.GetById(change.AccountStatusChangeId)!;
        Assert.That(fromDb.Reason, Is.EqualTo("Нова причина"));
        Assert.That(fromDb.NewIsActive, Is.True);
    }

    [Test]
    public void Delete_ExistingRecord_RemovesIt()
    {
        var change = CreateTestChange();

        Assert.That(_dal.Delete(change.AccountStatusChangeId), Is.True);
        Assert.That(_dal.GetById(change.AccountStatusChangeId), Is.Null);
    }

    [Test]
    public void GetByUser_OneChange_ReturnsOneRecord()
    {
        var change = CreateTestChange();

        Assert.That(_dal.GetByUser(change.UserId), Has.Count.EqualTo(1));
    }

    [Test]
    public void Create_NotExistingUser_ThrowsSqlException()
    {
        var change = new AccountStatusChangeDto { UserId = -1, ChangedByUserId = 1, NewIsActive = false };

        Assert.Throws<SqlException>(() => _dal.Create(change));
    }
}