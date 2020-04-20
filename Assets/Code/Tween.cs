using System;
using System.Collections.Generic;
using UnityEngine;

public class Tween : MonoBehaviour
{
    public static Tween ins;
    public struct TweenData
    {
        public const float DoneThreshold = 0.07f;
        public Transform target;
        public float duration;
        public Vector3 destination;
        public Space space;
        public TweenType type;
        public Direction dir;
        public Action<Transform> finishCallback;
        private float initialDistance;
        public float time;
        public AnimationCurve curve;
        
        public TweenData(Transform target, float duration, AnimationCurve curve, Vector3 dest, Space space, Action<Transform> callback)
        {
            this.target = target;
            this.duration = duration;
            destination = dest;
            this.space = space;
            type = TweenType.FromTo;
            dir = Direction.None;
            finishCallback = callback;
            initialDistance = 0f;
            time = 0f;
            this.curve = curve;
            if (this.curve == null)
                curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            SetInitialDist();
        }

        private void SetInitialDist()
        {
            if (space == Space.Local)
            {
                initialDistance = (destination - target.localPosition).sqrMagnitude;
            }
            else if (space == Space.World)
            {
                initialDistance = (destination - target.position).sqrMagnitude;
            }
        }

        public float Dist
        {
            get
            {
                float dist = 0f;
                if (space == Space.Local)
                {
                    dist = (destination - target.localPosition).sqrMagnitude;
                }else if(space == Space.World)
                {
                    dist = (destination - target.position).sqrMagnitude;
                }

                return dist;
            }
        }
        public float Completed
        {
            get
            {
                return 1f - (Dist / initialDistance);
            }
        }
        public bool Done
        {
            get
            {
                return (Completed >= (1f - DoneThreshold));
            }
        }

        public float DurationPercent
        {
            get
            {
                return (time / duration);
            }
        }
    }

    private List<TweenData> tweens = new List<TweenData>();

    private void Awake()
    {
        if (ins != null)
            Destroy(this);
        ins = this;
    }
    private void Update()
    {
        if (tweens == null || tweens.Count == 0) return;
        
        for (int i = 0; i < tweens.Count; i++)
        {
            TweenData data = tweens[i];
            if (data.type == TweenType.FromTo)
            {
                if (LerpTo(data.target, ref data))
                {
                    RemoveTween(data.target);
                    data.finishCallback?.Invoke(data.target);
                    if (tweens.Count == 0) return;
                    else continue;
                }
            }
            tweens[i] = data;
        }
    }

    private bool LerpTo(Transform tar, ref TweenData data)
    {
        Vector3 pos = Vector3.zero;
        if(data.space == Space.Local)
        {
            pos = tar.localPosition;
            pos = Lerp(pos, data.destination, data.curve.Evaluate(data.DurationPercent));
            tar.localPosition = pos;
        }else if(data.space == Space.World)
        {
            pos = tar.position;
            pos = Lerp(pos, data.destination, data.curve.Evaluate(data.DurationPercent));
            tar.position = pos;
        }

        data.time += Time.deltaTime;

        return data.Done;
    }

    public bool Has(Transform t)
    {
        bool val = false;

        for (int i = 0; i < tweens.Count; i++)
        {
            if(tweens[i].target == t)
            {
                val = true;
                break;
            }
        }

        return val;
    }
    public TweenData DataFromTarget(Transform target)
    {
        TweenData t = default;
        if (Has(target) == false) return t;

        for (int i = 0; i < tweens.Count; i++)
        {
            if (tweens[i].target == target)
            {
                t = tweens[i];
                break;
            }
        }

        return t;
    }
    public void AddTween(TweenData data)
    {
        tweens.Add(data);
    }
    public void RemoveTween(Transform tar)
    {
        if (Has(tar) == false) return;
        tweens.Remove(DataFromTarget(tar));
    }

    public static void FromToPosition(Transform tar, Vector3 destination, float duration, Space space, AnimationCurve curve = null, Action<Transform> finishCallback = null)
    {
        Debug.Log(ins.Has(tar));
        if (ins.Has(tar) == true) return;

        TweenData data = new TweenData(tar, duration, curve, destination, space, finishCallback);
        ins.AddTween(data);
    }

    public static float Lerp(float a, float b, float percent)
    {
        float sample = (b - a) * percent + a;
        return sample;
    }
    public static Vector3 Lerp(Vector3 a, Vector3 b, float percent)
    {
        float x = Lerp(a.x, b.x, percent);
        float y = Lerp(a.y, b.y, percent);
        float z = Lerp(a.z, b.z, percent);

        return new Vector3(x, y, z);
    }
}
