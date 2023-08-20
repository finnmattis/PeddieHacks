using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosion;
    [SerializeField] private GameObject _flag2;

    public delegate void CheckpointAction(Vector3 transform);
    public static event CheckpointAction OnCheckpoint;

    private void OnTriggerEnter2D(Collider2D other) {
        _flag2.SetActive(true);
        _explosion.Play();

        if (IsPlayer(other)) OnCheckpoint?.Invoke(transform.position);
    }

    private bool IsPlayer(Collider2D other) {
        int layerOfCollider = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layerOfCollider);
        return layerName == "Player";
    }

}
