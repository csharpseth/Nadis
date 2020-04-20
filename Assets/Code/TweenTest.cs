using UnityEngine;

public class TweenTest : MonoBehaviour
{
    public Transform target;
    public Vector3 dest1;
    public Vector3 dest2;
    [Range(0.1f, 100f)]
    public float speed = 10f;
    public AnimationCurve curve;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            Tween.FromToPosition(target, dest1, speed, Space.World, curve, FinishFirst);
        }
    }

    private void FinishFirst(Transform t)
    {
        Tween.FromToPosition(t, dest2, speed, Space.World, curve, null);
    }

    private void FinishTwo(Transform t)
    {
        Tween.FromToPosition(t, dest1, speed, Space.World, curve, FinishFirst);
    }
}
