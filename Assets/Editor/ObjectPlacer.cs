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
    private string ignoreTag;
    public GameObject prefab;
    int maxPlaced = 500;
    float placementChance;

    Vector3 minScale;
    Vector3 maxScale;

    float minHeight = 5f;
    float maxHeight = 100f;

    float minAngle = -1f;
    float maxAngle = 1f;

    float positionOffset = 0.5f;

    bool alignToNormal = true;

    private List<GameObject> placed;

    private string layerName = "_PlacedObjects";
    private Transform layerTransform;


    [MenuItem("Tools/Object Placer")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ObjectPlacer window = (ObjectPlacer)GetWindow(typeof(ObjectPlacer));
        window.Show();
    }

    private void OnGUI()
    {
        layerName = EditorGUILayout.TextField("Layer Name", layerName);
        region = EditorGUILayout.Vector3IntField("Region", region);
        hitMask = EditorGUILayout.LayerField("Mask", hitMask);
        ignoreTag = EditorGUILayout.TagField("Ignore Tag", ignoreTag);
        EditorGUILayout.Space(10f);
        stepSize = EditorGUILayout.Slider("Step Size", stepSize, 0.1f, 5f);
        destroyPreviouslyPlaced = EditorGUILayout.Toggle("Destroy Previously Placed Prefabs", destroyPreviouslyPlaced);
        maxPlaced = EditorGUILayout.IntField("Maximum To Place", maxPlaced);

        minScale = EditorGUILayout.Vector3Field("Min Scale", minScale);
        maxScale = EditorGUILayout.Vector3Field("Max Scale", maxScale);
        SideBySide("Height(min, max)", ref minHeight, ref maxHeight);

        alignToNormal = EditorGUILayout.Toggle("Align To Normal", alignToNormal);
        EditorGUILayout.LabelField("Min Angle: " + minAngle + "  Max Angle: " + maxAngle);
        EditorGUILayout.MinMaxSlider(ref minAngle, ref maxAngle, -1f, 1f);

        positionOffset = EditorGUILayout.Slider("Position Offset", positionOffset, 0f, 5f);


        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), null);

        placementChance = EditorGUILayout.Slider("Chance Of Placement", placementChance, 0.01f, 1f);
        if (GUILayout.Button("Place"))
            Generate();

        if (GUILayout.Button("Clear Referenced"))
            Clear();

        if (GUILayout.Button("Delete Referenced"))
            DeleteReferenced();
    }

    private void SideBySide(string label, ref float a, ref float b)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        a = EditorGUILayout.FloatField(a);
        b = EditorGUILayout.FloatField(b);
        EditorGUILayout.EndHorizontal();
    }

    private void Generate()
    {
        if (prefab == null) return;

        if(destroyPreviouslyPlaced)
            DeleteReferenced();

        if (placed == null)
            placed = new List<GameObject>();

        if (layerTransform == null)
        {
            GameObject temp = GameObject.Find(layerName);
            if(temp == null)
            {
                temp = new GameObject(layerName);
            }
            layerTransform = temp.transform;
        }

        Transform container = new GameObject(prefab.name + "_" + placed.Count).transform;
        container.SetParent(layerTransform);

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

            if (hits[i].transform.tag == ignoreTag) continue;

            if (hits[i].point.y < minHeight || hits[i].point.y > maxHeight) continue;

            if (hits[i].transform.gameObject == prefab) continue;

            if (Vector3.Dot(Vector3.up, hits[i].normal) > maxAngle || Vector3.Dot(Vector3.up, hits[i].normal) < minAngle) continue;

            float sample = UnityEngine.Random.value;
            if (sample > placementChance) continue;


            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.position = hits[i].point;
            go.transform.SetParent(container);
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
        newScale.x *= UnityEngine.Random.Range(minScale.x, maxScale.x);
        newScale.y *= UnityEngine.Random.Range(minScale.y, maxScale.y);
        newScale.z *= UnityEngine.Random.Range(minScale.z, maxScale.z);

        t.localScale = newScale;

        Vector3 pos = t.position;
        pos.x += UnityEngine.Random.Range(-positionOffset, positionOffset);
        pos.z += UnityEngine.Random.Range(-positionOffset, positionOffset);
        t.position = pos;


        Vector3 newRot = t.eulerAngles;
        newRot += (t.up * UnityEngine.Random.Range(-180f, 180f));
        t.eulerAngles = newRot;

    }

    private void DeleteReferenced()
    {
        if (placed == null || placed.Count == 0) return;

        for (int i = 0; i < placed.Count; i++)
        {
            DestroyImmediate(placed[i]);
        }
        Clear();
    }

    private void Clear()
    {
        placed = new List<GameObject>();
        layerTransform = null;
        Debug.Log("Cleared Object Placer's Referenced Objects");
    }

    private bool AngleTooSteep(Vector3 hitNormal, Vector3 hitPoint)
    {
        float angle = Vector3.Angle(hitPoint, hitPoint + hitNormal);
        return angle > maxAngle;
    }


}
