using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class FadeRenderer : PostProcessEffectRenderer<Fade>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Fade"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
