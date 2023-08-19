using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFactoryBehavior : MonoBehaviour
{
    public int maxCars;
    public int randomFactor;

    public GameObject selfDriveCarPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectsWithTag("SelfDriveCar").Length < maxCars)
        {
            if (Random.Range(0, randomFactor) == 0)
            {
                SpawnCar();
            }
        }
    }

    void SpawnCar()
    {
        Instantiate(selfDriveCarPrefab, transform.position + transform.up * (-5), Quaternion.identity);
    }
}
