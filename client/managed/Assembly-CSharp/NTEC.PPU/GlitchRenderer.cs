using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class GlitchRenderer : PostProcessEffectRenderer<Glitch>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Glitch"));
		propertySheet.properties.SetFloat("_Speed", base.settings.Speed);
		propertySheet.properties.SetFloat("_Size", base.settings.Size);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
