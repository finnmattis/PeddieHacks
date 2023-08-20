using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerTrashPickup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI trashDisplay;
    public int TrashCount = 0;

    private void OnEnable() {
        GameManager.OnRespawn += ResetTrash;
    }

    private void OnDisable() {
        GameManager.OnRespawn -= ResetTrash;
    }

    private void ResetTrash(Vector3 _) {
        TrashCount = 0;
    }

    void Start()
    {
        trashDisplay.text = "Trash Collected: " + TrashCount;
    }


    private void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.transform.root.CompareTag("Trash"))
        {
            // pick up trash
            TrashCount++;
            Destroy(other.gameObject.transform.root.gameObject);
            trashDisplay.text = "Trash Collected: " + TrashCount;
        }
    }
}
