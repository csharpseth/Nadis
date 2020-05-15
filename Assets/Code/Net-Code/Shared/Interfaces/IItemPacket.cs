namespace Nadis.Net
{
    public interface IItemPacket : IPacketData
    {
        int NetworkID { get; }
        int ItemID { get; }
    }
}