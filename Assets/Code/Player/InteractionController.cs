using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public static InteractionController ins;
    
    public float interactionReach = 6f;
    public LayerMask interactionMask;

    public Vector2 CenterScreen
    {
        get
        {
            return new Vector2(Screen.width / 2, Screen.height / 2);
        }
    }
    public Ray CenterScreenRay
    {
        get
        {
            return Camera.ScreenPointToRay(CenterScreen);
        }
    }
    public Camera Camera
    {
        get
        {
            return PerspectiveController.ins.ActiveCamera;
        }
    }
    public Transform LookingAt
    {
        get
        {
            RaycastHit hit;
            if (Physics.Raycast(CenterScreenRay, out hit))
                return hit.transform;
            return null;
        }
    }
    public PhysicalItem ItemLookingAt
    {
        get
        {
            Transform t = LookingAt;
            if (t != null)
                return t.GetComponent<PhysicalItem>();
            return null;
        }
    }

    private Inventory localInventory;

    public BipedProceduralAnimator Animator { get; private set; }
    
    private void Awake()
    {
        if (ins == null)
            ins = this;

        localInventory = GetComponent<Inventory>();
    }
    private void Update()
    {
        if (InputManager.Interact.PrimaryDown)
        {
            Events.BipedAnimator.ExecuteAnimation(0, "punch", null);
            //Events.Item.OnRequestSpawnItem(0, hit.point, Vector3.zero, true);
        }

        UseActiveItem();
        Interaction();
    }

    private void UseActiveItem()
    {
        if (localInventory == null) return;
        if (localInventory.ActiveItem == null) return;

        //localInventory.Use(1, Input.GetKeyDown(PlayerInput.FireOne));
        //localInventory.Use(2, Input.GetKey(PlayerInput.FireTwo));
    }
    private void Interaction()
    {
        if (localInventory == null) return;
        if (localInventory.ActiveItem != null) return;

        RaycastHit hit;
        Physics.Raycast(CenterScreenRay, out hit, interactionReach, interactionMask);
        if (hit.transform == null) return;
        
        

        if(false)
        {
            PhysicalItem item = hit.transform.GetComponent<PhysicalItem>();
            if (item == null) return;

            Events.Item.OnRequestDestroyItem(item.InstanceID, true);
        }
    }

}
