using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class NoiseRenderer : PostProcessEffectRenderer<Noise>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Noise"));
		RenderTexture.ReleaseTemporary(base.settings.tempRT0);
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetFloat("_Saturation", base.settings.Saturation);
		base.settings.tempRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, base.settings.tempRT0, propertySheet, 0);
		context.command.BlitFullscreenTriangle(base.settings.tempRT0, context.destination, propertySheet, 0);
	}
}
