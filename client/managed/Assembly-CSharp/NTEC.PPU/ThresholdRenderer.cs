using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class ThresholdRenderer : PostProcessEffectRenderer<Threshold>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Threshold"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetFloat("_Level", base.settings.Level);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
