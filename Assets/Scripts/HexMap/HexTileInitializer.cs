// Assets/Scripts/HexMap/HexTileInitializer.cs

using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class HexTileInitializer : MonoBehaviour
{
    public float height = 0.1f;

    void Awake()
    {
        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = HexMeshGenerator.GetSharedHexMesh(height);

        // Add a collider
        if (!TryGetComponent<Collider>(out _))
        {
            gameObject.AddComponent<MeshCollider>(); // or BoxCollider
        }
    }
}
