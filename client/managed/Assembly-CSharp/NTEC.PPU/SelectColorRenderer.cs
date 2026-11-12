using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class SelectColorRenderer : PostProcessEffectRenderer<SelectColor>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/SelectColor"));
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetColor("_Selected", base.settings.Selected);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
