using UnityEngine;

public class KeepZeroed : MonoBehaviour
{
    Vector3 offset = new Vector3(0f, 0f, 0f);

    private void LateUpdate()
    {
        transform.localPosition = offset;
        transform.localEulerAngles = Vector3.zero;
    }
}
