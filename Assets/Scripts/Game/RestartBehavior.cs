using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartBehavior : MonoBehaviour
{
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
