#if UNITY_6000_0_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;
using System.Collections.Generic;

[Tooltip("Draws outlines for Poiyomi Shaders.")]
public class PoiOutlineURPRenderFeature : ScriptableRendererFeature
{
    [SerializeField] PoiOutlineURPRenderFeatureSettings settings;
    PoiOutlineURPRenderFeaturePass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        name = "Poiyomi Outlines";
        m_ScriptablePass = new PoiOutlineURPRenderFeaturePass(settings);

        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_ScriptablePass == null)
        {
            return;
        }
        renderer.EnqueuePass(m_ScriptablePass);
    }

    // Use this class to pass around settings from the feature to the pass
    [Serializable]
    public class PoiOutlineURPRenderFeatureSettings
    {
        public RenderPassEvent outlineEvent = RenderPassEvent.BeforeRenderingTransparents;
        public LayerMask layerMask = 1;
        public RenderQueueRange renderQueue = RenderQueueRange.all;
        public SortingCriteria sorting = SortingCriteria.CommonTransparent;
    }

    class PoiOutlineURPRenderFeaturePass : ScriptableRenderPass
    {
        List<ShaderTagId> OutlineTags = new List<ShaderTagId> { new ShaderTagId("PoiOutline") };
        static string name = "Poiyomi Outlines";
        readonly PoiOutlineURPRenderFeatureSettings settings;

        public PoiOutlineURPRenderFeaturePass(PoiOutlineURPRenderFeatureSettings settings)
        {
            this.settings = settings;
        }

        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class PassData
        {
            public RendererListHandle rl;
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var urpRes = frameData.Get<UniversalResourceData>();
            var camData = frameData.Get<UniversalCameraData>();
            var urpRD = frameData.Get<UniversalRenderingData>();
            var lightData = frameData.Get<UniversalLightData>();

            // Update Settings
            renderPassEvent = settings.outlineEvent;

            var filteringSettings = new FilteringSettings(settings.renderQueue, settings.layerMask);
            var drawingSettings = CreateDrawingSettings(OutlineTags, urpRD, camData, lightData, settings.sorting);
            var rendererListParams = new RendererListParams(urpRD.cullResults, drawingSettings, filteringSettings);
            var rl = renderGraph.CreateRendererList(rendererListParams);

            using var builder = renderGraph.AddRasterRenderPass<PassData>(name, out var data);
            builder.SetRenderAttachment(urpRes.activeColorTexture, 0);
            //builder.SetRenderAttachmentDepth(urpRes.activeDepthTexture, AccessFlags.Write);
            //builder.AllowPassCulling(false);

            data.rl = rl;
            builder.UseRendererList(rl);
            builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
            {
                //ctx.cmd.ClearRenderTarget(true, true, Color.black);
                ctx.cmd.DrawRendererList(d.rl);
            });
        }
    }
}

#endif