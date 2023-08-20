using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup _pauseMenuCanvasGroup;
    [SerializeField] private GameObject _levelFinisher;
    private LevelFinisher _levelFinisherScript;

    private bool isPaused = false;

    private void Start()
    {
        _levelFinisherScript = _levelFinisher.GetComponent<LevelFinisher>();

        if (!_pauseMenuCanvasGroup)
        {
            Debug.LogError("CanvasGroup is not assigned!");
            return;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_levelFinisherScript.LevelEnd)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        _pauseMenuCanvasGroup.alpha = 0; 
        _pauseMenuCanvasGroup.blocksRaycasts = false; 

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Pause()
    {
        _pauseMenuCanvasGroup.alpha = 1; 
        _pauseMenuCanvasGroup.blocksRaycasts = true; 

        Time.timeScale = 0f;
        isPaused = true;
    }
}
