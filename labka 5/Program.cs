class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        if (!AuthManager.AuthMenu()) return;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==============================================");
            Console.WriteLine("   СИСТЕМА УПРАВЛIННЯ БРОНЮВАННЯМИ");
            Console.WriteLine("==============================================");
            Console.WriteLine($"   Користувач: {AuthManager.CurrentUser?.Email}");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("1. Робота з товарами");
            Console.WriteLine("2. Робота з клiєнтами");
            Console.WriteLine("3. Робота з бронюваннями");
            Console.WriteLine("0. Вихiд");
            Console.Write("\nВиберiть роздiл: ");

            string choice = Console.ReadLine() ?? "";
            switch (choice)
            {
                case "1": ProductsMenu(); break;
                case "2": ClientsMenu(); break;
                case "3": BookingsMenu(); break;
                case "0": return;
                default:
                    Console.WriteLine("Некоректний вибiр! Натиснiть Enter...");
                    Console.ReadLine();
                    break;
            }
        }
    }

    // ==================== ШЛЯХИ ДО ФАЙЛIВ ====================

    const string PRODUCTS_FILE = "data/products.csv";
    const string PRODUCTS_HEADER = "Id,Name,Category,Price,Stock";
    const string CLIENTS_FILE = "data/clients.csv";
    const string CLIENTS_HEADER = "Id,Name,Phone,Email";
    const string BOOKINGS_FILE = "data/bookings.csv";
    const string BOOKINGS_HEADER = "Id,ClientId,ProductId,Quantity,Date,Status";

    // ==================== ТОВАРИ ====================

    static void ProductsMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========== ТОВАРИ ==========");
            Console.WriteLine("1. Додати товар");
            Console.WriteLine("2. Вивести всi товари");
            Console.WriteLine("3. Пошук товару");
            Console.WriteLine("4. Редагувати товар");
            Console.WriteLine("5. Видалити товар");
            Console.WriteLine("6. Сортування товарiв");
            Console.WriteLine("7. Статистика товарiв");
            Console.WriteLine("0. Назад");
            Console.Write("\nВиберiть дiю: ");

            string choice = Console.ReadLine() ?? "";
            switch (choice)
            {
                case "1": AddProduct(); break;
                case "2": DisplayProducts(); break;
                case "3": SearchProducts(); break;
                case "4": EditProduct(); break;
                case "5": DeleteProduct(); break;
                case "6": SortProducts(); break;
                case "7": ProductStatistics(); break;
                case "0": return;
                default:
                    Console.WriteLine("Некоректний вибiр! Натиснiть Enter...");
                    Console.ReadLine();
                    break;
            }
        }
    }

    static List<Product> LoadProducts()
    {
        CsvManager.EnsureDataDir();
        return CsvManager.ReadAll(ProductsDataPath(), PRODUCTS_HEADER, Product.FromCsv);
    }

    static string ProductsDataPath() =>
        CsvManager.DataPath("products.csv");

    static void SaveProducts(List<Product> products)
    {
        CsvManager.WriteAll(ProductsDataPath(), PRODUCTS_HEADER, products, p => p.ToCsv());
    }

    static int NextProductId() =>
        CsvManager.GenerateNextId(ProductsDataPath(), Product.FromCsv, p => p.Id);

    static void AddProduct()
    {
        Console.Clear();
        Console.WriteLine("--- Додавання товарiв ---");
        Console.Write("Скiльки товарiв додати? ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Console.WriteLine("Некоректна кiлькiсть! Натиснiть Enter...");
            Console.ReadLine();
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"\n--- Товар #{i + 1} ---");
            try
            {
                Console.Write("Назва: ");
                string name = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Назва не може бути порожньою!");
                    i--; continue;
                }

                Console.Write("Категорiя: ");
                string category = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(category))
                {
                    Console.WriteLine("Категорiя не може бути порожньою!");
                    i--; continue;
                }

                Console.Write("Цiна: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price <= 0)
                {
                    Console.WriteLine("Некоректна цiна!");
                    i--; continue;
                }

                Console.Write("Кiлькiсть на складi: ");
                if (!int.TryParse(Console.ReadLine(), out int stock) || stock < 0)
                {
                    Console.WriteLine("Некоректна кiлькiсть!");
                    i--; continue;
                }

                int newId = NextProductId();
                var product = new Product(newId, name, category, price, stock);
                CsvManager.Append(ProductsDataPath(), PRODUCTS_HEADER, product, p => p.ToCsv());
                Console.WriteLine("Товар успiшно додано!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
                i--;
            }
        }

        Console.WriteLine($"\nДодано {count} товар(ів). Натиснiть Enter...");
        Console.ReadLine();
    }

    static void DisplayProducts()
    {
        Console.Clear();
        Console.WriteLine("--- Список товарiв ---\n");

        var products = LoadProducts();
        if (products.Count == 0)
        {
            Console.WriteLine("Товари вiдсутнi.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"{"ID",-5} {"Назва",-22} {"Категорiя",-17} {"Цiна",10} {"Склад",6}");
        Console.WriteLine(new string('-', 62));
        foreach (var p in products)
            Console.WriteLine(p);
        Console.WriteLine($"\nВсього товарiв: {products.Count}");
        Console.ReadLine();
    }

    static void SearchProducts()
    {
        Console.Clear();
        Console.WriteLine("--- Пошук товарiв ---");
        Console.WriteLine("1. Пошук за ID");
        Console.WriteLine("2. Пошук за назвою");
        Console.WriteLine("3. Пошук за категорiєю");
        Console.Write("Виберiть критерiй: ");

        string choice = Console.ReadLine() ?? "";
        var products = LoadProducts();

        switch (choice)
        {
            case "1":
                Console.Write("Введiть ID: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var found = products.Find(p => p.Id == id);
                    if (found != null)
                    {
                        Console.WriteLine($"\n{"ID",-5} {"Назва",-22} {"Категорiя",-17} {"Цiна",10} {"Склад",6}");
                        Console.WriteLine(new string('-', 62));
                        Console.WriteLine(found);
                    }
                    else Console.WriteLine("Товар з таким ID не знайдено.");
                }
                else Console.WriteLine("Некоректний ID.");
                break;

            case "2":
                Console.Write("Введiть назву (або частину): ");
                string nameSearch = (Console.ReadLine() ?? "").ToLower();
                var byName = products.FindAll(p => p.Name.ToLower().Contains(nameSearch));
                if (byName.Count > 0)
                {
                    Console.WriteLine($"\nЗнайдено {byName.Count} товар(ів):");
                    Console.WriteLine($"{"ID",-5} {"Назва",-22} {"Категорiя",-17} {"Цiна",10} {"Склад",6}");
                    Console.WriteLine(new string('-', 62));
                    foreach (var p in byName) Console.WriteLine(p);
                }
                else Console.WriteLine("Товарiв за таким запитом не знайдено.");
                break;

            case "3":
                Console.Write("Введiть категорiю: ");
                string catSearch = (Console.ReadLine() ?? "").ToLower();
                var byCat = products.FindAll(p => p.Category.ToLower().Contains(catSearch));
                if (byCat.Count > 0)
                {
                    Console.WriteLine($"\nЗнайдено {byCat.Count} товар(ів):");
                    Console.WriteLine($"{"ID",-5} {"Назва",-22} {"Категорiя",-17} {"Цiна",10} {"Склад",6}");
                    Console.WriteLine(new string('-', 62));
                    foreach (var p in byCat) Console.WriteLine(p);
                }
                else Console.WriteLine("Товарiв у такiй категорiї не знайдено.");
                break;

            default:
                Console.WriteLine("Некоректний вибiр.");
                break;
        }

        Console.ReadLine();
    }

    static void EditProduct()
    {
        Console.Clear();
        Console.WriteLine("--- Редагування товару ---");
        Console.Write("Введiть ID товару для редагування: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некоректний ID!");
            Console.ReadLine();
            return;
        }

        var products = LoadProducts();
        int index = products.FindIndex(p => p.Id == id);
        if (index < 0)
        {
            Console.WriteLine("Товар з таким ID не знайдено.");
            Console.ReadLine();
            return;
        }

        var p = products[index];
        Console.WriteLine($"\nРедагування товару: {p.Name} (ID={p.Id})");
        Console.WriteLine("Залиште поле порожнiм, щоб не змiнювати.\n");

        Console.Write($"Назва [{p.Name}]: ");
        string name = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(name)) p.Name = name;

        Console.Write($"Категорiя [{p.Category}]: ");
        string category = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(category)) p.Category = category;

        Console.Write($"Цiна [{p.Price}]: ");
        string priceStr = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(priceStr) && decimal.TryParse(priceStr, out decimal price) && price > 0)
            p.Price = price;

        Console.Write($"Кiлькiсть [{p.Stock}]: ");
        string stockStr = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(stockStr) && int.TryParse(stockStr, out int stock) && stock >= 0)
            p.Stock = stock;

        SaveProducts(products);
        Console.WriteLine("Товар оновлено! Натиснiть Enter...");
        Console.ReadLine();
    }

    static void DeleteProduct()
    {
        Console.Clear();
        Console.WriteLine("--- Видалення товару ---");
        Console.Write("Введiть ID товару: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некоректний ID!");
            Console.ReadLine();
            return;
        }

        var products = LoadProducts();
        int index = products.FindIndex(p => p.Id == id);
        if (index < 0)
        {
            Console.WriteLine("Товар з таким ID не знайдено.");
        }
        else
        {
            Product removed = products[index];
            products.RemoveAt(index);
            SaveProducts(products);
            Console.WriteLine($"Товар \"{removed.Name}\" (ID={removed.Id}) видалено.");
        }

        Console.ReadLine();
    }

    static void SortProducts()
    {
        Console.Clear();
        Console.WriteLine("--- Сортування товарiв ---");
        Console.WriteLine("1. За назвою (А-Я)");
        Console.WriteLine("2. За назвою (Я-А)");
        Console.WriteLine("3. За цiною (зростання)");
        Console.WriteLine("4. За цiною (спадання)");
        Console.WriteLine("5. За категорiєю");
        Console.WriteLine("6. За кiлькiстю на складi");
        Console.Write("Виберiть тип сортування: ");

        string choice = Console.ReadLine() ?? "";
        var products = LoadProducts();

        switch (choice)
        {
            case "1": products.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture)); break;
            case "2": products.Sort((a, b) => string.Compare(b.Name, a.Name, StringComparison.CurrentCulture)); break;
            case "3": products.Sort((a, b) => a.Price.CompareTo(b.Price)); break;
            case "4": products.Sort((a, b) => b.Price.CompareTo(a.Price)); break;
            case "5": products.Sort((a, b) => string.Compare(a.Category, b.Category, StringComparison.CurrentCulture)); break;
            case "6": products.Sort((a, b) => a.Stock.CompareTo(b.Stock)); break;
            default:
                Console.WriteLine("Некоректний вибiр.");
                Console.ReadLine();
                return;
        }

        Console.Clear();
        Console.WriteLine("--- Вiдсортованi товари ---\n");
        Console.WriteLine($"{"ID",-5} {"Назва",-22} {"Категорiя",-17} {"Цiна",10} {"Склад",6}");
        Console.WriteLine(new string('-', 62));
        foreach (var p in products) Console.WriteLine(p);
        Console.WriteLine($"\nВсього товарiв: {products.Count}");
        Console.ReadLine();
    }

    static void ProductStatistics()
    {
        Console.Clear();
        Console.WriteLine("--- Статистика товарiв ---\n");

        var products = LoadProducts();
        if (products.Count == 0)
        {
            Console.WriteLine("Немає товарiв для статистики.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"Кiлькiсть товарiв: {products.Count}");

        Product minPrice = products[0], maxPrice = products[0];
        decimal sum = 0;

        foreach (var p in products)
        {
            if (p.Price < minPrice.Price) minPrice = p;
            if (p.Price > maxPrice.Price) maxPrice = p;
            sum += p.Price;
        }

        decimal avg = sum / products.Count;

        Console.WriteLine($"Мiнiмальна цiна: {minPrice.Price:F2} грн ({minPrice.Name})");
        Console.WriteLine($"Максимальна цiна: {maxPrice.Price:F2} грн ({maxPrice.Name})");
        Console.WriteLine($"Сума цiн: {sum:F2} грн");
        Console.WriteLine($"Середня цiна: {avg:F2} грн");

        var grouped = new Dictionary<string, int>();
        foreach (var p in products)
        {
            if (grouped.ContainsKey(p.Category))
                grouped[p.Category]++;
            else
                grouped[p.Category] = 1;
        }

        Console.WriteLine("\nРозподiл за категорiями:");
        foreach (var g in grouped)
            Console.WriteLine($"  {g.Key}: {g.Value} товар(ів)");

        Console.ReadLine();
    }

    // ==================== КЛIЄНТИ ====================

    static void ClientsMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========== КЛIЄНТИ ==========");
            Console.WriteLine("1. Додати клiєнта");
            Console.WriteLine("2. Вивести всiх клiєнтiв");
            Console.WriteLine("3. Пошук клiєнта");
            Console.WriteLine("4. Редагувати клiєнта");
            Console.WriteLine("5. Видалити клiєнта");
            Console.WriteLine("6. Сортування клiєнтiв");
            Console.WriteLine("7. Статистика клiєнтiв");
            Console.WriteLine("0. Назад");
            Console.Write("\nВиберiть дiю: ");

            string choice = Console.ReadLine() ?? "";
            switch (choice)
            {
                case "1": AddClient(); break;
                case "2": DisplayClients(); break;
                case "3": SearchClients(); break;
                case "4": EditClient(); break;
                case "5": DeleteClient(); break;
                case "6": SortClients(); break;
                case "7": ClientStatistics(); break;
                case "0": return;
                default:
                    Console.WriteLine("Некоректний вибiр! Натиснiть Enter...");
                    Console.ReadLine();
                    break;
            }
        }
    }

    static List<Client> LoadClients()
    {
        CsvManager.EnsureDataDir();
        return CsvManager.ReadAll(ClientsDataPath(), CLIENTS_HEADER, Client.FromCsv);
    }

    static string ClientsDataPath() =>
        CsvManager.DataPath("clients.csv");

    static void SaveClients(List<Client> clients)
    {
        CsvManager.WriteAll(ClientsDataPath(), CLIENTS_HEADER, clients, c => c.ToCsv());
    }

    static int NextClientId() =>
        CsvManager.GenerateNextId(ClientsDataPath(), Client.FromCsv, c => c.Id);

    static void AddClient()
    {
        Console.Clear();
        Console.WriteLine("--- Додавання клiєнтiв ---");
        Console.Write("Скiльки клiєнтiв додати? ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Console.WriteLine("Некоректна кiлькiсть! Натиснiть Enter...");
            Console.ReadLine();
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"\n--- Клiєнт #{i + 1} ---");
            try
            {
                Console.Write("ПIБ: ");
                string name = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("ПIБ не може бути порожнiм!");
                    i--; continue;
                }

                Console.Write("Телефон: ");
                string phone = Console.ReadLine() ?? "";
                Console.Write("Email: ");
                string email = Console.ReadLine() ?? "";

                int newId = NextClientId();
                var client = new Client(newId, name, phone, email);
                CsvManager.Append(ClientsDataPath(), CLIENTS_HEADER, client, c => c.ToCsv());
                Console.WriteLine("Клiєнта успiшно додано!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
                i--;
            }
        }

        Console.WriteLine($"\nДодано {count} клiєнт(ів). Натиснiть Enter...");
        Console.ReadLine();
    }

    static void DisplayClients()
    {
        Console.Clear();
        Console.WriteLine("--- Список клiєнтiв ---\n");

        var clients = LoadClients();
        if (clients.Count == 0)
        {
            Console.WriteLine("Клiєнти вiдсутнi.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"{"ID",-5} {"ПIБ",-22} {"Телефон",-17} {"Email",-28}");
        Console.WriteLine(new string('-', 74));
        foreach (var c in clients)
            Console.WriteLine(c);
        Console.WriteLine($"\nВсього клiєнтiв: {clients.Count}");
        Console.ReadLine();
    }

    static void SearchClients()
    {
        Console.Clear();
        Console.WriteLine("--- Пошук клiєнтiв ---");
        Console.WriteLine("1. Пошук за ID");
        Console.WriteLine("2. Пошук за ПIБ");
        Console.WriteLine("3. Пошук за телефоном");
        Console.Write("Виберiть критерiй: ");

        string choice = Console.ReadLine() ?? "";
        var clients = LoadClients();

        switch (choice)
        {
            case "1":
                Console.Write("Введiть ID: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var found = clients.Find(c => c.Id == id);
                    if (found != null)
                    {
                        Console.WriteLine($"\n{"ID",-5} {"ПIБ",-22} {"Телефон",-17} {"Email",-28}");
                        Console.WriteLine(new string('-', 74));
                        Console.WriteLine(found);
                    }
                    else Console.WriteLine("Клiєнта з таким ID не знайдено.");
                }
                else Console.WriteLine("Некоректний ID.");
                break;

            case "2":
                Console.Write("Введiть ПIБ (або частину): ");
                string nameSearch = (Console.ReadLine() ?? "").ToLower();
                var byName = clients.FindAll(c => c.Name.ToLower().Contains(nameSearch));
                if (byName.Count > 0)
                {
                    Console.WriteLine($"\nЗнайдено {byName.Count} клiєнт(ів):");
                    Console.WriteLine($"{"ID",-5} {"ПIБ",-22} {"Телефон",-17} {"Email",-28}");
                    Console.WriteLine(new string('-', 74));
                    foreach (var c in byName) Console.WriteLine(c);
                }
                else Console.WriteLine("Клiєнтiв за таким запитом не знайдено.");
                break;

            case "3":
                Console.Write("Введiть телефон: ");
                string phoneSearch = (Console.ReadLine() ?? "").ToLower();
                var byPhone = clients.FindAll(c => c.Phone.ToLower().Contains(phoneSearch));
                if (byPhone.Count > 0)
                {
                    Console.WriteLine($"\nЗнайдено {byPhone.Count} клiєнт(ів):");
                    Console.WriteLine($"{"ID",-5} {"ПIБ",-22} {"Телефон",-17} {"Email",-28}");
                    Console.WriteLine(new string('-', 74));
                    foreach (var c in byPhone) Console.WriteLine(c);
                }
                else Console.WriteLine("Клiєнтiв за таким телефоном не знайдено.");
                break;

            default:
                Console.WriteLine("Некоректний вибiр.");
                break;
        }

        Console.ReadLine();
    }

    static void EditClient()
    {
        Console.Clear();
        Console.WriteLine("--- Редагування клiєнта ---");
        Console.Write("Введiть ID клiєнта для редагування: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некоректний ID!");
            Console.ReadLine();
            return;
        }

        var clients = LoadClients();
        int index = clients.FindIndex(c => c.Id == id);
        if (index < 0)
        {
            Console.WriteLine("Клiєнта з таким ID не знайдено.");
            Console.ReadLine();
            return;
        }

        var c = clients[index];
        Console.WriteLine($"\nРедагування клiєнта: {c.Name} (ID={c.Id})");
        Console.WriteLine("Залиште поле порожнiм, щоб не змiнювати.\n");

        Console.Write($"ПIБ [{c.Name}]: ");
        string name = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(name)) c.Name = name;

        Console.Write($"Телефон [{c.Phone}]: ");
        string phone = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(phone)) c.Phone = phone;

        Console.Write($"Email [{c.Email}]: ");
        string email = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(email)) c.Email = email;

        SaveClients(clients);
        Console.WriteLine("Клiєнта оновлено! Натиснiть Enter...");
        Console.ReadLine();
    }

    static void DeleteClient()
    {
        Console.Clear();
        Console.WriteLine("--- Видалення клiєнта ---");
        Console.Write("Введiть ID клiєнта: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некоректний ID!");
            Console.ReadLine();
            return;
        }

        var clients = LoadClients();
        int index = clients.FindIndex(c => c.Id == id);
        if (index < 0)
        {
            Console.WriteLine("Клiєнта з таким ID не знайдено.");
        }
        else
        {
            Client removed = clients[index];
            clients.RemoveAt(index);
            SaveClients(clients);
            Console.WriteLine($"Клiєнта \"{removed.Name}\" (ID={removed.Id}) видалено.");
        }

        Console.ReadLine();
    }

    static void SortClients()
    {
        Console.Clear();
        Console.WriteLine("--- Сортування клiєнтiв ---");
        Console.WriteLine("1. За ПIБ (А-Я)");
        Console.WriteLine("2. За ПIБ (Я-А)");
        Console.WriteLine("3. За ID (зростання)");
        Console.WriteLine("4. За ID (спадання)");
        Console.Write("Виберiть тип сортування: ");

        string choice = Console.ReadLine() ?? "";
        var clients = LoadClients();

        switch (choice)
        {
            case "1": clients.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture)); break;
            case "2": clients.Sort((a, b) => string.Compare(b.Name, a.Name, StringComparison.CurrentCulture)); break;
            case "3": clients.Sort((a, b) => a.Id.CompareTo(b.Id)); break;
            case "4": clients.Sort((a, b) => b.Id.CompareTo(a.Id)); break;
            default:
                Console.WriteLine("Некоректний вибiр.");
                Console.ReadLine();
                return;
        }

        Console.Clear();
        Console.WriteLine("--- Вiдсортованi клiєнти ---\n");
        Console.WriteLine($"{"ID",-5} {"ПIБ",-22} {"Телефон",-17} {"Email",-28}");
        Console.WriteLine(new string('-', 74));
        foreach (var c in clients) Console.WriteLine(c);
        Console.WriteLine($"\nВсього клiєнтiв: {clients.Count}");
        Console.ReadLine();
    }

    static void ClientStatistics()
    {
        Console.Clear();
        Console.WriteLine("--- Статистика клiєнтiв ---\n");

        var clients = LoadClients();
        if (clients.Count == 0)
        {
            Console.WriteLine("Немає клiєнтiв для статистики.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"Кiлькiсть клiєнтiв: {clients.Count}");

        int minNameLen = clients[0].Name.Length;
        int maxNameLen = clients[0].Name.Length;
        string minName = clients[0].Name;
        string maxName = clients[0].Name;

        foreach (var c in clients)
        {
            if (c.Name.Length < minNameLen) { minNameLen = c.Name.Length; minName = c.Name; }
            if (c.Name.Length > maxNameLen) { maxNameLen = c.Name.Length; maxName = c.Name; }
        }

        Console.WriteLine($"Найкоротше ПIБ: \"{minName}\" ({minNameLen} символiв)");
        Console.WriteLine($"Найдовше ПIБ: \"{maxName}\" ({maxNameLen} символiв)");

        Console.ReadLine();
    }

    // ==================== БРОНЮВАННЯ ====================

    static void BookingsMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========== БРОНЮВАННЯ ==========");
            Console.WriteLine("1. Додати бронювання");
            Console.WriteLine("2. Вивести всi бронювання");
            Console.WriteLine("3. Пошук бронювання");
            Console.WriteLine("4. Редагувати бронювання");
            Console.WriteLine("5. Видалити бронювання");
            Console.WriteLine("6. Сортування бронювань");
            Console.WriteLine("7. Статистика бронювань");
            Console.WriteLine("0. Назад");
            Console.Write("\nВиберiть дiю: ");

            string choice = Console.ReadLine() ?? "";
            switch (choice)
            {
                case "1": AddBooking(); break;
                case "2": DisplayBookings(); break;
                case "3": SearchBookings(); break;
                case "4": EditBooking(); break;
                case "5": DeleteBooking(); break;
                case "6": SortBookings(); break;
                case "7": BookingStatistics(); break;
                case "0": return;
                default:
                    Console.WriteLine("Некоректний вибiр! Натиснiть Enter...");
                    Console.ReadLine();
                    break;
            }
        }
    }

    static List<Booking> LoadBookings()
    {
        CsvManager.EnsureDataDir();
        return CsvManager.ReadAll(BookingsDataPath(), BOOKINGS_HEADER, Booking.FromCsv);
    }

    static string BookingsDataPath() =>
        CsvManager.DataPath("bookings.csv");

    static void SaveBookings(List<Booking> bookings)
    {
        CsvManager.WriteAll(BookingsDataPath(), BOOKINGS_HEADER, bookings, b => b.ToCsv());
    }

    static int NextBookingId() =>
        CsvManager.GenerateNextId(BookingsDataPath(), Booking.FromCsv, b => b.Id);

    static void AddBooking()
    {
        Console.Clear();
        Console.WriteLine("--- Додавання бронювань ---");
        Console.Write("Скiльки бронювань додати? ");
        if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
        {
            Console.WriteLine("Некоректна кiлькiсть! Натиснiть Enter...");
            Console.ReadLine();
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"\n--- Бронювання #{i + 1} ---");
            try
            {
                Console.Write("ID клiєнта: ");
                if (!int.TryParse(Console.ReadLine(), out int clientId) || clientId <= 0)
                {
                    Console.WriteLine("Некоректний ID клiєнта!");
                    i--; continue;
                }

                Console.Write("ID товару: ");
                if (!int.TryParse(Console.ReadLine(), out int productId) || productId <= 0)
                {
                    Console.WriteLine("Некоректний ID товару!");
                    i--; continue;
                }

                Console.Write("Кiлькiсть: ");
                if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
                {
                    Console.WriteLine("Некоректна кiлькiсть!");
                    i--; continue;
                }

                Console.Write("Статус (Нове/Пiдтверджено/Очiкує/Скасовано): ");
                string status = Console.ReadLine() ?? "Нове";

                int newId = NextBookingId();
                var booking = new Booking(newId, clientId, productId, qty, DateTime.Now, status);
                CsvManager.Append(BookingsDataPath(), BOOKINGS_HEADER, booking, b => b.ToCsv());
                Console.WriteLine("Бронювання успiшно додано!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
                i--;
            }
        }

        Console.WriteLine($"\nДодано {count} бронювань. Натиснiть Enter...");
        Console.ReadLine();
    }

    static void DisplayBookings()
    {
        Console.Clear();
        Console.WriteLine("--- Список бронювань ---\n");

        var bookings = LoadBookings();
        if (bookings.Count == 0)
        {
            Console.WriteLine("Бронювання вiдсутнi.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"{"ID",-5} {"КлiєнтID",-10} {"ТоварID",-12} {"Кiлькiсть",-8} {"Дата",-14} {"Статус",-12}");
        Console.WriteLine(new string('-', 63));
        foreach (var b in bookings)
            Console.WriteLine(b);
        Console.WriteLine($"\nВсього бронювань: {bookings.Count}");
        Console.ReadLine();
    }

    static void SearchBookings()
    {
        Console.Clear();
        Console.WriteLine("--- Пошук бронювань ---");
        Console.WriteLine("1. Пошук за ID");
        Console.WriteLine("2. Пошук за ID клiєнта");
        Console.WriteLine("3. Пошук за статусом");
        Console.Write("Виберiть критерiй: ");

        string choice = Console.ReadLine() ?? "";
        var bookings = LoadBookings();

        switch (choice)
        {
            case "1":
                Console.Write("Введiть ID бронювання: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var found = bookings.Find(b => b.Id == id);
                    if (found != null)
                    {
                        Console.WriteLine($"\n{"ID",-5} {"КлiєнтID",-10} {"ТоварID",-12} {"Кiлькiсть",-8} {"Дата",-14} {"Статус",-12}");
                        Console.WriteLine(new string('-', 63));
                        Console.WriteLine(found);
                    }
                    else Console.WriteLine("Бронювання з таким ID не знайдено.");
                }
                else Console.WriteLine("Некоректний ID.");
                break;

            case "2":
                Console.Write("Введiть ID клiєнта: ");
                if (int.TryParse(Console.ReadLine(), out int clientId))
                {
                    var byClient = bookings.FindAll(b => b.ClientId == clientId);
                    if (byClient.Count > 0)
                    {
                        Console.WriteLine($"\nЗнайдено {byClient.Count} бронювань:");
                        Console.WriteLine($"{"ID",-5} {"КлiєнтID",-10} {"ТоварID",-12} {"Кiлькiсть",-8} {"Дата",-14} {"Статус",-12}");
                        Console.WriteLine(new string('-', 63));
                        foreach (var b in byClient) Console.WriteLine(b);
                    }
                    else Console.WriteLine("Бронювань для цього клiєнта не знайдено.");
                }
                break;

            case "3":
                Console.Write("Введiть статус: ");
                string statusSearch = (Console.ReadLine() ?? "").ToLower();
                var byStatus = bookings.FindAll(b => b.Status.ToLower().Contains(statusSearch));
                if (byStatus.Count > 0)
                {
                    Console.WriteLine($"\nЗнайдено {byStatus.Count} бронювань:");
                    Console.WriteLine($"{"ID",-5} {"КлiєнтID",-10} {"ТоварID",-12} {"Кiлькiсть",-8} {"Дата",-14} {"Статус",-12}");
                    Console.WriteLine(new string('-', 63));
                    foreach (var b in byStatus) Console.WriteLine(b);
                }
                else Console.WriteLine("Бронювань з таким статусом не знайдено.");
                break;

            default:
                Console.WriteLine("Некоректний вибiр.");
                break;
        }

        Console.ReadLine();
    }

    static void EditBooking()
    {
        Console.Clear();
        Console.WriteLine("--- Редагування бронювання ---");
        Console.Write("Введiть ID бронювання: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некоректний ID!");
            Console.ReadLine();
            return;
        }

        var bookings = LoadBookings();
        int index = bookings.FindIndex(b => b.Id == id);
        if (index < 0)
        {
            Console.WriteLine("Бронювання з таким ID не знайдено.");
            Console.ReadLine();
            return;
        }

        var b = bookings[index];
        Console.WriteLine($"\nРедагування бронювання #{b.Id}");
        Console.WriteLine("Залиште поле порожнiм, щоб не змiнювати.\n");

        Console.Write($"ID клiєнта [{b.ClientId}]: ");
        string clientStr = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(clientStr) && int.TryParse(clientStr, out int clientId) && clientId > 0)
            b.ClientId = clientId;

        Console.Write($"ID товару [{b.ProductId}]: ");
        string productStr = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(productStr) && int.TryParse(productStr, out int productId) && productId > 0)
            b.ProductId = productId;

        Console.Write($"Кiлькiсть [{b.Quantity}]: ");
        string qtyStr = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(qtyStr) && int.TryParse(qtyStr, out int qty) && qty > 0)
            b.Quantity = qty;

        Console.Write($"Статус [{b.Status}]: ");
        string status = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(status)) b.Status = status;

        SaveBookings(bookings);
        Console.WriteLine("Бронювання оновлено! Натиснiть Enter...");
        Console.ReadLine();
    }

    static void DeleteBooking()
    {
        Console.Clear();
        Console.WriteLine("--- Видалення бронювання ---");
        Console.Write("Введiть ID бронювання: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Некоректний ID!");
            Console.ReadLine();
            return;
        }

        var bookings = LoadBookings();
        int index = bookings.FindIndex(b => b.Id == id);
        if (index < 0)
        {
            Console.WriteLine("Бронювання з таким ID не знайдено.");
        }
        else
        {
            Booking removed = bookings[index];
            bookings.RemoveAt(index);
            SaveBookings(bookings);
            Console.WriteLine($"Бронювання #{removed.Id} (клiєнт {removed.ClientId}) видалено.");
        }

        Console.ReadLine();
    }

    static void SortBookings()
    {
        Console.Clear();
        Console.WriteLine("--- Сортування бронювань ---");
        Console.WriteLine("1. За датою (зростання)");
        Console.WriteLine("2. За датою (спадання)");
        Console.WriteLine("3. За статусом");
        Console.WriteLine("4. За ID клiєнта");
        Console.Write("Виберiть тип сортування: ");

        string choice = Console.ReadLine() ?? "";
        var bookings = LoadBookings();

        switch (choice)
        {
            case "1": bookings.Sort((a, b) => a.Date.CompareTo(b.Date)); break;
            case "2": bookings.Sort((a, b) => b.Date.CompareTo(a.Date)); break;
            case "3": bookings.Sort((a, b) => string.Compare(a.Status, b.Status, StringComparison.CurrentCulture)); break;
            case "4": bookings.Sort((a, b) => a.ClientId.CompareTo(b.ClientId)); break;
            default:
                Console.WriteLine("Некоректний вибiр.");
                Console.ReadLine();
                return;
        }

        Console.Clear();
        Console.WriteLine("--- Вiдсортованi бронювання ---\n");
        Console.WriteLine($"{"ID",-5} {"КлiєнтID",-10} {"ТоварID",-12} {"Кiлькiсть",-8} {"Дата",-14} {"Статус",-12}");
        Console.WriteLine(new string('-', 63));
        foreach (var b in bookings) Console.WriteLine(b);
        Console.WriteLine($"\nВсього бронювань: {bookings.Count}");
        Console.ReadLine();
    }

    static void BookingStatistics()
    {
        Console.Clear();
        Console.WriteLine("--- Статистика бронювань ---\n");

        var bookings = LoadBookings();
        if (bookings.Count == 0)
        {
            Console.WriteLine("Немає бронювань для статистики.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"Кiлькiсть бронювань: {bookings.Count}");

        Booking minQty = bookings[0], maxQty = bookings[0];
        int totalQty = 0;

        foreach (var b in bookings)
        {
            if (b.Quantity < minQty.Quantity) minQty = b;
            if (b.Quantity > maxQty.Quantity) maxQty = b;
            totalQty += b.Quantity;
        }

        double avgQty = (double)totalQty / bookings.Count;

        Console.WriteLine($"Мiнiмальна кiлькiсть у бронюваннi: {minQty.Quantity} (ID={minQty.Id})");
        Console.WriteLine($"Максимальна кiлькiсть у бронюваннi: {maxQty.Quantity} (ID={maxQty.Id})");
        Console.WriteLine($"Загальна кiлькiсть замовлених товарiв: {totalQty}");
        Console.WriteLine($"Середня кiлькiсть на бронювання: {avgQty:F2}");

        var statusCount = new Dictionary<string, int>();
        foreach (var b in bookings)
        {
            if (statusCount.ContainsKey(b.Status))
                statusCount[b.Status]++;
            else
                statusCount[b.Status] = 1;
        }

        Console.WriteLine("\nРозподiл за статусами:");
        foreach (var s in statusCount)
            Console.WriteLine($"  {s.Key}: {s.Value} бронювань");

        Console.ReadLine();
    }
}
