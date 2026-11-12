using System;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NPreviewCamera;

public class PerspectiveDefault : IPreviewCamera
{
	private struct CameraInfo
	{
		public Vector3 Pos;

		public Vector3 Dir;

		public float Distance;
	}

	public float ZoomIn_Distance = 1.5f;

	public Vector3 ZoomIn_Offset = Vector3.zero;

	public float ZoomOut_Distance = 6f;

	public Vector3 ZoomOut_Offset = Vector3.zero;

	public float LookAngleY;

	public float LookAngleX;

	private float _zoomOutRate;

	private HeroPreview _preview;

	public float ZoomOutRate
	{
		get
		{
			return _zoomOutRate;
		}
		set
		{
			_zoomOutRate = Mathf.Clamp01(value);
		}
	}

	public float FieldOfView => GameSetting.data.Ui.HeroDetailView.FieldOfView;

	void IPreviewCamera.AssignPreview(HeroPreview preview)
	{
		_preview = preview;
		_preview.MainCamera.orthographic = false;
		_preview.MainCamera.nearClipPlane = 0.1f;
		_preview.MainCamera.farClipPlane = 50f;
	}

	void IPreviewCamera.UpdateCamera()
	{
		if (!(_preview == null) && !(_preview.RenderFacade == null))
		{
			CameraInfo cameraInfo = CalcCameraInfo();
			_preview.MainCamera.fieldOfView = FieldOfView;
			_preview.MainCamera.transform.position = cameraInfo.Pos - cameraInfo.Dir * cameraInfo.Distance;
			_preview.MainCamera.transform.rotation = Quaternion.LookRotation(cameraInfo.Dir);
			float magnitude = _preview.BackgroundRoot.transform.localPosition.magnitude;
			float num = Mathf.Tan(FieldOfView * (MathF.PI / 180f)) * magnitude;
			if (_preview.Info.RenderTargetWidth > _preview.Info.RenderTargetHeight)
			{
				num = num * (float)_preview.Info.RenderTargetWidth / (float)_preview.Info.RenderTargetHeight;
			}
			_preview.BackgroundRoot.transform.localScale = Vector3.one * num;
		}
	}

	private CameraInfo CalcCameraInfo()
	{
		if (ZoomOutRate < Mathf.Epsilon)
		{
			return GetZoomInInfo();
		}
		if (ZoomOutRate > 1f - Mathf.Epsilon)
		{
			return GetZoomOutInfo();
		}
		CameraInfo zoomOutInfo = GetZoomOutInfo();
		CameraInfo zoomInInfo = GetZoomInInfo();
		return new CameraInfo
		{
			Pos = Vector3.Lerp(zoomInInfo.Pos, zoomOutInfo.Pos, ZoomOutRate),
			Dir = Vector3.Lerp(zoomInInfo.Dir, zoomOutInfo.Dir, ZoomOutRate),
			Distance = Mathf.Lerp(zoomInInfo.Distance, zoomOutInfo.Distance, ZoomOutRate)
		};
	}

	private Vector3 CalcLookDir()
	{
		return Quaternion.AngleAxis(LookAngleY, Vector3.up) * Quaternion.AngleAxis(LookAngleX, Vector3.right) * Vector3.forward;
	}

	private CameraInfo GetZoomOutInfo()
	{
		Bounds shapeBounds = _preview.RenderFacade.ShapeBounds;
		return new CameraInfo
		{
			Distance = ZoomOut_Distance,
			Pos = Vector3.up * shapeBounds.extents.y + ZoomOut_Offset,
			Dir = CalcLookDir()
		};
	}

	private CameraInfo GetZoomInInfo()
	{
		if (!_preview.RenderFacade.AssetCreatureAsync.ISVALID())
		{
			return GetZoomOutInfo();
		}
		Bounds shapeBounds = _preview.RenderFacade.ShapeBounds;
		Transform transform = _preview.RenderFacade.Pivot.FindTransform("Bip001 Head");
		Vector3 vector = Vector3.up * shapeBounds.extents.y;
		if (transform != null)
		{
			vector = transform.position;
		}
		return new CameraInfo
		{
			Distance = ZoomIn_Distance,
			Pos = vector + ZoomIn_Offset,
			Dir = CalcLookDir()
		};
	}
}
