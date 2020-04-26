namespace Nadis.Net
{
    public enum DataRequestType
    {
        IslandPosition = 1,
    }

    public enum ServerPackets
    {
        SWelcomeMSG = 1,
        SPlayerPos,
        SPlayerRot,
        SPlayerConnected,
        SPlayerDisconnected,
        SConnectionSuccessful,
        SPlayerMoveData,
        SInventoryEvent,
        SSpawnItem,
        SDestroyItem,
        SItemEvent,
        SPlayerSetHandPosition,
        SPlayerEndCurrentHandPosition,
        SPlayerStatEvent,
        SDataRequest
    }

    public enum ClientPackets
    {
        CPing = 1,
        CPlayerPos,
        CPlayerRot,
        CPlayerMoveData,
        CRequestSpawnItem,
        CRequestDestroyItem,
        CItemEvent,
        CInventoryEvent,
        CPlayerSetHandTarget,
        CPlayerEndCurrentHandTarget,
        CPlayerStatsEvent,
        CPlayerSpawnedSuccessfully,
        CSendServerRequestedData
    }
}

//Player
public enum PlayerMoveState
{
    None = 1,
    Walking,
    Running,
    Crouching,
    CrouchWalking
}

//Tween
public enum TweenType
{
    FromTo,
    DirForDuration,

}
public enum Space
{
    Local,
    World
}
public enum Direction
{
    None,
    Forward,
    Reverse,
    Up,
    Down,
    Left,
    Right
}

//Decorations
public enum LogicType
{
    None,
    Tree,
    Ore
}

//Entities
public enum WeaponFireType
{
    Single = 1,
    Semi,
    Full
}

//Procedural Animator
public enum Side
{
    Right = 1,
    Left = 2,
    Both = 3
}
public enum AnimatorTarget
{
    None,
    head,
    chest,
    pelvis,
    Hands
}

