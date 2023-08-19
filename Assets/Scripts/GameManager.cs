using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject Player;
    private IPlayerController _player;
    public static float Energy { get; private set; }
    public static bool OutOfEnergy { get; private set; }

    private void Start()
    {
        Energy = 50;
    }

    private void OnEnable() {
        _player = Player.GetComponent<IPlayerController>();
        _player.DashingChanged += OnDashChange;
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
}
