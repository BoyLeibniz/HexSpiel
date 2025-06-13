using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoord coord;
    public Vector3 worldPosition;

    public void Init(HexCoord coord, Vector3 position)
    {
        this.coord = coord;
        this.worldPosition = position;
        transform.position = position;
        name = $"Hex {coord.q},{coord.r}";
    }
}
