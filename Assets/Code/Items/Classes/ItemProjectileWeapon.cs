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
    internal bool _aimed;
    internal bool _aiming;

    [SerializeField]
    internal bool _toggleAim;
    [Header("Audio:")]
    public AudioClip fireSound;


    private bool aimStarted = false;
    private float fireTime = 0f;
    private BipedProceduralAnimator anim;
    private bool canFire = true;

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
        if(canFire)
        {
            if (_fireType == WeaponFireType.Full && Inp.Interact.Primary)
            {
                canFire = false;
                Fire(0);
            }
            else if (_fireType == WeaponFireType.Semi && Inp.Interact.PrimaryDown)
            {
                canFire = false;
                Fire(0);
            }
        }
    }

    private void AimCheck()
    {
        if (_toggleAim)
        {
            if (Inp.Interact.SecondaryDown)
            {
                _aimed = !_aimed;
                Aim(0);
            }
        }
        else
        {
            if (Inp.Interact.Secondary && _aimed == false)
            {
                _aimed = true;
                Aim(0);
            }
            else if (_aimed == true)
            {
                _aimed = false;
                Aim(0);
            }
        }
    }

    public void Aim(ulong playerID)
    {
        /*
        if (anim == null)
            anim = TesterMenu.GetAnimator(playerID);
        */

        if (_aimed)
        {
            transform.SetParent(anim.TargetFrom(AnimatorTarget.head).target);

            anim.SetHandTarget(transform, Side.Right, _aimHandPosition, _aimHandRotation);

            /*Tween.FromTo(anim.targets.rightHand.target, _aimHandPosition, _aimHandRotation, _timeToAim, Space.Local, false, null, (Transform t) => {
                t.SetParent(transform);
                t.localPosition = _aimHandPosition;
                t.localEulerAngles = _aimHandRotation;
            });*/

            //anim.SetHandTarget(transform, Side.Right, _aimHandPosition);
            //anim.targets.rightHand.localEulerAngles = _aimHandRotation;

            _aiming = true;
            Tween.FromTo(transform, _aimPosition, _aimRotation, _timeToAim, Space.Local, false, null, (Transform t) => {
                t.localPosition = _aimPosition;
                t.localEulerAngles = _aimRotation;
                _aiming = false;
            });
        }
        else
        {
            anim.EndCurrentHandTarget(0);
            transform.SetParent(anim.targets.rightHand.obj);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = _holdRotation;
        }
    }

    public void Fire(ulong playerID)
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
