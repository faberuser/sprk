using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class FrameRenderer : PostProcessEffectRenderer<Frame>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Frame"));
		propertySheet.properties.SetFloat("_Horizontal", base.settings.Horizontal);
		propertySheet.properties.SetFloat("_Vertical", base.settings.Vertical);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
