using UnityEngine;

[CreateAssetMenu]
public class PlayerStats : ScriptableObject {
    [Header("LAYERS")]
    [Tooltip("Set this to the layer your player is on.")]
    public LayerMask PlayerLayer;

    [Tooltip("Set this to the layer climbable walls are on.")]
    public LayerMask ClimbableLayer;

    [Tooltip("Set this to the layer ice is on.")]
    public LayerMask IceLayer;

    [Header("INPUT")]
    [Tooltip("Makes all Input snap to an integer.")]
    public bool SnapInput = true;

    [Tooltip("Minimum input required before up or down is recognized. Avoids drifting.")]
    public float VerticalDeadzoneThreshold = 0.3f;

    [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting."), Range(0.01f, 0.99f)]
    public float HorizontalDeadzoneThreshold = 0.1f;

    [Header("MOVEMENT")]
    [Tooltip("Top horizontal movement speed.")]
    public float MaxSpeed = 14;

    [Tooltip("Horizontal Acceleration.")]
    public float Acceleration = 120;

    [Tooltip("Horizontal Deceleration.")]
    public float GroundDeceleration = 60;

    [Tooltip("Deceleration in air only after stopping input mid-air.")]
    public float AirDeceleration = 30;

    [Tooltip("A constant downward force applied while grounded. Helps on slopes."), Range(0f, -10f)]
    public float GroundingForce = -1.5f;

    [Tooltip("The improved deceleration after landing without input. Helps accuracy while platforming. To disable, set to 1."), Range(1f, 10f)]
    public float StickyFeetMultiplier = 2f;

    [Tooltip("The detection distance for grounding and roof detection."), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;

    [Tooltip("A speed multiplier while crouching."), Range(0f, 1f)]
    public float CrouchSpeedPenalty = 0.5f;

    [Tooltip("The amount of frames it takes to hit the full crouch speed penalty. Higher values provide more crouch sliding."), Min(0)]
    public int CrouchSlowdownFrames = 50;

    [Tooltip("Obstacle detection vertical offset to stand back up from a crouch. A 0 will detect the floor as an obstacle.")]
    public float CrouchBufferCheck = 0.01f;

    [Header("JUMP")] 

    [Tooltip("The immediate velocity applied when jumping.")]
    public float JumpPower = 36;

    [Tooltip("The maximum vertical movement speed.")]
    public float MaxFallSpeed = 40;

    [Tooltip("The player's capacity to gain fall speed (Gravity).")]
    public float FallAcceleration = 110;

    [Tooltip("The gravity multiplier added when jump is released early.")]
    public float JumpEndEarlyGravityModifier = 3;

    [Tooltip("The fixed frames before coyote jump becomes unusable.")]
    public int CoyoteFrames = 7;

    [Tooltip("The amount of fixed frames to buffer a jump." )]
    public int JumpBufferFrames = 7;

    [Header("WALLS")] 

    [Tooltip("How fast you climb walls.")]
    public float WallClimbSpeed = 10;

    [Tooltip("The player's capacity to gain wall sliding speed. 0 = stick to wall.")]
    public float WallFallAcceleration = 8;

    [Tooltip("Clamps the maximum fall speed.")]
    public float MaxWallFallSpeed = 15;

    [Tooltip("The immediate velocity horizontal velocity applied when wall jumping.")]
    public Vector2 WallJumpPower = new(35, 30);

    [Tooltip("The frames before full horizontal movement is returned after a wall jump."), Min(1)]
    public int WallJumpInputLossFrames = 18;

    [Tooltip("The amount of fixed frames where you can still wall jump after pressing to leave a wall.")]
    public int WallJumpCoyoteFrames = 5;

    [Tooltip("Bounds for detecting walls on either side. Ensure it's wider than your vertical capsule collider.")]
    public Vector2 WallDetectorSize = new(0.75f, 1.25f);

    [Header("DASH")] 

    [Tooltip("The velocity of the dash.")] 
    public float DashVelocity = 50;

    [Tooltip("How many fixed frames the dash will last.")]
    public int DashDurationFrames = 5;

    [Tooltip("Seconds between dash activations.")]
    public float DashCooldown = 1.5f;

    [Tooltip("The horizontal speed retained when dash has completed.")]
    public float DashEndHorizontalMultiplier = 0.25f;


    [Header("EXTERNAL")] 
        [Tooltip("The rate at which external velocity decays. Should be close to Fall Acceleration")]
        public int ExternalVelocityDecay = 100; 

#if UNITY_EDITOR
    [Header("GIZMOS")] 
        [Tooltip("Color: White")]
        public bool ShowWallDetection = true;

    [Tooltip("Color: Red")]
    public bool ShowLedgeDetection = true;

    private void OnValidate() {
        if (PlayerLayer.value <= 1) Debug.LogWarning("Please assign a Player Layer that matches the one given to your Player.", this);
        if (ClimbableLayer.value <= 1) Debug.LogWarning("Please assign a Climbable Layer that matches your Climbable colliders.", this);
        if (IceLayer.value <= 1) Debug.LogWarning("Please assign an Ice Layer that mathces your ice tiles.", this);
    }
#endif
}
