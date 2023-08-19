using UnityEngine;

public class DashCircle : MonoBehaviour
{
    public delegate void OnDashEnterAction();
    public static event OnDashEnterAction OnDashEnter;

    public delegate void OnDashExitAction();
    public static event OnDashExitAction OnDashExit;

    private void OnTriggerEnter2D(Collider2D other) {
        if (IsPlayer(other)) OnDashEnter?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (IsPlayer(other)) OnDashExit?.Invoke();
    }

    private bool IsPlayer(Collider2D other) {
        int layerOfCollider = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layerOfCollider);
        return layerName == "Player";
    }
}
