using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class PixelizeRenderer : PostProcessEffectRenderer<Pixelize>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Pixelize"));
		propertySheet.properties.SetFloat("_Horizontal", (int)base.settings.Horizontal);
		propertySheet.properties.SetFloat("_Vertical", (int)base.settings.Vertical);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
