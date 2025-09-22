using System.Collections.Concurrent;
using System.Data;

namespace BitCraftHeatMap;

public class LocationHandler
{
    private readonly BlockingCollection<PlayerLocation> _buffer;
    private readonly Dictionary<ulong, PlayerLocation> _cache;
    
    private readonly DatabaseHandler _databaseHandler;
    private readonly int _bufferCapacity;
    private readonly int _batchSize;
    
    private readonly CancellationTokenSource _cts;
    private readonly Task _consumerTask;

    public LocationHandler(
        DatabaseHandler databaseHandler,
        int bufferCapacity = 10000,
        int batchSize = 200
    )
    {
        _databaseHandler = databaseHandler;
        _batchSize = batchSize;
        _bufferCapacity = bufferCapacity;
        
        _buffer = new BlockingCollection<PlayerLocation>(_bufferCapacity);
        _cache =  new Dictionary<ulong, PlayerLocation>();
        _cts = new CancellationTokenSource();

        _consumerTask = Task.Run(async () => await ConsumeLocationsAsync(_cts.Token));
    }

    public void AddLocation(PlayerLocation location)
    {
        if (_cache.ContainsKey(location.EntityId))
        {
            var cachedLocation = _cache.GetValueOrDefault(location.EntityId);
            if (cachedLocation != null && !cachedLocation.EqualsLocation(location))
            {
                if (!_buffer.TryAdd(location))
                {
                    Console.Error.WriteLine($"[LocationHandler] Buffer is full ({_buffer.Count} of {_bufferCapacity}). Location could not be added to the buffer.");
                }
                _cache[location.EntityId] = location;
            }
        }
        else
        {
            if (!_buffer.TryAdd(location))
            {
                Console.Error.WriteLine($"[LocationHandler] Buffer is full ({_buffer.Count} of {_bufferCapacity}). Location could not be added to the buffer.");
            }
            _cache[location.EntityId] = location;
        }
    }

    private async Task ConsumeLocationsAsync(CancellationToken cancellationToken)
    {
        Console.Out.WriteLine("[LocationHandler] Init");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var batch = new List<PlayerLocation>();
                for (var i = 0; i < _batchSize; i++)
                {
                    if (_buffer.TryTake(out var location, 10, cancellationToken))
                    {
                        batch.Add(location);
                    }
                }

                if (batch.Count > 0)
                {
                    if (_databaseHandler.Conn.State != ConnectionState.Open)
                    {
                        Console.Error.WriteLine("[LocationHandler] Waiting for Database Connection to be reestablished to continue insert...");
                        Console.Error.WriteLine($"[LocationHandler] Buffer can handle {_bufferCapacity} locations until discarding new elements");
                        while (_databaseHandler.Conn.State != ConnectionState.Open)
                        {
                            // loop until connection is established again
                            if (_buffer.Count % 1000 == 0)
                            {
                                Console.Error.WriteLine($"[LocationHandler] Buffer capacity: ({_buffer.Count} of {_bufferCapacity})");
                            }
                        }
                        Console.Out.WriteLine("[LocationHandler] Database Connection reestablished! Inserts continue.");
                    }
                    // Console.Out.WriteLine($"[LocationHandler] Inserting batch of {batch.Count} locations into database");
                    await _databaseHandler.InsertPlayerLocationsAsync(batch);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[LocationHandler] Consumer task was cancelled.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[LocationHandler] Consumer task encountered an error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(30);
        }
    }
    
    public async Task StopAsync()
    {
        Console.WriteLine("[LocationHandler] Stopping LocationHandler...");
        _buffer.CompleteAdding();
        await _cts.CancelAsync();
        await _consumerTask;
    }
}
