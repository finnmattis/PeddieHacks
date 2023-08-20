using UnityEngine;
using System.Collections;

public class DeathPlane : MonoBehaviour
{
    public delegate void DeathAction();
    public static event DeathAction OnDeath;

    private void OnTriggerEnter2D(Collider2D other) {
        if (IsPlayer(other)) OnDeath?.Invoke();
    }

    private bool IsPlayer(Collider2D other) {
        int layerOfCollider = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layerOfCollider);
        return layerName == "Player";
    }
}


