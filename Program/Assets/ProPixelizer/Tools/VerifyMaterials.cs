// Copyright Elliot Bentine, 2018-
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;


#if (UNITY_EDITOR)
using ProPixelizer.Tools.Migration;

/// <summary>
/// A tool for verifying that materials are correctly serialized.
/// </summary>
public class VerifyMaterials : EditorWindow
{
    [MenuItem("Window/ProPixelizer/Update and Verify Materials")]
    public static void ShowWindow()
    {
        GetWindow(typeof(VerifyMaterials));
    }
    
    void OnGUI()
    {
        //GUILayout.Label("TextureIndexer", EditorStyles.largeLabel);
        EditorGUILayout.LabelField("ProPixelizer | Update and Verify Materials", EditorStyles.boldLabel);
        if (GUILayout.Button("User Guide")) Application.OpenURL("https://sites.google.com/view/propixelizer/user-guide");
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This tool checks ProPixelizer materials in the project to make sure they are correctly serialized. It fixes any broken keywords and updates material properties to the most recent version of ProPixelizer.", MessageType.Info);
        EditorGUILayout.HelpBox("This tool will check all materials in your project, please be patient for larger projects. It is strongly recommended to backup your files, e.g. using version control.", MessageType.Warning);
        EditorGUILayout.Space();
        if (GUILayout.Button("Check materials", EditorStyles.miniButton))
        {
            VerifyShaders();
        }
    }

    void VerifyShaders()
    {
        var AShader = Shader.Find("ProPixelizer/SRP/ProPixelizerBase");
        var APOShader = Shader.Find("ProPixelizer/SRP/PixelizedWithOutline");

        // Update appearance materials to appearance+outline.
        int appearanceMaterialCount = 0;
        string[] allMaterials = AssetDatabase.FindAssets("t:Material");
        foreach (string materialID in allMaterials)
        {
            var path = AssetDatabase.GUIDToAssetPath(materialID);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material.shader != AShader)
                continue;

            appearanceMaterialCount++;
            material.shader = APOShader;
            material.SetColor("_OutlineColor", new Color(0f, 0f, 0f, 0f));
            material.SetColor("_EdgeHighlightColor", new Color(0f, 0f, 0f, 0f));
        }
        Debug.Log(string.Format("ProPixelizer found {0} materials using the ProPixelizerBase shader - these have been upgraded to the appearance and outline material.", appearanceMaterialCount));

        // Check and perform upgrades for all Appearance+Outline materials.
        int APOMaterialCount = 0;
        int APOMaterialUpdatedCount = 0;
        var updater = new ProPixelizer1_8MaterialUpdater();
        foreach (string materialID in allMaterials)
        {
            var path = AssetDatabase.GUIDToAssetPath(materialID);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path) as Material;
            if (material.shader != APOShader)
                continue;

            APOMaterialCount++;
            var serializedObject = new SerializedObject(material);
            if (updater.CheckForUpdate(serializedObject)) {
                updater.DoUpdate(serializedObject);
                APOMaterialUpdatedCount++;
            }
        }
        Debug.Log(string.Format("ProPixelizer found {0} materials using the Appearance+Outline shader - {1} of these upgraded to newest version.", APOMaterialCount, APOMaterialUpdatedCount));
    }
}

#endif