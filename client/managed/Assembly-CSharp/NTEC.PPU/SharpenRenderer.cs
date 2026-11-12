using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class SharpenRenderer : PostProcessEffectRenderer<Sharpen>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Sharpen"));
		RenderTexture.ReleaseTemporary(base.settings.tempRT0);
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		base.settings.tempRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
