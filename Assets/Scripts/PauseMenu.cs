using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public CanvasGroup pauseMenuCanvasGroup;

    private bool isPaused = false;

    private void Start()
    {
        if (!pauseMenuCanvasGroup)
        {
            Debug.LogError("CanvasGroup is not assigned!");
            return;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuCanvasGroup.alpha = 0; 
        pauseMenuCanvasGroup.blocksRaycasts = false; 

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuCanvasGroup.alpha = 1; 
        pauseMenuCanvasGroup.blocksRaycasts = true; 

        Time.timeScale = 0f;
        isPaused = true;
    }
}
