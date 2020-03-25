using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour
{
    public Vector3 offset;
    public int xSize;
    public int ySize;
    public float prefabScale;

    public GameObject prefab;

    public bool generateOnAwake = true;

    private void Awake()
    {
        if (generateOnAwake)
            Generate();
    }

    private void Generate()
    {
        int halfX = xSize / 2;
        int halfY = ySize / 2;
        Transform t = new GameObject("Water").transform;
        t.parent = transform;

        for (int x = -halfX; x < halfX; x++)
        {
            for (int y = -halfY; y < halfY; y++)
            {
                Vector3 pos = offset + new Vector3((xSize * x), 0f, (ySize * y));
                Instantiate(prefab, pos, Quaternion.identity, t);
            }
        }
    }

}
