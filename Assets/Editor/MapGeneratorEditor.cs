using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MapGenerator mg = (MapGenerator)target;

        if(mg != null)
        {
            if(GUILayout.Button("Generate"))
            {
                mg.Generate();
            }
        }

    }
}
