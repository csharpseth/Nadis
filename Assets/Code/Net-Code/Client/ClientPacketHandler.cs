using System.Collections.Generic;

namespace Nadis.Net.Client
{
    public static class ClientPacketHandler
    {
        private static Dictionary<int, PacketHandlerData> handlers;

        public static void Initialize()
        {
            handlers = new Dictionary<int, PacketHandlerData>();
            PopulateHandlers();
        }
        public static void Handle(int packetID, PacketBuffer buffer)
        {
            if(handlers.ContainsKey(packetID) == false)
            {
                UnityEngine.Debug.LogErrorFormat("Failed to Handle Packet With ID of '{0}', No Handler Exists.", packetID);
                return;
            }
            
            handlers[packetID].Invoke(buffer);
        }
        /*
        public static void Handle(int packetID, JobPacketBuffer buffer)
        {
            if (handlers.ContainsKey(packetID) == false)
            {
                UnityEngine.Debug.LogErrorFormat("Failed to Handle Packet With ID of '{0}', No Handler Exists.", packetID);
                return;
            }

            handlers[packetID].Invoke(buffer);
        }
        */
        public static void SubscribeTo(int packetID, PacketHandlerData.ReceiveCallback callback)
        {
            if (handlers.ContainsKey(packetID) == false) return;

            handlers[packetID].Subscribe(callback);
        }
        public static void UnSubscribeFrom(int packetID, PacketHandlerData.ReceiveCallback callback)
        {
            if (handlers.ContainsKey(packetID) == false) return;

            handlers[packetID].UnSubscribe(callback);
        }

        //The Created Handlers should only utilize SharedPacketID & ServerPacketID
        //You should never need to use ClientPacketID here.
        private static void PopulateHandlers()
        {
            CreateHandler((int)ServerPacket.WelcomeMessage, new PacketWelcomeMessage(), (IPacketData data) => {
                PacketWelcomeMessage msg = (PacketWelcomeMessage)data;
                PlayerPopulatorSystem.CreatePlayer(msg.clientID);
            });
            CreateHandler((int)SharedPacket.PlayerTransform, new PacketPlayerTransform());
        }

        private static void CreateHandler(int packetID, IPacketData packetType,
            PacketHandlerData.ReceiveCallback callback = null)
        {
            PacketHandlerData handler = new PacketHandlerData(packetType, callback);
            handlers.Add(packetID, handler);
        }


    }
}
