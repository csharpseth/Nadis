using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithMouse : MonoBehaviour
{
    public RotateAxis axis;
    public float lookSpeed = 300f;
    public float maxAngle = 50f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float h = lookSpeed * Inp.Move.LookDir.x;
        float v = -lookSpeed * Inp.Move.LookDir.y;
        Vector3 rot = transform.eulerAngles;

        if (axis == RotateAxis.X || axis == RotateAxis.XandY)
        {
            rot.x += v;

            if (rot.x > 0f && rot.x < 180f && rot.x > Settings.player.MinHorizontalAngle)
            {
                rot.x = Settings.player.MinHorizontalAngle;
            }
            if (rot.x > 0f && rot.x > 180f && rot.x < (360f - Settings.player.MaxHorizontalAngle))
                rot.x = (360f - Settings.player.MaxHorizontalAngle);
        }
        if (axis == RotateAxis.Y || axis == RotateAxis.XandY)
        {
            rot.y += h;
        }
        
        transform.eulerAngles = rot;
    }

}

public enum RotateAxis
{
    X,
    Y,
    XandY
}