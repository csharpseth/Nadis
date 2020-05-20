using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshCombiner : EditorWindow
{
    public const int MaxVertsPerMesh = 65535;
    Transform target;

    [MenuItem("Tools/Mesh Combiner")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MeshCombiner window = (MeshCombiner)GetWindow(typeof(MeshCombiner));
        window.Show();
    }

    private void OnGUI()
    {
        target = (Transform)EditorGUILayout.ObjectField(target, typeof(Transform));
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
        while (batchIndex < numBatches)
        {
            GameObject batch = new GameObject("Batch_" + batchIndex);
            MeshFilter mf = batch.AddComponent<MeshFilter>();
            MeshRenderer mr = batch.AddComponent<MeshRenderer>();

            mr.sharedMaterial = mat;

            Mesh m = new Mesh();
            m.name = meshFilters[0].name + "_Batch_" + batchIndex;
            CombineInstance[] combine = new CombineInstance[numObjectsPerBatch];

            for (int i = 0; i < numObjectsPerBatch; i++)
            {
                int mfIndex = offset + i;
                combine[i].mesh = meshFilters[mfIndex].sharedMesh;
                combine[i].transform = meshFilters[mfIndex].transform.localToWorldMatrix;
                meshFilters[mfIndex].gameObject.SetActive(false);
            }
            offset += numObjectsPerBatch;

            m.CombineMeshes(combine);
            mf.sharedMesh = m;


            batchIndex++;
        }

    }

}
