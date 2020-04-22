using UnityEngine;

public class EntityProjectileWeapon : EntityWeapon
{
    [Header("Projectile Weapon Data:")]
    [SerializeField]
    internal Vector3 _aimPosition;
    [SerializeField]
    internal Vector3 _aimRotation;
    [SerializeField]
    internal Vector3 _aimHandOffset;
    [SerializeField]
    internal Vector3 _holdRotation;
    [Space(20)]
    [SerializeField]
    internal float fireDelay;
    [Range(0f, 1f)]
    [SerializeField]
    internal float _reqoil;
    [SerializeField]
    internal float _timeToAim;
    [SerializeField]
    internal WeaponFireType _fireType;
    internal bool _aimed;
    internal bool _aiming;

    [SerializeField]
    internal bool _toggleAim;

    private bool aimStarted = false;
    private float fireTime = 0f;
    private BipedProceduralAnimator anim;
    private bool fired = false;

    internal override void Awake()
    {
        base.Awake();
        fireTime = fireDelay;
    }

    public override void ActiveUpdate()
    {
        AimCheck();
        FireCheck();
    }

    private void FireCheck()
    {
        if(fired == true)
        {
            fireTime += Time.deltaTime;
            if(fireTime >= fireDelay)
            {
                fireTime = 0f;
                fired = false;
            }
        }

        if (fired == true) return;

        if(_fireType == WeaponFireType.Full && Inp.Interact.Primary)
        {
            Fire(NetworkedPlayer.LocalID);
        }

        if (_fireType == WeaponFireType.Semi && Inp.Interact.PrimaryDown)
        {
            Fire(NetworkedPlayer.LocalID);
        }
    }

    private void AimCheck()
    {
        if (_toggleAim)
        {
            if (Inp.Interact.SecondaryDown)
            {
                _aimed = !_aimed;
                Aim(NetworkedPlayer.LocalID);
            }
        }
        else
        {
            if (Inp.Interact.Secondary && _aimed == false)
            {
                _aimed = true;
                Aim(NetworkedPlayer.LocalID);
            }
            else if (_aimed == true)
            {
                _aimed = false;
                Aim(NetworkedPlayer.LocalID);
            }
        }
    }

    public void Aim(ulong playerID)
    {
        if (anim == null)
            anim = TesterMenu.GetAnimator(playerID);

        if (_aimed)
        {
            transform.SetParent(anim.head.target);
            anim.SetHandTarget(transform, Side.Right, _aimHandOffset);
            _aiming = true;
            Tween.FromTo(transform, _aimPosition, _aimRotation, _timeToAim, Space.Local, false, null, (Transform t) => {
                t.localPosition = _aimPosition;
                t.localEulerAngles = _aimRotation;
                _aiming = false;
            });
        }
        else
        {
            anim.EndCurrentHandTarget((int)TesterMenu.LocalPlayerID);
            transform.SetParent(anim.rightHand.obj);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = _holdRotation;
        }
    }

    public void Fire(ulong playerID)
    {
        fired = true;
        Vector3 recoilOffset = (Vector3.back * _reqoil);
        recoilOffset += _aimPosition;
        Tween.FromToPosition(transform, recoilOffset, 0.075f, Space.Local, true, null, (Transform t) =>
        {
            transform.localPosition = _aimPosition;
        });
    }

    public override void Update()
    {
        
    }
}
