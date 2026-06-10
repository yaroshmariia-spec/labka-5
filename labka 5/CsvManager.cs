static class CsvManager
{
    public const string DATA_DIR = "data";

    public static string DataPath(string fileName) =>
        Path.Combine(DATA_DIR, fileName);

    public static void EnsureDataDir()
    {
        if (!Directory.Exists(DATA_DIR))
            Directory.CreateDirectory(DATA_DIR);
    }

    public static List<T> ReadAll<T>(string path, string expectedHeader, Func<string, T?> parser) where T : class
    {
        var result = new List<T>();
        try
        {
            if (!File.Exists(path)) return result;

            var lines = File.ReadAllLines(path);
            if (lines.Length == 0) return result;

            if (lines[0].Trim() != expectedHeader)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Попередження: неочікувана шапка у файлi {path}. Очiкувалось: {expectedHeader}");
                Console.ResetColor();
                return result;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                var item = parser(lines[i]);
                if (item != null)
                    result.Add(item);
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Попередження: пропущено некоректний рядок {i + 1} у {path}: {lines[i]}");
                    Console.ResetColor();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка читання файлу {path}: {ex.Message}");
            Console.ResetColor();
        }
        return result;
    }

    public static void WriteAll<T>(string path, string header, List<T> items, Func<T, string> toCsv)
    {
        try
        {
            var lines = new List<string> { header };
            foreach (var item in items)
                lines.Add(toCsv(item));
            File.WriteAllLines(path, lines);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка запису файлу {path}: {ex.Message}");
            Console.ResetColor();
        }
    }

    public static void Append<T>(string path, string header, T item, Func<T, string> toCsv)
    {
        try
        {
            if (!File.Exists(path))
            {
                File.WriteAllLines(path, [header, toCsv(item)]);
                return;
            }
            File.AppendAllText(path, "\n" + toCsv(item));
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка дозапису у файл {path}: {ex.Message}");
            Console.ResetColor();
        }
    }

    public static int GenerateNextId<T>(string path, Func<string, T?> parser, Func<T, int> getId) where T : class
    {
        if (!File.Exists(path)) return 1;
        try
        {
            var lines = File.ReadAllLines(path);
            int max = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var item = parser(lines[i]);
                if (item != null)
                {
                    int id = getId(item);
                    if (id > max) max = id;
                }
            }
            return max + 1;
        }
        catch { return 1; }
    }
}
