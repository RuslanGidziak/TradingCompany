using TradingCompany.DTO;

namespace TradingCompany.DAL.Interfaces;

public interface IUserDal
{
    // CRUD
    List<UserDto> GetAll();
    UserDto? GetById(int id);
    UserDto Create(UserDto user, string password);
    bool Update(UserDto user);
    bool Delete(int id);

    // Додатково для ролі Administrator
    UserDto? GetByLogin(string login);
    List<UserDto> Search(string? text, UserSortField sortBy, bool descending);
    bool SetActive(int userId, bool isActive);
    int CountActiveAdmins();
}