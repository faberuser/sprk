using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class SepiaRenderer : PostProcessEffectRenderer<Sepia>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Sepia"));
		propertySheet.properties.SetColor("_Color", base.settings.Color);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
