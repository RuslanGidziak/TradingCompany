using TradingCompany.DTO;

namespace TradingCompany.DAL.Interfaces;

public interface IAccountStatusChangeDal
{
    List<AccountStatusChangeDto> GetAll();
    AccountStatusChangeDto? GetById(int id);
    AccountStatusChangeDto Create(AccountStatusChangeDto change);
    bool Update(AccountStatusChangeDto change);
    bool Delete(int id);

    List<AccountStatusChangeDto> GetByUser(int userId);
}