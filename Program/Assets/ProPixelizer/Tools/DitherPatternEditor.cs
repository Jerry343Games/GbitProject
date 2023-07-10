// Copyright Elliot Bentine, 2018-
#if (UNITY_EDITOR) 

using UnityEditor;
using UnityEngine;

namespace ProPixelizer.Tools
{
    [CustomEditor(typeof(DitherPattern))]
    public class DitherPatternEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DitherPattern p = (DitherPattern)target;

            EditorGUILayout.LabelField("ProPixelizer | Dither Pattern", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(SHORT_HELP, MessageType.Info);
            EditorGUILayout.LabelField("");

            serializedObject.Update();

            EditorGUILayout.LabelField("Pattern", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Define the dither pattern by specifying numbers in the range 0 to 16. These determine the thresholds for the transition between the two nearest colors.", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            var m00 = EditorGUILayout.FloatField((p.m00 * 17));
            var m01 = EditorGUILayout.FloatField((p.m01 * 17));
            var m02 = EditorGUILayout.FloatField((p.m02 * 17));
            var m03 = EditorGUILayout.FloatField((p.m03 * 17));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var m10 = EditorGUILayout.FloatField((p.m10 * 17));
            var m11 = EditorGUILayout.FloatField((p.m11 * 17));
            var m12 = EditorGUILayout.FloatField((p.m12 * 17));
            var m13 = EditorGUILayout.FloatField((p.m13 * 17));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var m20 = EditorGUILayout.FloatField((p.m20 * 17));
            var m21 = EditorGUILayout.FloatField((p.m21 * 17));
            var m22 = EditorGUILayout.FloatField((p.m22 * 17));
            var m23 = EditorGUILayout.FloatField((p.m23 * 17));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var m30 = EditorGUILayout.FloatField((p.m30 * 17));
            var m31 = EditorGUILayout.FloatField((p.m31 * 17));
            var m32 = EditorGUILayout.FloatField((p.m32 * 17));
            var m33 = EditorGUILayout.FloatField((p.m33 * 17));
            EditorGUILayout.EndHorizontal();

            serializedObject.FindProperty("m00").floatValue = Coerce(m00);
            serializedObject.FindProperty("m01").floatValue = Coerce(m01);
            serializedObject.FindProperty("m02").floatValue = Coerce(m02);
            serializedObject.FindProperty("m03").floatValue = Coerce(m03);
            serializedObject.FindProperty("m10").floatValue = Coerce(m10);
            serializedObject.FindProperty("m11").floatValue = Coerce(m11);
            serializedObject.FindProperty("m12").floatValue = Coerce(m12);
            serializedObject.FindProperty("m13").floatValue = Coerce(m13);
            serializedObject.FindProperty("m20").floatValue = Coerce(m20);
            serializedObject.FindProperty("m21").floatValue = Coerce(m21);
            serializedObject.FindProperty("m22").floatValue = Coerce(m22);
            serializedObject.FindProperty("m23").floatValue = Coerce(m23);
            serializedObject.FindProperty("m30").floatValue = Coerce(m30);
            serializedObject.FindProperty("m31").floatValue = Coerce(m31);
            serializedObject.FindProperty("m32").floatValue = Coerce(m32);
            serializedObject.FindProperty("m33").floatValue = Coerce(m33);

            serializedObject.ApplyModifiedProperties();

            // Create a preview image.
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Preview Image");
            int previewSize = 256;
            Texture2D preview = new Texture2D(previewSize, previewSize, TextureFormat.ARGB32, false, true);

            Color[] colors = new Color[previewSize * previewSize];
            for (int i = 0; i< colors.Length; i++)
            {
                //do x, y
                //linear gradient at 45
                int x = i / previewSize;
                int y = i % previewSize;
                var fx = (float)x / previewSize;
                var fy = (float)y / previewSize;
                var grad = (fx + fy) / 2f;
                colors[i] = p.PickColor(grad, p.GetOrder(x, y)) ? Color.black : Color.white;
            }

            preview.SetPixels(colors);
            preview.Apply();

            
            var previewRect = EditorGUILayout.GetControlRect(false, previewSize, GUILayout.Width(previewSize));
            EditorGUI.DrawPreviewTexture(previewRect, preview);

            EditorGUILayout.LabelField("");
            EditorGUILayout.HelpBox("This tool is new and in beta - I'm very interested to hear your thoughts and feedback!", MessageType.Info);

        }

        public const string SHORT_HELP = "A dither pattern samples two colors to create the illusion of a color gradient.";

        public enum PreviewSize { Small, Big };

        public float Coerce(float threshold) => Mathf.Clamp(threshold, 0f, 16f) / 17;
    }
}

#endif