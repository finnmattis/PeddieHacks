using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public void OpenScene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
