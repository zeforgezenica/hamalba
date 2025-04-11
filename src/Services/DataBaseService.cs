using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace hamalba.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        //public void InitializeDatabase()
        //{
        //    using (var connection = new MySqlConnection(_connectionString))
        //    {
        //        try
        //        {
        //            connection.Open();
        //            Console.WriteLine("Uspješno povezano s bazom podataka!");

        //            Random random = new Random();
        //            string tableName = "RandomTable_" + random.Next(1000, 9999);

        //            string createTableQuery = $@"
        //                CREATE TABLE {tableName} (
        //                    ID INT PRIMARY KEY AUTO_INCREMENT,
        //                    Name VARCHAR(50),
        //                    Age INT
        //                );";

        //            using (var cmd = new MySqlCommand(createTableQuery, connection))
        //            {
        //                cmd.ExecuteNonQuery();
        //                Console.WriteLine($"Tabela {tableName} je kreirana.");
        //            }

        //            string insertDataQuery = $@"
        //                INSERT INTO {tableName} (Name, Age) VALUES 
        //                ('User_{random.Next(1, 100)}', {random.Next(18, 60)}), 
        //                ('User_{random.Next(1, 100)}', {random.Next(18, 60)});";

        //            using (var cmd = new MySqlCommand(insertDataQuery, connection))
        //            {
        //                cmd.ExecuteNonQuery();
        //                Console.WriteLine("Podaci su ubačeni u tabelu.");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Greška pri radu s bazom: " + ex.Message);
        //        }
        //    }
        //}
    }
}
