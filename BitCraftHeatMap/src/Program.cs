using Microsoft.Extensions.Configuration;

namespace BitCraftHeatMap;

public class Program
{
    
    public static IConfigurationRoot Configuration { get; private set; }
    public static Settings Settings { get; private set; }
    
    private static DatabaseHandler _databaseHandler;
    private static LocationHandler _locationHandler;
    private static BitCraftHandler _bitCraftHandler;
    
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    
        Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();
        CreateSettings();
        
        _databaseHandler = new DatabaseHandler(
            Settings.DatabaseSettings.Host,
            Settings.DatabaseSettings.Port,
            Settings.DatabaseSettings.Database,
            Settings.DatabaseSettings.Username,
            Settings.DatabaseSettings.Password
        );
        _locationHandler = new LocationHandler(_databaseHandler);
        _bitCraftHandler = new BitCraftHandler(
            Settings.BitCraftSettings.Host,
            Settings.BitCraftSettings.Region,
            Settings.BitCraftSettings.Token,
            _locationHandler
        );
        
        _databaseHandler.OpenConnection();
        _bitCraftHandler.Connect();
    }

    // private static void Test()
    // {
    //     _locationHandler.AddLocation(new PlayerLocation2(123, 100000, 300, 300));
    //     _locationHandler.AddLocation(new PlayerLocation2(123, 200000, 300, 300));
    //     _locationHandler.AddLocation(new PlayerLocation2(123, 300000, 400, 400));
    //     _locationHandler.AddLocation(new PlayerLocation2(123, 400000, 300, 300));
    //     _locationHandler.AddLocation(new PlayerLocation2(123, 500000, 200, 200));
    //     _locationHandler.AddLocation(new PlayerLocation2(123, 600000, 400, 400));
    //     var thread = new Thread(() =>
    //     {
    //         while (true)
    //         {
    //             Thread.Sleep(100);
    //         }
    //     });
    //     thread.Start();
    // }
    
    
    private static void CreateSettings()
    {
        var bitCraftSettings = new BitCraftSettings
        {
            Host = Configuration.GetValue<string>("BitCraft:Host", "wss://bitcraft-early-access.spacetimedb.com"),
            Region = Configuration.GetValue<string>("BitCraft:Region", "bitcraft-8"),
            Token = Configuration.GetValue<string>("BitCraft:Token", "INSERT_TOKEN_HERE")
        };
        var databaseSettings = new DatabaseSettings
        {
            Host = Configuration.GetValue<string>("Database:Host", "localhost"),
            Port = Configuration.GetValue<int>("Database:Port", 5432),
            Database = Configuration.GetValue<string>("Database:Database", "postgres"),
            Username = Configuration.GetValue<string>("Database:Username", "postgres"),
            Password = Configuration.GetValue<string>("Database:Password", "CHANGE_ME")
        };
        Settings = new Settings
        {
            BitCraftSettings = bitCraftSettings,
            DatabaseSettings = databaseSettings
        };
    }
    
}

public class Settings
{
    public BitCraftSettings BitCraftSettings { get; set; }
    public DatabaseSettings DatabaseSettings { get; set; }
}

public class BitCraftSettings
{
    public string Host { get; set; }
    public string Region { get; set; }
    public string Token { get; set; }
}

public class DatabaseSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Database { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
