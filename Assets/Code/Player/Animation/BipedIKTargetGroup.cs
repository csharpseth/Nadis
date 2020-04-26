[System.Serializable]
public struct BipedIKTargetGroup
{
    public UnityEngine.Transform targets;
    public IKTarget root;
    public IKTarget head;
    public IKTarget chest;
    public IKTarget pelvis;
    public IKTarget rightHip;
    public IKTarget leftHip;
    public FootTarget rightFoot;
    public FootTarget leftFoot;
    public IKTarget rightHand;
    public IKTarget leftHand;
}