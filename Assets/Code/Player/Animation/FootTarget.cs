using UnityEngine;

[System.Serializable]
public struct FootTarget
{
    public Transform obj;
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
            return obj.localPosition;
        }
        set
        {
            obj.localPosition = value;
        }
    }
    public Vector3 position { get { return obj.position; } set { obj.position = value; } }
    public Quaternion rotation { get { return obj.rotation; } set { obj.rotation = value; } }
    public Quaternion localRotation { get { return obj.localRotation; } set { obj.localRotation = value; } }

    public Vector3 localEulerAngles { get { return obj.localEulerAngles; } set { obj.localEulerAngles = value; } }
    public void SetParent(Transform t) { obj.SetParent(t); }

    public static IKTarget Empty { get { return default; } }

    public void Init(BipedProceduralAnimator anim)
    {
        defaultPosition = obj.localPosition;
        defaultParent = obj.parent;
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