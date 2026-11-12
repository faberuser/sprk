using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class DrunkRenderer : PostProcessEffectRenderer<Drunk>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Drunk"));
		propertySheet.properties.SetFloat("_Horizontal", base.settings.Horizontal);
		propertySheet.properties.SetFloat("_Vertical", base.settings.Vertical);
		propertySheet.properties.SetFloat("_Speed", base.settings.Speed);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
