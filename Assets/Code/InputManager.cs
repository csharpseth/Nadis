using Nadis.Net;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputMovement Move;
    public static InputInteract Interact;

    private MovementController move;
    private PlayerSync player;

    private bool crouch = false;

    private void Awake()
    {
        move = GetComponent<MovementController>();
        player = GetComponent<PlayerSync>();
    }

    private void Update()
    {
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            if(Move.JumpDown)
            {
                Events.BipedAnimator.ExecuteAnimation(player.ID, "jump", move.Jump);
            }

            if(Move.CrouchDown)
            {
                crouch = !crouch;
                if(crouch)
                {
                    Events.BipedAnimator.ExecuteAnimation(player.ID, "crouch", null);
                }else
                {
                    Events.BipedAnimator.EndAnimation(player.ID, "crouch");
                }
            }

            if(Move.SprintDown)
            {
                move.Run = !move.Run;
            }
        }

    }
}


public struct InputMovement
{
    public Keyboard ActiveBoard { get { return Keyboard.current; } }
    public Mouse ActiveMouse { get { return Mouse.current; } }
    public Gamepad ActivePad { get { return Gamepad.current; } }

    public bool UseBoard { get { return (ActiveBoard != null); } }
    public bool UsePad { get { return (ActivePad != null); } }
    public bool UseMouse { get { return (ActiveMouse != null); } }

    public Vector2 InputDir
    {
        get
        {
            Vector2 dir = Vector2.zero;
            if (UseBoard)
            {
                if (ActiveBoard.wKey.isPressed) dir.y = 1f;
                else if (ActiveBoard.sKey.isPressed) dir.y = -1f;
                else dir.y = 0f;

                if (ActiveBoard.dKey.isPressed) dir.x = 1f;
                else if (ActiveBoard.aKey.isPressed) dir.x = -1f;
                else dir.x = 0f;
            }

            if (UsePad)
            {
                dir += ActivePad.leftStick.ReadValue();
            }

            return dir;
        }
    }
    public Vector2 LookDir
    {
        get
        {
            Vector2 dir = Vector2.zero;
            if (UseMouse)
            {
                dir = ActiveMouse.radius.ReadValue();

            }

            if(UsePad)
            {
                dir += ActivePad.rightStick.ReadValue();
            }

            return dir;
        }
    }

    public bool JumpDown
    {
        get
        {
            bool board = false;
            bool pad = false;
            if(UseBoard)
            {
                board = ActiveBoard.spaceKey.wasPressedThisFrame;
            }
            if(UsePad)
            {
                pad = ActivePad.aButton.wasPressedThisFrame;
            }

            return (board || pad);
        }
    }
    public bool CrouchDown
    {
        get
        {
            bool board = false;
            bool pad = false;
            if (UseBoard)
            {
                board = ActiveBoard.leftCtrlKey.wasPressedThisFrame;
            }
            if (UsePad)
            {
                pad = ActivePad.bButton.wasPressedThisFrame;
            }

            return (board || pad);
        }
    }
    public bool SprintDown
    {
        get
        {
            bool board = false;
            bool pad = false;
            if (UseBoard)
            {
                board = ActiveBoard.leftShiftKey.wasPressedThisFrame;
            }
            if (UsePad)
            {
                pad = ActivePad.leftStickButton.wasPressedThisFrame;
            }

            return (board || pad);
        }
    }
}

public struct InputInteract
{
    public Keyboard ActiveBoard { get { return Keyboard.current; } }
    public Mouse ActiveMouse { get { return Mouse.current; } }
    public Gamepad ActivePad { get { return Gamepad.current; } }

    public bool UseBoard { get { return (ActiveBoard != null); } }
    public bool UsePad { get { return (ActivePad != null); } }
    public bool UseMouse { get { return (ActiveMouse != null); } }
    
    public bool PrimaryDown
    {
        get
        {
            bool mouse = false;
            bool pad = false;
            if (UseMouse)
            {
                mouse = ActiveMouse.leftButton.wasPressedThisFrame;
            }
            if (UsePad)
            {
                pad = ActivePad.rightTrigger.wasPressedThisFrame;
            }

            return (mouse || pad);
        }
    }
    public bool SecondaryDown
    {
        get
        {
            bool mouse = false;
            bool pad = false;
            if (UseMouse)
            {
                mouse = ActiveMouse.rightButton.wasPressedThisFrame;
            }
            if (UsePad)
            {
                pad = ActivePad.leftTrigger.wasPressedThisFrame;
            }

            return (mouse || pad);
        }
    }
    public bool Next
    {
        get
        {
            bool mouse = false;
            bool pad = false;
            if (UseMouse)
            {
                mouse = ActiveMouse.middleButton.ReadValue() > 0f;
            }
            if (UsePad)
            {
                pad = ActivePad.rightShoulder.wasPressedThisFrame;
            }

            return (mouse || pad);
        }
    }
    public bool Previous
    {
        get
        {
            bool mouse = false;
            bool pad = false;
            if (UseMouse)
            {
                mouse = ActiveMouse.middleButton.ReadValue() < 0f;
            }
            if (UsePad)
            {
                pad = ActivePad.leftShoulder.wasPressedThisFrame;
            }

            return (mouse || pad);
        }
    }

}
