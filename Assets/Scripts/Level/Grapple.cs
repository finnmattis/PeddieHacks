using UnityEngine;

public class Grapple : MonoBehaviour
{
    public delegate void GrappleEnterAction(Vector2 transform);
    public static event GrappleEnterAction OnGrappleEnter;

    public delegate void GrappleExitAction(Vector2 transform);
    public static event GrappleExitAction OnGrappleExit;

    private void OnTriggerEnter2D(Collider2D other) {
        if (IsPlayer(other)) OnGrappleEnter?.Invoke(transform.position);
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (IsPlayer(other)) OnGrappleExit?.Invoke(transform.position);
    }

    private bool IsPlayer(Collider2D other) {
        int layerOfCollider = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layerOfCollider);
        return layerName == "Player";
    }
}
