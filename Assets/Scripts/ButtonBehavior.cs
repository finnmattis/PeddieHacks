using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    [Tooltip("Everything in here will be toggled on/off when button is pressed. \nUsually, a door and something to indicate that the button is active.")]
    [SerializeField] private GameObject[] _objectsToToggleActive;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // entered collision
        foreach (GameObject gameObject in _objectsToToggleActive)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
