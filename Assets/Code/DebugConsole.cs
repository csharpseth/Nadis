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
        string inputString = input.ToString();

        if(args != null && args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string replace = "{" + i + "}";
                inputString = inputString.Replace(replace, args[i].ToString());
            }
        }

        inputString += "\n";
        consoleText.text += inputString;
    }
}
