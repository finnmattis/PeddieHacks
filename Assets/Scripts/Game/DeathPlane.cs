using UnityEngine;
using System.Collections;

public class DeathPlane : MonoBehaviour
{
    [SerializeField] GameObject _gameManager;

    private void OnTriggerEnter2D(Collider2D other) {
        if (IsPlayer(other)) _gameManager.GetComponent<GameManager>().TriggerDeath();
    }

    private bool IsPlayer(Collider2D other) {
        int layerOfCollider = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layerOfCollider);
        return layerName == "Player";
    }
}


