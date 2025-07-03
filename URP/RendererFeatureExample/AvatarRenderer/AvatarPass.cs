using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RendererFeatures
{
    class AvatarPass : ScriptableRenderPass
    {
        private readonly AvatarRenderer _avatarRenderer;
        
        public AvatarPass(AvatarRenderer avatarRenderer)
        {
            _avatarRenderer = avatarRenderer;
        }
        
        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class AvatarPassData
        {
            internal RendererListHandle objectRendererList;
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        static void ExecutePass(AvatarPassData data, RasterGraphContext context)
        {
            // Clear the render target to black
            context.cmd.ClearRenderTarget(true, true, Color.clear);

            // Draw the objects in the list
            context.cmd.DrawRendererList(data.objectRendererList);
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Avatar render pass";
            
            // Get data needed later from the frame
            var resourceData = frameData.Get<UniversalResourceData>();
            var sourceColor = resourceData.cameraColor;
            
            // occasionally in editor switching to a different window or tab will invalidate the camera color textures
            if (!sourceColor.IsValid()) return;
            
            // Use the current camera color and depth descriptions so we don't get mismatches, which can cause validation errors.
            var destinationColorDesc = renderGraph.GetTextureDesc(sourceColor);
            destinationColorDesc.name = "Avatar render texture";
            destinationColorDesc.clearBuffer = true;
            
            // Create render textures from the input textures
            var avatarHandle = RTHandles.Alloc(_avatarRenderer.avatarRenderTexture);
                
            // Create a texture handle that the shader graph system can use
            var avatarTexture = renderGraph.ImportTexture(avatarHandle);

            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddRasterRenderPass<AvatarPassData>(passName, out var passData))
            {
                if (_avatarRenderer.avatarRenderTexture == null || _avatarRenderer.avatarMaterial == null) return;
                
                // Get the rendering data from the frame to use
                var renderingData = frameData.Get<UniversalRenderingData>();

                // Get the data needed to create the list of objects to draw
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();
                var sortFlags = cameraData.defaultOpaqueSortFlags;
                var renderQueueRange = RenderQueueRange.opaque;
                var filterSettings = new FilteringSettings(renderQueueRange, _avatarRenderer.avatarLayer);
                
                // Redraw only objects that have their LightMode tag set to UniversalForward or SRPDefaultUnlit
                var shadersToOverride = new List<ShaderTagId>
                {
                    new("UniversalForward"),
                    new("SRPDefaultUnlit")
                };

                // Create drawing settings
                var drawSettings = RenderingUtils.CreateDrawingSettings(shadersToOverride, renderingData, cameraData,
                    lightData, sortFlags);

                // override material
                drawSettings.overrideMaterial = _avatarRenderer.avatarMaterial;

                // Create the list of objects to draw
                var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
                
                // Convert the list to a list handle that the render graph system can use
                passData.objectRendererList = renderGraph.CreateRendererList(rendererListParameters);
                
                // Set the render target as the color and depth textures of the active camera texture
                builder.UseRendererList(passData.objectRendererList);
                builder.SetRenderAttachment(avatarTexture, 0);

                builder.SetRenderFunc((AvatarPassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
}
