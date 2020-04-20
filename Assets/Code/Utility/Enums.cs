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

public enum LogicType
{
    None,
    Tree,
    Ore
}
