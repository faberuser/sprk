using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class EdgeDetectionRenderer : PostProcessEffectRenderer<EdgeDetection>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/EdgeDetection"));
		propertySheet.properties.SetFloat("_Edge", base.settings.Edge);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
