using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class IndustrialCameraRenderer : PostProcessEffectRenderer<IndustrialCamera>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/IndustrialCamera"));
		RenderTexture.ReleaseTemporary(base.settings.tempRT0);
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetFloat("_Lines", base.settings.Lines);
		base.settings.tempRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
