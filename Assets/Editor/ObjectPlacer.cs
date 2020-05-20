using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public class ObjectPlacer : EditorWindow
{
    Vector3Int region = Vector3Int.one * 100;
    float stepSize = 1f;
    bool destroyPreviouslyPlaced;
    LayerMask hitMask;
    public GameObject prefab;
    int maxPlaced = 500;
    float placementChance;

    float minScale = 0.2f;
    float maxScale = 1f;
    bool randomOnIndependentAxis = false;

    float minHeight = 5f;
    float maxHeight = 100f;

    float minAngle = -1f;
    float maxAngle = 1f;

    bool alignToNormal = true;

    private List<GameObject> placed;


    [MenuItem("Tools/Object Placer")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ObjectPlacer window = (ObjectPlacer)GetWindow(typeof(ObjectPlacer));
        window.Show();
    }

    private void OnGUI()
    {
        region = EditorGUILayout.Vector3IntField("Region", region);
        hitMask = EditorGUILayout.LayerField("Mask", hitMask);
        EditorGUILayout.Space(10f);
        stepSize = EditorGUILayout.Slider("Step Size", stepSize, 0.1f, 5f);
        destroyPreviouslyPlaced = EditorGUILayout.Toggle("Destroy Previously Placed Prefabs", destroyPreviouslyPlaced);
        maxPlaced = EditorGUILayout.IntField("Maximum To Place", maxPlaced);

        EditorGUILayout.LabelField("Min Scale: " + minScale + "  Max Scale: " + maxScale);
        EditorGUILayout.MinMaxSlider("Scale", ref minScale, ref maxScale, 0.1f, 5f);
        randomOnIndependentAxis = EditorGUILayout.Toggle("Scale On Independent Axis", randomOnIndependentAxis);


        EditorGUILayout.LabelField("Min Height: " + minHeight + "  Max Height: " + maxHeight);
        EditorGUILayout.MinMaxSlider("Height", ref minHeight, ref maxHeight, 0f, region.y);

        alignToNormal = EditorGUILayout.Toggle("Align To Normal", alignToNormal);
        EditorGUILayout.LabelField("Min Angle: " + minAngle + "  Max Angle: " + maxAngle);
        EditorGUILayout.MinMaxSlider(ref minAngle, ref maxAngle, -1f, 1f);

        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), null);

        placementChance = EditorGUILayout.Slider("Chance Of Placement", placementChance, 0.01f, 1f);
        if (GUILayout.Button("Place"))
            Generate();

        if (GUILayout.Button("Clear Referenced"))
            Clear();

        if (GUILayout.Button("Delete Referenced"))
            DeleteReferenced();
    }

    private void Generate()
    {
        if (prefab == null) return;

        if(destroyPreviouslyPlaced)
            DeleteReferenced();

        if (placed == null)
            placed = new List<GameObject>();

        Transform container = new GameObject(prefab.name + "_" + placed.Count).transform;

        int xCount = (int)math.round(region.x / stepSize);
        int yCount = (int)math.round(region.z / stepSize);
        int totalSize = xCount * yCount;

        NativeArray<RaycastCommand> cmds = new NativeArray<RaycastCommand>(totalSize, Allocator.TempJob);
        NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(totalSize, Allocator.TempJob);

        float halfRegionX = region.x / 2f;
        float halfRegionY = region.z / 2f;

        int index = 0;
        Vector3 origin = Vector3.up * 100f;
        Vector3 dir = Vector3.down;

        for (float x = -halfRegionX; x < halfRegionX; x += stepSize)
        {
            for (float y = -halfRegionY; y < halfRegionY; y += stepSize)
            {
                if (index >= totalSize) break;

                origin.x = x;
                origin.z = y;

                cmds[index] = new RaycastCommand(origin, dir, 200f);

                index++;
            }
        }

        JobHandle handle = RaycastCommand.ScheduleBatch(cmds, hits, 5);
        handle.Complete();

        int numPlaced = 0;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == null) continue;

            if (hits[i].point.y < minHeight || hits[i].point.y > maxHeight) continue;

            if (hits[i].transform.gameObject == prefab) continue;

            if (Vector3.Dot(Vector3.up, hits[i].normal) > maxAngle || Vector3.Dot(Vector3.up, hits[i].normal) < minAngle) continue;

            float sample = UnityEngine.Random.value;
            if (sample > placementChance) continue;

            GameObject go = Instantiate(prefab, hits[i].point, Quaternion.identity, container);
            if(alignToNormal)
            {
                go.transform.rotation = Quaternion.FromToRotation(Vector3.up, hits[i].normal);
            }

            Randomize(go.transform);

            numPlaced++;
            if (numPlaced > maxPlaced) break;

        }


        placed.Add(container.gameObject);

        cmds.Dispose();
        hits.Dispose();
    }

    private void Randomize(Transform t)
    {
        Vector3 newScale = Vector3.one;
        if (randomOnIndependentAxis)
        {
            newScale.x *= UnityEngine.Random.Range(minScale, maxScale);
            newScale.y *= UnityEngine.Random.Range(minScale, maxScale);
            newScale.z *= UnityEngine.Random.Range(minScale, maxScale);
        }
        else
        {
            newScale *= UnityEngine.Random.Range(minScale, maxScale);
        }

        t.localScale = newScale;



        //Vector3 newRot = t.eulerAngles;
        //newRot += (t.up * UnityEngine.Random.Range(-180f, 180f));
        //t.eulerAngles = newRot;

    }

    private void DeleteReferenced()
    {
        if (placed == null || placed.Count == 0) return;

        for (int i = 0; i < placed.Count; i++)
        {
            DestroyImmediate(placed[i]);
        }

        placed = new List<GameObject>();
    }

    private void Clear()
    {
        placed = new List<GameObject>();
        Debug.Log("Cleared Object Placer's Referenced Objects");
    }

    private bool AngleTooSteep(Vector3 hitNormal, Vector3 hitPoint)
    {
        float angle = Vector3.Angle(hitPoint, hitPoint + hitNormal);
        return angle > maxAngle;
    }


}
