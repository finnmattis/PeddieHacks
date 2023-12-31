using UnityEngine;
using System;

public class PlayerAnimation : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _sprite;

    [Header("Settings")]
    [SerializeField] private float _maxTilt = 5; // In degrees around the Z axis
    [SerializeField] private float _tiltSpeed = 20;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;
    [SerializeField] private ParticleSystem _dashParticles;
    [SerializeField] private ParticleSystem _dashRingParticles;
    [SerializeField] private Transform _dashRingTransform;
    [SerializeField] private TrailRenderer _grapplingTrailRenderer;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip _dashClip;
    [SerializeField] private AudioClip _grappleClip;
    [SerializeField] private AudioClip[] _footsteps;
    [SerializeField] private AudioClip[] _slideClips;

    private AudioSource _source;
    private IPlayerController _player;
    private Vector2 _defaultSpriteSize;

    private void Awake() {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<IPlayerController>();
        _defaultSpriteSize = _sprite.size;
    }

    private void OnEnable() {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.DashingChanged += OnDashChanged;
        _player.StateChanged += OnStateChange;
        _player.WallGrabChanged += OnWallGrab;

        _moveParticles.Play();
    }

    private void OnDisable() {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.DashingChanged -= OnDashChanged;
        _player.StateChanged -= OnStateChange;
        _player.WallGrabChanged -= OnWallGrab;

        _moveParticles.Stop();
    }

    private void OnStateChange(int state) {
        _anim.SetInteger("State", state);
    }

    private void OnWallGrab(bool grabbing) {
        if (grabbing) _anim.SetTrigger("Grabbing");
        if (!grabbing) _anim.ResetTrigger("Grabbing");
    }

    private void Update() {
        if (_player == null) return;

        var xInput = _player.Input.x;

        HandleSpeed();
        
        DetectGroundColor();

        HandleSpriteFlip(xInput);

        HandleCharacterTilt(xInput);

        HandleGrappling();
    }

    private void HandleSpeed() {
        _anim.SetFloat("Speed", Math.Abs(_player.Speed.x));
    }

    // Face the direction of your last input
    private void HandleSpriteFlip(float xInput) {
        if (_player.Input.x != 0) _sprite.flipX = xInput < 0; // _player.Input.x > 0 ? 1 : -1, 1, 1);
    }

    private void HandleCharacterTilt(float xInput) {
        var runningTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * xInput) : Quaternion.identity;
        var targetRot = _grounded && _player.GroundNormal != Vector2.up ? runningTilt * _player.GroundNormal : runningTilt * Vector2.up;

        _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, targetRot, _tiltSpeed * Time.deltaTime, 0f);
    }

    private bool _grappling;
    private void HandleGrappling() {
        if (!_grappling && _player.Grappling) _source.PlayOneShot(_grappleClip);
        if (_player.Grappling)
        {
            _grappling = true;
            _grapplingTrailRenderer.enabled = true;
        }
        else 
        {
            _grappling = false;
            _grapplingTrailRenderer.enabled = false;
        }
    }

    #region Event Callbacks

    private void OnJumped(bool wallJumped) {
        _anim.SetTrigger("Jumping");

        // Only play particles when grounded (avoid coyote)
        if (_grounded) {
            SetColor(_jumpParticles);
            SetColor(_launchParticles);
            _jumpParticles.Play();
        }
    }

    private bool _grounded;
    private void OnGroundedChanged(bool grounded, float impact) {
        _grounded = grounded;
        if (grounded) {
            _anim.ResetTrigger("Jumping");
            _source.PlayOneShot(_footsteps[UnityEngine.Random.Range(0, _footsteps.Length)]);
            _moveParticles.Play();

            _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            SetColor(_landParticles);
            _landParticles.Play();
        }
        else {
            _moveParticles.Stop();
        }
    }

    private void OnDashChanged(bool dashing, Vector2 dir) {
        if (dashing) {
            _dashParticles.Play();
            _dashRingTransform.up = dir;
            _dashRingParticles.Play();
            _source.PlayOneShot(_dashClip);
        }
        else {
            _dashParticles.Stop();
        }
    }

    #endregion

    #region Helper Methods
    
    private ParticleSystem.MinMaxGradient _currentGradient;

    private void DetectGroundColor() {
        // Detect ground color. Little bit of garbage allocation, but faster computationally. Change to NonAlloc if you'd prefer
        var groundHits = Physics2D.RaycastAll(transform.position, Vector3.down, 2);
        foreach (var hit in groundHits) {
            if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) continue;
            _currentGradient = new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f);
            SetColor(_moveParticles);
            return;
        }
    }

    private void SetColor(ParticleSystem ps) {
        var main = ps.main;
        main.startColor = _currentGradient;
    }

    #endregion
}
