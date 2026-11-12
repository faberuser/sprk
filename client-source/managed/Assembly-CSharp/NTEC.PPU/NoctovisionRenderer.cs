using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class NoctovisionRenderer : PostProcessEffectRenderer<Noctovision>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Noctovision"));
		RenderTexture.ReleaseTemporary(base.settings.tempRT0);
		propertySheet.properties.SetFloat("_Green", base.settings.Green);
		propertySheet.properties.SetFloat("_Noise", base.settings.Noise);
		base.settings.tempRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, base.settings.tempRT0, propertySheet, 0);
		context.command.BlitFullscreenTriangle(base.settings.tempRT0, context.destination, propertySheet, 1);
	}
}
