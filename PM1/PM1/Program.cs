using System;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // Створення або підключення до бази даних
        string connectionString = "Data Source=jokes.db;Version=3;";
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            // Створення таблиці, якщо вона не існує
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Jokes (Id INTEGER PRIMARY KEY AUTOINCREMENT, Setup TEXT, Punchline TEXT)";
            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        // Виконати запит до API та отримати жарт
        string apiUrl = "https://official-joke-api.appspot.com/random_joke";
        var joke = await GetJokeFromApi(apiUrl);

        // Вивести жарт на екран
        Console.WriteLine("Жарт:");
        Console.WriteLine($"Setup: {joke.Setup}");
        Console.WriteLine($"Punchline: {joke.Punchline}");

        // Зберегти жарт у базу даних
        SaveJokeToDatabase(joke, connectionString);

        // Вивести всі жарти з бази даних
        Console.WriteLine("\nЗбережені жарти в базі:");
        GetJokesFromDatabase(connectionString);
    }

    // Функція для запиту до API
    static async Task<Joke> GetJokeFromApi(string apiUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetStringAsync(apiUrl);
            var joke = JsonConvert.DeserializeObject<Joke>(response);
            return joke;
        }
    }

    // Функція для збереження жарту в базу даних
    static void SaveJokeToDatabase(Joke joke, string connectionString)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string insertQuery = "INSERT INTO Jokes (Setup, Punchline) VALUES (@Setup, @Punchline)";
            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Setup", joke.Setup);
                command.Parameters.AddWithValue("@Punchline", joke.Punchline);
                command.ExecuteNonQuery();
            }
        }
    }

    // Функція для виведення жартів з бази даних
    static void GetJokesFromDatabase(string connectionString)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string selectQuery = "SELECT * FROM Jokes";
            using (var command = new SQLiteCommand(selectQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Id: {reader["Id"]}, Setup: {reader["Setup"]}, Punchline: {reader["Punchline"]}");
                }
            }
        }
    }
}

// Клас для збереження структури жарту
public class Joke
{
    public string Setup { get; set; }
    public string Punchline { get; set; }
}
