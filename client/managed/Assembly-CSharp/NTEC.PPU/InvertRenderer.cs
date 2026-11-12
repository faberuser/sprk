using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class InvertRenderer : PostProcessEffectRenderer<Invert>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Invert"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
