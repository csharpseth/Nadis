using UnityEngine;
using System.Collections;

public class NetworkedPlayer : MonoBehaviour, INetworkInitialized
{
    public int NetID { get; private set; }

    public void InitFromNetwork(int playerID)
    {
        NetID = playerID;
    }
}
