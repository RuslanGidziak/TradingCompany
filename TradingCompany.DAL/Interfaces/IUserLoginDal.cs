using TradingCompany.DTO;

namespace TradingCompany.DAL.Interfaces;

public interface IUserLoginDal
{
    List<UserLoginDto> GetAll();
    UserLoginDto? GetById(int id);
    UserLoginDto Create(UserLoginDto login);
    bool Update(UserLoginDto login);
    bool Delete(int id);

    List<UserLoginDto> GetByUser(int userId);
}