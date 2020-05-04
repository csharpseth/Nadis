using UnityEngine;

public class IKComponent : MonoBehaviour, IIkTargetComponent
{
    public AnimatorTarget Target { get { return _target; } }
    public Side Side { get { return _side; } }
    public AnimatorTargetType Type { get { return _type; } }
    public Transform Obj { get { return transform; } }

    [SerializeField]
    private AnimatorTarget _target;
    [SerializeField]
    private Side _side;
    [SerializeField]
    private AnimatorTargetType _type;
}