using System;
using NGame2.NRender;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NRenderTargetCinema;

public class CustomizingCameraController : CameraController
{
	private ClientCustomizingCameraData _clientCustomzingCameraData;

	private int _index;

	public CustomizingCameraController(RenderTargetCinema cinema, int heroUid)
		: base(cinema, heroUid)
	{
	}

	public void ResetCamera(AccessoryPartType partType)
	{
		_clientCustomzingCameraData = GameRoot.Instance.ClientCustomizingCameraDataContainer.GetData(partType);
		if (_clientCustomzingCameraData == null)
		{
			VespaDebug.LogError("CustomizingCameraController::ResetCamera Fail " + partType);
			return;
		}
		SetRenderFacadeRotation(_clientCustomzingCameraData.CreatureRotation);
		base.ZoomOutRate = _clientCustomzingCameraData.ZoomRate;
	}

	private void SetRenderFacadeRotation(float angle)
	{
		if (angle != 0f)
		{
			RenderFacade renderFacade = GetRenderFacade();
			if (renderFacade != null)
			{
				renderFacade.Action.SetDirection(new VectorXZ(Mathf.Sin(angle * (MathF.PI / 180f)), 0f - Mathf.Cos(angle * (MathF.PI / 180f))), blendMotion: false);
				renderFacade.AnimationData.ChainUpdate();
			}
		}
	}

	public override void SetCameraInfo(int heroIndex, int costumeIndex)
	{
		ClientCreatureData clientCreatureDataByIndex = DataHelper.GetClientCreatureDataByIndex(heroIndex, costumeIndex);
		base.ZoomOutRate = 0f;
		ZoomIn_Offset = ((clientCreatureDataByIndex != null) ? NVespa.Math.Vec3FromArray(clientCreatureDataByIndex.DetailViewZoomInOffset) : Vector3.zero);
		ZoomIn_Distance = clientCreatureDataByIndex?.DetailViewZoomInDistance ?? 2f;
		ZoomOut_Offset = ((clientCreatureDataByIndex != null) ? NVespa.Math.Vec3FromArray(clientCreatureDataByIndex.DetailViewZoomOutOffset) : Vector3.zero);
		ZoomOut_Distance = clientCreatureDataByIndex?.DetailViewZoomOutDistance ?? 10f;
		LookAngleX = GameSetting.data.Ui.Hero3dView.CameraAngle;
	}

	protected override CameraInfo GetZoomOutInfo()
	{
		RenderFacade renderFacade = GetRenderFacade();
		if (renderFacade == null)
		{
			return default(CameraInfo);
		}
		Bounds shapeBounds = renderFacade.ShapeBounds;
		return new CameraInfo
		{
			Distance = ZoomOut_Distance,
			Pos = Vector3.up * shapeBounds.extents.y + ZoomOut_Offset,
			Dir = CalcLookDir()
		};
	}

	protected override CameraInfo GetZoomInInfo()
	{
		RenderFacade renderFacade = GetRenderFacade();
		if (renderFacade == null)
		{
			return GetZoomOutInfo();
		}
		if (!renderFacade.AssetCreatureAsync.ISVALID())
		{
			return GetZoomOutInfo();
		}
		return new CameraInfo
		{
			Distance = ZoomIn_Distance,
			Pos = GetZoomInPos(renderFacade) + ZoomIn_Offset,
			Dir = CalcLookDir()
		};
	}

	private Vector3 GetZoomInPos(RenderFacade renderFacade)
	{
		Bounds shapeBounds = renderFacade.ShapeBounds;
		Vector3 result = Vector3.up * shapeBounds.extents.y;
		Transform transform = renderFacade.Pivot.FindTransform(GetTransformName());
		if (transform != null)
		{
			result = transform.position;
		}
		return result;
	}

	private string GetTransformName()
	{
		if (_clientCustomzingCameraData != null)
		{
			return _clientCustomzingCameraData.TransformName;
		}
		return "Bip001 Head";
	}
}
