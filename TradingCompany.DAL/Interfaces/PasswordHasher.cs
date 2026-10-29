using System.Security.Cryptography;
using System.Text;

namespace TradingCompany.DAL.Interfaces;

public static class PasswordHasher
{
    public static string Hash(string password) =>
        Convert.ToHexString(SHA256.HashData(Encoding.Unicode.GetBytes(password)));

    public static bool Verify(string password, string hash) => Hash(password) == hash;
}