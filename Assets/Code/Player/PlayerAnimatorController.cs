using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour, INetworkInitialized, IEventAccessor
{
    private Animator anim;
    private MovementController move;
    private PlayerMouseController mouse;

    public float baseMoveDamp = 5f;
    public float aimOffset = 0f;
    
    [HideInInspector] public float forwardBlend;
    [HideInInspector] public float sideBlend;
    [HideInInspector] public float aimPercent;
    [HideInInspector] public bool crouched;

    public int NetID { get; private set; }

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        move = GetComponent<MovementController>();
        mouse = GetComponent<PlayerMouseController>();
    }

    private void Update()
    {
        if (NetID == NetData.LocalPlayerID)
        {
            forwardBlend = move.InputSpeed.y;
            sideBlend = move.InputSpeed.x;
            crouched = move.data.state == PlayerMoveState.Crouching || move.data.state == PlayerMoveState.CrouchWalking;
        }
        
        anim.SetFloat("forward_blend", forwardBlend);
        anim.SetFloat("side_blend", sideBlend);
        //anim.SetBool("crouch", crouched);
    }

    public void SetBool(int playerID, string id, bool value)
    {
        if (playerID != NetID) return;

        anim.SetBool(id, value);
    }
    public void SetFloat(int playerID, string id, float value)
    {
        if (playerID != NetID) return;

        anim.SetFloat(id, value);
    }
    public void SetTrigger(int playerID, string id)
    {
        if (playerID != NetID) return;

        anim.SetTrigger(id);
    }


    private void SetAimOffset(int playerID, float offset)
    {
        if (playerID != NetID) return;

        aimOffset = offset;
    }

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        Subscribe();
    }

    public void Subscribe()
    {
        Events.Player.SetAnimatorBool += SetBool;
        Events.Player.SetAnimatorFloat += SetFloat;
        Events.Player.SetAnimatorTrigger += SetTrigger;
        Events.Player.SetAimOffset += SetAimOffset;
        Events.Player.UnSubscribe += UnSubscribe;
    }

    public void UnSubscribe(int netID)
    {
        Events.Player.SetAnimatorBool -= SetBool;
        Events.Player.SetAnimatorFloat -= SetFloat;
        Events.Player.SetAnimatorTrigger -= SetTrigger;
        Events.Player.SetAimOffset -= SetAimOffset;
        Events.Player.UnSubscribe -= UnSubscribe;
    }
}
