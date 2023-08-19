using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup _startScreen;
    [SerializeField] private CanvasGroup _blockRaycastPanel;
    [SerializeField] private CanvasGroup _levelSelect;

    public void SwitchToStageSelect() {
        _startScreen.alpha = 0;
        _levelSelect.alpha = 1;
        _blockRaycastPanel.blocksRaycasts = false;
    }
}
