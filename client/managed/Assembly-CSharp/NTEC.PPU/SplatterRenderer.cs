using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class SplatterRenderer : PostProcessEffectRenderer<Splatter>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Splatter"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		if (base.settings.Image.value != null)
		{
			propertySheet.properties.SetTexture("_Image", base.settings.Image);
		}
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
