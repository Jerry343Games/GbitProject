// Copyright Elliot Bentine, 2018-
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// This monobehaviour sets the object IDs that are used when determining outlines.
/// </summary>
public class OutlineControl : MonoBehaviour
{
    [Tooltip("ID to use when drawing the outlines of this object. Outlines occur when two adjacent pixels have different IDs")]
    public byte ID;

    float FloatUID { get { return ID % 255f; } }

    [Tooltip("If true, randomly generate a random outline ID for this object at runtime.")]
    public bool UseRandomUID = true;

    [Tooltip("Should this object use the outline ID of the root transform?")]
    public bool UseRootUID;

    [Tooltip("Should the color of this object's outline copy that of the root transform?")]
    public bool UseRootColor = true;

    [Tooltip("The color to use for the outline.")]
    public Color Color;

    List<int> OutlineMaterialIndices;

    private void Awake()
    {
        OutlineMaterialIndices = new List<int>();
        Renderer mesh = GetComponent<Renderer>();
        if (mesh != null)
        {
            for (int i=0; i < mesh.materials.Length; i++)
            {
                if (mesh.materials[i].HasProperty("_OutlineColor"))
                {
                    mesh.materials[i] = new Material(mesh.materials[i]); //create instance
                    OutlineMaterialIndices.Add(i); //needed b/c other things might create a new instance of material
                }
            }
        }
        if (UseRandomUID)
            ID = (byte)Random.Range(0, 255);
    }

    void Start()
    {
        var root = transform.root.gameObject;
        var rootObjectUID = root.GetComponent<OutlineControl>();
        if (rootObjectUID != null)
        {
            if (UseRootUID)
                ID = rootObjectUID.ID;
            if (UseRootColor)
                Color = rootObjectUID.Color;
        }
        else
        {
            if (UseRootColor || UseRootUID)
                Debug.LogWarning("Root ID or Color was requested for the outline, but the root transform doesn't have an OutlineControl MonoBehaviour! Please add the component.");
        }

        var mesh = GetComponent<Renderer>();
        if (mesh != null)
        {
            var vector = new Vector4(Color.r, Color.g, Color.b, FloatUID);
            foreach (int i in OutlineMaterialIndices) {
                mesh.materials[i].SetFloat("_ID", FloatUID);
                mesh.materials[i].SetColor("_OutlineColor", Color);
            }
        }
    }
}
