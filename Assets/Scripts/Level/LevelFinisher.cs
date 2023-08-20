using UnityEngine;
using TMPro;

public class LevelFinisher : MonoBehaviour
{
    [SerializeField] private CanvasGroup _energyBar;
    [SerializeField] private CanvasGroup _trashCounter;
    [SerializeField] private GameObject _endScreen;
    [SerializeField] private GameObject _player;
    public bool LevelEnd = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        LevelEnd = true;

        // Time.timeScale = 0;
        var deathText = _endScreen.transform.Find("Level End Message");
        var tmpText = deathText.GetComponent<TMP_Text>();

        var trashScript = _player.GetComponent<PlayerTrashPickup>();


        if (trashScript.TrashCount == 0) tmpText.text = "Level finished with no trash collected!";
        else if (trashScript.TrashCount == 1) tmpText.text = "Level finished with " + trashScript.TrashCount + " piece of trash collected!";
        else tmpText.text = "Level finished with " + trashScript.TrashCount + " pieces of trash collected!";
        _energyBar.alpha = 0;
        _trashCounter.alpha = 0;
        _endScreen.SetActive(true);

        _endScreen.transform.SetSiblingIndex(_endScreen.transform.parent.childCount - 1);
    }

    private bool IsPlayer(Collider2D other)
    {
        int layerOfCollider = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layerOfCollider);
        return layerName == "Player";
    }
}
