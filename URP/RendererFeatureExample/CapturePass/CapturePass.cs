using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

public class CaptureRendererFeature : ScriptableRendererFeature
{
    // The texture to use as input 
    public RenderPassEvent CaptureRenderingEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    // public RenderPassEvent AfterRenderingEvent = RenderPassEvent.AfterRenderingPostProcessing;
    private CapturePass capturePass;

    public override void Create()
    {
        capturePass = new CapturePass();
        capturePass.renderPassEvent = CaptureRenderingEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (capturePass == null)
        {
            return;
        }
        renderer.EnqueuePass(capturePass);
    }

    // Create the custom data class that contains the new texture
    public class CaptureData : ContextItem
    {
        public TextureHandle captureTexture;

        public override void Reset()
        {
            captureTexture = TextureHandle.nullHandle;
        }
    }

    private class CapturePass : ScriptableRenderPass
    {
        private int globalTextureID = Shader.PropertyToID("_CapturePassTexture");

        private class PassData
        {
            public TextureHandle ColorBuffer;
        }

        public CapturePass()
        {
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Capture", out var passData))
            {
                // Create a texture
                RenderTextureDescriptor textureProperties = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
                TextureHandle texture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureProperties, "Capture Screen Texture", false);

                // Store the texture in the frame data for later use 【这是另外一个跨越pass传递texture的方法】
                // CaptureData splitScreenData = frameData.Create<CaptureData>();
                // splitScreenData.captureTexture = texture;

                // Set render target
                builder.SetRenderAttachment(texture, 0, AccessFlags.Write);

                // Set Pass Data
                var resourceData = frameData.Get<UniversalResourceData>();
                passData.ColorBuffer = resourceData.activeColorTexture;

                // Set a texture to the global texture 【当前使用的跨越pass传递texture的方法】
                builder.SetGlobalTextureAfterPass(texture, globalTextureID);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        private static void ExecutePass(PassData data, RasterGraphContext context)
        {
            // Copy the Camera color texture to the new texture
            Blitter.BlitTexture(context.cmd, data.ColorBuffer, new Vector4(1.0f, 1.0f, 0, 0), 0, false);
        }
    }

}
