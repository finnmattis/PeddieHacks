using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public CanvasGroup pauseMenuCanvasGroup; // Assign the CanvasGroup component here.

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
        pauseMenuCanvasGroup.alpha = 0;              // Hide the UI
        pauseMenuCanvasGroup.blocksRaycasts = false; // Make it non-interactable

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuCanvasGroup.alpha = 1;              // Show the UI
        pauseMenuCanvasGroup.blocksRaycasts = true;  // Make it interactable

        Time.timeScale = 0f;
        isPaused = true;
    }
}
