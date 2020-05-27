using UnityEngine;
using UnityEngine.InputSystem;

public class Inp
{
    public static InputMovement Move;
    public static InputInteract Interact;
    public static InputInterface Interface;
}


public struct InputMovement
{
    public Keyboard ActiveBoard { get { return Keyboard.current; } }
    public Mouse ActiveMouse { get { return Mouse.current; } }
    public Gamepad ActivePad { get { return Gamepad.current; } }

    public bool UseBoard { get { return (ActiveBoard != null); } }
    public bool UsePad { get { return (ActivePad != null); } }
    public bool UseMouse { get { return (ActiveMouse != null); } }

    public bool overrideInput;

    public Vector2 overrideInputDirection;
    public Vector2 overrideLookDirection;

    public Vector2 InputDir
    {
        get
        {
            Vector2 dir = Vector2.zero;
            if(overrideInput == false)
            {
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
            }else
            {
                dir = overrideInputDirection;
            }

            return dir;
        }
    }
    public Vector2 LookDir
    {
        get
        {
            Vector2 dir = Vector2.zero;
            if(overrideInput == false)
            {
                if (UseMouse)
                {
                    dir.x = Input.GetAxisRaw("Mouse X");
                    dir.y = Input.GetAxisRaw("Mouse Y");

                }

                if (UsePad)
                {
                    dir += ActivePad.rightStick.ReadValue();
                }
            }else
            {
                dir = overrideLookDirection;
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
    public bool Crouch
    {
        get
        {
            bool board = false;
            bool pad = false;
            if (UseBoard)
            {
                board = ActiveBoard.leftCtrlKey.isPressed;
            }
            if (UsePad)
            {
                pad = ActivePad.bButton.isPressed;
            }

            return (board || pad);
        }
    }
    public bool Sprint
    {
        get
        {
            bool board = false;
            bool pad = false;
            if (UseBoard)
            {
                board = ActiveBoard.leftShiftKey.isPressed;
            }
            if (UsePad)
            {
                pad = ActivePad.leftStickButton.isPressed;
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

    public bool Primary
    {
        get
        {
            bool mouse = false;
            bool pad = false;
            if (UseMouse)
            {
                mouse = ActiveMouse.leftButton.isPressed;
            }
            if (UsePad)
            {
                pad = ActivePad.rightTrigger.isPressed;
            }

            return (mouse || pad);
        }
    }
    public bool Secondary
    {
        get
        {
            bool mouse = false;
            bool pad = false;
            if (UseMouse)
            {
                mouse = ActiveMouse.rightButton.isPressed;
            }
            if (UsePad)
            {
                pad = ActivePad.leftTrigger.isPressed;
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
                mouse = ActiveMouse.scroll.ReadValue().y > 0f;
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
                mouse = ActiveMouse.scroll.y.ReadValue() < 0f;
            }
            if (UsePad)
            {
                pad = ActivePad.leftShoulder.wasPressedThisFrame;
            }

            return (mouse || pad);
        }
    }

    public bool InteractDown
    {
        get
        {
            bool board = false;
            bool pad = false;

            if (UseBoard)
            {
                board = ActiveBoard.fKey.wasPressedThisFrame;
            }
            if (UsePad)
            {
                pad = ActivePad.xButton.wasPressedThisFrame;
            }

            return board || pad;
        }
    }
    public bool DropDown
    {
        get
        {
            bool board = false;
            bool pad = false;

            if (UseBoard)
            {
                board = ActiveBoard.gKey.wasPressedThisFrame;
            }
            if (UsePad)
            {
                pad = ActivePad.yButton.wasPressedThisFrame;
            }

            return board || pad;
        }
    }

}

public struct InputInterface
{
    public Keyboard ActiveBoard { get { return Keyboard.current; } }
    public Mouse ActiveMouse { get { return Mouse.current; } }
    public Gamepad ActivePad { get { return Gamepad.current; } }

    public bool UseBoard { get { return (ActiveBoard != null); } }
    public bool UsePad { get { return (ActivePad != null); } }
    public bool UseMouse { get { return (ActiveMouse != null); } }

    public bool Pause
    {
        get
        {
            bool board = false;
            bool pad = false;

            if (UseBoard)
            {
                board = ActiveBoard.escapeKey.wasPressedThisFrame;
            }
            if (UsePad)
            {
                pad = ActivePad.startButton.wasPressedThisFrame;
            }

            return board || pad;
        }
    }
}
