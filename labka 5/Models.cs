using System.Security.Cryptography;
using System.Text;

class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public Product(int id, string name, string category, decimal price, int stock)
    {
        Id = id; Name = name; Category = category; Price = price; Stock = stock;
    }

    public override string ToString()
        => $"{Id,-5} {Name,-22} {Category,-17} {Price,8:F2} грн {Stock,5} шт";

    public string ToCsv() => $"{Id},{Name},{Category},{Price},{Stock}";

    public static Product? FromCsv(string line)
    {
        try
        {
            var parts = line.Split(',');
            if (parts.Length != 5) return null;
            if (!int.TryParse(parts[0], out int id)) return null;
            if (!decimal.TryParse(parts[3], out decimal price)) return null;
            if (!int.TryParse(parts[4], out int stock)) return null;
            return new Product(id, parts[1], parts[2], price, stock);
        }
        catch { return null; }
    }
}

class Client
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    public Client(int id, string name, string phone, string email)
    {
        Id = id; Name = name; Phone = phone; Email = email;
    }

    public override string ToString()
        => $"{Id,-5} {Name,-22} {Phone,-17} {Email,-28}";

    public string ToCsv() => $"{Id},{Name},{Phone},{Email}";

    public static Client? FromCsv(string line)
    {
        try
        {
            var parts = line.Split(',');
            if (parts.Length != 4) return null;
            if (!int.TryParse(parts[0], out int id)) return null;
            return new Client(id, parts[1], parts[2], parts[3]);
        }
        catch { return null; }
    }
}

class Booking
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }

    public Booking(int id, int clientId, int productId, int quantity, DateTime date, string status)
    {
        Id = id; ClientId = clientId; ProductId = productId; Quantity = quantity; Date = date; Status = status;
    }

    public override string ToString()
        => $"{Id,-5} {ClientId,-10} {ProductId,-12} {Quantity,-8} {Date:dd.MM.yyyy,-14} {Status,-12}";

    public string ToCsv() => $"{Id},{ClientId},{ProductId},{Quantity},{Date:dd.MM.yyyy},{Status}";

    public static Booking? FromCsv(string line)
    {
        try
        {
            var parts = line.Split(',');
            if (parts.Length != 6) return null;
            if (!int.TryParse(parts[0], out int id)) return null;
            if (!int.TryParse(parts[1], out int clientId)) return null;
            if (!int.TryParse(parts[2], out int productId)) return null;
            if (!int.TryParse(parts[3], out int qty)) return null;
            if (!DateTime.TryParse(parts[4], out DateTime date)) return null;
            return new Booking(id, clientId, productId, qty, date, parts[5]);
        }
        catch { return null; }
    }
}

class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    public User(int id, string email, string passwordHash)
    {
        Id = id; Email = email; PasswordHash = passwordHash;
    }

    public string ToCsv() => $"{Id},{Email},{PasswordHash}";

    public static User? FromCsv(string line)
    {
        try
        {
            var parts = line.Split(',');
            if (parts.Length != 3) return null;
            if (!int.TryParse(parts[0], out int id)) return null;
            return new User(id, parts[1], parts[2]);
        }
        catch { return null; }
    }
}

static class PasswordHelper
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
