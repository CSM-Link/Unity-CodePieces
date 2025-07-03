using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RendererFeatures
{
    public class OutlineRenderFeature : ScriptableRendererFeature
    {
        [Header("Random pass")]
        public Material randomMaterial;
        public Texture2D randomSeedTexture;

        [Header("Object normal pass")]
        public Material objectNormalMaterial;
        public LayerMask normalLayers;

        [Header("Object index light pass")]
        public Material objectIndexLightMaterial;
        public LayerMask indexLightLayers;

        [Header("Outline pass")]
        public Texture3D lozengeTexture;
        public float lozengeTiling;
        public Texture2D crossHatchTexture1;
        public Texture2D crossHatchTexture2;
        public float crossHatchTiling;
        [Range(0f, 1f)] public float crossHatchThreshold;
        [Range(0f, 1f)] public float crossHatchAngleThreshold;
        public Color outlineColor = Color.black;
        [Range(0.0001f, 5f)] public float timeScale;
        [Range(0.001f, 0.99f)] public float outlineGrainScale = 0.5f;
        [Range(0.0f, 20f)] public float outlineThickness = 1f;
        [Range(0.0f, 20f)] public float normalOutlineThickness = 1f;
        [Range(0f, 1f)] public float minNormalThreshold = 0.01f;
        [Range(0f, 2f)] public float shadowBrightness = 1.0f;
        public Material outlineMaterial;
        public RenderPassEvent outlineRenderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        
        public TextureHandle randomTextureHandle;
        public TextureHandle normalTexture;
        public TextureHandle indexLightTexture;
        
        private RandomPass _randomPass;
        private ObjectNormalPass _objectNormalPass;
        private IndexLightPass _indexLightPass;
        private OutlineRenderPass _outlinePass;

        /// <inheritdoc/>
        public override void Create()
        {
            _randomPass = new RandomPass(this)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };

            _objectNormalPass = new ObjectNormalPass(this)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents
            };
            
            _indexLightPass = new IndexLightPass(this)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents
            };
            
            _outlinePass = new OutlineRenderPass(this)
            {
                // Configures where the render pass should be injected.
                renderPassEvent = outlineRenderPassEvent
            };
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_randomPass);
            renderer.EnqueuePass(_objectNormalPass);
            renderer.EnqueuePass(_indexLightPass);
            renderer.EnqueuePass(_outlinePass);
        }
    }
}
