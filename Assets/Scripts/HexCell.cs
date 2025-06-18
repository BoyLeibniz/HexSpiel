using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoord coord;
    public Vector3 worldPosition;
    public string terrainType = "Plain";  // Default type
    public float movementCost = 1f;       // Default movement cost

    public void Init(HexCoord coord, Vector3 position)
    {
        this.coord = coord;
        this.worldPosition = position;
        transform.position = position;
        name = $"Hex {coord.q},{coord.r}";
    }

    public void SetProperties(string type, float cost)
    {
        terrainType = type;
        movementCost = cost;
    }

}
