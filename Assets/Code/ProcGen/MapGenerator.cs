using UnityEngine;

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
            Generate();
    }

    public void Generate(int seed = -1)
    {
        Debug.Log("START :: Generating Map...");
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
        }

        GenerateSpawnPoints(points, terrain.terrainData, 0.1f, 0.2f, 0.3f, seed);
        
        Debug.Log("FINSIH :: Map Generated");
    }

    public void GenerateSpawnPoints(float[,] heightMap, TerrainData terrain, float chance, float minHeight, float maxHeight, int seed, int maxPoints = 1)
    {
        float min = 500f;
        float max = -500f;

        int pointCount = 0;

        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                if (pointCount >= maxPoints) return;

                if (heightMap[x, y] > max) max = heightMap[x, y];
                if (heightMap[x, y] < min) min = heightMap[x, y];

                if (heightMap[x, y] <= maxHeight && heightMap[x, y] >= minHeight)
                {
                    float val = Random.value;
                    if (val <= chance)
                    {
                        RaycastHit hit;
                        Vector3 pos = new Vector3(x, terrain.size.y + 25f, y);
                        if (Physics.Raycast(pos, Vector3.down, out hit))
                        {
                            pos.y = hit.point.y;
                            NetworkManager.ins.RegisterSpawnPoint(pos);
                            pointCount++;
                        }

                        
                    }
                }
            }
        }

        Debug.LogFormat("Min:{0}  Max:{1}  ActualMaxHeight:{2}", min, max, terrain.size.y);
    }
    

}
