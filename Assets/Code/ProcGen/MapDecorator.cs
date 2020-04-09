﻿using LibNoise.Generator;
using System.Collections.Generic;
using UnityEngine;

public class MapDecorator : MonoBehaviour
{
    public Vector3Int regionSize;
    public Vector3 center { get { return regionSize / 2; } }
    public float stepSize = 0.5f;
    public float offset = 0.2f;
    private List<SpawnPoint> points;
    private float min, max;

    public float minHeight, maxHeight;
    public float threshold;
    public float noiseScale;
    [Range(0f, 1f)]
    public float chance = 0.2f;
    [Header("Debug:")]
    public int maxGizmos = 5000;
    public bool spawn = false;


    public List<GameObject> Decorate(MapDecoratorData[] datas, int seed, Transform parent)
    {
        List<GameObject> gos = new List<GameObject>();

        for (int i = 0; i < datas.Length; i++)
        {
            if(datas[i].enabled)
            {
                gos.Add(Decorate(datas[i], seed, parent));
            }
        }

        return gos;
    }

    public GameObject Decorate(MapDecoratorData data, int seed, Transform parent)
    {
        GameObject g = Generate(data.minHeight, data.maxHeight, data.density, data.maximumPoints, data.maxNormalAngle, data.maxPrefabAngle, seed, data.prefabs, data.prefabOffset, data.name);
        if(g != null)
            g.transform.SetParent(parent);
        return g;
    }

    public GameObject Generate(float minHeight, float maxHeight, float density, int maxPoints, float maxNormalAngle, float maxAngle, int seed = 0, GameObject[] prefabs = null, Vector3 prefabOffset = default(Vector3), string groupName = "")
    {
        if(points == null || points.Count == 0)
        {
            points = new List<SpawnPoint>();
            Perlin p = new Perlin(1f, 0.5f, 1.8f, 2, seed, LibNoise.QualityMode.Medium);

            for (float x = 0; x < regionSize.x; x += stepSize)
            {
                for (float y = 0; y < regionSize.z; y += stepSize)
                {

                    System.Random rX = new System.Random(seed + Mathf.RoundToInt(x));
                    System.Random rY = new System.Random(seed + Mathf.RoundToInt(y));
                    float offsetX = (float)rX.NextDouble() * offset;
                    float offsetY = (float)rY.NextDouble() * offset;
                    float pVal = (float)p.GetValue(x / noiseScale, 1f, y / noiseScale);

                    if (pVal < threshold) continue;

                    Vector3 origin = new Vector3(x + offsetX, regionSize.y, y + offsetY);
                    RaycastHit hit;
                    if (Physics.Raycast(origin, Vector3.down, out hit))
                    {
                        Vector3 normal = hit.normal * 90f;
                        if (Mathf.Abs(normal.x) <= maxNormalAngle && Mathf.Abs(normal.z) <= maxNormalAngle)
                        {
                            SpawnPoint point = new SpawnPoint(hit.point, ClampVector(normal, maxAngle));
                            points.Add(point);

                            if (point.pos.y > max)
                                max = point.pos.y;
                            if (point.pos.y < min)
                                min = point.pos.y;
                        }
                    }

                }
            }
        }

        if (spawn == false) return null;
        
        float minH = (max - min) * minHeight;
        float maxH = (max - min) * maxHeight;

        List<SpawnPoint> processedPoints = new List<SpawnPoint>();
        
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].pos.y >= minH && points[i].pos.y <= maxH)
            {
                processedPoints.Add(points[i]);
            }
        }
        
        int pointCount = Mathf.RoundToInt((float)processedPoints.Count * density);
        
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

        for (int i = 0; i < pointCount; i++)
        {
            System.Random r = new System.Random(seed + i);
            int index = r.Next(0, processedPoints.Count - 1);

            if(spawnPoints.Count < maxPoints)
            {
                spawnPoints.Add(processedPoints[index]);
                processedPoints.RemoveAt(index);
            }else
            {
                break;
            }

        }
        
        if(prefabs != null)
        {
            if (string.IsNullOrEmpty(groupName)) groupName = "No_Name";

            Transform parent = new GameObject(groupName).transform;
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                System.Random r = new System.Random(seed + i);
                float rRot = (float)r.NextDouble() * 180f;
                int rIndex = r.Next(0, prefabs.Length - 1);

                Vector3 rot = spawnPoints[i].rot;
                rot.x = Mathf.Clamp(rot.x, -maxAngle, maxAngle);
                rot.z = Mathf.Clamp(rot.z, -maxAngle, maxAngle);
                rot.y = rRot;

                Instantiate(prefabs[rIndex], spawnPoints[i].pos + prefabOffset, Quaternion.Euler(rot), parent);
            }
            
            return parent.gameObject;
        }
        
        return null;

    }

    public void GeneratePoints(TerrainData data, int size, int seed)
    {
        Perlin p = new Perlin(1f, 0.5f, 1.8f, 1, seed, LibNoise.QualityMode.Low);
        float[,] heightMap = data.GetHeights(0, 0, size, size);

        points = new List<SpawnPoint>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float perlinValue = (float)p.GetValue((float)x / noiseScale, 0, (float)y / noiseScale);
                float heightMapVal = heightMap[x, y];
                float val = Random.value;

                if (heightMapVal >= minHeight && heightMapVal <= maxHeight && perlinValue > threshold && val <= chance)
                {
                    Vector3 origin = new Vector3(x, 500f, y);
                    RaycastHit hit;
                    Physics.Raycast(origin, Vector3.down, out hit);
                    if (hit.transform != null) points.Add(new SpawnPoint(hit.point, hit.normal));
                }

            }
        }
    }

    public Vector3 ClampVector(Vector3 input, float maxAngle)
    {
        if (input.x > maxAngle)
            input.x = maxAngle;
        if (input.x < -maxAngle)
            input.x = -maxAngle;
        if (input.z > maxAngle)
            input.z = maxAngle;
        if (input.z < -maxAngle)
            input.z = -maxAngle;

        return input;
    }

    private void OnDrawGizmos()
    {
        Vector3 halfSize = regionSize / 2;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(halfSize, regionSize);

        if(points != null)
        {
            for (int i = 0; i < Mathf.Clamp(points.Count, 0, maxGizmos); i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(points[i].pos, 0.2f);
            }
        }
    }

}

public struct SpawnPoint
{
    public Vector3 pos;
    public Vector3 rot;

    public SpawnPoint(Vector3 pos, Vector3 rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
}

[System.Serializable]
public struct MapDecoratorData
{
    public string name;
    public bool enabled;
    [Range(0f, 1f)]
    public float minHeight;
    [Range(0f, 1f)]
    public float maxHeight;
    [Range(0.001f, 0.5f)]
    public float density;
    public float maxNormalAngle;
    public float maxPrefabAngle;
    public int maximumPoints;

    public Vector3 prefabOffset;

    public GameObject[] prefabs;

}
