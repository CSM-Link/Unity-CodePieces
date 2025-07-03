using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RendererFeatures
{
    public class IndexLightPass : ScriptableRenderPass
    {
        private readonly OutlineRenderFeature _renderFeature;
        private static readonly int ShadowBrightness = Shader.PropertyToID("_ShadowBrightness");

        public IndexLightPass(OutlineRenderFeature renderFeature)
        {
            _renderFeature = renderFeature;
        }
        
        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class IndexLightPassData
        {
            internal RendererListHandle objectRendererList;
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        private static void ExecutePass(IndexLightPassData data, RasterGraphContext context)
        {
            // Clear the render target to black
            context.cmd.ClearRenderTarget(true, true, Color.white);

            // Draw the objects in the list
            context.cmd.DrawRendererList(data.objectRendererList);
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Index light render pass";
            
            // Get data needed later from the frame
            var resourceData = frameData.Get<UniversalResourceData>();
            var sourceColor = resourceData.cameraColor;
            
            // occasionally in editor switching to a different window or tab will invalidate the camera color textures
            if (!sourceColor.IsValid()) return;
            
            // Use the current camera color and depth descriptions so we don't get mismatches, which can cause validation errors.
            var destinationColorDesc = renderGraph.GetTextureDesc(sourceColor);
            destinationColorDesc.name = "Index light texture";
            destinationColorDesc.clearBuffer = true;

            // Create the render textures to be used by the rendergraph and store them for use later
            _renderFeature.indexLightTexture = renderGraph.CreateTexture(destinationColorDesc);
            
            using (var builder = renderGraph.AddRasterRenderPass<IndexLightPassData>(passName, out var passData))
            {
                // Get the rendering data from the frame to use
                var renderingData = frameData.Get<UniversalRenderingData>();
                
                if (_renderFeature.objectIndexLightMaterial == null) return;
                
                // Get the data needed to create the list of objects to draw
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();
                var sortFlags = cameraData.defaultOpaqueSortFlags;
                var renderQueueRange = RenderQueueRange.opaque;
                var filterSettings = new FilteringSettings(renderQueueRange, _renderFeature.indexLightLayers);
                
                // Redraw only objects that have their LightMode tag set to UniversalForward 
                var shadersToOverride = new ShaderTagId("UniversalForward");
                
                // Create drawing settings
                var drawSettings = RenderingUtils.CreateDrawingSettings(shadersToOverride, renderingData, cameraData,
                    lightData, sortFlags);
                
                // Add the override material to the drawing settings
                _renderFeature.objectIndexLightMaterial.SetFloat(ShadowBrightness, _renderFeature.shadowBrightness);
                drawSettings.overrideMaterial = _renderFeature.objectIndexLightMaterial;
                
                // Create the list of objects to draw
                var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
                
                // Convert the list to a list handle that the render graph system can use
                passData.objectRendererList = renderGraph.CreateRendererList(rendererListParameters);
                
                // Set the render target as the color and depth textures of the active camera texture
                builder.UseRendererList(passData.objectRendererList);
                builder.SetRenderAttachment(_renderFeature.indexLightTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
                
                builder.SetRenderFunc((IndexLightPassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
}