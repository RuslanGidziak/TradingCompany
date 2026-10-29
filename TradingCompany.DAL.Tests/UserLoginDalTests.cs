using NUnit.Framework;
using TradingCompany.DAL.Concrete;
using TradingCompany.DTO;

namespace TradingCompany.DAL.Tests;

[TestFixture]
public class UserLoginDalTests : TransactionalTestBase
{
    private UserLoginDal _dal = null!;

    [SetUp]
    public void SetUp()
    {
        _dal = new UserLoginDal(ConnectionString);
    }

    [Test]
    public void GetAll_WhenSeeded_ReturnsAtLeast20Records()
    {
        Assert.That(_dal.GetAll(), Has.Count.GreaterThanOrEqualTo(20));
    }

    [Test]
    public void Create_NewLogin_CanBeReadById()
    {
        var user = CreateTestUser();

        var created = _dal.Create(new UserLoginDto { UserId = user.UserId, IsSuccessful = true, Device = "TEST-PC" });
        var fromDb = _dal.GetById(created.UserLoginId);

        Assert.That(fromDb, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fromDb!.UserId, Is.EqualTo(user.UserId));
            Assert.That(fromDb.IsSuccessful, Is.True);
            Assert.That(fromDb.Device, Is.EqualTo("TEST-PC"));
        });
    }

    [Test]
    public void GetByUser_TwoLogins_ReturnsBoth()
    {
        var user = CreateTestUser();
        _dal.Create(new UserLoginDto { UserId = user.UserId, IsSuccessful = false });
        _dal.Create(new UserLoginDto { UserId = user.UserId, IsSuccessful = true });

        var list = _dal.GetByUser(user.UserId);

        Assert.That(list, Has.Count.EqualTo(2));
        Assert.That(list.All(l => l.UserId == user.UserId), Is.True);
    }

    [Test]
    public void Update_ChangedResult_IsSaved()
    {
        var login = _dal.Create(new UserLoginDto { UserId = CreateTestUser().UserId, IsSuccessful = false });
        login.IsSuccessful = true;

        Assert.That(_dal.Update(login), Is.True);
        Assert.That(_dal.GetById(login.UserLoginId)!.IsSuccessful, Is.True);
    }

    [Test]
    public void Delete_ExistingRecord_RemovesIt()
    {
        var login = _dal.Create(new UserLoginDto { UserId = CreateTestUser().UserId, IsSuccessful = true });

        Assert.That(_dal.Delete(login.UserLoginId), Is.True);
        Assert.That(_dal.GetById(login.UserLoginId), Is.Null);
    }

    [Test]
    public void DeleteUser_WithLogins_CascadeDeletesLogins()
    {
        var user = CreateTestUser();
        var login = _dal.Create(new UserLoginDto { UserId = user.UserId, IsSuccessful = true });

        new UserDal(ConnectionString).Delete(user.UserId);

        Assert.That(_dal.GetById(login.UserLoginId), Is.Null);
    }
}