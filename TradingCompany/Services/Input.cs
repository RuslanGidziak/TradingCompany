using System.Text;

namespace TradingCompany;

public static class Input
{
    public static string ReadText(string prompt, string? current = null)
    {
        while (true)
        {
            Console.Write(current is null ? $"{prompt}: " : $"{prompt} [{current}]: ");
            var text = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
            if (current is not null) return current;
            Error("Поле обов'язкове.");
        }
    }

    public static string? ReadOptional(string prompt)
    {
        Console.Write($"{prompt}: ");
        var text = Console.ReadLine();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            if (int.TryParse(Console.ReadLine(), out var number)) return number;
            Error("Введіть ціле число.");
        }
    }

    public static string ReadPassword(string prompt)
    {
        Console.Write($"{prompt}: ");
        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write('*');
            }
        }
        Console.WriteLine();
        return password.ToString();
    }

    public static bool Confirm(string question)
    {
        Console.Write($"{question} (y/n): ");
        var answer = Console.ReadLine()?.Trim().ToLower();
        return answer is "y" or "yes" or "т" or "так";
    }

    public static void Header(string text)
    {
        Console.Clear();
        WriteColored($"===== {text} =====\n", ConsoleColor.Cyan);
    }

    public static void Success(string text) => WriteColored(text, ConsoleColor.Green);
    public static void Error(string text) => WriteColored(text, ConsoleColor.Red);

    public static void Pause()
    {
        Console.Write("\nНатисніть Enter, щоб продовжити...");
        Console.ReadLine();
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = old;
    }
}