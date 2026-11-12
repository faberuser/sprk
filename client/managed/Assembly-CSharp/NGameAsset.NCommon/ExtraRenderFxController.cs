using System;
using NShader;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NCommon;

public class ExtraRenderFxController
{
	public interface IOwner
	{
		int Layer { get; }

		RenderGroupModifier2 RenderGroupModifier { get; }
	}

	private Camera _camera;

	private bool _enableExtraRenderFx;

	public IOwner Owner { get; private set; }

	public Transform ShaderTransformPivot { get; private set; }

	public ShaderTransformData ShaderTransform { get; private set; }

	public bool GrayscaleEnabled { get; private set; }

	public ExtraRenderFxController(IOwner owner)
	{
		Owner = owner;
	}

	public void Open(ShaderTransformData shaderTransform, Transform pivotTm)
	{
		ShaderTransformPivot = pivotTm;
		ShaderTransform = shaderTransform;
		if (ShaderTransform != null)
		{
			_camera = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(Owner.Layer);
		}
		UpdateMaterialKeywords();
	}

	public void SetGrayscale(bool isEnable)
	{
		GrayscaleEnabled = isEnable;
		UpdateMaterialKeywords();
	}

	private void UpdateMaterialKeywords()
	{
		_enableExtraRenderFx = ShaderTransform != null || GrayscaleEnabled;
		RenderGroupModifier2 renderGroupModifier = Owner.RenderGroupModifier;
		renderGroupModifier.SetMaterialKeyword(Keyword.EXTRARENDERFX_OFF, !_enableExtraRenderFx);
		renderGroupModifier.SetMaterialKeyword(Keyword.EXTRARENDERFX_ON, _enableExtraRenderFx);
	}

	public void ChainUpdate()
	{
		if (_enableExtraRenderFx)
		{
			RenderGroupModifier2 renderGroupModifier = Owner.RenderGroupModifier;
			renderGroupModifier.SetFloatProperty(NShader.Define._ExtraRenderFx_Grayscale, GrayscaleEnabled ? 1f : 0f);
			bool flag = _camera != null && ShaderTransformPivot != null && ShaderTransform != null;
			renderGroupModifier.SetFloatProperty(NShader.Define._ShaderTransform_IsEnable, flag ? 1f : 0f);
			if (flag)
			{
				Vector3 forward = _camera.transform.forward;
				Vector3 position = _camera.transform.parent.position;
				UpdateShaderTransform(forward, position);
			}
		}
	}

	private void UpdateShaderTransform(Vector3 cameraForward, Vector3 cameraParentPos)
	{
		float num = ((!Mathf.Approximately(ShaderTransform.Angle, 0f)) ? Mathf.Tan(MathF.PI / 180f * Mathf.Clamp(90f - Mathf.Abs(ShaderTransform.Angle), 20f, 70f)) : 0f);
		Vector3 position = ShaderTransformPivot.position;
		Vector3 pos = position;
		if (ShaderTransform.DepthShift)
		{
			pos += cameraForward * num * (position.y - cameraParentPos.y);
		}
		Quaternion q = Quaternion.AngleAxis(ShaderTransform.Angle, Vector3.right);
		Matrix4x4 matrix4x = Matrix4x4.TRS(-position, Quaternion.identity, Vector3.one);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(pos, q, Vector3.one);
		Matrix4x4 matrix = matrix4x2 * matrix4x;
		RenderGroupModifier2 renderGroupModifier = Owner.RenderGroupModifier;
		renderGroupModifier.SetMatrixProperty(NShader.Define._ShaderTransform_PreTransform, matrix4x);
		renderGroupModifier.SetMatrixProperty(NShader.Define._ShaderTransform_PostTransform, matrix4x2);
		renderGroupModifier.SetMatrixProperty(NShader.Define._ShaderTransform_NormalTransform, matrix);
	}
}
