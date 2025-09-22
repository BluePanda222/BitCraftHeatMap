using SpacetimeDB;
using BitCraftRegion.Types;

namespace BitCraftHeatMap;

public class BitCraftHandler
{

    private readonly string _host;
    private readonly string _region;
    private readonly string _token;
    private readonly LocationHandler _locationHandler;
    
    private CancellationTokenSource _cts;
    private Thread _thread;

    public BitCraftHandler(string host, string region, string token, LocationHandler locationHandler)
    {
        _host = host;
        _region = region;
        _token = token;
        _locationHandler = locationHandler;
    }
    
    public void Connect()
    {
        var conn = ConnectToBitCraft();
        
        RegisterCallbacks(conn);
        
        _cts = new CancellationTokenSource();
        _thread = new Thread(() => ProcessThread(conn, _cts.Token));
        _thread.Start();
    }

    public void Disconnect(DbConnection conn)
    {
        conn.Disconnect();
        _cts.Cancel();
        _thread.Join();

        Console.Out.WriteLine("[BitCraftHandler] Trying to reconnect to BitCraft...");
        Connect();
    }
    
    
    private DbConnection ConnectToBitCraft()
    {
        var conn = DbConnection.Builder()
            .WithUri(_host)
            .WithModuleName(_region)
            .WithToken(_token)
            .OnConnect(OnConnected)
            .OnConnectError(OnConnectError)
            .OnDisconnect(OnDisconnected)
            .Build();
        return conn;
    }
    
    private void OnConnected(DbConnection conn, Identity identity, string authToken)
    {
        var queries = new[] {
            "SELECT * FROM player_state;",
            "SELECT location.* FROM mobile_entity_state location JOIN player_state player ON location.entity_id = player.entity_id WHERE location.dimension = '1';"
        };
        
        conn.SubscriptionBuilder()
            .Subscribe(queries);
    }
    
    private void RegisterCallbacks(DbConnection conn)
    {
        conn.Db.MobileEntityState.OnUpdate += MobileEntityState_OnUpdate;
    }
    
    private void MobileEntityState_OnUpdate(EventContext ctx, MobileEntityState oldValue, MobileEntityState newValue)
    {
        var location = new PlayerLocation(
            newValue.EntityId,
            newValue.Timestamp,
            newValue.LocationX,
            newValue.LocationZ
        );
        _locationHandler.AddLocation(location);
    }
    
    
    private void OnConnectError(Exception e)
    {
        Console.Error.Write($"[BitCraftHandler] Error while connecting: {e}");
        Environment.Exit(20);
    }
    
    private void OnDisconnected(DbConnection conn, Exception? e)
    {
        if (e != null)
        {
            Console.Out.Write($"[BitCraftHandler] Connection ended abnormally: {e}");
        }
        else
        {
            Console.Out.Write("[BitCraftHandler] Connection ended normally.");
        }
        
        Disconnect(conn);
    }
    
    private void ProcessThread(DbConnection conn, CancellationToken ct)
    {
        try
        {
            // loop until cancellation token
            while (!ct.IsCancellationRequested)
            {
                conn.FrameTick();
                Thread.Sleep(100);
            }
        }
        catch (OperationCanceledException) {}
        catch (Exception e)
        {
            Console.Error.Write($"[BitCraftHandler] Error in ProcessThread: {e}");
        }
    }
    
}
