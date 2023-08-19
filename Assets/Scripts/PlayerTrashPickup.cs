using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Put this on the player.
/// </summary>
public class PlayerTrashPickup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI trashDisplay;
    private int trashCount = 0;
    void Start()
    {
        trashDisplay.text = "Trash Collected: " + trashCount;
    }


    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.transform.root.CompareTag("Trash"))
        {
            // pick up trash
            trashCount++;
            Destroy(other.gameObject.transform.root.gameObject);
            trashDisplay.text = "Trash Collected: " + trashCount;
        }
    }
}
