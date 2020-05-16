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

}
