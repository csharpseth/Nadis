using UnityEngine;

public class ItemProjectileWeapon : ItemWeapon
{
    [Header("Projectile Weapon Data:")]
    [SerializeField]
    internal float _aimOffset;
    [SerializeField]
    internal Vector3 _fireOrigin;
    [Space(20)]
    [SerializeField]
    internal float fireDelay;
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
                Debug.Log("Fire");
                Fire(ownerID);
            }
        }else
        {
            fireTime += Time.deltaTime;
            if (fireTime >= fireDelay)
            {
                canFire = true;
                fireTime = 0f;
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
            _aimed = Inp.Interact.Secondary;
            Aim(ownerID);
        }
    }

    public void Aim(int playerID)
    {
        Events.Player.SetAimOffset(playerID, _aimOffset);
        Events.Player.SetAnimatorBool(playerID, "aim", _aimed);
    }

    public void Fire(int playerID)
    {
        if(Source != null && fireSound != null)
            Source.PlayOneShot(fireSound);

        /*
        Vector3 pos = anim.targets.rightHand.localPosition;
        pos -= (Vector3.forward * _reqoil);
        Tween.FromToPosition(anim.targets.rightHand.target, pos, _reqoilDuration, Space.Local, true, null, null);
        */

        RaycastHit hit;
        if(Physics.Raycast(LocalToGlobal(_fireOrigin), transform.forward, out hit, _range))
        {
            FXController.HitAt(hit.point);
            NetworkedPlayer player = hit.transform.GetComponent<NetworkedPlayer>();
            if(player != null)
            {
                player.RequestDamageThisPlayer(Damage);
            }
        }
    }

    public override void Update()
    {
        
    }

    private Vector3 LocalToGlobal(Vector3 input)
    {
        Vector3 origin = transform.position;
        origin += transform.right * input.x;
        origin += transform.up * input.y;
        origin += transform.forward * input.z;
        return origin;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 origin = LocalToGlobal(_fireOrigin);

        Gizmos.DrawSphere(origin, 0.01f);
        Gizmos.DrawLine(origin, origin + transform.forward);
        Gizmos.DrawSphere(origin + transform.forward, 0.01f);
    }
}
