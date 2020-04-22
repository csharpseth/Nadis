using MLAPI;
using UnityEngine;

public class NetworkedPlayer : MonoBehaviour
{
    public static NetworkedPlayer Local;
    public static ulong LocalID { get { return (Local != null) ? Local.ID : 10000; } }

    NetworkedObject netObj;
    public ulong ID { get; private set; }
    public bool IsLocal { get; private set; }

    private void Start()
    {
        netObj = GetComponent<NetworkedObject>();
        ID = netObj.NetworkId;
        IsLocal = netObj.IsLocalPlayer;
        
        if(IsLocal == false)
        {
            Camera[] cams = GetComponentsInChildren<Camera>();
            RotateWithMouse[] rot = GetComponentsInChildren<RotateWithMouse>();
            MovementController move = GetComponent<MovementController>();
            PerspectiveController pers = GetComponent<PerspectiveController>();
            Destroy(GetComponent<PlayerInteractionController>());
            Destroy(GetComponent<PlayerUIController>());
            Destroy(pers);
            for (int i = 0; i < cams.Length; i++)
            {
                Destroy(cams[i].gameObject);
            }
            for (int i = 0; i < rot.Length; i++)
            {
                Destroy(rot[i]);
            }

            Destroy(move);
            
        }else
        {
            Local = this;
            TesterMenu.LocalPlayerID = ID;
        }

        TesterMenu.RegisterClient(this);
        Inventory.Create(ID, 7);
    }
}
