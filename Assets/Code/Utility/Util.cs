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

}
