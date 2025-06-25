// Assets/Scripts/SaveSystem/HexTileConverter.cs

public static class HexTileConverter
{
    public static HexTileData ToData(HexCell cell)
    {
        return new HexTileData
        {
            q = cell.coord.q,
            r = cell.coord.r,
            type = cell.terrainType,
            cost = cell.movementCost,
            label = cell.label ?? ""
        };
    }

    public static void ApplyDataToCell(HexTileData data, HexCell cell)
    {
        cell.coord = new HexCoord(data.q, data.r);
        cell.SetProperties(data.type, data.cost);
        cell.label = data.label ?? "";
    }
}
