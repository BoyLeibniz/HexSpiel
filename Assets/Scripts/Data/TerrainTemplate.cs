using UnityEngine;

[System.Serializable]
public class TerrainTemplate
{
    public string name;
    public Color color;
    public float movementCost;

    public TerrainTemplate(string name, Color color, float cost)
    {
        this.name = name;
        this.color = color;
        this.movementCost = cost;
    }
}
