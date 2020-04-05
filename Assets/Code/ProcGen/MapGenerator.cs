using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;

public class MapGenerator : MonoBehaviour
{
    public MapData mapData;
    public MapDecoratorData spawnPointsData;
    public bool useTerraform = true;
    public TerraformData terraformData;
    public ProcDebug procDebug;
    public bool debug = true;

    public Terrain terrain;
    public float beachHeight = 0.5f;
    public float beachBlend = 0.2f;
    public bool applyDecorations = false;
    public MapDecoratorData[] decorationLayers;
    public bool networked = true;

    float[,] points;

    private MapDecorator mapDecorator;

    private void Awake()
    {
        mapDecorator = GetComponent<MapDecorator>();
        if (networked == false)
            Generate(Random.Range(0, int.MaxValue));
    }

    public void Generate(int seed = -1)
    {
        if (seed == -1)
            seed = Random.Range(0, int.MaxValue);
        int size = terrain.terrainData.heightmapResolution;
        float[,] heightMap = mapData.Generate(size, seed);
        points = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float val = Random.value;
                if(val <= 0.05f && heightMap[x,y] > beachHeight + 0.1f)
                {
                    points[x, y] = heightMap[x,y];
                }
            }
        }
        
        terrain.terrainData.SetHeights(0, 0, heightMap);
        
        if(applyDecorations && mapDecorator != null)
        {
            mapDecorator.Decorate(decorationLayers, seed, transform);
        }else
        {
            Debug.LogFormat("No Decorations Were Applied -- Decorate:{0}   Component:{1}", applyDecorations, mapDecorator);
        }


        if (mapDecorator != null && networked == true)
            mapDecorator.Generate(spawnPointsData.minHeight, spawnPointsData.maxHeight, spawnPointsData.density, spawnPointsData.maximumPoints, spawnPointsData.maxNormalAngle, spawnPointsData.maxPrefabAngle, seed, null, spawnPointsData.name);
    }

    private void OnDrawGizmos()
    {
        if (points == null) return;

        int size = terrain.terrainData.heightmapResolution;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if(points[x, y] > 0f)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(new Vector3(x * 2, points[x, y], y * 2), 10f);
                }
            }
        }
    }

}
