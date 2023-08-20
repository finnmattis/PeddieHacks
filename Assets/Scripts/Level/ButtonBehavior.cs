using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _whenButtonNotPressed;
    [SerializeField] private GameObject _whenButtonPressed;
    [SerializeField] private GameObject _door;
    private bool _pressed = false;
    private float _nextPressTime;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_nextPressTime > Time.time) return;
        if (!_pressed) {
            _whenButtonPressed.SetActive(true);
            _door.SetActive(false);
            _pressed = true;
        } else {
            _whenButtonPressed.SetActive(false);
            _door.SetActive(true);
            _pressed = false;
        }
        _nextPressTime = Time.time + 0.5f;
    }
}
