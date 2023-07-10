// Copyright Elliot Bentine, 2018-
#if (UNITY_EDITOR)

using UnityEditor;
using UnityEngine;

namespace ProPixelizer.Tools
{
    [CustomEditor(typeof(CameraSnapSRP))]
    public class CameraSnapSRPEditor : Editor
    {
        public const string SHORT_HELP = "This component snaps the position of GameObjects with an ObjectRenderSnapable component before rendering to remove pixel creep.";

        public override void OnInspectorGUI()
        {
            CameraSnapSRP snap = (CameraSnapSRP)target;

            EditorGUILayout.LabelField("ProPixelizer | Camera Snap", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(SHORT_HELP, MessageType.Info);
            EditorGUILayout.LabelField("");

            if (!snap.gameObject.GetComponent<Camera>().orthographic)
                EditorGUILayout.HelpBox("The camera snap behaviour is intended for orthographic cameras. Pixel creep cannot be completely eradicted for perspective projections because the object size changes as it moves across the screen.", MessageType.Warning);

            serializedObject.Update();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Mode"));
            switch (snap.Mode)
            {
                case CameraSnapSRP.PixelSizeMode.FixedWorldSpacePixelSize:
                    EditorGUILayout.HelpBox("The camera will be resized so that the size of one pixel corresponds to the defined world-space units.", MessageType.Info);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PixelSize"));
                    break;
                case CameraSnapSRP.PixelSizeMode.UseCameraSize:
                    EditorGUILayout.HelpBox("The camera will use the size specified in the editor.", MessageType.Info);
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PixelSize"));
                    GUI.enabled = true;
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif