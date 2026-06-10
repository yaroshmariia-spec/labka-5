static class AuthManager
{
    private const string USERS_FILE = "data/users.csv";
    private const string USERS_HEADER = "Id,Email,Password";

    public static User? CurrentUser { get; private set; }

    public static bool IsLoggedIn => CurrentUser != null;

    public static void Logout()
    {
        CurrentUser = null;
    }

    public static bool Register()
    {
        Console.Clear();
        Console.WriteLine("=== РЕЄСТРАЦIЯ НОВОГО КОРИСТУВАЧА ===\n");

        Console.Write("Email: ");
        string email = (Console.ReadLine() ?? "").Trim().ToLower();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            Console.WriteLine("Некоректний email! Натиснiть Enter...");
            Console.ReadLine();
            return false;
        }

        Console.Write("Пароль: ");
        string password = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(password) || password.Length < 3)
        {
            Console.WriteLine("Пароль має мiстити щонайменше 3 символи! Натиснiть Enter...");
            Console.ReadLine();
            return false;
        }

        Console.Write("Пiдтвердiть пароль: ");
        string confirm = Console.ReadLine() ?? "";

        if (password != confirm)
        {
            Console.WriteLine("Паролi не спiвпадають! Натиснiть Enter...");
            Console.ReadLine();
            return false;
        }

        CsvManager.EnsureDataDir();

        var users = CsvManager.ReadAll(USERS_FILE, USERS_HEADER, User.FromCsv);

        foreach (var u in users)
        {
            if (u.Email.ToLower() == email)
            {
                Console.WriteLine("Користувач з таким email вже iснує! Натиснiть Enter...");
                Console.ReadLine();
                return false;
            }
        }

        int newId = CsvManager.GenerateNextId(USERS_FILE, User.FromCsv, u => u.Id);
        var user = new User(newId, email, PasswordHelper.Hash(password));
        CsvManager.Append(USERS_FILE, USERS_HEADER, user, u => u.ToCsv());

        Console.WriteLine("Реєстрацiя успiшна! Натиснiть Enter...");
        Console.ReadLine();
        return true;
    }

    public static bool Login()
    {
        Console.Clear();
        Console.WriteLine("=== АВТОРИЗАЦIЯ ===\n");

        Console.Write("Email: ");
        string email = (Console.ReadLine() ?? "").Trim().ToLower();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email не може бути порожнiм! Натиснiть Enter...");
            Console.ReadLine();
            return false;
        }

        Console.Write("Пароль: ");
        string password = Console.ReadLine() ?? "";

        CsvManager.EnsureDataDir();

        var users = CsvManager.ReadAll(USERS_FILE, USERS_HEADER, User.FromCsv);
        string hash = PasswordHelper.Hash(password);

        foreach (var u in users)
        {
            if (u.Email.ToLower() == email && u.PasswordHash == hash)
            {
                CurrentUser = u;
                Console.WriteLine($"\nВiтаємо, {u.Email}!");
                Console.ReadLine();
                return true;
            }
        }

        Console.WriteLine("Невiрний email або пароль! Натиснiть Enter...");
        Console.ReadLine();
        return false;
    }

    public static bool AuthMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("==============================================");
            Console.WriteLine("   СИСТЕМА УПРАВЛIННЯ БРОНЮВАННЯМИ");
            Console.WriteLine("==============================================");
            Console.WriteLine("1. Увiйти");
            Console.WriteLine("2. Зареєструватися");
            Console.WriteLine("0. Вийти");
            Console.Write("\nВиберiть дiю: ");

            string choice = Console.ReadLine() ?? "";
            switch (choice)
            {
                case "1":
                    if (Login()) return true;
                    break;
                case "2":
                    Register();
                    break;
                case "0":
                    return false;
                default:
                    Console.WriteLine("Некоректний вибiр! Натиснiть Enter...");
                    Console.ReadLine();
                    break;
            }
        }
    }
}
