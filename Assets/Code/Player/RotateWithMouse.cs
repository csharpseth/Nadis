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


        float h = lookSpeed * Input.GetAxisRaw("Mouse X");
        float v = -lookSpeed * Input.GetAxisRaw("Mouse Y");
        Vector3 rot = transform.eulerAngles;

        if (axis == RotateAxis.X || axis == RotateAxis.XandY)
        {
            rot.x += v;

            if (rot.x > 0f && rot.x < 180f && rot.x > maxAngle)
            {
                rot.x = maxAngle;
            }
            if (rot.x > 0f && rot.x > 180f && rot.x < (360f - maxAngle))
                rot.x = (360f - maxAngle);
        }
        if (axis == RotateAxis.Y || axis == RotateAxis.XandY)
        {
            rot.y += h;
            if (maxAngle != 0f)
            {
                if (rot.y > maxAngle)
                    rot.y = maxAngle;

                /*
                if (rot.y < -maxAngle)
                    rot.y = -maxAngle;
                    */
            }
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