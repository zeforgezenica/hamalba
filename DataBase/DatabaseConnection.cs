
using MySql.Data.MySqlClient;

namespace hamalba.DataBase
{
    public class DatabaseConnection
    {
        private string connectionString = "server=localhost;database=hamalba;user=root;password=root;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public void TestConnection()
        {
            using (MySqlConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Uspješno povezano s bazom podataka!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška pri povezivanju: " + ex.Message);
                }
            }
        }
    }
}
