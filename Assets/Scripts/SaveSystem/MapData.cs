using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    public int width;
    public int height;
    public List<HexTileData> tiles = new();
}

[System.Serializable]
public class HexTileData
{
    public int q, r;
    public string type;
    public float cost;
}
