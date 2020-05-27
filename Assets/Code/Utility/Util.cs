using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class Util
{
    public static int FastAbs(int value)
    {
        int sign = (value >= 0) ? 1 : -1;
        return value * sign;
    }

    public static float FastAbs(float value)
    {
        float sign = (value >= 0) ? 1f : -1f;
        return value * sign;
    }

    public static float FastMax(float a, float b)
    {
        return (a > b) ? a : b;
    }

    public static float CustomDistanceScore(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
    {
        float diffX = FastAbs(a.x - b.x);
        float diffZ = FastAbs(a.z - b.z);
        return FastMax(diffX, diffZ);
    }

    public static float FastAndRoughDistance(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
    {
        float diffX = FastAbs(a.x - b.x);
        float diffZ = FastAbs(a.z - b.z);
        return (diffX + diffZ) / 2f;
    }

    public static int EnsureNegative(int value)
    {
        return -FastAbs(value);
    }

    public static Vector3 NormalizeToLayer(Vector3 point, LayerMask layer)
    {
        Vector3 origin = point + (Vector3.up * 500f);
        RaycastHit[] hits = new RaycastHit[1];
        Vector3 normalized = point;
        if(Physics.RaycastNonAlloc(origin, Vector3.down, hits, 1000f, layer) > 0)
        {
            normalized = hits[0].point;
        }
        return normalized;
    }

    public static Vector3 RandomInsideSphereNormalized(Vector3 origin, float radius, LayerMask normalizedLayer)
    {
        Vector3 pt = UnityEngine.Random.insideUnitSphere * radius;
        pt = pt + origin;
        pt = NormalizeToLayer(pt, normalizedLayer);
        return pt;
    }

    public static float RollValue(int seed = -1)
    {
        int actualSeed = (seed == -1) ? (int)Time.realtimeSinceStartup : seed;
        Unity.Mathematics.Random r = new Unity.Mathematics.Random((uint)actualSeed);

        return r.NextFloat(0f, 1f);
    }

    public static float RollValue(float min, float max, int seed = -1)
    {
        float diff = (max - min);

        return min + (RollValue(seed) * diff);
    }

    public static Vector3[] GetNormalizedGridPoints(float areaWidth, float areaLength, float stepSize, float pointChance, int maxPoints, LayerMask mask)
    {
        float halfWidth = areaWidth / 2f;
        float halfLength = areaLength / 2f;

        int size = (int)math.round((areaWidth / stepSize) * (areaLength / stepSize));

        NativeArray<RaycastCommand> cmds = new NativeArray<RaycastCommand>(size, Allocator.TempJob);
        NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(size, Allocator.TempJob);

        Vector3 origin = new Vector3(-halfWidth, 500f, -halfLength);
        Vector3 dir = Vector3.down;
        int index = 0;
        for (float x = -halfWidth; x < halfWidth; x += stepSize)
        {
            for (float y = -halfLength; y < halfLength; y += stepSize)
            {
                origin.x = x;
                origin.z = y;

                cmds[index] = new RaycastCommand(origin, dir, 1000f, mask, 1);

                index++;
            }
        }


        JobHandle handle = RaycastCommand.ScheduleBatch(cmds, hits, 4);
        handle.Complete();

        NativeList<Vector3> points = new NativeList<Vector3>(Allocator.Temp);

        for (int i = 0; i < hits.Length; i++)
        {
            float roll = RollValue();
            if(roll <= pointChance)
            {
                points.Add(hits[i].point);

                if (points.Length >= maxPoints)
                    break;
            }
        }

        Vector3[] array = points.ToArray();
        points.Dispose();

        return array;
    }
    static public string LongToIP(long longIP)
    {
        string ip = string.Empty;
        for (int i = 0; i< 4; i++)
        {
            int num = (int)(longIP / Mathf.Pow(256, (3 - i)));
            longIP = longIP - (long) (num* Mathf.Pow(256, (3 - i)));

            if (i == 0)
                ip = num.ToString();
            else
                ip  = ip + "." + num.ToString();
        }
        return ip;

    }

    public static long IP2Long(string ip)
    {
        string[] ipBytes;
        double num = 0;
        if(!string.IsNullOrEmpty(ip))
        {
            ipBytes = ip.Split('.');
            for (int i = ipBytes.Length - 1; i >= 0; i--)
            {
                num += ((int.Parse(ipBytes[i]) % 256) * Mathf.Pow(256, (3 - i)));
            }
        }
        return (long) num;
    }
}
