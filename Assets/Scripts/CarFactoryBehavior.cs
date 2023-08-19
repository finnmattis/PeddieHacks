using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFactoryBehavior : MonoBehaviour
{
    [SerializeField] private int _maxCars;
    [Tooltip("1/X chance of spawning car each second")]
    [SerializeField] private int _randomFactor;
    [SerializeField] private GameObject _selfDriveCarPrefab;

    private float _spawnInterval = 1.0f; 
    private float _timeSinceLastSpawn = 0.0f;

    void FixedUpdate()
    {
        _timeSinceLastSpawn += Time.fixedDeltaTime;

        if (GameObject.FindGameObjectsWithTag("SelfDriveCar").Length < _maxCars)
        {
            if (_timeSinceLastSpawn >= _spawnInterval)
            {
                if (Random.Range(0, _randomFactor) == 0)
                {
                    SpawnCar();
                    _timeSinceLastSpawn = 0.0f; 
                }
                else
                {
                    _timeSinceLastSpawn = 0.0f;
                }
            }
        }
    }

    void SpawnCar()
    {
        Instantiate(_selfDriveCarPrefab, transform.position + transform.up * (-5), Quaternion.identity);
    }
}
