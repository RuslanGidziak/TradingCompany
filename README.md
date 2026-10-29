**UserLogins**
![UserLogins](docs/table-userlogins.png)

**AccountStatusChanges**
![AccountStatusChanges](docs/table-accountstatuschanges.png)

---

## Частина 2 — Застосунок C# (.NET 8)

| Проєкт | Призначення |
|---|---|
| `TradingCompany.DTO` | DTO-класи: `UserDto`, `UserLoginDto`, `AccountStatusChangeDto` |
| `TradingCompany.DAL` | Доступ до даних через **ADO.NET**: окремий інтерфейс + клас з CRUD для кожної сутності |
| `TradingCompany.DAL.Tests` | Тести **NUnit** для кожної сутності (з транзакціями — база не змінюється) |
| `TradingCompany` | Консольна інтерактивна програма для ролі Administrator |

### Функціонал ролі Administrator
- вхід у систему (логін + пароль);
- стартова сторінка з меню переходу на сторінку своєї ролі;
- адмін-сторінка: таблиця користувачів, пошук, сортування;
- блокування та активація акаунтів користувачів;
- у системі завжди лишається хоча б один адміністратор;
- вихід з акаунта.

## Як запустити
1. Виконати в SSMS `Database/01_CreateDatabase.sql`, потім `Database/02_SeedData.sql`
2. Вказати свій сервер і пароль у `TradingCompany.DAL/DbConfig.cs`
3. Запустити проєкт `TradingCompany` у Visual Studio
4. Тести: **Test → Test Explorer → Run All**

Тестовий вхід: `admin` / `Pass123
