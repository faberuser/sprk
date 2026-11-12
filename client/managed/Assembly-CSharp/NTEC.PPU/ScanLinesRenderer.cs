using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class ScanLinesRenderer : PostProcessEffectRenderer<ScanLines>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/ScanLines"));
		RenderTexture.ReleaseTemporary(base.settings.tempRT0);
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetFloat("_Width", base.settings.Width);
		propertySheet.properties.SetFloat("_Gap", base.settings.Gap);
		base.settings.tempRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
