using UnityEngine;
using System.Collections;

public class PlayerInteractionController : MonoBehaviour
{
    public float reach = 5f;
    public LayerMask interactionMask;
    public GameObject testItem = null;
    
    private Camera cam;
    private int _activeIndex = 0;
    private int ActiveIndex { get { return _activeIndex; } set { _activeIndex = Mathf.Clamp(value, 0, Inventory.GetSize(0) - 1); Inventory.DisableAllExcept(0, _activeIndex); } }
    private Item ActiveItem { get { return Inventory.GetItem(0, ActiveIndex); } }

    public Vector2 CenterScreen { get { return new Vector2(Screen.width / 2f, Screen.height / 2f); } }
    public Ray CenterScreenRay{ get { return cam.ScreenPointToRay(CenterScreen); } }

    private void Awake()
    {
        cam = GetComponent<PerspectiveController>().firstPersonCam;
    }

    private void Update()
    {
        if (Inp.Interact.Next)
            ActiveIndex++;
        else if (Inp.Interact.Previous)
            ActiveIndex--;


        if(ActiveItem != null)
        {
            if (ActiveItem.Active == false)
                ActiveItem.Hide(false);

            ActiveItem.ActiveUpdate();
        }else
        {
            if(Inp.Interact.PrimaryDown)
            {
                RaycastHit hit;
                if(Physics.Raycast(CenterScreenRay, out hit, reach))
                {
                    //TesterMenu.SpawnObject(testItem, hit.point);
                }
            }

            if (Inp.Interact.InteractDown)
                Interact();
        }
    }

    private void Interact()
    {
        RaycastHit hit;
        if(Physics.Raycast(CenterScreenRay, out hit, reach, interactionMask))
        {
            Item ent = hit.transform.GetComponent<Item>();
            if(ent != null)
            {
                //ent.Interact(NetworkedPlayer.LocalID);
            }
        }
    }
}
