using MySql.Data.MySqlClient;
using System.Windows;

public class DatabaseManager
{
    private static string connectionString = "Server=localhost;Database=msu_access;Uid=root;Pwd=password;";

    public static string ConnectionString
    {
        get { return connectionString; }
        set { connectionString = value; }
    }

    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(ConnectionString);
    }

    public static void InitializeDatabase()
    {
        try
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                // Check if database exists, if not create it
                var cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS msu_access", connection);
                cmd.ExecuteNonQuery();

                // Use the database
                cmd = new MySqlCommand("USE msu_access", connection);
                cmd.ExecuteNonQuery();

                // Create tables if they don't exist
                CreateTables(connection);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void CreateTables(MySqlConnection connection)
    {
        // Create Administrators table
        var query = @"CREATE TABLE IF NOT EXISTS Administrators (
                    ID INT AUTO_INCREMENT PRIMARY KEY,
                    RFIDUID VARCHAR(50),
                    Username VARCHAR(50) NOT NULL UNIQUE,
                    Password VARCHAR(100) NOT NULL,
                    Role VARCHAR(50) NOT NULL
                )";
        var cmd = new MySqlCommand(query, connection);
        cmd.ExecuteNonQuery();

        // Create Users table
        query = @"CREATE TABLE IF NOT EXISTS Users (
                ID INT AUTO_INCREMENT PRIMARY KEY,
                RFIDUID VARCHAR(50) NOT NULL,
                IDNumber VARCHAR(20) NOT NULL UNIQUE,
                FirstName VARCHAR(50) NOT NULL,
                LastName VARCHAR(50) NOT NULL,
                Department VARCHAR(50),
                CourseYrPosition VARCHAR(50),
                UserType VARCHAR(20) NOT NULL,
                Photo LONGBLOB
            )";
        cmd = new MySqlCommand(query, connection);
        cmd.ExecuteNonQuery();

        // Create AccessLogs table
        query = @"CREATE TABLE IF NOT EXISTS AccessLogs (
                ID INT AUTO_INCREMENT PRIMARY KEY,
                TimeStamp DATETIME NOT NULL,
                IDNumber VARCHAR(20) NOT NULL,
                Name VARCHAR(100) NOT NULL,
                Department VARCHAR(50),
                UserType VARCHAR(20) NOT NULL,
                EventType VARCHAR(10) NOT NULL
            )";
        cmd = new MySqlCommand(query, connection);
        cmd.ExecuteNonQuery();

        // Check if there are any administrators, if not add a default one
        query = "SELECT COUNT(*) FROM Administrators";
        cmd = new MySqlCommand(query, connection);
        int count = Convert.ToInt32(cmd.ExecuteScalar());

        if (count == 0)
        {
            // Add default admin
            query = @"INSERT INTO Administrators (RFIDUID, Username, Password, Role) 
                   VALUES ('1A2B1C1D', 'Admin', 'password', 'Super Admin')";
            cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }
    }
}