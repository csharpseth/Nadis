using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshCombiner : EditorWindow
{
    public const int MaxVertsPerMesh = 65535;
    Transform target;
    bool destroyChildren = false;
    bool removeMeshDataWhenDone = true;

    [MenuItem("Tools/Mesh Combiner")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MeshCombiner window = (MeshCombiner)GetWindow(typeof(MeshCombiner));
        window.Show();
    }

    private void OnGUI()
    {
        target = Selection.activeTransform;
        EditorGUILayout.LabelField("Target: " + target);
        destroyChildren = EditorGUILayout.Toggle("Destroy Children", destroyChildren);
        if(destroyChildren == false)
            removeMeshDataWhenDone = EditorGUILayout.Toggle("Remove Child Mesh", removeMeshDataWhenDone);


        if (GUILayout.Button("Combine Children"))
            CreateSingleMesh();
    }

    private void CreateSingleMesh()
    {
        MeshFilter[] meshFilters = target.GetComponentsInChildren<MeshFilter>();
        Material mat = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        int numBatches = Mathf.RoundToInt((float)(meshFilters[0].sharedMesh.vertices.Length * meshFilters.Length) / (float)MaxVertsPerMesh);
        int numObjectsPerBatch = meshFilters.Length / numBatches;

        int batchIndex = 0;
        int offset = 0;
        string batchName = meshFilters[0].name;

        while (batchIndex < numBatches)
        {
            GameObject batch = new GameObject("Batch_" + batchIndex);
            MeshFilter mf = batch.AddComponent<MeshFilter>();
            MeshRenderer mr = batch.AddComponent<MeshRenderer>();

            mr.sharedMaterial = mat;

            Mesh m = new Mesh();
            m.name = batchName + "_Batch_" + batchIndex;
            CombineInstance[] combine = new CombineInstance[numObjectsPerBatch];

            for (int i = 0; i < numObjectsPerBatch; i++)
            {
                int mfIndex = offset + i;
                combine[i].mesh = meshFilters[mfIndex].sharedMesh;
                combine[i].transform = meshFilters[mfIndex].transform.localToWorldMatrix;
                if(destroyChildren)
                {
                    DestroyImmediate(meshFilters[mfIndex].gameObject);
                }else if(removeMeshDataWhenDone)
                {
                    MeshFilter tempMf = meshFilters[mfIndex];
                    MeshRenderer tempMr = mf.GetComponent<MeshRenderer>();
                    if (mf != null)
                        DestroyImmediate(tempMf);
                    if(mr != null)
                        DestroyImmediate(tempMr);
                }

            }
            offset += numObjectsPerBatch;

            m.CombineMeshes(combine);
            mf.sharedMesh = m;


            batchIndex++;
        }

    }

}
