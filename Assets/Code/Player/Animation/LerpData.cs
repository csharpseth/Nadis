using System;
using System.Collections.Generic;
using UnityEngine;

public class FootLerpData
{
    public const float FinishThreshold = 0.2f;
    public Vector3 destination;
    public Vector3 initialOrigin;
    public float speed;
    public float height;
    public float stepSize;
    private bool midReached = false;
    public bool done = false;
    public float currentHeight = 0f;
    private Action callback;

    public Vector3 Mid
    {
        get
        {
            Vector3 m = (destination + initialOrigin) / 2f;
            m.y += height;
            return m;
        }
    }

    public FootLerpData()
    {

    }
    public FootLerpData(Vector3 destination, Vector3 origin, float speed, float height, float stepSize, Action callback = null)
    {
        this.destination = destination;
        this.initialOrigin = origin;
        this.speed = speed;
        this.height = height;
        this.stepSize = stepSize;
        this.callback = callback;
    }

    public Vector3 DoLerpFrom(Vector3 target)
    {
        if (Close(target, destination) == true)
        {
            done = true;
            callback?.Invoke();
        }
        currentHeight = target.y;
        if (midReached == false)
        {
            Vector3 newPos = Vector3.Lerp(target, Mid, speed * Time.deltaTime);
            if (Close(Mid, newPos))
            {
                midReached = true;
            }

            return newPos;
        }



        return Vector3.Lerp(target, destination, speed * Time.deltaTime);

    }
    public bool Close(Vector3 origin, Vector3 destination)
    {
        float sqrDist = (destination - origin).sqrMagnitude;
        return (sqrDist <= (FinishThreshold * FinishThreshold));

    }

    public void Init(Vector3 destination, Vector3 origin, float speed, float height, float stepSize, Action callback = null)
    {
        this.destination = destination;
        this.initialOrigin = origin;
        this.speed = speed;
        this.height = height;
        this.stepSize = stepSize;
        this.callback = callback;

        done = false;
    }
}

public class LerpData
{
    private float dist;
    private const float FinishThreshold = (0.1f * 0.1f);
    private Queue<LerpPoint> posQueue;
    private LerpPoint nextPos;
    private IKTarget target;
    private Action doneCallback;
    private bool local;
    private bool persistent;
    private LerpPoint finalDestination;
    private Transform seekTarget;

    public string Animation { get; set; }
    public bool Done { get; private set; }

    public LerpData() {
        dist = 0f;
        nextPos = default;
        target = null;
    }

    public LerpData(IKTarget tar, Vector3 pos, float speed, bool local = true, bool persistent = false, Action doneCallback = null)
    {
        target = tar;
        nextPos.position = pos;
        nextPos.speed = speed;
        finalDestination.position = pos;
        this.local = local;
        this.doneCallback = doneCallback;
        this.persistent = persistent;
    }
    public LerpData(IKTarget tar, LerpPoint[] pos, bool local = true, bool persistent = false, Action doneCallback = null)
    {
        target = tar;
        posQueue = new Queue<LerpPoint>();
        for (int i = 0; i < pos.Length; i++)
        {
            posQueue.Enqueue(pos[i]);
        }

        finalDestination = pos[pos.Length - 1];

        this.local = local;
        this.doneCallback = doneCallback;
        this.persistent = persistent;
    }

    public LerpData(IKTarget tar, Transform seekTarget, Vector3 offset = default, Vector3 rot = default)
    {
        target = tar;
        this.seekTarget = seekTarget;

        target.SetParent(seekTarget);
        target.localPosition = offset;
        target.localEulerAngles = rot;
    }

    public void DoLerp()
    {
        if (seekTarget != null) return;

        if (posQueue == null || posQueue.Count == 0)
        {
            if (nextPos.position == Vector3.zero && persistent == false)
            {
                Done = true;
                doneCallback?.Invoke();
                return;
            }
        }

        SetNextPosition();

        if (local)
            LocalLerp();
        else
            GlobalLerp();
    }
    private bool SetNextPosition()
    {
        if (nextPos.position != Vector3.zero)
            return false;
        if (posQueue == null || posQueue.Count == 0)
            return false;

        nextPos = posQueue.Dequeue();
        return true;

    }
    private void LocalLerp()
    {
        if (nextPos.position == Vector3.zero)
            return;

        target.localPosition = Vector3.Lerp(target.localPosition, nextPos.position, nextPos.speed * Time.deltaTime);
        target.localEulerAngles = nextPos.rotation;
        dist = (nextPos.position - target.localPosition).sqrMagnitude;

        if (dist <= FinishThreshold)
        {
            nextPos.position = Vector3.zero;
        }

    }
    private void GlobalLerp()
    {
        if (nextPos.position == Vector3.zero)
            return;

        target.position = Vector3.Lerp(target.position, nextPos.position, nextPos.speed * Time.deltaTime);
        target.eulerAngles = nextPos.rotation;
        float dist = (nextPos.position - target.position).sqrMagnitude;
        if (dist <= FinishThreshold)
        {
            nextPos.position = Vector3.zero;
        }
    }

    public void Init(IKTarget tar, Vector3 pos, Vector3 rot, float speed, bool local = true, bool persistent = false, Action doneCallback = null)
    {
        target = tar;
        nextPos.position = pos;
        nextPos.rotation = rot;
        nextPos.speed = speed;
        finalDestination.position = pos;
        this.local = local;
        this.doneCallback = doneCallback;
        this.persistent = persistent;

        Done = false;
    }

    public void Init(IKTarget tar, LerpPoint[] pos, bool local = true, bool persistent = false, Action doneCallback = null)
    {
        target = tar;
        posQueue = new Queue<LerpPoint>();
        for (int i = 0; i < pos.Length; i++)
        {
            posQueue.Enqueue(pos[i]);
        }

        finalDestination = pos[pos.Length - 1];

        this.local = local;
        this.doneCallback = doneCallback;
        this.persistent = persistent;
        Done = false;
    }

    public void Init(IKTarget tar, Transform seekTarget, Vector3 offset = default, Vector3 rot = default)
    {
        target = tar;
        this.seekTarget = seekTarget;

        target.SetParent(seekTarget);
        target.localPosition = offset;
        target.localEulerAngles = rot;
        Done = false;
    }

}
