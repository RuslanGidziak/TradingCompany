using System.Text;
using Microsoft.Data.SqlClient;
using TradingCompany;
using TradingCompany.DAL;
using TradingCompany.DAL.Concrete;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DTO;
using TradingCompany.Services;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// ---------- Створюємо DAL-класи (ADO.NET) ----------
var connectionString = DbConfig.ConnectionString;
IUserDal userDal = new UserDal(connectionString);
IUserLoginDal userLoginDal = new UserLoginDal(connectionString);
IAccountStatusChangeDal statusChangeDal = new AccountStatusChangeDal(connectionString);

var authService = new AuthService(userDal, userLoginDal);
var adminService = new AdminService(userDal, statusChangeDal);

// ---------- Головний цикл: Вхід -> Стартова сторінка -> Вихід -> Вхід ----------
while (true)
{
    var currentUser = LoginPage();
    if (currentUser is null) break;   // порожній логін = закрити програму
    StartPage(currentUser);           // повертається, коли користувач розлогінився
}
Console.WriteLine("До побачення!");


// ======================= СТОРІНКА ВХОДУ =======================
UserDto? LoginPage()
{
    while (true)
    {
        Input.Header("TradingCompany — вхід у систему");
        Console.WriteLine("(залиште логін порожнім і натисніть Enter, щоб закрити програму)\n");

        Console.Write("Логін: ");
        var login = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(login)) return null;

        var password = Input.ReadPassword("Пароль");

        try
        {
            var user = authService.Login(login, password, out var message);
            if (user is not null)
            {
                Input.Success(message);
                Input.Pause();
                return user;
            }
            Input.Error(message);
        }
        catch (SqlException ex)
        {
            Input.Error("Немає зв'язку з базою даних: " + ex.Message);
        }
        Input.Pause();
    }
}


// ======================= СТАРТОВА СТОРІНКА =======================
void StartPage(UserDto me)
{
    while (true)
    {
        Input.Header("Стартова сторінка");
        Console.WriteLine($"Ви увійшли як: {me.FullName} ({me.Login}), роль: {me.Role}\n");
        Console.WriteLine(me.Role == Roles.Administrator
            ? "1. Перейти на адмін-сторінку"
            : "1. Перейти на сторінку своєї ролі");
        Console.WriteLine("2. Мій профіль та історія входів");
        Console.WriteLine("0. Вийти з акаунта");
        Console.Write("\nВаш вибір: ");

        switch (Console.ReadLine())
        {
            case "1":
                if (me.Role == Roles.Administrator) AdminPage(me);
                else ProfilePage(me); // у звичайного користувача — лише профіль
                break;
            case "2":
                ProfilePage(me);
                break;
            case "0":
                Input.Success("Ви вийшли з акаунта.");
                Input.Pause();
                return;
        }
    }
}


// ======================= ПРОФІЛЬ =======================
void ProfilePage(UserDto me)
{
    Input.Header("Мій профіль");
    Console.WriteLine($"Логін:    {me.Login}");
    Console.WriteLine($"Ім'я:     {me.FullName}");
    Console.WriteLine($"Email:    {me.Email}");
    Console.WriteLine($"Роль:     {me.Role}");
    Console.WriteLine($"Створено: {me.CreatedAt:dd.MM.yyyy}\n");
    Console.WriteLine("Останні 10 входів:");
    foreach (var login in userLoginDal.GetByUser(me.UserId).Take(10))
    {
        Console.WriteLine(login);
    }
    Input.Pause();
}


// ======================= АДМІН-СТОРІНКА =======================
void AdminPage(UserDto me)
{
    string? search = null;
    var sortBy = UserSortField.Id;
    var descending = false;

    while (true)
    {
        Input.Header("Адмін-сторінка: таблиця користувачів");
        Console.WriteLine($"Пошук: {search ?? "(немає)"}   |   Сортування: {sortBy} {(descending ? "за спаданням" : "за зростанням")}\n");

        try
        {
            PrintUsersTable(adminService.GetUsers(search, sortBy, descending));
        }
        catch (SqlException ex)
        {
            Input.Error("Помилка бази даних: " + ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("1. Пошук               2. Сортування          3. Скинути пошук і сортування");
        Console.WriteLine("4. Заблокувати         5. Активувати");
        Console.WriteLine("6. Додати користувача  7. Редагувати          8. Видалити");
        Console.WriteLine("9. Журнал блокувань    10. Історія входів");
        Console.WriteLine("0. Назад на стартову сторінку");
        Console.Write("\nВаш вибір: ");

        var choice = Console.ReadLine();
        if (choice == "0") return;

        try
        {
            switch (choice)
            {
                case "1": search = Input.ReadOptional("Текст для пошуку (логін, ім'я, прізвище або email)"); break;
                case "2": (sortBy, descending) = ChooseSort(); break;
                case "3": search = null; sortBy = UserSortField.Id; descending = false; break;
                case "4": ChangeStatus(me, activate: false); break;
                case "5": ChangeStatus(me, activate: true); break;
                case "6": AddUser(); break;
                case "7": EditUser(me); break;
                case "8": DeleteUser(me); break;
                case "9": ShowList("Журнал блокувань і активацій", statusChangeDal.GetAll()); break;
                case "10": ShowList("Історія входів (останні 30)", userLoginDal.GetAll().Take(30)); break;
            }
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            Input.Error("Запис пов'язаний з іншими даними (цей адмін блокував інших). Заблокуйте його замість видалення.");
            Input.Pause();
        }
        catch (SqlException ex)
        {
            Input.Error("Помилка бази даних: " + ex.Message);
            Input.Pause();
        }
    }
}


// ======================= ДОПОМІЖНІ МЕТОДИ =======================
static void PrintUsersTable(List<UserDto> users)
{
    Console.WriteLine($"{"Id",-4}| {"Логін",-11}| {"Ім'я та прізвище",-22}| {"Email",-20}| {"Роль",-14}| {"Статус",-12}| Створено");
    Console.WriteLine(new string('-', 104));
    foreach (var u in users)
    {
        var old = Console.ForegroundColor;
        if (!u.IsActive) Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"{u.UserId,-4}| {u.Login,-11}| {u.FullName,-22}| {u.Email,-20}| {u.Role,-14}| {(u.IsActive ? "Активний" : "Заблоковано"),-12}| {u.CreatedAt:dd.MM.yyyy}");
        Console.ForegroundColor = old;
    }
    Console.WriteLine($"\nЗнайдено: {users.Count}");
}

static (UserSortField, bool) ChooseSort()
{
    var fields = Enum.GetValues<UserSortField>();
    Console.WriteLine("Сортувати за:");
    for (var i = 0; i < fields.Length; i++)
    {
        Console.WriteLine($"  {i + 1}. {fields[i]}");
    }
    var number = Input.ReadInt("Номер поля");
    var field = number >= 1 && number <= fields.Length ? fields[number - 1] : UserSortField.Id;
    var desc = Input.Confirm("За спаданням?");
    return (field, desc);
}

void ChangeStatus(UserDto me, bool activate)
{
    var id = Input.ReadInt("Id користувача");
    var reason = Input.ReadOptional("Причина (необов'язково)");

    if (adminService.ChangeStatus(me, id, activate, reason, out var message))
        Input.Success(message);
    else
        Input.Error(message);

    Input.Pause();
}

void AddUser()
{
    var user = new UserDto
    {
        Login = Input.ReadText("Логін"),
        Email = Input.ReadText("Email"),
        FirstName = Input.ReadText("Ім'я"),
        LastName = Input.ReadText("Прізвище"),
        Role = Input.ReadInt("Роль (1 — Administrator, 2 — User)") == 1 ? Roles.Administrator : Roles.User
    };
    var password = Input.ReadPassword("Пароль");

    userDal.Create(user, password);
    Input.Success($"Створено користувача з Id = {user.UserId}");
    Input.Pause();
}

void EditUser(UserDto me)
{
    var user = userDal.GetById(Input.ReadInt("Id користувача"));
    if (user is null)
    {
        Input.Error("Не знайдено.");
        Input.Pause();
        return;
    }

    Console.WriteLine("(Enter — залишити як є)");
    user.Login = Input.ReadText("Логін", user.Login);
    user.Email = Input.ReadText("Email", user.Email);
    user.FirstName = Input.ReadText("Ім'я", user.FirstName);
    user.LastName = Input.ReadText("Прізвище", user.LastName);

    if (user.UserId != me.UserId) // власну роль змінювати не можна
    {
        var roleNumber = Input.ReadText("Роль (1 — Administrator, 2 — User)", user.Role == Roles.Administrator ? "1" : "2");
        var newRole = roleNumber == "1" ? Roles.Administrator : Roles.User;
        if (newRole != user.Role && adminService.IsLastActiveAdmin(user))
        {
            Input.Error("Це останній активний адміністратор — роль змінити не можна.");
            Input.Pause();
            return;
        }
        user.Role = newRole;
    }

    if (userDal.Update(user))
    {
        Input.Success("Збережено.");
        if (user.UserId == me.UserId)
        {
            me.Login = user.Login;
            me.Email = user.Email;
            me.FirstName = user.FirstName;
            me.LastName = user.LastName;
        }
    }
    else
    {
        Input.Error("Не вдалося зберегти.");
    }
    Input.Pause();
}

void DeleteUser(UserDto me)
{
    var user = userDal.GetById(Input.ReadInt("Id користувача"));
    if (user is null)
    {
        Input.Error("Не знайдено.");
        Input.Pause();
        return;
    }
    if (user.UserId == me.UserId)
    {
        Input.Error("Не можна видалити власний акаунт.");
        Input.Pause();
        return;
    }
    if (adminService.IsLastActiveAdmin(user))
    {
        Input.Error("Не можна видалити останнього адміністратора.");
        Input.Pause();
        return;
    }
    if (!Input.Confirm($"Видалити користувача {user.Login}?")) return;

    if (userDal.Delete(user.UserId)) Input.Success("Користувача видалено.");
    else Input.Error("Не знайдено.");
    Input.Pause();
}

static void ShowList<T>(string title, IEnumerable<T> items)
{
    Input.Header(title);
    foreach (var item in items)
    {
        Console.WriteLine(item);
    }
    Input.Pause();
}