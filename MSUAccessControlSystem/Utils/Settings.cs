using MSUAccessControlSystem.Data;

public class Settings
{
    public static string ComPort { get; set; } = "COM7";
    public static string DatabaseHost { get; set; } = "localhost";
    public static string DatabaseUser { get; set; } = "root";
    public static string DatabasePassword { get; set; } = "gigatt0702";
    public static string DatabaseName { get; set; } = "msu_access";

    public static void Save()
    {
        Properties.Settings.Default.ComPort = ComPort;
        Properties.Settings.Default.DatabaseHost = DatabaseHost;
        Properties.Settings.Default.DatabaseUser = DatabaseUser;
        Properties.Settings.Default.DatabasePassword = DatabasePassword;
        Properties.Settings.Default.DatabaseName = DatabaseName;
        Properties.Settings.Default.Save();
    }

    public static void Load()
    {
        ComPort = Properties.Settings.Default.ComPort;
        DatabaseHost = Properties.Settings.Default.DatabaseHost;
        DatabaseUser = Properties.Settings.Default.DatabaseUser;
        DatabasePassword = Properties.Settings.Default.DatabasePassword;
        DatabaseName = Properties.Settings.Default.DatabaseName;

        // Update DatabaseManager connection string
        DatabaseManager.ConnectionString = $"Server={DatabaseHost};Database={DatabaseName};Uid={DatabaseUser};Pwd={DatabasePassword};";
    }
}
} 