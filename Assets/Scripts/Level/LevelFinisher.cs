using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFinisher : MonoBehaviour
{
    [SerializeField] private GameObject _endScreen;

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Time.timeScale = 0;
        _endScreen.SetActive(true);
    }
}
