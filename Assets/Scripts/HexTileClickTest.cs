using UnityEngine;

public class HexTileClickTest : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log($"🟩 Clicked on tile: {gameObject.name} at {transform.position}");

        // Optional: change color for visual feedback
        GetComponent<Renderer>().material.color = Random.ColorHSV();
    }
}
