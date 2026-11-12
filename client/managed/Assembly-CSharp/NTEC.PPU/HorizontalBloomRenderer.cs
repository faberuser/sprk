using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class HorizontalBloomRenderer : PostProcessEffectRenderer<HorizontalBloom>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/HorizontalBloom"));
		RenderTexture.ReleaseTemporary(base.settings.tempRT0);
		RenderTexture.ReleaseTemporary(base.settings.tempRT1);
		RenderTexture.ReleaseTemporary(base.settings.loopRT0);
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetFloat("_Threshold", base.settings.Threshold);
		propertySheet.properties.SetFloat("_Quality", (int)base.settings.Quality);
		base.settings.tempRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		base.settings.tempRT1 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, base.settings.tempRT0, propertySheet, 0);
		base.settings.loopRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(base.settings.tempRT0, base.settings.loopRT0);
		for (int i = 0; i < (int)base.settings.Quality; i++)
		{
			base.settings.loopRT1 = RenderTexture.GetTemporary(Screen.width, Screen.height);
			context.command.BlitFullscreenTriangle(base.settings.loopRT0, base.settings.loopRT1, propertySheet, 1);
			RenderTexture.ReleaseTemporary(base.settings.loopRT0);
			base.settings.loopRT0 = base.settings.loopRT1;
		}
		context.command.BlitFullscreenTriangle(base.settings.loopRT0, base.settings.tempRT1);
		propertySheet.properties.SetTexture("_tempRT1", base.settings.tempRT1);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 2);
	}
}
