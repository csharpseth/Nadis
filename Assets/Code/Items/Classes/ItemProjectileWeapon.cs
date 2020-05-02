using UnityEngine;

public class ItemProjectileWeapon : ItemWeapon
{
    [Header("Projectile Weapon Data:")]
    [SerializeField]
    internal Vector3 _aimPosition;
    [SerializeField]
    internal Vector3 _aimRotation;
    [SerializeField]
    internal Vector3 _aimHandPosition;
    [SerializeField]
    internal Vector3 _aimHandRotation;
    [SerializeField]
    internal Vector3 _holdRotation;
    [Space(20)]
    [SerializeField]
    internal float fireDelay;
    [Range(0f, 1f)]
    [SerializeField]
    internal float _reqoil;
    internal float _reqoilDuration { get { return (fireDelay - 0.02f); } }
    [SerializeField]
    internal float _timeToAim;
    [SerializeField]
    internal WeaponFireType _fireType;
    internal bool _aimed = false;
    internal bool _aiming = false;

    [SerializeField]
    internal bool _toggleAim;
    [Header("Audio:")]
    public AudioClip fireSound;


    private bool aimStarted = false;
    private float fireTime = 0f;
    private BipedProceduralAnimator anim;
    private NetworkedPlayer player;
    private bool canFire = true;
    
    public override void InitFromNetwork(int netID)
    {
        base.InitFromNetwork(netID);
        fireTime = fireDelay;
    }

    public override void ActiveUpdate(int ownerID)
    {
        AimCheck(ownerID);
        FireCheck(ownerID);
    }

    private void FireCheck(int ownerID)
    {
        if(canFire)
        {
            if (_fireType == WeaponFireType.Full && Inp.Interact.Primary)
            {
                canFire = false;
                Fire(ownerID);
            }
            else if (_fireType == WeaponFireType.Semi && Inp.Interact.PrimaryDown)
            {
                canFire = false;
                Fire(ownerID);
            }
        }
    }

    private void AimCheck(int ownerID)
    {
        if (_toggleAim)
        {
            if (Inp.Interact.SecondaryDown)
            {
                _aimed = !_aimed;
                Aim(ownerID);
            }
        }
        else
        {
            if (Inp.Interact.Secondary && _aimed == false)
            {
                _aimed = true;
                Aim(ownerID);
            }
            else if (_aimed == true)
            {
                _aimed = false;
                Aim(ownerID);
            }
        }
    }

    public void Aim(int playerID)
    {
        if(player == null || player.NetID != playerID)
        {
            Events.Player.GetPlayer(playerID, ref player);
            anim = player.Animator;
        }
        if (_aimed)
        {
            player.SendPlayerAnimatorTargetSet(_aimPosition, _aimRotation, AnimatorTarget.Hands, 5f, Space.Local, true, AnimatorTarget.Head, Side.Right);


            /*
            transform.SetParent(anim.TargetFrom(AnimatorTarget.Head).target);

            anim.SetHandTarget(transform, Side.Right, _aimHandPosition, _aimHandRotation);

            /*Tween.FromTo(anim.targets.rightHand.target, _aimHandPosition, _aimHandRotation, _timeToAim, Space.Local, false, null, (Transform t) => {
                t.SetParent(transform);
                t.localPosition = _aimHandPosition;
                t.localEulerAngles = _aimHandRotation;
            });

            //anim.SetHandTarget(transform, Side.Right, _aimHandPosition);
            //anim.targets.rightHand.localEulerAngles = _aimHandRotation;

            _aiming = true;
            Tween.FromTo(transform, _aimPosition, _aimRotation, _timeToAim, Space.Local, false, null, (Transform t) => {
                t.localPosition = _aimPosition;
                t.localEulerAngles = _aimRotation;
                _aiming = false;
            });
            */
        }
        else
        {

            player.SendPlayerAnimatorTargetEnd(AnimatorTarget.Hands, Side.Right);
            /*
            anim.EndCurrentHandTarget(playerID);
            transform.SetParent(anim.targets.rightHand.obj);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = _holdRotation;
            */
        }
    }

    public void Fire(int playerID)
    {
        if(Source != null && fireSound != null)
            Source.PlayOneShot(fireSound);

        Vector3 recoilOffset = (Vector3.back * _reqoil);
        recoilOffset += _aimPosition;
        Tween.FromToPosition(transform, recoilOffset, _reqoilDuration, Space.Local, true, null, (Transform t) =>
        {
            canFire = true;
            transform.localPosition = _aimPosition;
        });
    }

    public override void Update()
    {
        
    }
}
