// Copyright Elliot Bentine, 2018-
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using ProPixelizer;

public class PixelisationFeature : ScriptableRendererFeature
{
    [FormerlySerializedAs("DepthTestOutlines")]
    [Tooltip("Perform depth testing for outlines where object IDs differ. This prevents outlines appearing when one object intersects another, but requires an extra depth sample.")]
    public bool UseDepthTestingForIDOutlines = true;

    [Tooltip("The threshold value used when depth comparing outlines.")]
    public float DepthTestThreshold = 0.001f;

    [Tooltip("Use normals for edge detection. This will analyse pixelated screen normals to determine where edges occur within an objects silhouette.")]
    public bool UseNormalsForEdgeDetection = true;

    public float NormalEdgeDetectionSensitivity = 1f;

    [HideInInspector, SerializeField]
    PixelizationPass.ShaderResources PixelizationShaders;
    [HideInInspector, SerializeField]
    OutlineDetectionPass.ShaderResources OutlineShaders;

    PixelizationPass _PixelisationPass;
    OutlineDetectionPass _OutlinePass;

    public override void Create()
    {
        PixelizationShaders = new PixelizationPass.ShaderResources().Load();
        OutlineShaders = new OutlineDetectionPass.ShaderResources().Load();
        _OutlinePass = new OutlineDetectionPass(OutlineShaders);
        _OutlinePass.DepthTestOutlines = UseDepthTestingForIDOutlines;
        _OutlinePass.DepthTestThreshold = DepthTestThreshold;
        _OutlinePass.UseNormalsForEdgeDetection = UseNormalsForEdgeDetection;
        _OutlinePass.NormalEdgeDetectionSensitivity = NormalEdgeDetectionSensitivity;
        _PixelisationPass = new PixelizationPass(PixelizationShaders, _OutlinePass);
        _PixelisationPass.SourceBuffer = PixelizationPass.PixelizationSource.ProPixelizerMetadata;
        ProPixelizerVerification.Check();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _PixelisationPass.ConfigureInput(ScriptableRenderPassInput.Color);
        _PixelisationPass.ConfigureInput(ScriptableRenderPassInput.Depth);
        renderer.EnqueuePass(_PixelisationPass);
        renderer.EnqueuePass(_OutlinePass);
    }

#if BLIT_API
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        _PixelisationPass.ConfigureInput(ScriptableRenderPassInput.Color);
        _PixelisationPass.ConfigureInput(ScriptableRenderPassInput.Depth);
    }
#endif

#if URP_13
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _OutlinePass.Dispose();
            _PixelisationPass.Dispose();
        }
    }
#endif
}