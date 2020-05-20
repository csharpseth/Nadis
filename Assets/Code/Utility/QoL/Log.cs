using UnityEngine;

public static class Log
{
    public static bool LogText = true;
    public static bool LogWarnings = true;
    public static bool LogErrors = true;
    public static bool LogNotifications = true;
    public static bool LogEvents = true;

    public static void Not(object input, params object[] args)
    {
        if (LogNotifications == false) return;

        if (args == null || args.Length == 0)
            Debug.Log(input);
        else if (args != null && args.Length > 0)
            Debug.LogFormat(input.ToString(), args);
    }

    public static void Txt(object input, params object[] args)
    {
        if (LogText == false) return;

        if (args == null || args.Length == 0)
            Debug.Log(input);
        else if (args != null && args.Length > 0)
            Debug.LogFormat(input.ToString(), args);
    }
    public static void Wrn(object input, params object[] args)
    {
        if (LogWarnings == false) return;

        if (args == null || args.Length == 0)
            Debug.LogWarning(input);
        else if (args != null && args.Length > 0)
            Debug.LogWarningFormat(input.ToString(), args);
    }
    public static void Err(object input, params object[] args)
    {
        if (LogErrors == false) return;

        if (args == null || args.Length == 0)
            Debug.LogError(input);
        else if (args != null && args.Length > 0)
            Debug.LogErrorFormat(input.ToString(), args);
    }

    public static void Event(object input, params object[] args)
    {
        if(LogEvents == false) return;

        if(args == null || args.Length == 0)
            Debug.Log("EVENT::" + input);
        else
            Debug.LogFormat("EVENT::" + input, args);
    }
}
