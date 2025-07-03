using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RendererFeatures
{
    public class AvatarRenderer : ScriptableRendererFeature
    {
        public RenderTexture avatarRenderTexture;
        public LayerMask avatarLayer;
        public Material avatarMaterial;

        private AvatarPass _avatarPass;

        /// <inheritdoc/>
        public override void Create()
        {
            _avatarPass = new AvatarPass(this)
            {
                // Configures where the render pass should be injected.
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents
            };
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_avatarPass);
        }
    }
}
