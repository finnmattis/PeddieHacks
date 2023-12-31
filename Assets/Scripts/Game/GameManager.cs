using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject Player;
    private IPlayerController _player;
    public static float Energy { get; private set; }
    public static bool OutOfEnergy { get; private set; }
    private bool isDead;
    [SerializeField] private GameObject _deathScreen;

    private void Start()
    {
        Energy = 50;
    }

    private void OnEnable() {
        _player = Player.GetComponent<IPlayerController>();
        _player.DashingChanged += OnDashChange;
        Checkpoint.OnCheckpoint += OnCheckpoint;
    }

    private void OnDisable() {
        _player.DashingChanged -= OnDashChange;
    }

    private void Update()
    {
        Energy += Time.deltaTime * 5;
        if (_player.Sprinting || _player.Climbing || _player.Sliding) Energy -= Time.deltaTime * 15;
        if (_player.Grappling) Energy -= Time.deltaTime * 20;

        if (Energy < 0) {
            Energy = 0;
            OutOfEnergy = true;
        }
        if (Energy > 100) Energy = 100;
        if (OutOfEnergy && Energy > 50) OutOfEnergy = false;
    }

    private void OnDashChange(bool dashing, Vector2 dir) {
        if (dashing) Energy -= 30;
    }

    #region Death
    public delegate void RespawnAction(Vector3 checkpoint);
    public static event RespawnAction OnRespawn;
    public delegate void DeathAction();
    public static event DeathAction OnDeath;

    private bool _dead;
    private float _invincibleTime;
    private Vector3 _currentCheckpoint = new Vector3(0, 0, 0);

    public void OnCheckpoint(Vector3 transform) {
        if (transform.x > _currentCheckpoint.x) _currentCheckpoint = transform;
    }

    public void TriggerDeath() {
        if (_dead == false && Time.time > _invincibleTime) {
            OnDeath?.Invoke();
            _dead = true;
            StartCoroutine(FadeInDeathScreen());
        }
    }

        private IEnumerator FadeInDeathScreen()
    {
        var _deathScreenCanvasGroup = _deathScreen.GetComponent<CanvasGroup>();
        _deathScreenCanvasGroup.alpha = 0;
        _deathScreen.SetActive(true);

        float startAlpha = _deathScreenCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;
            _deathScreenCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1, elapsed / 1);
            yield return null;
        }
        _deathScreenCanvasGroup.alpha = 1; 
    }

    public void Respawn() {
        StopCoroutine("FadeInDeathScreen");
        Time.timeScale = 1f;
        _deathScreen.SetActive(false);
        _dead = false;
        _invincibleTime = Time.time + 3;
        Energy = 50;
        OnRespawn?.Invoke(_currentCheckpoint);
    }

    #endregion
}
