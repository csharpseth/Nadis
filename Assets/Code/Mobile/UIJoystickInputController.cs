using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIJoystickInputController : MonoBehaviour
{
    [SerializeField]
    private FixedJoystick inputDirJoystick;
    [SerializeField]
    private FixedJoystick lookDirJoystick;
    [SerializeField]
    private Text ipTextField;
    [SerializeField]
    private NetTester netTester;

    public string currentIP;

    private void Awake()
    {
        Inp.Move.overrideInput = true;
    }

    private void Update()
    {
        if(inputDirJoystick != null)
        {
            InputMovement move = Inp.Move;
            move.overrideInputDirection = inputDirJoystick.Direction;
            Inp.Move = move;
        }

        if(lookDirJoystick != null)
        {
            InputMovement move = Inp.Move;
            move.overrideLookDirection = lookDirJoystick.Direction;
            Inp.Move = move;
        }

        if(ipTextField != null)
        {
            currentIP = ipTextField.text;
        }
    }

    public void Connect()
    {
        netTester.Connect();
    }

    public void Disable(bool disable)
    {
        Inp.Move.overrideInput = !disable;
        gameObject.SetActive(!disable);
    }
}
