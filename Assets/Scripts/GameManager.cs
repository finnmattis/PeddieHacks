using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject Player;
    private IPlayerController _player;
    public static float Energy { get; private set; }
    public static bool OutOfEnergy { get; private set; }
    private bool isDead;
    [SerializeField] private CanvasGroup _deathScreen;

    private void Start()
    {
        Energy = 50;
    }

    private void OnEnable() {
        _player = Player.GetComponent<IPlayerController>();
        _player.DashingChanged += OnDashChange;
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
        if (OutOfEnergy && Energy > 50) OutOfEnergy = false;
    }

    private void OnDashChange(bool dashing, Vector2 dir) {
        if (dashing) Energy -= 30;
    }

    #region Death
    public delegate void RespawnAction();
    public static event RespawnAction OnRespawn;

    public void TriggerDeath() {

        StartCoroutine(FadeInDeathScreen());
    }

        private IEnumerator FadeInDeathScreen()
    {
        float startAlpha = _deathScreen.alpha;
        float elapsed = 0f;

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;
            _deathScreen.alpha = Mathf.Lerp(startAlpha, 1, elapsed / 1);
            yield return null;
        }
        _deathScreen.alpha = 1; 
        _deathScreen.blocksRaycasts = true;
    }

    public void Respawn() {
        StopCoroutine("FadeInDeathScreen");
        _deathScreen.alpha = 0;
        _deathScreen.blocksRaycasts = false;
        OnRespawn?.Invoke();
    }

    #endregion
}
