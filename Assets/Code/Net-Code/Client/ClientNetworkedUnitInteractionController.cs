using UnityEngine;
using Nadis.Net;
using Nadis.Net.Client;

public class ClientNetworkedUnitInteractionController : MonoBehaviour, INetworkInitialized
{
    public MuzzleFlash bigFlash;
    public MuzzleFlash smallFlash;
    public AudioClip fireSound;


    public int NetID { get; private set; }

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        Subscribe();
    }

    private void OnUnitAction(IPacketData packet)
    {
        PacketUnitAction data = (PacketUnitAction)packet;
        if (data.unitID != NetID) return;

        if (data.action == UnitActionType.Fire_BigGun)
            Fire_BigGun();

        if (data.action == UnitActionType.Fire_SmallGun)
            Fire_SmallGun();
    }

    private void Fire_BigGun()
    {
        if (bigFlash == null) return;

        bigFlash.Trigger();
        SFX.PlayAt(fireSound, bigFlash.transform.position, 250f, 0.1f, 1f);
    }

    private void Fire_SmallGun()
    {
        if (smallFlash == null) return;

        smallFlash.Trigger();
        SFX.PlayAt(fireSound, bigFlash.transform.position, 250f, 0.1f, 1f);
    }

    private void Subscribe()
    {
        ClientPacketHandler.SubscribeTo((int)ServerPacket.UnitAction, OnUnitAction);
    }

}

public enum UnitActionType
{
    Fire_SmallGun = 0,
    Fire_BigGun,

}