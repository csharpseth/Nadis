using UnityEngine;

[System.Serializable]
public class IKTarget
{
    public const float ReturnSpeed = 8f;

    public Transform target;
    public Transform obj;
    [HideInInspector]
    public Vector3 defaultPosition;
    [HideInInspector]
    public Vector3 defaultRotation;
    [HideInInspector]
    public Transform defaultParent;
    [HideInInspector]
    public LerpData lerp = null;
    [HideInInspector]
    public BipedProceduralAnimator animator;

    private bool reset = false;

    public bool Lerping { get { return (lerp != null); } }
    public bool Null { get { return !target; } }

    public Vector3 localPosition
    {
        get
        {
            return target.localPosition;
        }
        set
        {
            target.localPosition = value;
        }
    }
    public Vector3 position { get { return target.position; } set { target.position = value; } }
    public Quaternion rotation { get { return target.rotation; } set { target.rotation = value; } }
    public Quaternion localRotation { get { return target.localRotation; } set { target.localRotation = value; } }

    public Vector3 localEulerAngles { get { return target.localEulerAngles; } set { target.localEulerAngles = value; } }
    public Vector3 eulerAngles { get { return target.eulerAngles; } set { target.eulerAngles = value; } }
    public void SetParent(Transform t) { target.SetParent(t); }

    public static IKTarget Empty { get { return default(IKTarget); } }

    public void Init(BipedProceduralAnimator anim)
    {
        defaultPosition = target.localPosition;
        defaultRotation = target.localEulerAngles;
        defaultParent = target.parent;
        animator = anim;
    }
    public void Reset()
    {
        if (reset == true) { animator.lerpDatas.Release(lerp); lerp = null; reset = false; target.localPosition = defaultPosition; return; }

        reset = true;
        SetParent(defaultParent);
        lerp = animator.lerpDatas.GetInstance();
        lerp.Init(this, defaultPosition, defaultRotation, ReturnSpeed, true, false, Reset); ;
    }
}
