#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProPixelizer
{
    public class ProPixelizerBaseGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            materialEditor.serializedObject.Update();

            EditorGUILayout.LabelField("ProPixelizer | ProPixelizer Base", EditorStyles.boldLabel);
            if (GUILayout.Button("User Guide")) Application.OpenURL("https://sites.google.com/view/propixelizer/user-guide");
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Please use the ProPixelizer PixelizedWithOutline material instead.", MessageType.Error);
            EditorGUILayout.HelpBox(
                "This shader is generated from the ProPixelizerBase ShaderGraph and provides " +
                "the 'Universal Forward', 'Shadowcaster' and 'Depth' passes used by the " +
                "PixelizedWithOutline shader. The PixelizedWithOutline shader also adds other " +
                "passes required to support ProPixelizer. If you wish to render pixelated objects " +
                "without outlines, you should still use the PixelizedWithOutline shader and set " +
                "the outline alpha to zero.",
                MessageType.None);
        }
    }
}
#endif