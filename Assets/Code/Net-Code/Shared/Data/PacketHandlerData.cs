namespace Nadis.Net
{
    public class PacketHandlerData
    {
        public delegate void ReceiveCallback(IPacketData packet);
        private ReceiveCallback callback;
        private IPacketData packet;

        public PacketHandlerData(IPacketData packet, ReceiveCallback callback = null)
        {
            this.packet = packet;
            this.callback = callback;
        }

        public void Subscribe(ReceiveCallback callback)
        {
            this.callback += callback;
        }

        public void UnSubscribe(ReceiveCallback callback)
        {
            this.callback -= callback;
        }

        public void Invoke(PacketBuffer buffer)
        {
            packet.Deserialize(buffer);

            callback?.Invoke(packet);
            buffer.Dispose();
        }

        /*
        public void Invoke(Nadis.Net.JobPacketBuffer buffer)
        {
            packet.JobDeserialize(buffer);

            callback?.Invoke(packet);
            buffer.Dispose();
        }
        */
    }
}