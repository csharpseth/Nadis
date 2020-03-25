using UnityEngine;
using System.Collections;

public class ProcDebug : MonoBehaviour
{
    public MeshRenderer mr;

    public void New(int size, float[,] heightMap, TerraformData data) 
    {
        Texture2D tex = new Texture2D(size, size);
        float[,] terraform = TerraformGenerator.Generate(size, heightMap, data);


        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if(terraform[x,y] > 0f)
                    tex.SetPixel(x, y, Color.white);
                else
                    tex.SetPixel(x, y, Color.black);
            }
        }

        tex.Apply();
        mr.sharedMaterial.SetTexture("_MainTexture", tex);

    }

    public static Texture2D FalloffMaskFromHeightMap(float[,] map, float threshold, float blend)
    {
        int size = map.GetLength(0);

        Texture2D tex = new Texture2D(size, size);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float val = Mathf.Clamp01(map[x, y]);
                float distFromThreshold = (threshold - val);
                float percent = Mathf.Clamp01(distFromThreshold / blend);
                val = percent;

                if (map[x, y] > threshold)
                    val = 0f;

                Color c = new Color(val, val, val, 1f);

                tex.SetPixel(x, y, c);

            }
        }

        tex.Apply();

        return tex;

    }

}
