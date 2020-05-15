namespace Nadis.Net
{
    public enum PlayerAnimatorEventType
    {
        SetFloat = 1,
        SetBool,
        SetTrigger
    }

    public struct PacketPlayerAnimatorEvent : IPacketData
    {
        public int PacketID => (int)SharedPacket.PlayerAnimatorEventData;

        public int playerID;
        public string id;
        public PlayerAnimatorEventType eventType;
        public float fValue;
        public bool bValue;


        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            id = buffer.ReadString();
            eventType = (PlayerAnimatorEventType)buffer.ReadInt();
            if(eventType == PlayerAnimatorEventType.SetTrigger)
                return;

            if (eventType == PlayerAnimatorEventType.SetBool)
                bValue = buffer.ReadBool();
            else if (eventType == PlayerAnimatorEventType.SetFloat)
                fValue = buffer.ReadFloat();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer
            buffer.Write(playerID);
            buffer.Write(id);
            buffer.Write((int)eventType);
            if(eventType != PlayerAnimatorEventType.SetTrigger)
            {
                if (eventType == PlayerAnimatorEventType.SetBool)
                    buffer.Write(bValue);
                else if (eventType == PlayerAnimatorEventType.SetFloat)
                    buffer.Write(fValue);
            }


            buffer.WriteLength();
            return buffer;
        }
    }
}