using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NShader;

public sealed class GrayscaleRenderer : PostProcessEffectRenderer<Grayscale>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("Vespa/Filter/GrayscaleBlend"));
		propertySheet.properties.SetFloat("_Blend", base.settings.blend);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
