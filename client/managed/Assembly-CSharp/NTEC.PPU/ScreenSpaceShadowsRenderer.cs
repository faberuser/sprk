using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class ScreenSpaceShadowsRenderer : PostProcessEffectRenderer<ScreenSpaceShadows>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/ScreenSpaceShadows"));
		RenderTexture.ReleaseTemporary(base.settings.tempRT0);
		RenderTexture.ReleaseTemporary(base.settings.tempRT1);
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetFloat("_Power", (int)base.settings.Power);
		base.settings.tempRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		base.settings.tempRT1 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, base.settings.tempRT0, propertySheet, 0);
		context.command.BlitFullscreenTriangle(base.settings.tempRT0, base.settings.tempRT1, propertySheet, 1);
		propertySheet.properties.SetTexture("_tempRT1", base.settings.tempRT1);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 2);
	}
}
