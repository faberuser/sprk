using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class DistortionRenderer : PostProcessEffectRenderer<Distortion>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Distortion"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		if (base.settings.Coords.value != null)
		{
			propertySheet.properties.SetTexture("_Coords", base.settings.Coords);
		}
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
