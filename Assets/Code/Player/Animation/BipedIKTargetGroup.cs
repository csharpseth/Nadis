using System.Collections.Generic;
using UnityEngine;

public struct BipedIKTargetGroup
{
    public UnityEngine.Transform targets;
    public IKTarget root;
    public IKTarget head;
    public IKTarget chest;
    public IKTarget pelvis;
    //public IKTarget rightHip;
    //public IKTarget leftHip;
    public FootTarget rightFoot;
    public FootTarget leftFoot;
    public IKTarget rightHand;
    public IKTarget leftHand;
    
    private void SetOrCreate(AnimatorTarget target, Side side, AnimatorTargetType type, Transform trans)
    {
        if(type == AnimatorTargetType.Target)
        {
            switch (target)
            {
                case AnimatorTarget.None:
                    return;
                case AnimatorTarget.Head:
                    if (head == null)
                        head = new IKTarget();

                    head.target = trans;
                    break;
                case AnimatorTarget.Chest:
                    if (chest == null)
                        chest = new IKTarget();

                    chest.target = trans;
                    break;
                case AnimatorTarget.Pelvis:
                    if (pelvis == null)
                        pelvis = new IKTarget();

                    pelvis.target = trans;
                    break;
                case AnimatorTarget.Hands:
                    if(side == Side.Left)
                    {
                        if (leftHand == null) leftHand = new IKTarget();
                        leftHand.target = trans;
                    }else if(side == Side.Right)
                    {
                        if (rightHand == null) rightHand = new IKTarget();
                        rightHand.target = trans;
                    }

                    break;
                case AnimatorTarget.Feet:
                    if (side == Side.Right)
                    {
                        if (rightFoot == null) rightFoot = new FootTarget();
                        rightFoot.target = trans;
                    }
                    else if (side == Side.Left)
                    {
                        if (leftFoot == null) leftFoot = new FootTarget();
                        leftFoot.target = trans;
                    }

                    break;
                default:
                    break;
            }
        }else
        {
            switch (target)
            {
                case AnimatorTarget.None:
                    return;
                case AnimatorTarget.Head:
                    if (head == null)
                        head = new IKTarget();

                    head.obj = trans;
                    break;
                case AnimatorTarget.Chest:
                    if (chest == null)
                        chest = new IKTarget();

                    chest.obj = trans;
                    break;
                case AnimatorTarget.Pelvis:
                    if (pelvis == null)
                        pelvis = new IKTarget();

                    pelvis.obj = trans;
                    break;
                case AnimatorTarget.Hands:
                    if (side == Side.Left)
                    {
                        if (leftHand == null) leftHand = new IKTarget();
                        leftHand.obj = trans;
                    }
                    else if (side == Side.Right)
                    {
                        if (rightHand == null) rightHand = new IKTarget();
                        rightHand.obj = trans;
                    }

                    break;
                default:
                    break;
            }
        }
    }

    public void PopulateFrom(Transform root, BipedProceduralAnimator animator = null)
    {
        this.root = new IKTarget();
        this.root.target = root;
        IIkTargetComponent[] comps = root.GetComponentsInChildren<IIkTargetComponent>();
        for (int i = 0; i < comps.Length; i++)
        {
            IIkTargetComponent comp = comps[i];
            SetOrCreate(comp.Target, comp.Side, comp.Type, comp.Obj);
        }

        if (animator != null)
            InitAll(animator);
    }
    public void InitAll(BipedProceduralAnimator animator)
    {
        root.Init(animator);
        head.Init(animator);
        chest.Init(animator);
        pelvis.Init(animator);
        rightFoot.Init(animator);
        leftFoot.Init(animator);
        rightHand.Init(animator);
        leftHand.Init(animator);
    }
}