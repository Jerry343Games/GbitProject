// Copyright Elliot Bentine, 2018-
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProPixelizer
{
    /// <summary>
    /// Performs the outline rendering and detection pass.
    /// </summary>
    public class PixelizationPass : ProPixelizerPass
    {
        public PixelizationPass(ShaderResources shaders, OutlineDetectionPass outlines)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            SourceBuffer = PixelizationSource.SceneColor;
            Materials = new MaterialLibrary(shaders);
            OutlinePass = outlines;
        }

        private MaterialLibrary Materials;
        private OutlineDetectionPass OutlinePass;

        /// <summary>
        /// Shader resources used by the PixelizationPass.
        /// </summary>
        [Serializable]
        public sealed class ShaderResources
        {
            public Shader PixelizationMap;
            public Shader CopyDepth;
            public Shader CopyMainTexAndDepth;
            public Shader ApplyPixelizationMap;

            public ShaderResources Load()
            {
                PixelizationMap = Shader.Find(PixelizationMapShaderName);
                CopyDepth = Shader.Find(CopyDepthShaderName);
                ApplyPixelizationMap = Shader.Find(ApplyPixelizationMapShaderName);
                CopyMainTexAndDepth = Shader.Find(CopyMainTexAndDepthShaderName);
                return this;
            }
        }

        /// <summary>
        /// Materials used by the PixelizationPass
        /// </summary>
        public sealed class MaterialLibrary
        {
            private ShaderResources Resources;

            private Material _PixelizationMap;
            public Material PixelizationMap
            {
                get
                {
                    if (_PixelizationMap == null)
                        _PixelizationMap = new Material(Resources.PixelizationMap);
                    return _PixelizationMap;
                }
            }
            private Material _CopyDepth;
            public Material CopyDepth
            {
                get
                {
                    if (_CopyDepth == null)
                        _CopyDepth = new Material(Resources.CopyDepth);
                    return _CopyDepth;
                }
            }
            private Material _CopyMainTexAndDepth;
            public Material CopyMainTexAndDepth
            {
                get
                {
                    if (_CopyMainTexAndDepth == null)
                        _CopyMainTexAndDepth = new Material(Resources.CopyMainTexAndDepth);
                    return _CopyMainTexAndDepth;
                }
            }
            private Material _ApplyPixelizationMap;
            public Material ApplyPixelizationMap
            {
                get
                {
                    if (_ApplyPixelizationMap == null)
                        _ApplyPixelizationMap = new Material(Resources.ApplyPixelizationMap);
                    return _ApplyPixelizationMap;
                }
            }

            public MaterialLibrary(ShaderResources resources)
            {
                Resources = resources;
            }
        }

#if URP_13
        private RTHandle _PixelizationMap;
        private RTHandle _OriginalScene;
        private RTHandle _CameraColorTexture;
        private RTHandle _PixelatedScene;
        private RTHandle _PixelatedScene_Depth;
        private RTHandle _CameraDepthAttachment;
        private RTHandle _CameraDepthAttachmentTemp;
        private RTHandle _CameraDepthTexture;
        private RTHandle _ProPixelizerOutline;
        private RTHandle _ProPixelizerOutlineObject;
#else
        private int _PixelizationMap;
        private int _OriginalScene;
        private int _CameraColorTexture;
        private int _PixelatedScene;
        private int _PixelatedScene_Depth;
        private int _CameraDepthAttachment;
        private int _CameraDepthAttachmentTemp;
        private int _CameraDepthTexture;
        private int _ProPixelizerOutline;
        private int _ProPixelizerOutlineObject;
#endif

#if BLIT_API
        private const string ApplyPixelMapDepthOutputKeywordString = "PIXELMAP_DEPTH_OUTPUT_ON";
        private GlobalKeyword ApplyPixelMapDepthOutputKeyword = GlobalKeyword.Create(ApplyPixelMapDepthOutputKeywordString);
#endif
        private const string CopyDepthShaderName = "Hidden/ProPixelizer/SRP/BlitCopyDepth";
        private const string CopyMainTexAndDepthShaderName = "Hidden/ProPixelizer/SRP/BlitCopyMainTexAndDepth";
        private const string PixelizationMapShaderName = "Hidden/ProPixelizer/SRP/Pixelization Map";
        private const string ApplyPixelizationMapShaderName = "Hidden/ProPixelizer/SRP/ApplyPixelizationMap";

        private Vector4 TexelSize;

        public enum PixelizationSource
        {
            SceneColor,
            ProPixelizerMetadata
        }

        public PixelizationSource SourceBuffer;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            #if DISABLE_PROPIX_PREVIEW_WINDOW
                // Inspector window in 2022.2 is bugged with a null render target.
                // https://forum.unity.com/threads/preview-camera-does-not-call-setuprenderpasses.1377363/
                // https://forum.unity.com/threads/nullreferenceexception-in-2022-2-1f1-urp-14-0-4-when-rendering-the-inspector-preview-window.1377240/
                if (renderingData.cameraData.cameraType == CameraType.Preview)
                    return;
            #endif

            #if BLIT_API
                ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            #endif
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.useMipMap = false;
            
            var depthDescriptor = cameraTextureDescriptor;
            depthDescriptor.colorFormat = RenderTextureFormat.Depth;

            var pixelizationMapDescriptor = cameraTextureDescriptor;
            pixelizationMapDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            pixelizationMapDescriptor.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            pixelizationMapDescriptor.depthBufferBits = 0;

#if URP_13
            RenderingUtils.ReAllocateIfNeeded(ref _PixelatedScene_Depth, cameraTextureDescriptor, name: "ProP_PixelatedScene");
            cameraTextureDescriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _PixelatedScene, cameraTextureDescriptor, name: "ProP_PixelatedScene");
            RenderingUtils.ReAllocateIfNeeded(ref _OriginalScene, cameraTextureDescriptor, name: "ProP_OriginalScene");
            _CameraColorTexture = colorAttachmentHandle;
            _ProPixelizerOutline = OutlinePass._OutlineBuffer;
            _ProPixelizerOutlineObject = OutlinePass._OutlineObjectBuffer;
            _CameraDepthAttachment = depthAttachmentHandle;
            RenderingUtils.ReAllocateIfNeeded(ref _CameraDepthAttachmentTemp, depthDescriptor, name: "ProP_CameraDepthAttachmentTemp");
            RenderingUtils.ReAllocateIfNeeded(ref _PixelizationMap, pixelizationMapDescriptor, name: "ProP_PixelizationMap");
#else
            _PixelizationMap = Shader.PropertyToID("_PixelizationMap");
            _CameraColorTexture = Shader.PropertyToID("_CameraColorTexture");
            _PixelatedScene = Shader.PropertyToID("_PixelatedScene");
            _PixelatedScene_Depth = Shader.PropertyToID("_PixelatedScene_Depth");
            _OriginalScene = Shader.PropertyToID("_OriginalScene");
            _ProPixelizerOutline = Shader.PropertyToID(OutlineDetectionPass.OUTLINE_BUFFER);
            _ProPixelizerOutlineObject = Shader.PropertyToID(OutlineDetectionPass.PROPIXELIZER_OBJECT_BUFFER);

            var pixelatedTexDesc = cameraTextureDescriptor;
            // Fix more Metal/Vulkan on Mac.
            // For some reason, on Metal/Vulkan on Mac the cameraTextureDescription has 0 depth buffer bits.
            // This isn't the case on any other platform.
            pixelatedTexDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(_PixelatedScene, pixelatedTexDesc);
            cmd.GetTemporaryRT(_PixelatedScene_Depth, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 32, FilterMode.Point, RenderTextureFormat.Depth);
            cmd.GetTemporaryRT(_OriginalScene, cameraTextureDescriptor, FilterMode.Point);

            _CameraDepthAttachment = Shader.PropertyToID("_CameraDepthAttachment");
            _CameraDepthAttachmentTemp = Shader.PropertyToID("_CameraDepthAttachmentTemp");
            _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");

            cmd.GetTemporaryRT(_CameraDepthAttachment, depthDescriptor);
            cmd.GetTemporaryRT(_CameraDepthAttachmentTemp, depthDescriptor);
            cmd.GetTemporaryRT(_PixelizationMap, pixelizationMapDescriptor);
#endif

            TexelSize = new Vector4(
                1f / cameraTextureDescriptor.width,
                1f / cameraTextureDescriptor.height,
                cameraTextureDescriptor.width,
                cameraTextureDescriptor.height
            );
        }

        public const string PROFILER_TAG = "PIXELISATION";

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            #if DISABLE_PROPIX_PREVIEW_WINDOW
            // Inspector window in 2022.2 is bugged with a null render target.
            // https://forum.unity.com/threads/preview-camera-does-not-call-setuprenderpasses.1377363/
            // https://forum.unity.com/threads/nullreferenceexception-in-2022-2-1f1-urp-14-0-4-when-rendering-the-inspector-preview-window.1377240/
            if (renderingData.cameraData.cameraType == CameraType.Preview)
                return;
            #endif

            CommandBuffer buffer = CommandBufferPool.Get(PROFILER_TAG);
            buffer.name = "ProPixelizer Pixelisation";

            // Configure keywords for pixelising material.
            if (renderingData.cameraData.camera.orthographic)
                Materials.PixelizationMap.EnableKeyword("ORTHO_PROJECTION");
            else
                Materials.PixelizationMap.DisableKeyword("ORTHO_PROJECTION");

            if (renderingData.cameraData.camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay)
                Materials.PixelizationMap.EnableKeyword("OVERLAY_CAMERA"); 
            else
                Materials.PixelizationMap.DisableKeyword("OVERLAY_CAMERA");

#if CAMERA_COLOR_TEX_PROP
#if URP_13
            RTHandle ColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            RTHandle cameraDepthTarget = renderingData.cameraData.renderer.cameraDepthTargetHandle;           
#else
            RenderTargetIdentifier ColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            RenderTargetIdentifier cameraDepthTarget = renderingData.cameraData.renderer.cameraDepthTarget;
#endif
            bool applyFeature = true;
#else
            // Preview camera does not use _CameraColorTexture target in more recent 2019 LTS.
            // It now uses a temporary buffer, and API does not provide a means to get the ID.
            int ColorTarget = _CameraColorTexture;
            bool applyFeature = !renderingData.cameraData.isPreviewCamera;
#endif

            if (!applyFeature)
            {
                context.ExecuteCommandBuffer(buffer);
                CommandBufferPool.Release(buffer);
                return;
            }

            // Blit scene into _OriginalScene - so that we can guarantee point filtering of colors.
            #if BLIT_API
                Blitter.BlitCameraTexture(buffer, ColorTarget, _OriginalScene);
            #else
                Blit(buffer, ColorTarget, _OriginalScene);
            #endif

            #if CAMERA_COLOR_TEX_PROP
                bool isOverlay = renderingData.cameraData.camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay;
            #else
                bool isOverlay = false;
            #endif

            // Create pixelization map, to determine how to pixelate the screen.
            if (SourceBuffer == PixelizationSource.SceneColor)
            {
                buffer.SetGlobalTexture("_MainTex", _OriginalScene);
#if CAMERA_COLOR_TEX_PROP
                // Overlay cameras do not have a _CameraDepthTexture assigned, so our first action is to write
                // the opaque depth buffer into _CameraDepthTexture.
                if (isOverlay)
                {
                    buffer.SetGlobalTexture("_SourceDepthTexture", cameraDepthTarget);
                    buffer.SetGlobalTexture("_SceneDepthTexture", cameraDepthTarget);
                }
                else
                {
                    buffer.SetGlobalTexture("_SourceDepthTexture", _CameraDepthTexture);
                    buffer.SetGlobalTexture("_SceneDepthTexture", _CameraDepthTexture);
                }
#else
                buffer.SetGlobalTexture("_SourceDepthTexture", _CameraDepthTexture);
                buffer.SetGlobalTexture("_SceneDepthTexture", _CameraDepthTexture);
#endif
            } else {
                buffer.SetGlobalTexture("_MainTex", _ProPixelizerOutlineObject);
                buffer.SetGlobalTexture("_SourceDepthTexture", OutlinePass._OutlineObjectBuffer_Depth, RenderTextureSubElement.Depth);
#if URP_13
                    buffer.SetGlobalTexture("_SceneDepthTexture", cameraDepthTarget);
#else
                    if (renderingData.cameraData.camera.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay)
                    {
                        buffer.SetGlobalTexture("_SceneDepthTexture", renderingData.cameraData.renderer.cameraDepthTarget);
                    }
                    else
                    {
                        buffer.SetGlobalTexture("_SceneDepthTexture", _CameraDepthTexture);
                    }
#endif
            }
#if BLIT_API
                Blitter.BlitCameraTexture(buffer, _OriginalScene, _PixelizationMap, Materials.PixelizationMap, 0);
#else
                Blit(buffer, _OriginalScene, _PixelizationMap, Materials.PixelizationMap);
#endif


            // Pixelise the appearance texture
            // Note: For RTHandles we need to blit to a separate depth buffer.
            buffer.SetGlobalTexture("_MainTex", _OriginalScene);
            buffer.SetGlobalTexture("_PixelizationMap", _PixelizationMap);
#if BLIT_API
            // I would prefer to do this with local keywords, but they are broken w/buffers at least in 2022.2
            buffer.DisableKeyword(ApplyPixelMapDepthOutputKeyword);
            Blitter.BlitCameraTexture(buffer, _OriginalScene, _PixelatedScene, Materials.ApplyPixelizationMap, 0);
            // need to check which camera depth target to use here.
            buffer.EnableKeyword(ApplyPixelMapDepthOutputKeyword);
            Blitter.BlitCameraTexture(buffer, cameraDepthTarget, _PixelatedScene_Depth, Materials.ApplyPixelizationMap, 0);
#else
                buffer.SetRenderTarget(_PixelatedScene, (RenderTargetIdentifier)_PixelatedScene_Depth);
                buffer.SetViewMatrix(Matrix4x4.identity);
                buffer.SetProjectionMatrix(Matrix4x4.identity);
                buffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, Materials.ApplyPixelizationMap);
#endif

            // Copy depth into Color target so that transparents work in scene view tab
            buffer.SetGlobalTexture("_MainTex", _PixelatedScene);
            buffer.SetGlobalTexture("_SourceTex", _PixelatedScene); // so that it works for fallback pass for platforms that fail to compile BCMT&D
            buffer.SetGlobalTexture("_Depth", _PixelatedScene_Depth);
#if BLIT_API
                Blitter.BlitCameraTexture(buffer, _PixelatedScene, ColorTarget);
                Blitter.BlitCameraTexture(buffer, _PixelatedScene_Depth, cameraDepthTarget, Materials.CopyDepth, 0);
#else
#if URP_13
                    buffer.SetRenderTarget(ColorTarget, cameraDepthTarget);
#else
                    // Note that for 2020 and 2021, the cameraDepthTarget only works for the Game tab, and not the Scene tab camera. Instead, you have to
                    // blit to the depth element of the ColorTarget, which works for Scene view.
                    // HOWEVER, the issue is that this element doesn't exist for the Metal API, and so an error gets thrown. It seems there is no way
                    // to blit to the scene tab's depth buffer for Metal on 2020/2021.
                    // 
                    // https://forum.unity.com/threads/scriptablerenderpasses-cannot-consistently-access-the-depth-buffer.1263044/
                    var device = SystemInfo.graphicsDeviceType;
                    if (device == GraphicsDeviceType.Metal)
                    {
                        buffer.SetRenderTarget(ColorTarget, cameraDepthTarget);
                    }
                    else
                    {
                        buffer.SetRenderTarget(ColorTarget); //use color target depth buffer.
                    }
#endif
                buffer.SetViewMatrix(Matrix4x4.identity);
                buffer.SetProjectionMatrix(Matrix4x4.identity);
                buffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, Materials.CopyMainTexAndDepth);
#endif

            // Copy pixelated depth texture
#if BLIT_API
            Blitter.BlitCameraTexture(buffer, _PixelatedScene_Depth, _CameraDepthAttachmentTemp, Materials.CopyDepth, 0);
#else
            buffer.SetGlobalTexture("_MainTex", _PixelatedScene_Depth);
            buffer.SetRenderTarget(_CameraDepthAttachmentTemp);
            buffer.SetViewMatrix(Matrix4x4.identity);
            buffer.SetProjectionMatrix(Matrix4x4.identity);
            buffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, Materials.CopyDepth);
#endif

            // ...then restore transformations:
#if CAMERADATA_MATRICES
            buffer.SetViewMatrix(renderingData.cameraData.GetViewMatrix());
            buffer.SetProjectionMatrix(renderingData.cameraData.GetProjectionMatrix());
#else
            buffer.SetViewMatrix(renderingData.cameraData.camera.worldToCameraMatrix);
            buffer.SetProjectionMatrix(renderingData.cameraData.camera.projectionMatrix);
#endif

#if URP_13
                //// Blit pixelised depth into the used depth texture
                /// (2022.1b and beyond)
                if (renderingData.cameraData.cameraType != CameraType.Preview)
                    Blit(buffer, _CameraDepthAttachmentTemp, cameraDepthTarget, Materials.CopyDepth);
#else
            // 2020 LTS: required to fix depth buffer in scene view.
            if (!isOverlay)
                    Blit(buffer, _CameraDepthAttachmentTemp, _CameraDepthTexture, Materials.CopyDepth);
                Blit(buffer, _CameraDepthAttachmentTemp, _CameraDepthAttachment, Materials.CopyDepth);

            if (!isOverlay)
                buffer.SetGlobalTexture("_CameraDepthTexture", _CameraDepthTexture);
#endif

            // We also need to copy the depth texture into the render target used to represent scene depth for post processing/scene depth nodes.
            // e.g. _CameraDepthTexture_<x>x<y>_R32_SFloat_Tex2D
            // do this only for the base camera, not for overlay cameras.
            // 
            // Herein lies a problem. I can access e.g. `_CameraDepthAttachment_<X>x<Y>_Depth`,
            // which is the render target referred to by `renderer.cameraDepthTarget`.
            // I cannot reliably access the associated `_CameraDepthTexture_<X>x<Y>_R32_SFloat_Tex2D`,
            // which the CopyDepth pass uses as render target. The first is used for depth e.g. when
            // drawing transparents, the second is a texture used for PostProcessing effects and scene depth nodes.
            // So I need to be able to reliably patch into it, and I cannot yet.
            // 
            // A workaround, for now, is to change the Forward Renderer's ordering of the CopyDepth to
            // `After Transparents`, so the copy is performed by URP after ProPixelizer has finished.
            // Long term I would like to fix this - give me a way Unity!

            //buffer.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTarget);

            // ...and restore transformations:
#if CAMERADATA_MATRICES
            buffer.SetViewMatrix(renderingData.cameraData.GetViewMatrix());
            buffer.SetProjectionMatrix(renderingData.cameraData.GetProjectionMatrix());
#else
            buffer.SetViewMatrix(renderingData.cameraData.camera.worldToCameraMatrix);
            buffer.SetProjectionMatrix(renderingData.cameraData.camera.projectionMatrix);
#endif
            context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }


#if URP_13
        public void Dispose()
        {
            _PixelatedScene?.Release();
            _PixelatedScene_Depth?.Release();
            _OriginalScene?.Release();
            _CameraDepthAttachmentTemp?.Release();
            _PixelizationMap?.Release();
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // set externally managed RTHandle references to null.
        }
#endif

        public override void FrameCleanup(CommandBuffer cmd)
        {
#if URP_13

#else
            cmd.ReleaseTemporaryRT(_PixelizationMap);
            cmd.ReleaseTemporaryRT(_CameraDepthAttachment);
            cmd.ReleaseTemporaryRT(_CameraDepthAttachmentTemp);
            cmd.ReleaseTemporaryRT(_PixelatedScene);
            cmd.ReleaseTemporaryRT(_PixelatedScene_Depth);
            cmd.ReleaseTemporaryRT(_OriginalScene);
#endif
        }
    }
}