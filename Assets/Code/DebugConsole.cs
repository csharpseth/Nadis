using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public TextMeshProUGUI consoleText;

    // Start is called before the first frame update
    void Start()
    {
        Log.OnLog += OnLog;
    }

    public void OnLog(object input, object[] args)
    {
        string inputString = string.Format(input.ToString(), args);

        inputString += "\n";
        consoleText.text += inputString;
    }
}
