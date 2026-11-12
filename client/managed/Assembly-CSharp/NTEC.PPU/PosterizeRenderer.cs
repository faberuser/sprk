using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class PosterizeRenderer : PostProcessEffectRenderer<Posterize>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Posterize"));
		propertySheet.properties.SetFloat("_Colors", (int)base.settings.Colors);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
