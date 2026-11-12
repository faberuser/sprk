using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class EightColors2Renderer : PostProcessEffectRenderer<EightColors2>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/EightColors2"));
		propertySheet.properties.SetColor("_Start", base.settings.Start);
		propertySheet.properties.SetColor("_OneEighth", base.settings.OneEighth);
		propertySheet.properties.SetColor("_TwoEights", base.settings.TwoEights);
		propertySheet.properties.SetColor("_ThreeEights", base.settings.ThreeEights);
		propertySheet.properties.SetColor("_FourEights", base.settings.FourEights);
		propertySheet.properties.SetColor("_FiveEights", base.settings.FiveEights);
		propertySheet.properties.SetColor("_SixEights", base.settings.SixEights);
		propertySheet.properties.SetColor("_SevenEights", base.settings.SevenEights);
		propertySheet.properties.SetColor("_Full", base.settings.Full);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
