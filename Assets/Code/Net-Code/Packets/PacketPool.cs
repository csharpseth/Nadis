using UnityEngine;
using UnityEngine.PlayerLoop;
using System.Collections;
using Nadis.Net;
using System.Collections.Generic;

public static class PacketPool
{
    private static Queue<PacketBuffer> bufferQueue;
    private static int Size = 500;


    [RuntimeInitializeOnLoadMethod]
    public static void Initialize()
    {
        bufferQueue = new Queue<PacketBuffer>();
        for (int i = 0; i < Size; i++)
        {
            bufferQueue.Enqueue(new PacketBuffer());
        }
    }

    public static PacketBuffer GetBuffer()
    {
        PacketBuffer buffer = bufferQueue.Dequeue().Reset(true);
        bufferQueue.Enqueue(buffer);
        return buffer;
    }

}
