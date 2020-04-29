//Starts at 1
/// <summary>
/// These are packets that both the SERVER & CLIENT share, and can be sent
/// or received from/to either one or both.
/// IDs start at 1.
/// </summary>
public enum SharedPacket
{
    PlayerPosition = 1,
    PlayerRotation,
    PlayerAnimatorData,
}


//Starts at 250
/// <summary>
/// These are packets that are sent from the SERVER ---> CLIENT
/// IDs start at 250.
/// </summary>
public enum ServerPacket
{
    PlayerConnection = 250,

}

//Starts at 500
/// <summary>
/// These are packets that are sent from the CLIENT ---> SERVER
/// IDs start at 500.
/// </summary>
public enum ClientPacket
{

}