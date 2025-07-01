using UnityEngine;
using UnityEngine.SceneManagement;

public class AppController : MonoBehaviour
{
    public void LaunchMapBuilder()
    {
        SceneManager.LoadScene("HexMap");
    }

    public void LaunchArmyBuilder()
    {
        Debug.Log("Army Builder not available yet.");
    }

    public void LaunchGamePlayer()
    {
        Debug.Log("Game Player not available yet.");
    }
}
