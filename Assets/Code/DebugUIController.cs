using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUIController : MonoBehaviour
{
    public static DebugUIController ins;
    private string content = "";
    public TextMeshProUGUI text;

    private void Awake()
    {
        if (ins == null)
            ins = this;
    }

    private void LateUpdate()
    {
        text.text = content;
        content = "";
    }

    public void AppendNewLine(object value)
    {
        content += "\n" + value.ToString();
    }

}
