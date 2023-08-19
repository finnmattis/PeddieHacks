using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    [SerializeField] private Slider _energySlider;
    [SerializeField] private Image _fill;

    private void FixedUpdate()
    {
        _energySlider.value = GameManager.Energy;
        if (!GameManager.OutOfEnergy && GameManager.Energy >= 30) _fill.color = Color.green;
        else if (!GameManager.OutOfEnergy && GameManager.Energy < 30) _fill.color = Color.yellow;
        else _fill.color = Color.red;
    }
}
