using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LibNoise.Generator;

public class FalloffGenerator
{
    public static float[,] Generate(int size, AnimationCurve curve)
    {
        float[,] map = new float[size, size];
        int center = size / 2;
        Perlin p = new Perlin(0.5f, 0.5f, 1.8f, 2, 100, LibNoise.QualityMode.Low);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float sqrDist = (new Vector2(x, y) - new Vector2(center, center)).sqrMagnitude;
                float noiseVal = (float)p.GetValue(x / 30f, 1f, y / 30f) * 0.05f;
                float percent = (sqrDist / (center * center)) + noiseVal;

                float val = curve.Evaluate(percent);

                map[x, y] = val;
            }
        }

        return map;
    }
}
