using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class OldFilmRenderer : PostProcessEffectRenderer<OldFilm>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/OldFilm"));
		RenderTexture.ReleaseTemporary(base.settings.tempRT0);
		RenderTexture.ReleaseTemporary(base.settings.tempRT1);
		RenderTexture.ReleaseTemporary(base.settings.tempRT2);
		propertySheet.properties.SetFloat("_Noise", base.settings.Noise);
		propertySheet.properties.SetFloat("_Oscillation", base.settings.Oscillation);
		base.settings.tempRT0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		base.settings.tempRT1 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		base.settings.tempRT2 = RenderTexture.GetTemporary(Screen.width, Screen.height);
		context.command.BlitFullscreenTriangle(context.source, base.settings.tempRT0, propertySheet, 0);
		context.command.BlitFullscreenTriangle(base.settings.tempRT0, base.settings.tempRT1, propertySheet, 1);
		context.command.BlitFullscreenTriangle(base.settings.tempRT1, base.settings.tempRT2, propertySheet, 2);
		context.command.BlitFullscreenTriangle(base.settings.tempRT2, context.destination, propertySheet, 3);
	}
}
