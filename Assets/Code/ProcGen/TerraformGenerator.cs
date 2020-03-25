using LibNoise.Generator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraformGenerator
{
    public static float[,] Generate(int size, float[,] heightMap, TerraformData data)
    {
        float[,] map = new float[size, size];
        List<Vector2> townPoints = new List<Vector2>();
        System.DateTime time = System.DateTime.Now;
        System.Random r = new System.Random((int)time.Ticks);

        float min = 500f;
        float max = -500f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                //map[x, y] = heightMap[x, y];
                if (heightMap[x, y] > max)
                    max = heightMap[x, y];
                if (heightMap[x, y] < min)
                    min = heightMap[x, y];
                //map[x, y] = heightMap[x, y];
                if (heightMap[x, y] >= data.min && heightMap[x, y] <= data.max && townPoints.Count < data.towns)
                {
                    time = System.DateTime.Now;
                    r = new System.Random((int)time.Ticks);


                    int draw = r.Next(0, 1000);
                    if(draw < data.chance)
                    {
                        townPoints.Add(new Vector2(x, y));
                        
                    }
                }
            }
        }

        Debug.Log(min + "  --  " + max);

        List<List<TerraformPoint>> Areas = new List<List<TerraformPoint>>(); 

        for (int i = 0; i < townPoints.Count; i++)
        {
            Areas.Add(new List<TerraformPoint>());
            time = System.DateTime.Now;
            r = new System.Random((int)time.Ticks);
            int radius = r.Next(data.minRadius, data.maxRadius);

            int startX = Mathf.RoundToInt(townPoints[i].x - radius);
            int endX = Mathf.RoundToInt(townPoints[i].x + radius);
            int startY = Mathf.RoundToInt(townPoints[i].y - radius);
            int endY = Mathf.RoundToInt(townPoints[i].y + radius);

            if (startX < 0) startX = 0;
            if (endX > size - 1) endX = size - 1;
            if (startY < 0) startY = 0;
            if (endY > size - 1) endY = size - 1;

            float totalHeight = 0f;

            for (int x = startX; x < endX; x++)
            {
                for (int y = startX; y < endX; y++)
                {
                    float dist = (townPoints[i] - new Vector2(x, y)).sqrMagnitude;
                    float percent = (dist / (radius * radius));
                    if(percent <= 1f)
                    {
                        Areas[i].Add(new TerraformPoint(x, y, percent));
                        totalHeight += heightMap[x, y];
                    }
                }
            }

            float avgHeight = (totalHeight / Areas[i].Count);

            for (int j = 0; j < Areas[i].Count; j++)
            {
                int x = Areas[i][j].x;
                int y = Areas[i][j].y;
                float percent = Areas[i][j].distancePercent;
                map[x, y] = percent * 10f;
            }

        }

        return map;

    }
}

[System.Serializable]
public struct TerraformData
{
    public int towns;
    public float min, max;
    public int chance;
    [Range(20, 100)]
    public int minRadius, maxRadius;
    public int minDistFromLastTown;
    public AnimationCurve areaCurve;
}

public struct TerraformPoint
{
    public int x;
    public int y;

    public float distancePercent;

    public TerraformPoint(int x, int y, float percent)
    {
        this.x = x;
        this.y = y;
        this.distancePercent = percent;
    }
}
