using TradingCompany.DAL;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DTO;

namespace TradingCompany.Services;

public class AuthService(IUserDal userDal, IUserLoginDal userLoginDal)
{
    private readonly IUserDal _userDal = userDal;
    private readonly IUserLoginDal _userLoginDal = userLoginDal;

    public UserDto? Login(string login, string password, out string message)
    {
        var user = _userDal.GetByLogin(login);
        if (user is null)
        {
            message = "Невірний логін або пароль.";
            return null;
        }

        var passwordCorrect = PasswordHasher.Verify(password, user.PasswordHash);

        // Записуємо спробу входу в історію
        _userLoginDal.Create(new UserLoginDto
        {
            UserId = user.UserId,
            IsSuccessful = passwordCorrect && user.IsActive,
            Device = Environment.MachineName
        });

        if (!passwordCorrect)
        {
            message = "Невірний логін або пароль.";
            return null;
        }
        if (!user.IsActive)
        {
            message = "Ваш акаунт заблоковано. Зверніться до адміністратора.";
            return null;
        }

        message = $"Вітаємо, {user.FullName}!";
        return user;
    }
}