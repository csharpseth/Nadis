using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 axis = new Vector3(0, 1, 0);
    public float speed = 10f;

    private void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime);
    }
}
