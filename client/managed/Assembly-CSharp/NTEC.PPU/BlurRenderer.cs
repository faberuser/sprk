using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class BlurRenderer : PostProcessEffectRenderer<Blur>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Blur"));
		RenderTexture.ReleaseTemporary(base.settings.loopRT0);
		propertySheet.properties.SetFloat("_Iterations", (int)base.settings.Iterations);
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		base.settings.loopRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, base.settings.loopRT0);
		for (int i = 0; i < (int)base.settings.Iterations; i++)
		{
			base.settings.loopRT1 = RenderTexture.GetTemporary(Screen.width, Screen.height);
			context.command.BlitFullscreenTriangle(base.settings.loopRT0, base.settings.loopRT1, propertySheet, 0);
			RenderTexture.ReleaseTemporary(base.settings.loopRT0);
			base.settings.loopRT0 = base.settings.loopRT1;
		}
		context.command.BlitFullscreenTriangle(base.settings.loopRT0, context.destination);
	}
}
