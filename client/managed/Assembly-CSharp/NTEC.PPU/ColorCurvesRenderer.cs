using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class ColorCurvesRenderer : PostProcessEffectRenderer<ColorCurves>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/ColorCurves"));
		base.settings.CreateEvaluations();
		propertySheet.properties.SetTexture("_Red", base.settings.RedEVALUATION);
		propertySheet.properties.SetTexture("_Green", base.settings.GreenEVALUATION);
		propertySheet.properties.SetTexture("_Blue", base.settings.BlueEVALUATION);
		propertySheet.properties.SetColor("_Tint", base.settings.Tint);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
