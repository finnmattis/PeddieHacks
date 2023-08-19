using UnityEngine;

public class PlayerInput : MonoBehaviour {
    public FrameInput FrameInput { get; private set; }

    private void Update() => FrameInput = Gather();

    private FrameInput Gather() {
        return new FrameInput {
            JumpDown = Input.GetButtonDown("Jump"),
                     JumpHeld = Input.GetButton("Jump"),
                     Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                     SpecialDown = Input.GetKeyDown(KeyCode.RightShift),
                     SpecialHeld = Input.GetKey(KeyCode.RightShift),
                     HumanDown = Input.GetKeyDown(KeyCode.Alpha1),
                     MonkeyDown = Input.GetKeyDown(KeyCode.Alpha2),
                     CamelionDown = Input.GetKeyDown(KeyCode.Alpha3),
                     PenguinDown = Input.GetKey(KeyCode.Alpha4),
                     EagleDown = Input.GetKeyDown(KeyCode.Alpha5),
        };
    }
}

public struct FrameInput {
    public Vector2 Move;
    public bool JumpDown;
    public bool JumpHeld;
    public bool SpecialDown;
    public bool SpecialHeld;
    public bool HumanDown;
    public bool MonkeyDown;
    public bool CamelionDown;
    public bool PenguinDown;
    public bool EagleDown;
}
