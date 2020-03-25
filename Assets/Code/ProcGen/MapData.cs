using LibNoise;
using LibNoise.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map Data", menuName = "Procedural Data/Map Data")]
public class MapData : ScriptableObject
{
    public bool useFalloffMap = true;
    public AnimationCurve falloffCurve;
    public float finalAmplitude = 0.5f;
    public NoiseProfile[] noiseProfiles;

    public float[,] Generate(int size, int seed)
    {
        float[,] falloff = new float[size, size];
        if (useFalloffMap)
            falloff = FalloffGenerator.Generate(size, falloffCurve);

        for (int i = 0; i < noiseProfiles.Length; i++)
        {
            noiseProfiles[i].Generate(size, seed);
        }

        float[,] map = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int i = 0; i < noiseProfiles.Length; i++)
                {
                    BlendMode blend = noiseProfiles[i].blendMode;
                    if(blend == BlendMode.Add)
                    {
                        map[x, y] += noiseProfiles[i].Sample(x, y);
                    }
                }
            }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int i = 0; i < noiseProfiles.Length; i++)
                {
                    BlendMode blend = noiseProfiles[i].blendMode;
                    if (blend == BlendMode.Subtract)
                    {
                        map[x, y] -= noiseProfiles[i].Sample(x, y);
                    }
                }
            }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int i = 0; i < noiseProfiles.Length; i++)
                {
                    BlendMode blend = noiseProfiles[i].blendMode;
                    if (blend == BlendMode.Multiply)
                    {
                        map[x, y] *= noiseProfiles[i].Sample(x, y);
                    }else if(blend == BlendMode.Divide)
                    {
                        map[x, y] /= noiseProfiles[i].Sample(x, y);
                    }
                }


                if (useFalloffMap)
                    map[x, y] *= falloff[x, y];
            }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int i = 0; i < noiseProfiles.Length; i++)
                {
                    BlendMode blend = noiseProfiles[i].blendMode;
                    if (blend == BlendMode.AddAtEnd)
                    {
                        map[x, y] += noiseProfiles[i].Sample(x, y);
                    }
                }

                map[x, y] *= finalAmplitude;
            }
        }

        return map;
    }

}

[System.Serializable]
public class NoiseProfile
{
    public bool active = true;
    public NoiseType noiseType;
    public BlendMode blendMode;
    public float a_noiseScale = 50f;
    public float a_amplitude = 10f;
    public float a_frequency = 1.3f;
    public float lacunarity = 1.8f;
    public float persistence = 0.5f;
    public float displacement = 1f;
    public int octaves = 6;
    public QualityMode quality;
    public float[,] heightMap;

    public float Sample(int x, int y)
    {
        if (active == false)
            return 0f;

        return heightMap[x, y];
    }

    public void Generate(int size, int seed)
    {
        if (active == false)
            return;

        if (noiseType == NoiseType.Perlin)
            heightMap = GeneratePerlin(size, seed);
        else if (noiseType == NoiseType.RidgedMultifractal)
            heightMap = GenerateRidged(size, seed);
        else if (noiseType == NoiseType.Voronoi)
            heightMap = GenerateVoronoi(size, seed);
        else if (noiseType == NoiseType.Constant)
            heightMap = GenerateConstant(size);

    }

    private float[,] GeneratePerlin(int size, int seed)
    {
        Perlin perlin = new Perlin(a_frequency, lacunarity, persistence, octaves, seed, quality);
        float[,] map = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                map[x, y] = Mathf.Abs((float)perlin.GetValue(x/a_noiseScale, 0, y/a_noiseScale)) / 10f * a_amplitude;
            }
        }

        return map;
    }

    private float[,] GenerateRidged(int size, int seed)
    {
        RidgedMultifractal ridged = new RidgedMultifractal(a_frequency, lacunarity, octaves, seed, quality);
        float[,] map = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                map[x, y] = Mathf.Abs((float)ridged.GetValue(x / a_noiseScale, 0, y / a_noiseScale)) / 100f * a_amplitude;
            }
        }

        return map;
    }

    private float[,] GenerateVoronoi(int size, int seed)
    {
        Voronoi ridged = new Voronoi(a_frequency, displacement, seed, true);
        float[,] map = new float[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                map[x, y] = Mathf.Abs((float)ridged.GetValue(x / a_noiseScale, 0, y / a_noiseScale)) / 10f * a_amplitude;
            }
        }

        return map;
    }

    private float[,] GenerateConstant(int size)
    {
        float[,] map = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                map[x, y] = a_amplitude;
            }
        }

        return map;
    }



}

public enum NoiseType
{
    Perlin,
    RidgedMultifractal,
    Voronoi,
    Constant
}

public enum BlendMode
{
    Add,
    Subtract,
    Multiply,
    AddAtEnd,
    Divide
}
