// Copyright Elliot Bentine, 2018-
#if (UNITY_EDITOR) 

using UnityEditor;
using UnityEngine;

namespace ProPixelizer.Tools
{
    [CustomEditor(typeof(Palette))]
    public class PaletteEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Palette p = (Palette)target;

            EditorGUILayout.LabelField("ProPixelizer | Palette Template", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(SHORT_HELP, MessageType.Info);
            EditorGUILayout.LabelField("");

            serializedObject.Update();

            EditorGUILayout.LabelField("Source texture", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The source texture provides the collection of colors which will be included in the final palette.", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Source"));
            if (p.Source != null) {
                if (!p.Source.isReadable)
                {
                    EditorGUILayout.HelpBox("Source texture not marked as readable - please mark it as readable if you wish to use it for a palette. You can make the texture readable in the Texture Import settings.", MessageType.Warning);
                }
                else
                {
                    var palette = new Palette.IndexedColorPalette(p.Source);
                    EditorGUILayout.LabelField("Number of colors: " + palette.UniqueColorCount);
                    if (palette.UniqueColorCount > 65535)
                        EditorGUILayout.HelpBox("Large number of unique colors, palette generation may be slow.", MessageType.Warning);
                }
            }
            EditorGUILayout.LabelField("");

            EditorGUILayout.LabelField("Color reduction algorithm", UnityEditor.EditorStyles.boldLabel);
            var methodProp = serializedObject.FindProperty("Method");
            EditorGUILayout.PropertyField(methodProp);
            if (p.Method == Palette.ColorMethod.HSV_Weighted || p.Method == Palette.ColorMethod.HSV_Weighted_SquareDistance)
            {
                var hWeight = EditorGUILayout.FloatField("Hue weight", p.Weights.x);
                var sWeight = EditorGUILayout.FloatField("Saturation weight", p.Weights.y);
                var vWeight = EditorGUILayout.FloatField("Value weight", p.Weights.z);
                var weightsProp = serializedObject.FindProperty("Weights");
                weightsProp.vector3Value = new UnityEngine.Vector3(hWeight, sWeight, vWeight);
            }
            if (p.Method == Palette.ColorMethod.V_Nearest)
            {
                var curve = EditorGUILayout.CurveField("Value mapping", p.VConversionCurve, Color.green, new Rect(0f, 0f, 1f, 1f));
                p.VConversionCurve = curve;
            }
            EditorGUILayout.HelpBox(p.DescribeColorMethod(), MessageType.Info);
            EditorGUILayout.LabelField("");

            EditorGUILayout.LabelField("Dithering", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseDitherPattern"));
            if (p.UseDitherPattern)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DitherPattern"));
            EditorGUILayout.LabelField("");

            EditorGUILayout.LabelField("Output", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoGenerateFileName"));
            if (!p.AutoGenerateFileName)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedFileName"));
            EditorGUILayout.LabelField("");

            if (GUILayout.Button("Generate"))
            {
                p.Generate();
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("");
            EditorGUILayout.HelpBox("This tool is new and in beta - I'm very interested to hear your thoughts and feedback!", MessageType.Info);
        }

        public const string SHORT_HELP = "Palette templates are used to specify color grading and dither options for look-up-tables(LUTs) used in ProPixelizer.\n\nFor more information, see the ProPixelizer User Guide.";
    }
}

#endif