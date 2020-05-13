using UnityEngine;

public class KeepZeroed : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.localPosition = Vector3.zero;
    }
}
