using LibNoise.Generator;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MapDecorator : MonoBehaviour
{
    public const int MaxVerts = 65535;
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

    private void Awake()
    {
        Events.MapGenerator.GetPosition = GetPosition;
    }

    public void Decorate(MapDecoratorData[] datas, float[,] heightMap, int seed, Transform parent)
    {
        List<GameObject> gos = new List<GameObject>();
        GeneratePoints(heightMap, 0.15f, 90f, seed);

        for (int i = 0; i < datas.Length; i++)
        {
            if(datas[i].enabled)
            {
                gos.Add(Decorate(datas[i], seed, i, parent));
            }
        }
    }

    public GameObject Decorate(MapDecoratorData data, int seed, int index, Transform parent)
    {
        GameObject g = Generate(data, seed, index);
        if(g != null)
            g.transform.SetParent(parent);
        return g;
    }

    public GameObject Generate(MapDecoratorData data, int seed, int ind)
    {
        if (spawn == false) return null;

        Perlin p = new Perlin(0.75f, 0.5f, 1.8f, 2, seed, LibNoise.QualityMode.Medium);
        List<SpawnPoint> processedPoints = new List<SpawnPoint>();

        float minH = (minHeight * this.maxHeight);
        float maxH = (maxHeight * this.maxHeight);

        for (int i = 0; i < points.Count; i++)
        {
            float h = points[i].pos.y;
            if (h <= maxH && h >= minH)
            {
                processedPoints.Add(points[i]);
            }
        }
        
        int pointCount = Mathf.RoundToInt((float)processedPoints.Count * data.density);
        
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

        for (int i = 0; i < pointCount; i++)
        {
            System.Random r = new System.Random(seed + i * (ind + 1));
            int index = r.Next(0, processedPoints.Count - 1);
            float val = (float)p.GetValue(processedPoints[i].pos.x, 1, processedPoints[i].pos.y) + 1f / 2f;
            val = Mathf.Clamp01(val * 2f);

            if (spawnPoints.Count < data.maximumPoints)
            {
                spawnPoints.Add(processedPoints[index]);
                processedPoints.RemoveAt(index);
            }else
            {
                break;
            }

        }
        
        if(data.prefabs != null)
        {
            GameObject t = Instantiate(data.prefabs[0]);
            int verts = t.GetComponentInChildren<MeshFilter>().mesh.vertices.Length * data.maximumPoints;
            Destroy(t);
            int meshCount = RoundUpToInt((float)verts / MaxVerts);
            int laps = spawnPoints.Count / meshCount;
            int totalSpawned = 0;

            Transform godParent = new GameObject(data.name).transform;
            
            for (int y = 0; y < meshCount; y++)
            {
                GameObject parent = new GameObject(data.name + "_" + y);
                Mesh FinalMesh = new Mesh();
                MeshFilter[] filter = new MeshFilter[laps];
                CombineInstance[] combine = new CombineInstance[laps];
                Material mat = null;

                for (int i = 0; i < laps; i++)
                {
                    filter[i] = Instantiate(data.prefabs[0], spawnPoints[0].pos, Quaternion.Euler(spawnPoints[0].rot), parent.transform).GetComponentInChildren<MeshFilter>();
                    totalSpawned++;
                    spawnPoints.RemoveAt(0);

                    filter[i].transform.localScale = RandomVector(data.minScale, data.maxScale, seed, i, i * 10);
                    if (mat == null)
                        mat = filter[i].GetComponentInChildren<MeshRenderer>().sharedMaterial;
                }

                for (int i = 0; i < filter.Length; i++)
                {
                    combine[i].mesh = filter[i].mesh;
                    combine[i].transform = filter[i].transform.localToWorldMatrix;
                    Destroy(filter[i].gameObject);
                }
                FinalMesh.CombineMeshes(combine, true, true);
                parent.AddComponent<MeshFilter>().mesh = FinalMesh;
                parent.AddComponent<MeshRenderer>().sharedMaterial = mat;
                parent.transform.SetParent(godParent);
            }

            
            Debug.Log(totalSpawned);

            return godParent.gameObject;
        }
        
        return null;

    }

    private Vector3 RandomVector(Vector3 min, Vector3 max, int seed, int x, int y)
    {
        System.Random r = new System.Random(seed + x);
        float newX = min.x + ((max.x - min.x) * (float)r.NextDouble());
        r = new System.Random(seed + x + y);
        float newY = min.y + ((max.y - min.y) * (float)r.NextDouble());
        r = new System.Random(seed + x + y);
        float newZ = min.z + ((max.z - min.z) * (float)r.NextDouble());

        return new Vector3(newX, newY, newZ);
    }

    private int RoundUpToInt(float value)
    {
        int val = Mathf.RoundToInt(value);
        float other = value - val;
        if (other > 0f)
            return val + 1;

        return val;
    }

    public void GeneratePoints(float[,] heightMap, float minH, float maxNormalAngle, int seed)
    {
        points = new List<SpawnPoint>();
        int size = heightMap.GetLength(0);
        System.Random r = new System.Random(seed);
        for (float x = 0; x < size; x += stepSize)
        {
            for (float y = 0; y < size; y += stepSize)
            {
                int iX = Mathf.Clamp(Mathf.RoundToInt(x), 0, size - 1);
                int iY = Mathf.Clamp(Mathf.RoundToInt(y), 0, size - 1);

                if (heightMap[iX, iY] >= minH)
                {
                    r = new System.Random(seed + iX);
                    float xOff = (float)(r.Next(-1, 1) * r.NextDouble()) * offset;
                    r = new System.Random(seed + iY);
                    float yOff = (float)(r.Next(-1, 1) * r.NextDouble()) * offset;

                    RaycastHit hit;
                    Vector3 origin = new Vector3(x + xOff, 500f, y + yOff);

                    if (Physics.Raycast(origin, Vector3.down, out hit))
                    {
                        Vector3 ang = hit.normal * -90f;
                        if (Mathf.Abs(ang.x) <= maxNormalAngle && Mathf.Abs(ang.z) <= maxNormalAngle)
                        {
                            r = new System.Random(seed);
                            ang.y = (360f * (float)r.NextDouble());
                            points.Add(new SpawnPoint(hit.point, ang));

                            if (hit.point.y > maxHeight)
                                maxHeight = hit.point.y;
                            if (hit.point.y < minHeight)
                                minHeight = hit.point.y;
                        }
                    }
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

    public Vector3 GetPosition(float minH, float maxH, float maxAng)
    {
        List<Vector3> pts = new List<Vector3>();
        int size = Mathf.Clamp(100, 0, points.Count);
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < size; i++)
        {
            int index = UnityEngine.Random.Range(0, size);
            Vector3 p = points[index].pos;
            Vector3 r = points[index].rot;
            if (p.y >= minH && p.y <= maxH && Mathf.Abs(r.x) <= maxAng && Mathf.Abs(r.z) <= maxAng)
            {
                pos = p;
                break;
            }
        }

        return pos;
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
    public Vector3 minScale;
    public Vector3 maxScale;
    public int maximumPoints;

    public Vector3 prefabOffset;

    public GameObject[] prefabs;

}
