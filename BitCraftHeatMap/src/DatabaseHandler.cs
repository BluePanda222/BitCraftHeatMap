using System.Data;
using Npgsql;

namespace BitCraftHeatMap;

public class DatabaseHandler
{

    public readonly NpgsqlConnection Conn;

    public DatabaseHandler(string host, int port, string database, string username, string password)
    {
        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password
        };
        
        Conn = new NpgsqlConnection(csb.ConnectionString);

        Conn.StateChange += OnStateChange;
    }

    public void OpenConnection()
    {
        try
        {
            Conn.Open();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[DatabaseHandler] Error in Database connection: {e.Message}");
            Console.Error.WriteLine(e.StackTrace);
            Environment.Exit(10);
        }
    }

    private void OnStateChange(object sender, StateChangeEventArgs e)
    {
        switch (e.CurrentState)
        {
            case ConnectionState.Open:
                Console.Out.WriteLine("[DatabaseHandler] Connection opened!");
                break;
            case ConnectionState.Closed:
                Console.Out.WriteLine("[DatabaseHandler] Connection closed!");
                Console.Out.WriteLine("[DatabaseHandler] Trying to reconnect...");
                OpenConnection();
                break;
            case ConnectionState.Broken:
                Console.Error.WriteLine("[DatabaseHandler] Connection broken!");
                Conn.Close();
                break;
        }
    }
    
    public async Task InsertPlayerLocationsAsync(List<PlayerLocation> locations)
    {
        if (Conn.State != ConnectionState.Open)
        {
            return;
        }
        
        var locationsSet = locations.ToHashSet();
        if (locationsSet.Count < 1)
        {
            return;
        }

        var sqlBuilder = new System.Text.StringBuilder();
        sqlBuilder.Append("INSERT INTO player_locations_v2 (entity_id, timestamp, location_x, location_z) VALUES ");

        var parameters = new List<NpgsqlParameter>();
        var i = 0;

        foreach (var location in locationsSet)
        {
            if (i > 0)
            {
                sqlBuilder.Append(", ");
            }

            // Append a new VALUES clause for each location
            sqlBuilder.Append(
                $"(@entityId{i}, TO_TIMESTAMP(@timestamp{i} / 1000.0), @locationX{i}, @locationZ{i})"
            );

            // Add parameters for the current location with unique names
            parameters.Add(new NpgsqlParameter($"entityId{i}", (long) location.EntityId));
            parameters.Add(new NpgsqlParameter($"timestamp{i}", (long) location.Timestamp));
            parameters.Add(new NpgsqlParameter($"locationX{i}", location.LocationX));
            parameters.Add(new NpgsqlParameter($"locationZ{i}", location.LocationZ));

            i++;
        }

        // Events are sometimes received double.
        // Does insert all applicable entries and skips conflicts.
        sqlBuilder.Append("ON CONFLICT DO NOTHING");

        await using var cmd = new NpgsqlCommand(sqlBuilder.ToString(), Conn);
        cmd.Parameters.AddRange(parameters.ToArray()); // Add all collected parameters

        await cmd.ExecuteNonQueryAsync();
    }
    
}
