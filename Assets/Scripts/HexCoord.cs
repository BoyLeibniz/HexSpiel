[System.Serializable]
public struct HexCoord
{
    public int q, r;

    public HexCoord(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public static readonly HexCoord[] directions = {
        new HexCoord(+1, 0), new HexCoord(+1, -1), new HexCoord(0, -1),
        new HexCoord(-1, 0), new HexCoord(-1, +1), new HexCoord(0, +1)
    };

    public HexCoord GetNeighbor(int direction)
    {
        var dir = directions[direction % 6];
        return new HexCoord(q + dir.q, r + dir.r);
    }

    public override string ToString() => $"({q}, {r})";
}
