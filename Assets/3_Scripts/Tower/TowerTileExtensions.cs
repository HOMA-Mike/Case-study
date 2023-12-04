/// <summary>Extension methods for TowerTile</summary>
public static class TowerTileExtensions
{
    /// <summary>Returns the tile to it's pool in TilePool</summary>
    public static void ReturnToPool(this TowerTile tile) => TilePool.ReturnTile(tile);
}