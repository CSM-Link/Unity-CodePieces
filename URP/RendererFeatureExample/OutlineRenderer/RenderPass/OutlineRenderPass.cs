using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RendererFeatures
{
    public class OutlineRenderPass : ScriptableRenderPass
    {
        private readonly OutlineRenderFeature _renderFeature;
        private static readonly int RandomTexture = Shader.PropertyToID("_RandomTexture");
        private static readonly int StrokeGraininess = Shader.PropertyToID("_StrokeGraininess");
        private static readonly int NormalTexture = Shader.PropertyToID("_NormalTexture");
        private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");
        private static readonly int MinNormalThreshold = Shader.PropertyToID("_MinNormalThreshold");
        private static readonly int NormalOutlineThickness = Shader.PropertyToID("_NormalOutlineThickness");
        private static readonly int IndexLightTexture = Shader.PropertyToID("_IndexLightTexture");
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int LozengeTexture = Shader.PropertyToID("_LozengeTexture");
        private static readonly int SsaoTexture = Shader.PropertyToID("_SSAOTexture");
        private static readonly int LozengeTiling = Shader.PropertyToID("_LozengeTiling");
        private static readonly int DiagonalTiling = Shader.PropertyToID("_CrossHatchTiling");
        private static readonly int CrossHatchTexture1 = Shader.PropertyToID("_CrossHatchTexture1");
        private static readonly int CrossHatchTexture2 = Shader.PropertyToID("_CrossHatchTexture2");
        private static readonly int CrossHatchThreshold = Shader.PropertyToID("_CrossHatchThreshold");
        private static readonly int CrossHatchAngleThreshold = Shader.PropertyToID("_CrossHatchAngleThreshold");

        public OutlineRenderPass(OutlineRenderFeature renderFeature)
        {
            _renderFeature = renderFeature;
        }
        
        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class OutlinePassData
        {
            internal TextureHandle randomTexture;
            internal TextureHandle normalTexture;
            internal TextureHandle indexLightTexture;
            internal TextureHandle lozengeTexture;
            internal TextureHandle ssaoTexture;
            internal TextureHandle crossHatchTexture1;
            internal TextureHandle crossHatchTexture2;
            internal Material outlineMaterial;
            internal Color outlineColor;
            internal float outlineGrainScale;
            internal float outlineThickness;
            internal float minNormalThreshold;
            internal float normalOutlineThickness;
            internal float lozengeTiling;
            internal float crossHatchTiling;
            internal float crossHatchThreshold;
            internal float crossHatchAngleThreshold;
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        static void ExecutePass(OutlinePassData data, RasterGraphContext context)
        {
            UpdateMaterialParameters(data);
            
            Blitter.BlitTexture(context.cmd, data.randomTexture, new Vector4(1f,1f,0,0),
                data.outlineMaterial, 0);
        }

        private static void UpdateMaterialParameters(OutlinePassData passData)
        {
            passData.outlineMaterial.SetTexture(RandomTexture, passData.randomTexture);
            passData.outlineMaterial.SetTexture(NormalTexture, passData.normalTexture);
            passData.outlineMaterial.SetTexture(IndexLightTexture, passData.indexLightTexture);
            passData.outlineMaterial.SetTexture(LozengeTexture, passData.lozengeTexture);
            passData.outlineMaterial.SetTexture(SsaoTexture, passData.ssaoTexture);
            passData.outlineMaterial.SetTexture(CrossHatchTexture1, passData.crossHatchTexture1);
            passData.outlineMaterial.SetTexture(CrossHatchTexture2, passData.crossHatchTexture2);
            passData.outlineMaterial.SetColor(OutlineColor, passData.outlineColor);
            passData.outlineMaterial.SetFloat(StrokeGraininess, passData.outlineGrainScale);
            passData.outlineMaterial.SetFloat(OutlineThickness, passData.outlineThickness);
            passData.outlineMaterial.SetFloat(MinNormalThreshold, passData.minNormalThreshold);
            passData.outlineMaterial.SetFloat(NormalOutlineThickness, passData.normalOutlineThickness);
            passData.outlineMaterial.SetFloat(LozengeTiling, passData.lozengeTiling);
            passData.outlineMaterial.SetFloat(DiagonalTiling, passData.crossHatchTiling);
            passData.outlineMaterial.SetFloat(CrossHatchThreshold, passData.crossHatchThreshold);
            passData.outlineMaterial.SetFloat(CrossHatchAngleThreshold, passData.crossHatchAngleThreshold);
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Outline render pass";

            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddRasterRenderPass<OutlinePassData>(passName, out var passData))
            {
                // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                var resourceData = frameData.Get<UniversalResourceData>();

                // sanity checks to make sure we don't throw errors if something is not ready or not available
                // Switching windows in the editor or switching to another application can sometimes cause issues
                if (!_renderFeature.randomTextureHandle.IsValid() || !_renderFeature.normalTexture.IsValid() ||
                    !_renderFeature.indexLightTexture.IsValid() || _renderFeature.outlineMaterial == null ||
                    _renderFeature.lozengeTexture == null || _renderFeature.crossHatchTexture1 == null ||
                    _renderFeature.crossHatchTexture2 == null || !resourceData.ssaoTexture.IsValid())
                    return;
                
                // Setup pass inputs and outputs through the builder interface.
                builder.UseTexture(_renderFeature.randomTextureHandle);
                builder.UseTexture(_renderFeature.normalTexture);
                builder.UseTexture(_renderFeature.indexLightTexture);
                builder.UseTexture(resourceData.ssaoTexture);
                
                // Create render textures from the input textures
                var lozengeHandle = RTHandles.Alloc(_renderFeature.lozengeTexture);
                var crossHatchHandle1 = RTHandles.Alloc(_renderFeature.crossHatchTexture1);
                var crossHatchHandle2 = RTHandles.Alloc(_renderFeature.crossHatchTexture2);

                // Create a texture handle that the shader graph system can use
                var lozengeTexture = renderGraph.ImportTexture(lozengeHandle);
                var crossHatchTexture1 = renderGraph.ImportTexture(crossHatchHandle1);
                var crossHatchTexture2 = renderGraph.ImportTexture(crossHatchHandle2);

                passData.randomTexture = _renderFeature.randomTextureHandle;
                passData.normalTexture = _renderFeature.normalTexture;
                passData.indexLightTexture = _renderFeature.indexLightTexture;
                passData.lozengeTexture = lozengeTexture;
                passData.ssaoTexture = resourceData.ssaoTexture;
                passData.crossHatchTexture1 = crossHatchTexture1;
                passData.crossHatchTexture2 = crossHatchTexture2;
                passData.outlineColor = _renderFeature.outlineColor;
                passData.outlineMaterial = _renderFeature.outlineMaterial;
                passData.outlineGrainScale = _renderFeature.outlineGrainScale;
                passData.outlineThickness = _renderFeature.outlineThickness;
                passData.minNormalThreshold = _renderFeature.minNormalThreshold;
                passData.normalOutlineThickness = _renderFeature.normalOutlineThickness;
                passData.lozengeTiling = _renderFeature.lozengeTiling;
                passData.crossHatchTiling = _renderFeature.crossHatchTiling;
                passData.crossHatchThreshold = _renderFeature.crossHatchThreshold;
                passData.crossHatchAngleThreshold = _renderFeature.crossHatchAngleThreshold;

                // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                builder.SetRenderFunc((OutlinePassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
}
