using System.Transactions;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DTO;

namespace TradingCompany.Services;

public class AdminService(IUserDal userDal, IAccountStatusChangeDal statusChangeDal)
{
    private readonly IUserDal _userDal = userDal;
    private readonly IAccountStatusChangeDal _statusChangeDal = statusChangeDal;

    public List<UserDto> GetUsers(string? search, UserSortField sortBy, bool descending) =>
        _userDal.Search(search, sortBy, descending);

    public bool IsLastActiveAdmin(UserDto user) =>
        user.Role == Roles.Administrator && user.IsActive && _userDal.CountActiveAdmins() <= 1;

    public bool ChangeStatus(UserDto admin, int userId, bool activate, string? reason, out string message)
    {
        var target = _userDal.GetById(userId);
        if (target is null)
        {
            message = "Користувача не знайдено.";
            return false;
        }
        if (target.IsActive == activate)
        {
            message = activate ? "Акаунт уже активний." : "Акаунт уже заблоковано.";
            return false;
        }
        if (!activate && target.UserId == admin.UserId)
        {
            message = "Не можна заблокувати власний акаунт.";
            return false;
        }
        if (!activate && IsLastActiveAdmin(target))
        {
            message = "Це останній активний адміністратор — блокування заборонено.";
            return false;
        }

        // Зміна статусу і запис у журнал — в одній транзакції
        using (var scope = new TransactionScope())
        {
            _userDal.SetActive(userId, activate);
            _statusChangeDal.Create(new AccountStatusChangeDto
            {
                UserId = userId,
                ChangedByUserId = admin.UserId,
                NewIsActive = activate,
                Reason = reason
            });
            scope.Complete();
        }

        message = activate ? $"Акаунт {target.Login} активовано." : $"Акаунт {target.Login} заблоковано.";
        return true;
    }
}