using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class InfraredRenderer : PostProcessEffectRenderer<Infrared>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Infrared"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
