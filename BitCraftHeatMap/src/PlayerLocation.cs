namespace BitCraftHeatMap;

public class PlayerLocation
{

    public ulong EntityId { get; }
    public ulong Timestamp { get; }
    public int LocationX { get; }
    public int LocationZ { get; }

    public PlayerLocation(ulong entityId, ulong timestamp, int locationX, int locationZ)
    {
        EntityId = entityId;
        Timestamp = timestamp;
        LocationX = locationX / 3000;
        LocationZ = locationZ / 3000;
    }
    
    public override string ToString()
    {
        return $"[EntityId: {EntityId}, Timestamp: {Timestamp}, LocationX: {LocationX}, LocationZ: {LocationZ}]";
    }

    public bool EqualsLocation(PlayerLocation other)
    {
        return LocationX == other.LocationX && LocationZ == other.LocationZ;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is PlayerLocation other)
        {
            return EntityId == other.EntityId && Timestamp == other.Timestamp  && LocationX == other.LocationX && LocationZ == other.LocationZ;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return EntityId.GetHashCode() ^ Timestamp.GetHashCode() ^ LocationX.GetHashCode() ^ LocationZ.GetHashCode();
    }
}
