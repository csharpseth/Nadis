public interface IPacketData
{
    int PacketID { get; }

    Nadis.Net.PacketBuffer Serialize();
    void Deserialize(Nadis.Net.PacketBuffer buffer);
}