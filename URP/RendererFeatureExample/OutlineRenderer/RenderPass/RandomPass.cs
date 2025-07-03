using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RendererFeatures
{
    public class RandomPass : ScriptableRenderPass
    {
        private RenderTextureDescriptor _randomTextureDescriptor;
        private Vector4 _textureSize = Vector4.one;
        private readonly OutlineRenderFeature _renderFeature;
        private float _strokeGraininess;
        private static readonly int TextureSize = Shader.PropertyToID("_TextureSize");
        private static readonly int TimeScale = Shader.PropertyToID("_TimeScale");

        public RandomPass(OutlineRenderFeature renderFeature)
        {
            _randomTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
                RenderTextureFormat.Default, 0);

            //_randomTextureHandle = randomHandle;
            _renderFeature = renderFeature;
        }
        
        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class RandomPassData
        {
            internal TextureHandle randomSeedTexture;
            internal Material randomMaterial;
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        private static void ExecutePass(RandomPassData data, RasterGraphContext context)
        {
            Blitter.BlitTexture(context.cmd, data.randomSeedTexture, new Vector4(1f,1f,0,0), data.randomMaterial, 0);
        }

        private void UpdateRandomSettings()
        {
            _textureSize.x = _renderFeature.randomSeedTexture.width;
            _textureSize.y = _renderFeature.randomSeedTexture.height;
            _renderFeature.randomMaterial.SetVector(TextureSize, _textureSize);
            _renderFeature.randomMaterial.SetFloat(TimeScale, _renderFeature.timeScale);
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Random render pass";

            var textureScale = new Vector2(1f - _renderFeature.outlineGrainScale, 1f - _renderFeature.outlineGrainScale);
            
            // Make sure the textures are a valid size
            var textureWidth = math.max((int)(Screen.width * textureScale.x), 2);
            var textureHeight = math.max((int)(Screen.width * textureScale.x), 2);

            _randomTextureDescriptor.width = textureWidth;
            _randomTextureDescriptor.height = textureHeight;
            
            // Rebuild the random texture based on the new size and set it to be the render target
            _renderFeature.randomTextureHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph,
                _randomTextureDescriptor, "Random texture", false);
            
            UpdateRandomSettings();
            
            using (var builder = renderGraph.AddRasterRenderPass<RandomPassData>(passName, out var passData))
            {
                // Create a render texture from the input texture
                var seedTextureHandle = RTHandles.Alloc(_renderFeature.randomSeedTexture);

                // Create a texture handle that the shader graph system can use
                var inputSeedTexture = renderGraph.ImportTexture(seedTextureHandle);

                // Add the texture to the pass data and assign the material
                passData.randomSeedTexture = inputSeedTexture;
                
                // update material and set it to the pass data
                passData.randomMaterial = _renderFeature.randomMaterial;

                // Set the texture as readable (default flag is read)
                builder.UseTexture(passData.randomSeedTexture);
                builder.SetRenderAttachment(_renderFeature.randomTextureHandle, 0);
                builder.SetRenderFunc((RandomPassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
}
