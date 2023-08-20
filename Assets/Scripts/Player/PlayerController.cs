using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour, IPlayerController {
    [SerializeField] private PlayerStats _stats;
    #region Internal

    [HideInInspector] private Rigidbody2D _rb; 
    [SerializeField] private CapsuleCollider2D _standingCollider;
    [SerializeField] private DistanceJoint2D _distanceJoint;
    [SerializeField] private LineRenderer _lineRenderer;
    private PlayerInput _input;
    private bool _cachedTriggerSetting;

    private FrameInput FrameInput;
    private Vector2 _speed;
    private Vector2 _currentExternalVelocity;
    private int _fixedFrame;

    #endregion

    #region External

    public event Action<int> StateChanged;
    public event Action<bool, float> GroundedChanged;
    public event Action<bool, Vector2> DashingChanged;
    public event Action<bool> WallGrabChanged;
    public event Action<bool> Jumped;
    public PlayerStats PlayerStats => _stats;
    public Vector2 Input => FrameInput.Move;
    public Vector2 Velocity => _rb.velocity;
    public Vector2 Speed => _speed; 
    public Vector2 GroundNormal { get; private set; }
    public int WallDirection { get; private set; }
    public bool Sprinting { get; private set; }
    public bool Climbing { get; private set; }
    public bool Grappling { get; private set; }
    public bool Sliding { get; private set; }
    public string state; 

    #endregion

    private void Start() {
        _distanceJoint.enabled = false;
    }

    private void OnEnable() {
        Grapple.OnGrappleEnter += GrappleInRange;
        Grapple.OnGrappleExit += GrappleOutRange;

        DashCircle.OnDashEnter += DashInRange;
        DashCircle.OnDashExit += DashOutRange;

        GameManager.OnDeath += OnDeath;
        GameManager.OnRespawn += Respawn;
    }

    private void OnDisable() {
        Grapple.OnGrappleEnter -= GrappleInRange;
        Grapple.OnGrappleExit -= GrappleOutRange;

        DashCircle.OnDashEnter -= DashInRange;
        DashCircle.OnDashExit -= DashOutRange;

        GameManager.OnRespawn -= Respawn;
        GameManager.OnDeath -= OnDeath;
    }

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInput>();
        _cachedTriggerSetting = Physics2D.queriesHitTriggers;
        Physics2D.queriesStartInColliders = false;
        state = "human";
    }

    private void Update() {
        GatherInput();
    }

    private void GatherInput() {
        if (_dead) return;
        FrameInput = _input.FrameInput;
        if (FrameInput.HumanDown)
        {
            state = "human";
            StateChanged?.Invoke(0);
        }
        else if (FrameInput.MonkeyDown)
        {
            state = "monkey";
            StateChanged?.Invoke(1);
        }
        else if (FrameInput.PenguinDown)
        {
            state = "penguin";
            StateChanged?.Invoke(2);
        }
        else if (FrameInput.FalconDown) {
            state = "falcon";
            StateChanged?.Invoke(3);
        }

        if (state == "human" && FrameInput.SpecialHeld && !GameManager.OutOfEnergy) Sprinting = true;
        else Sprinting = false;
        if (state == "monkey" && FrameInput.SpecialHeld && !GameManager.OutOfEnergy) _grappleInput = true;
        else _grappleInput = false;
        if (state == "penguin" && FrameInput.SpecialHeld && !GameManager.OutOfEnergy) _slideInput = true;
        else _slideInput = false;
        if (state == "falcon" && FrameInput.SpecialDown && !GameManager.OutOfEnergy) _dashToConsume = true;


        if (_stats.SnapInput)
        {
            FrameInput.Move.x = Mathf.Abs(FrameInput.Move.x) < _stats.HorizontalDeadzoneThreshold ? 0 : Mathf.Sign(FrameInput.Move.x);
            FrameInput.Move.y = Mathf.Abs(FrameInput.Move.y) < _stats.VerticalDeadzoneThreshold ? 0 : Mathf.Sign(FrameInput.Move.y);
        }

        if (FrameInput.JumpDown) {
            _jumpToConsume = true;
            _frameJumpWasPressed = _fixedFrame;
        }

        if (FrameInput.Move.x != 0) _stickyFeet = false;

    }

    private void FixedUpdate() {
        _fixedFrame++;

        CheckCollisions();
        HandleCollisions();

        HandleJump();

        HandleWalls();
        HandleGrapple();
        HandleSlide();
        HandleDash();

        HandleHorizontal();
        HandleVertical();
        ApplyMovement();
    }

    #region Death

    private bool _dead = false;

    private void OnDeath() {
        _dead = true;
    }

    private void Respawn(Vector3 checkpoint) {
        state = "human";
        StateChanged?.Invoke(0);
        _speed = new Vector2(0, 0);
        _currentExternalVelocity = new Vector2(0, 0);
        transform.position = new Vector3(checkpoint.x, checkpoint.y, 2);;
        _dead = false;
    }

    #endregion

    #region Collisions

    private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[2];
    private readonly RaycastHit2D[] _iceHits = new RaycastHit2D[2];
    private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[2];
    private readonly Collider2D[] _wallHits = new Collider2D[2];
    private RaycastHit2D _hittingWall;
    private int _groundHitCount;
    private int _iceHitCount;
    private int _ceilingHitCount;
    private int _wallHitCount;
    private int _frameLeftGrounded = int.MinValue;
    private bool _grounded;
    private Vector2 _skinWidth = new(0.02f, 0.02f); // Expose this?

    private void CheckCollisions() {
        Physics2D.queriesHitTriggers = false;

        _groundHitCount = Physics2D.CapsuleCastNonAlloc(_standingCollider.bounds.center, _standingCollider.size, _standingCollider.direction, 0, Vector2.down, _groundHits, _stats.GrounderDistance, ~_stats.PlayerLayer);
        _ceilingHitCount = Physics2D.CapsuleCastNonAlloc(_standingCollider.bounds.center, _standingCollider.size, _standingCollider.direction, 0, Vector2.up, _ceilingHits, _stats.GrounderDistance, ~_stats.PlayerLayer);
        
        // Ice
        _iceHitCount = Physics2D.CapsuleCastNonAlloc(_standingCollider.bounds.center, _standingCollider.size, _standingCollider.direction, 0, Vector2.down, _iceHits, _stats.GrounderDistance, _stats.IceLayer);

        var bounds = GetWallDetectionBounds();
        _wallHitCount = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, _wallHits, _stats.ClimbableLayer);
        _hittingWall = Physics2D.CapsuleCast(_standingCollider.bounds.center, _standingCollider.size, _standingCollider.direction, 0, new Vector2(_input.FrameInput.Move.x, 0), _stats.GrounderDistance, ~_stats.PlayerLayer);
        

        Physics2D.queriesHitTriggers = true; 
        Physics2D.queriesHitTriggers = _cachedTriggerSetting;
    }

    private bool TryGetGroundNormal(out Vector2 groundNormal) {
        Physics2D.queriesHitTriggers = false;
        var hit = Physics2D.Raycast(_rb.position, Vector2.down, _stats.GrounderDistance * 2, ~_stats.PlayerLayer);
        Physics2D.queriesHitTriggers = _cachedTriggerSetting;
        groundNormal = hit.normal; // defaults to Vector2.zero if nothing was hit
        return hit.collider;
    }

    private Bounds GetWallDetectionBounds() {
        var colliderOrigin = _rb.position + _standingCollider.offset;
        return new Bounds(colliderOrigin, _stats.WallDetectorSize);
    }

    private void HandleCollisions() {
        // Hit a Ceiling
        if (_ceilingHitCount > 0) {
            // prevent sticking to ceiling if we did an InAir jump after receiving external velocity w/ PlayerForce.Decay
            _currentExternalVelocity.y = Mathf.Min(0f, _currentExternalVelocity.y);
            _speed.y = Mathf.Min(0, _speed.y);
        }

        // Landed on the Ground
        if (!_grounded && _groundHitCount > 0) {
            _grounded = true;
            ResetDash();
            ResetJump();
            GroundedChanged?.Invoke(true, Mathf.Abs(_speed.y));
            if (FrameInput.Move.x == 0) _stickyFeet = true;
        }
        // Left the Ground
        else if (_grounded && _groundHitCount == 0) {
            _grounded = false;
            _frameLeftGrounded = _fixedFrame;
            GroundedChanged?.Invoke(false, 0);
        }
    }

    private bool IsStandingPosClear(Vector2 pos) => CheckPos(pos, _standingCollider);

    private bool CheckPos(Vector2 pos, CapsuleCollider2D col) {
        Physics2D.queriesHitTriggers = false;
        var hit = Physics2D.OverlapCapsule(pos + col.offset, col.size - _skinWidth, col.direction, 0, ~_stats.PlayerLayer);
        Physics2D.queriesHitTriggers = _cachedTriggerSetting;
        return !hit;
    }

    #endregion

    #region Walls

    private readonly ContactPoint2D[] _wallContacts = new ContactPoint2D[2];
    private float _currentWallJumpMoveMultiplier = 1f; // aka "Horizontal input influence"
    private int _lastWallDirection; // for coyote wall jumps
    private int _frameLeftWall; // for coyote wall jumps
    private bool _isLeavingWall; // prevents immediate re-sticking to wall

    private void HandleWalls() {
        if (state != "monkey") return;

        _currentWallJumpMoveMultiplier = Mathf.MoveTowards(_currentWallJumpMoveMultiplier, 1f, 1f / _stats.WallJumpInputLossFrames);

        if (_wallHitCount > 0 && _wallHits[0].GetContacts(_wallContacts) > 0) {
            WallDirection = (int)Mathf.Sign(_wallContacts[0].point.x - transform.position.x);
            _lastWallDirection = WallDirection;
        }
        else WallDirection = 0;

        if (!Climbing && ShouldStickToWall() && _speed.y <= 0) ToggleOnWall(true);
        else if (Climbing && !ShouldStickToWall()) ToggleOnWall(false);

        bool ShouldStickToWall() {
            if (WallDirection == 0 || _grounded) return false;
            return true;
        }
    }

    private void ToggleOnWall(bool on) {
        Climbing = on;
        if (on) {
            _speed = Vector2.zero;
            _currentExternalVelocity = Vector2.zero;
            _bufferedJumpUsable = true;
            _wallJumpCoyoteUsable = true;
        }
        else {
            _frameLeftWall = _fixedFrame;
            _isLeavingWall = false; 
        }

        WallGrabChanged?.Invoke(on);
    }

    #endregion

    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private bool _wallJumpCoyoteUsable;
    private int _frameJumpWasPressed;
    private int _airJumpsRemaining;

    private bool HasBufferedJump => _bufferedJumpUsable && _fixedFrame < _frameJumpWasPressed + _stats.JumpBufferFrames;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _fixedFrame < _frameLeftGrounded + _stats.CoyoteFrames;
    private bool CanWallJump => (Climbing && !_isLeavingWall) || (_wallJumpCoyoteUsable && _fixedFrame < _frameLeftWall + _stats.WallJumpCoyoteFrames);
    private bool CanAirJump => !_grounded && _airJumpsRemaining > 0;

    private void HandleJump() {
        if (!_endedJumpEarly && !_grounded && !FrameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true; // Early end detection

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (CanWallJump) WallJump();
        else if (_grounded || CanUseCoyote) NormalJump();
        else if (Grappling) GrappleJump();

        _jumpToConsume = false; // Always consume the flag
    }

    private void NormalJump() {
        _endedJumpEarly = false;
        _frameJumpWasPressed = 0; // prevents double-dipping 1 input's jumpToConsume and buffered jump for low ceilings
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _speed.y = _stats.JumpPower;
        Jumped?.Invoke(false);
    }

    private void WallJump() {
        _endedJumpEarly = false;
        _bufferedJumpUsable = false;
        if (Climbing) _isLeavingWall = true; // only toggle if it's a real WallJump, not CoyoteWallJump
        _wallJumpCoyoteUsable = false;
        _currentWallJumpMoveMultiplier = 0;
        _speed = Vector2.Scale(_stats.WallJumpPower, new(-_lastWallDirection, 1));
        Jumped?.Invoke(true);
    }

    private void GrappleJump() {
        ToggleGrapple();
        _speed.x += 15 * Math.Sign(_speed.x);
        _speed.y = _stats.JumpPower * 1.5f;
        Jumped?.Invoke(true);

    }

    private void ResetJump() {
        _coyoteUsable = true;
        _bufferedJumpUsable = true;
        _endedJumpEarly = false;
    }


    #endregion

    #region Dashing

    private bool _dashToConsume;
    private bool _canDash;
    private Vector2 _dashVel;
    private bool _dashing;
    private int _startedDashing;
    private float _nextDashTime;
    private bool _dashInRange = false;

    private void DashInRange() {
        _dashInRange = true;
    }

    private void DashOutRange() {
        _dashInRange = false;
    }

    private void HandleDash() {
        if (state != "falcon") return;
        if (_dashInRange && _dashToConsume && _canDash && Time.time > _nextDashTime) {
            var dir = new Vector2(FrameInput.Move.x, Mathf.Max(FrameInput.Move.y, 0f)).normalized;
            if (dir == Vector2.zero) {
                _dashToConsume = false;
                return;
            }

            _dashVel = dir * _stats.DashVelocity;
            _dashing = true;
            _canDash = false;
            _startedDashing = _fixedFrame;
            _nextDashTime = Time.time + _stats.DashCooldown;
            DashingChanged?.Invoke(true, dir);

            _currentExternalVelocity = Vector2.zero; // Strip external buildup
        }

        if (_dashing) {
            _speed = _dashVel;
            // Cancel when the time is out or max safety distance
            if (_fixedFrame > _startedDashing + _stats.DashDurationFrames) {
                _dashing = false;
                DashingChanged?.Invoke(false, Vector2.zero);
                _speed.y = Mathf.Min(0, _speed.y);
                _speed.x *= _stats.DashEndHorizontalMultiplier;
                if (_grounded) ResetDash();
            }
        }

        _dashToConsume = false;
    }

    private void ResetDash() {
        _canDash = true;
    }

    #endregion

    #region Grapple

    private List<Vector2> _grapples = new List<Vector2>();
    private Vector2 _curGrappleTransform;
    private bool _grappleInput;
    private float _nextGrappleTime;

    void GrappleInRange(Vector2 transform) {
        _grapples.Add(transform);
    }

    void GrappleOutRange(Vector2 transform) {
        _grapples.Remove(transform);
    }

    private void HandleGrapple() {

        if (Grappling && !_grappleInput) // Cancel Grapple
        {
            _speed = new Vector2(_speed.x / 4, _speed.y / 4); // Letting go of the grapple w/o jumping causes the player to shoot down too much
            ToggleGrapple();
            return;
        }

        if (state != "monkey" || !_grappleInput || _grapples.Count == 0 || _grounded || _nextGrappleTime > Time.time) // Not Grappling
        { 
            return;
        }

        if (!Grappling && _grappleInput) ToggleGrapple();

        if (Grappling) {
            _lineRenderer.SetPosition(0, _curGrappleTransform);
            _lineRenderer.SetPosition(1, transform.position);
        }
    }

    private void ToggleGrapple() {
        if (!Grappling)
        {
            _curGrappleTransform = _grapples.OrderBy(v => Math.Abs(v.x - transform.position.x)).First(); // Get Nearest Grapple
            _lineRenderer.enabled = true;
            _distanceJoint.connectedAnchor = _curGrappleTransform;
            _distanceJoint.enabled = true;
            Grappling = true;
        } else {
            _distanceJoint.enabled = false;
            _lineRenderer.enabled = false;
            _nextGrappleTime = Time.time + 0.5f;
            Grappling = false;
        }
    }

    #endregion

    #region Slide

    private bool _slideInput;
    private bool _lastGroundIce;

    private void HandleSlide() {
        if (_iceHitCount > 0) _lastGroundIce = true;
        if (_iceHitCount == 0 && _groundHitCount > 0) _lastGroundIce = false;

        if (state != "penguin" || !_slideInput) {
            Sliding = false;
            return;
        }
        Sliding = true;
    }

    #endregion

    #region Horizontal

    private bool HorizontalInputPressed => Mathf.Abs(FrameInput.Move.x) > _stats.HorizontalDeadzoneThreshold;
    private bool _stickyFeet;

    private void HandleHorizontal() {
        if (_dashing) return;

        // Deceleration
        if (!HorizontalInputPressed) {
            var deceleration = _grounded ? (_iceHitCount == 0 ? _stats.GroundDeceleration : _stats.GroundDeceleration / 3) * (_stickyFeet ? _stats.StickyFeetMultiplier : 1) : _stats.AirDeceleration;
            _speed.x = Mathf.MoveTowards(_speed.x, 0, deceleration * Time.fixedDeltaTime);
        }

       // Regular Horizontal Movement
        else {
            // Prevent useless horizontal speed buildup when against a wall
            if (_hittingWall.collider && Mathf.Abs(_rb.velocity.x) < 0.01f && !_isLeavingWall) _speed.x = 0;
            var xInput = FrameInput.Move.x;
            var speedMultiplier = (Sprinting ? 1.4f : 1f) * (Grappling ? 3f : 1f) * (state == "penguin" && !Sliding ? 0.6f : 1f) * (state == "penguin" && _lastGroundIce && !Sliding ? 1.5f : 1f) * (Sliding && _lastGroundIce ? 3f : 1f);
            var curAcceleration = _iceHitCount == 0 ? _stats.Acceleration : _stats.Acceleration / 6;
            _speed.x = Mathf.MoveTowards(_speed.x, xInput * _stats.MaxSpeed * speedMultiplier, _currentWallJumpMoveMultiplier * curAcceleration * Time.fixedDeltaTime);
        }

        //Make player "stick" to wall
        if (Climbing && !_isLeavingWall) _speed.x += 0.5f * WallDirection ;

    }

    #endregion

    #region Vertical

    private void HandleVertical() {
        if (_dashing) return;

        // Grounded & Slopes
        else if (_grounded && _speed.y <= 0f) {
            _speed.y = _stats.GroundingForce;

            if (TryGetGroundNormal(out var groundNormal)) {
                GroundNormal = groundNormal;
                if (!Mathf.Approximately(GroundNormal.y, 1f)) {
                    // on a slope
                    _speed.y = _speed.x * -GroundNormal.x / GroundNormal.y;
                    if (_speed.x != 0) _speed.y += _stats.GroundingForce;
                }
            }
        }
        // Wall Climbing & Sliding
        else if (Climbing && !_isLeavingWall) {
            if (FrameInput.Move.y > 0) _speed.y = _stats.WallClimbSpeed;
            else if (FrameInput.Move.y < 0) _speed.y = -_stats.MaxWallFallSpeed;
            else _speed.y = Mathf.MoveTowards(Mathf.Min(_speed.y, 0), -_stats.MaxWallFallSpeed, _stats.WallFallAcceleration * Time.fixedDeltaTime);
        }
        // In Air
        else {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _speed.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            _speed.y = Mathf.MoveTowards(_speed.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    private void ApplyMovement() {

        _rb.velocity = _speed + _currentExternalVelocity;
        _currentExternalVelocity = Vector2.MoveTowards(_currentExternalVelocity, Vector2.zero, _stats.ExternalVelocityDecay * Time.fixedDeltaTime);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (_stats == null) return;

        if (_stats.ShowWallDetection && _standingCollider != null) {
            Gizmos.color = Color.white;
            var bounds = GetWallDetectionBounds();
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    private void OnValidate() {
        if (_stats == null) Debug.LogWarning("Please assign a PlayerStats asset to the Player Controller's Stats slot", this);
        if (_standingCollider == null) Debug.LogWarning("Please assign a Capsule Collider to the Standing Collider slot", this);
        if (_rb == null && !TryGetComponent(out _rb)) Debug.LogWarning("Ensure the GameObject with the Player Controller has a Rigidbody2D", this);
    }
#endif
}

public interface IPlayerController {
    public event Action<int> StateChanged; 
    public event Action<bool, float> GroundedChanged; // On the Ground - Impact Speed
    public event Action<bool, Vector2> DashingChanged; // Dashing - Dir
    public event Action<bool> WallGrabChanged;
    public event Action<bool> Jumped; // Is wall jump

    public PlayerStats PlayerStats { get; }
    public Vector2 Input { get; }
    public Vector2 Speed { get; }
    public Vector2 Velocity { get; }
    public Vector2 GroundNormal { get; }
    public int WallDirection { get; }
    public bool Sprinting { get; }
    public bool Climbing { get; }
    public bool Grappling { get; }
    public bool Sliding { get; }
}
