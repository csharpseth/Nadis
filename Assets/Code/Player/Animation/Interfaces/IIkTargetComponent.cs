using UnityEngine;

public interface IIkTargetComponent
{
    AnimatorTarget Target { get; }
    Side Side { get; }
    AnimatorTargetType Type { get; }
    Transform Obj { get; }
}