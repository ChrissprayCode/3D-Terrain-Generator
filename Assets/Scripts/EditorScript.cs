using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(TerrainController))]

public class EditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainController TC = (TerrainController)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            TC.GenerateTerrain();
        }

        if (GUILayout.Button("Destroy Trees"))
        {
            TC.DestroyTrees();
        }

    }
}
