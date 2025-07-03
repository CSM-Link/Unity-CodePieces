using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RendererFeatures
{
    public class ObjectNormalPass : ScriptableRenderPass
    {
        private readonly OutlineRenderFeature _renderFeature;

        public ObjectNormalPass(OutlineRenderFeature renderFeature)
        {
            _renderFeature = renderFeature;
        }
        
        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class ObjectNormalPassData
        {
            internal RendererListHandle objectRendererList;
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        private static void ExecutePass(ObjectNormalPassData data, RasterGraphContext context)
        {
            // Clear the render target to black
            context.cmd.ClearRenderTarget(true, true, Color.black);

            // Draw the objects in the list
            context.cmd.DrawRendererList(data.objectRendererList);
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Object normal render pass";
            
            // Get data needed later from the frame
            var resourceData = frameData.Get<UniversalResourceData>();
            var sourceColor = resourceData.cameraColor;
            
            // occasionally in editor switching to a different window or tab will invalidate the camera color textures
            if (!sourceColor.IsValid()) return;
            
            // Use the current camera color and depth descriptions so we don't get mismatches, which can cause validation errors.
            var destinationColorDesc = renderGraph.GetTextureDesc(sourceColor);
            destinationColorDesc.name = "Object normal texture";
            destinationColorDesc.clearBuffer = false;
            destinationColorDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;

            // Create the render textures to be used by the render graph and store them for use later
            _renderFeature.normalTexture = renderGraph.CreateTexture(destinationColorDesc);
            
            using (var builder = renderGraph.AddRasterRenderPass<ObjectNormalPassData>(passName, out var passData))
            {
                // Get the rendering data from the frame to use
                var renderingData = frameData.Get<UniversalRenderingData>();
                
                if (_renderFeature.objectNormalMaterial == null) return;
                
                // Get the data needed to create the list of objects to draw
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();
                var sortFlags = cameraData.defaultOpaqueSortFlags;
                var renderQueueRange = RenderQueueRange.opaque;
                var filterSettings = new FilteringSettings(renderQueueRange, _renderFeature.normalLayers);
                
                // Redraw only objects that have their LightMode tag set to UniversalForward 
                var shadersToOverride = new ShaderTagId("UniversalForward");
                
                // Create drawing settings
                var drawSettings = RenderingUtils.CreateDrawingSettings(shadersToOverride, renderingData, cameraData,
                    lightData, sortFlags);
                
                // Add the override material to the drawing settings
                drawSettings.overrideMaterial = _renderFeature.objectNormalMaterial;
                
                // Create the list of objects to draw
                var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
                
                // Convert the list to a list handle that the render graph system can use
                passData.objectRendererList = renderGraph.CreateRendererList(rendererListParameters);
                
                // Set the render target as the color and depth textures of the active camera texture
                builder.UseRendererList(passData.objectRendererList);
                builder.SetRenderAttachment(_renderFeature.normalTexture, 0);
                
                builder.SetRenderFunc((ObjectNormalPassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
}