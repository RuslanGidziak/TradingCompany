using Microsoft.Data.SqlClient;
using NUnit.Framework;
using TradingCompany.DAL.Concrete;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DTO;

namespace TradingCompany.DAL.Tests;

[TestFixture]
public class UserDalTests : TransactionalTestBase
{
    private UserDal _dal = null!;

    [SetUp]
    public void SetUp()
    {
        _dal = new UserDal(ConnectionString);
    }

    [Test]
    public void GetAll_WhenSeeded_ReturnsAtLeast20Users()
    {
        Assert.That(_dal.GetAll(), Has.Count.GreaterThanOrEqualTo(20));
    }

    [Test]
    public void Create_NewUser_CanBeReadById()
    {
        var created = CreateTestUser();

        var fromDb = _dal.GetById(created.UserId);

        Assert.That(fromDb, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fromDb!.Login, Is.EqualTo(created.Login));
            Assert.That(fromDb.Role, Is.EqualTo(Roles.User));
            Assert.That(fromDb.IsActive, Is.True);
            Assert.That(PasswordHasher.Verify("Secret1!", fromDb.PasswordHash), Is.True);
        });
    }

    [Test]
    public void Update_ChangedLastName_IsSaved()
    {
        var user = CreateTestUser();
        user.LastName = "Змінений";

        var result = _dal.Update(user);

        Assert.That(result, Is.True);
        Assert.That(_dal.GetById(user.UserId)!.LastName, Is.EqualTo("Змінений"));
    }

    [Test]
    public void Delete_ExistingUser_RemovesIt()
    {
        var user = CreateTestUser();

        var result = _dal.Delete(user.UserId);

        Assert.That(result, Is.True);
        Assert.That(_dal.GetById(user.UserId), Is.Null);
    }

    [Test]
    public void GetById_NotExistingId_ReturnsNull()
    {
        Assert.That(_dal.GetById(-1), Is.Null);
    }

    [Test]
    public void SetActive_BlockThenActivate_ChangesStatus()
    {
        var user = CreateTestUser();

        _dal.SetActive(user.UserId, false);
        Assert.That(_dal.GetById(user.UserId)!.IsActive, Is.False);

        _dal.SetActive(user.UserId, true);
        Assert.That(_dal.GetById(user.UserId)!.IsActive, Is.True);
    }

    [Test]
    public void GetByLogin_SeededAdmin_HasCorrectPassword()
    {
        var admin = _dal.GetByLogin("admin");

        Assert.That(admin, Is.Not.Null);
        Assert.That(PasswordHasher.Verify("Pass123!", admin!.PasswordHash), Is.True);
    }

    [Test]
    public void Search_ByLogin_FindsOnlyThatUser()
    {
        var user = CreateTestUser();

        var found = _dal.Search(user.Login, UserSortField.Id, false);

        Assert.That(found, Has.Count.EqualTo(1));
        Assert.That(found[0].UserId, Is.EqualTo(user.UserId));
    }

    [Test]
    public void Search_SortByIdDescending_ReturnsOrderedList()
    {
        var ids = _dal.Search(null, UserSortField.Id, true).Select(u => u.UserId).ToList();

        Assert.That(ids, Is.Ordered.Descending);
    }

    [Test]
    public void Search_SortByIsActive_BlockedUsersFirst()
    {
        var list = _dal.Search(null, UserSortField.IsActive, false);

        Assert.That(list.First().IsActive, Is.False);
        Assert.That(list.Last().IsActive, Is.True);
    }

    [Test]
    public void CountActiveAdmins_AfterAddingAdmin_IncreasesByOne()
    {
        var before = _dal.CountActiveAdmins();

        CreateTestUser(Roles.Administrator);

        Assert.That(_dal.CountActiveAdmins(), Is.EqualTo(before + 1));
    }

    [Test]
    public void SetActive_BlockLastActiveAdmin_ThrowsSqlException()
    {
        var admins = _dal.GetAll().Where(u => u.Role == Roles.Administrator && u.IsActive).ToList();
        for (var i = 0; i < admins.Count - 1; i++)
        {
            _dal.SetActive(admins[i].UserId, false);
        }

        // Тригер у БД не дозволяє заблокувати останнього адміна
        Assert.Throws<SqlException>(() => _dal.SetActive(admins[^1].UserId, false));
    }

    [Test]
    public void Create_DuplicateLogin_ThrowsSqlException()
    {
        var user = CreateTestUser();
        var duplicate = new UserDto { Login = user.Login, Email = "x" + user.Email, FirstName = "A", LastName = "B" };

        Assert.Throws<SqlException>(() => _dal.Create(duplicate, "123"));
    }

    [Test]
    public void Create_InvalidRole_ThrowsSqlException()
    {
        var login = Unique("bad");
        var user = new UserDto { Login = login, Email = login + "@t.ua", FirstName = "A", LastName = "B", Role = "Manager" };

        Assert.Throws<SqlException>(() => _dal.Create(user, "123"));
    }
}