using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class CrossedEyesRenderer : PostProcessEffectRenderer<CrossedEyes>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/CrossedEyes"));
		propertySheet.properties.SetFloat("_Shift", base.settings.Shift);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
