using UnityEngine;

[System.Serializable]
public class FootTarget
{
    public Transform target;
    [HideInInspector]
    public Vector3 defaultPosition;
    [HideInInspector]
    public Transform defaultParent;
    [HideInInspector]
    public FootLerpData lerp;
    [HideInInspector]
    public BipedProceduralAnimator animator;

    public bool Lerping { get { return (lerp != null); } }

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
    public void SetParent(Transform t) { target.SetParent(t); }

    public static IKTarget Empty { get { return default; } }

    public void Init(BipedProceduralAnimator anim)
    {
        defaultPosition = target.localPosition;
        defaultParent = target.parent;
        animator = anim;
    }
    public void Reset()
    {
        animator.footLerpDatas.Release(lerp);
        lerp = null;
        SetParent(defaultParent);
        localPosition = defaultPosition;
    }
}