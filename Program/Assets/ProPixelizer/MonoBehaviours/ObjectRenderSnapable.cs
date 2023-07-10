// Copyright Elliot Bentine, 2018-
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectRenderSnapable : MonoBehaviour
{ 
    /// <summary>
    /// Local position of the snapable before snapping.
    /// </summary>
    private Vector3 LocalPositionPreSnap;

    /// <summary>
    /// Local rotation of the object before snapping.
    /// </summary>
    private Quaternion LocalRotationPreSnap;

    /// <summary>
    /// World-space rotation of the object before snapping.
    /// </summary>
    private Quaternion WorldRotationPreSnap;

    /// <summary>
    /// World position of the snapable before snapping.
    /// </summary>
    public Vector3 WorldPositionPreSnap { get; private set; }

    /// <summary>
    /// Depth of the given transform, in the heirachy. Used for snap ordering.
    /// </summary>
    public int TransformDepth { get; private set; }

    [Tooltip("Should position be snapped?")]
    [FormerlySerializedAs("ShouldSnapPosition")]
    public bool SnapPosition = true;

    [Tooltip("Should Euler rotation angles be snapped?")]
    [FormerlySerializedAs("shouldSnapAngles")]
    public bool SnapEulerAngles = true;

    [Tooltip("Strategy that should be used for snapping rotation angles.")]
    public eSnapAngleStrategy SnapAngleStrategy;

    public enum eSnapAngleStrategy
    {
        WorldSpaceRotation,
        CameraSpaceY
    }

    /// <summary>
    /// Should angles be snapped?
    /// </summary>
    public bool ShouldSnapAngles() => SnapEulerAngles;

    [Tooltip("Resolution to which angles should be snapped")]
    public float angleResolution = 30f;

    /// <summary>
    /// Resolution (degrees) to which euler angles are snapped.
    /// </summary>
    public float AngleResolution() => angleResolution;

    public float OffsetBias => 0.5f;

    // Should we use a pixel grid aligned to the root entity's position? 
    [FormerlySerializedAs("UseRootPixelGrid")]
    [Tooltip("When true, the pixels of this object are snapped into alignment with another transform.")]
    public bool AlignPixelGrid = false;

    /// <summary>
    /// Transform to use as a reference for the pixel grid alignment.
    /// 
    /// If empty, defaults to root.
    /// </summary>
    [Tooltip("The transform to align this object's pixels to when 'Align Pixel Grid' is true. If empty, the root transform is used.")]
    public Transform PixelGridReference;

    private Renderer _renderer;

    public Vector3 PixelGridReferencePosition { get; private set; }

    public void SaveTransform()
    {
        LocalPositionPreSnap = transform.localPosition;
        WorldPositionPreSnap = transform.position;
        LocalRotationPreSnap = transform.localRotation;
        WorldRotationPreSnap = transform.rotation;
        if (PixelGridReference != null)
            PixelGridReferencePosition = PixelGridReference.position;
        else 
            PixelGridReferencePosition = transform.root.position;
    }

    /// <summary>
    /// Restore a previously saved transform.
    /// </summary>
    public void RestoreTransform()
    {
        transform.localPosition = LocalPositionPreSnap;
        transform.localRotation = LocalRotationPreSnap;
    }

    private int _pixelSize = 3;

    public void Start()
    {
        //Determine depth of the given behaviour's transform
        int depth = 0;
        Transform iter = transform;
        while (iter.parent != null && depth < 100)
        {
            depth++;
            iter = iter.parent;
        }
        TransformDepth = depth;

        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
            return;

        for (int i = 0; i < _renderer.materials.Length; i++)
        {
            if (_renderer.materials[i].HasProperty("_PixelGridOrigin"))
            {
                // Instance the propixelizer material.
                _renderer.materials[i] = new Material(_renderer.materials[i]);
                _renderer.materials[i].EnableKeyword("USE_OBJECT_POSITION_ON");
                _renderer.materials[i].EnableKeyword("USE_OBJECT_POSITION");
                _pixelSize = Mathf.RoundToInt(_renderer.materials[i].GetFloat("_PixelSize"));
            }
        }
    }

    public int GetPixelSize()
    {
        return _pixelSize;
    }

    /// <summary>
    /// Snap euler angles to specified AngleResolution.
    /// </summary>
    public void SnapAngles(Camera camera)
    {
        if (!ShouldSnapAngles())
            return;
        Vector3 angles = WorldRotationPreSnap.eulerAngles;
        var resolution = AngleResolution();
        switch (SnapAngleStrategy)
        {
            case eSnapAngleStrategy.WorldSpaceRotation:
                {
                    Vector3 snapped = new Vector3(
                        Mathf.Round(angles.x / resolution) * resolution,
                        Mathf.Round(angles.y / resolution) * resolution,
                        Mathf.Round(angles.z / resolution) * resolution);
                    transform.eulerAngles = snapped;
                    break;
                }
            case eSnapAngleStrategy.CameraSpaceY:
                {
                    float cameraY = camera.transform.eulerAngles.y;
                    //snap in relative angle space with respect to camera
                    angles.y -= cameraY;
                    Vector3 snapped = new Vector3(
                        Mathf.Round(angles.x / resolution) * resolution,
                        Mathf.Round(angles.y / resolution) * resolution,
                        Mathf.Round(angles.z / resolution) * resolution
                        );
                    snapped.y += cameraY;
                    transform.eulerAngles = snapped;
                    break;
                }
        }

    }
}