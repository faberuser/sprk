using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class Glitch2Renderer : PostProcessEffectRenderer<Glitch2>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Glitch2"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetFloat("_Speed", base.settings.Speed);
		propertySheet.properties.SetFloat("_Saturation", base.settings.Saturation);
		propertySheet.properties.SetFloat("_Lines", (int)base.settings.Lines);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
