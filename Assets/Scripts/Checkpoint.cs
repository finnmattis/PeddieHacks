using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public delegate void CheckpointAction(Vector3 transform);
    public static event CheckpointAction OnCheckpoint;

    private void OnTriggerEnter2D(Collider2D other) {
        if (IsPlayer(other)) OnCheckpoint?.Invoke(transform.position);
    }

    private bool IsPlayer(Collider2D other) {
        int layerOfCollider = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layerOfCollider);
        return layerName == "Player";
    }

}
