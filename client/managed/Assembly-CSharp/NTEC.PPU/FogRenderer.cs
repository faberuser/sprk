using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NTEC.PPU;

public sealed class FogRenderer : PostProcessEffectRenderer<Fog>
{
	public override void Render(PostProcessRenderContext context)
	{
		PropertySheet propertySheet = context.propertySheets.Get(Shader.Find("NTEC/Screen/Fog"));
		Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, renderIntoTexture: false);
		float num = (gPUProjectionMatrix[11] = 0f);
		float value = (gPUProjectionMatrix[14] = num);
		gPUProjectionMatrix[15] = value;
		value = gPUProjectionMatrix[15] + 1f;
		gPUProjectionMatrix[15] = value;
		propertySheet.properties.SetMatrix("_ProjMat", Matrix4x4.Inverse(gPUProjectionMatrix * context.camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0f, 0f, -1f * gPUProjectionMatrix[10]), Quaternion.identity, Vector3.one));
		propertySheet.properties.SetColor("_Color", base.settings.Color);
		propertySheet.properties.SetFloat("_Intensity", base.settings.Intensity);
		propertySheet.properties.SetFloat("_Distance", base.settings.Distance);
		propertySheet.properties.SetFloat("_Height", base.settings.Height);
		context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
	}
}
