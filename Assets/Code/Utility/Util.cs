using LibNoise.Generator;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Util
{
    private static Perlin constPerlin = new Perlin(1f, 0.5f, 1.8f, 1, System.DateTime.Now.Millisecond, LibNoise.QualityMode.Medium);
    private static Perlin perlin = new Perlin(1f, 0.5f, 1.8f, 1, System.DateTime.Now.Millisecond, LibNoise.QualityMode.Medium);

    public static float Perlin
    {
        get
        {
            return (float)constPerlin.GetValue(Random.Range(float.MinValue, float.MaxValue), Random.Range(float.MinValue, float.MaxValue), Random.Range(float.MinValue, float.MaxValue));
        }
    }
    public static float GetPerlin(float frequency)
    {
        perlin.Frequency = frequency;
        return (float)perlin.GetValue(Random.Range(0f, 100f), Random.Range(100f, 200f), Random.Range(200f, 300f));
    }
    public static float GetPerlin(float frequency, Vector3 pos)
    {
        perlin.Frequency = frequency;
        return (float)perlin.GetValue(pos);
    }

}
