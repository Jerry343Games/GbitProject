// Copyright Elliot Bentine, 2018-
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Camera behaviour that snaps the positions of all RenderSnapables before rendering and releases them afterwards.
/// </summary>
[
    RequireComponent(typeof(Camera))
    ]
public class CameraSnapSRP : MonoBehaviour
{
    IEnumerable<ObjectRenderSnapable> _snapables;
    Camera _camera;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        if (!_camera.orthographic)
        {
            Debug.LogWarning("Camera snap is designed to prevent pixel creep in orthographic projection. It is not possible to fix creep using perspective projection, as object pixel size can change.");
            return;
        }
    }

    public enum PixelSizeMode
    {
        FixedWorldSpacePixelSize,
        UseCameraSize
    }

    /// <summary>
    /// Mode to use this camera snap SRP in.
    /// </summary>
    public PixelSizeMode Mode;

    /// <summary>
    /// Size of a pixel in world units
    /// </summary>
    public float PixelSize = 0.032f;

    public void Update()
    {
        if (Mode == PixelSizeMode.FixedWorldSpacePixelSize)
            _camera.orthographicSize = _camera.scaledPixelHeight / 2f * PixelSize;
        else
            PixelSize = _camera.scaledPixelHeight / 2f * _camera.orthographicSize;
    }

    public void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;
    }

    public void Unsubscribe()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= EndCameraRendering;
    }

    public void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == _camera)
            Snap();
    }

    public void EndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == _camera)
            Release();
    }

    public void Snap()
    {
        if (_camera == null)
        {
            // This happens if the camera object gets deleted - but RPM still exists.
            Unsubscribe();
            return;
        }

        var pipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (pipelineAsset == null)
            pipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        if (pipelineAsset == null)
            throw new System.Exception("Render pipeline asset in QualitySettings and GraphicsSettings are either null or not a UniversalRenderPipelineAsset.");
        float renderScale = pipelineAsset.renderScale;

        //Find all objects required to snap and release.
        _snapables = new List<ObjectRenderSnapable>(FindObjectsOfType<ObjectRenderSnapable>());

        //Sort snapables so that parents are snapped before their children.
        ((List<ObjectRenderSnapable>)_snapables).Sort((comp1, comp2) => comp1.TransformDepth.CompareTo(comp2.TransformDepth));

        foreach (ObjectRenderSnapable snapable in _snapables)
            snapable.SaveTransform();

        foreach (ObjectRenderSnapable snapable in _snapables)
            snapable.SnapAngles(_camera);

        //Determine the size of a square screen pixel in world units.
        float pixelLength = (2f * _camera.orthographicSize) / (_camera.pixelHeight * renderScale);

        _camera.ResetWorldToCameraMatrix();
        Matrix4x4 camToWorld = _camera.cameraToWorldMatrix;
        Matrix4x4 worldToCam = _camera.worldToCameraMatrix;

        // Snap all objects to integer pixels in camera space.
        foreach (ObjectRenderSnapable snapable in _snapables)
        {
            Vector3 snappedPosition;
            if (snapable.AlignPixelGrid)
            {
                var rootCamSpace = worldToCam.MultiplyPoint(snapable.PixelGridReferencePosition);
                var snapableCamSpace = worldToCam.MultiplyPoint(snapable.WorldPositionPreSnap);
                var delta = (snapableCamSpace - rootCamSpace);
                var snapLength = snapable.GetPixelSize() * pixelLength;
                var roundedDelta = new Vector3(
                    Mathf.RoundToInt(delta.x / snapLength) + snapable.OffsetBias,
                    Mathf.RoundToInt(delta.y / snapLength) + snapable.OffsetBias,
                    Mathf.RoundToInt(delta.z / snapLength) + snapable.OffsetBias
                    );
                var roundedRootCamSpace = new Vector3(
                    Mathf.RoundToInt(rootCamSpace.x / pixelLength),
                    Mathf.RoundToInt(rootCamSpace.y / pixelLength),
                    Mathf.RoundToInt(rootCamSpace.z / pixelLength)
                    );
                snappedPosition = camToWorld.MultiplyPoint(pixelLength * roundedRootCamSpace + roundedDelta * snapLength);
            }
            else
            {
                Vector3 pixelPosCamSpace = worldToCam.MultiplyPoint(snapable.transform.position) / pixelLength;
                var roundedPos = new Vector3(
                    Mathf.RoundToInt(pixelPosCamSpace.x) + snapable.OffsetBias,
                    Mathf.RoundToInt(pixelPosCamSpace.y) + snapable.OffsetBias,
                    Mathf.RoundToInt(pixelPosCamSpace.z) + snapable.OffsetBias
                    );
                snappedPosition = camToWorld.MultiplyPoint(roundedPos * pixelLength);
            }
            

            if (snapable.SnapPosition)
                snapable.transform.position = snappedPosition;
        }        
    }

    public void Release()
    {
        // Note: the `release' loop is run in reverse.
        // This prevents shaking and movement from occuring when
        // parent and child transforms are both Snapable.
        //  e.g. For a heirachy A>B:
        //   * Snap A, then B.
        //   * Unsnap B, then A.
        // Doing Unsnap A, then unsnap B will produce jerking.
        foreach (ObjectRenderSnapable snapable in _snapables.Reverse())
            snapable.RestoreTransform();
    }
}