using UnityEngine;

[System.Serializable]
public class TerrainTemplate
{
    public string name;
    public Color color;
    public int movementCost;

    public TerrainTemplate(string name, Color color, int cost)
    {
        this.name = name;
        this.color = color;
        this.movementCost = cost;
    }
}
