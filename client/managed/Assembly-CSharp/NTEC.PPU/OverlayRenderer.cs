using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class OverlayRenderer : PostProcessEffectRenderer<Overlay>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Overlay"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		if (base.settings.Image.value != null)
		{
			propertySheet.properties.SetTexture("_Image", base.settings.Image);
		}
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
