using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LibNoise.Generator;

public class FalloffGenerator
{
    public static float[,] Generate(int size, int seed, AnimationCurve curve)
    {
        float[,] map = new float[size, size];
        int center = size / 2;
        Perlin p = new Perlin(0.2f, 0.5f, 1.8f, 1, seed, LibNoise.QualityMode.Medium);
        float multiplier = 25f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float noiseVal = (float)p.GetValue(x / 10f, 1f, y / 10f) * 0.05f;
                float sampleX = x + (noiseVal * multiplier);
                float sampleY = y + (noiseVal * multiplier);

                float sqrDist = (new Vector2(sampleX, sampleY) - new Vector2(center, center)).sqrMagnitude;
                
                float percent = (sqrDist / (center * center));

                float val = curve.Evaluate(percent);

                map[x, y] = val;
            }
        }

        return map;
    }

    public static float[,] GenerateSquare(int size, int seed, AnimationCurve curve)
    {
        float[,] map = new float[size, size];
        float[,] circle = Generate(size, seed, curve);

        int halfSize = size / 2;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float val = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(1f - val);
                map[i, j] -= (1f - circle[i, j]);
                map[i, j] = Mathf.Clamp01(map[i, j]);
            }
        }

        return map;
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 0.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
