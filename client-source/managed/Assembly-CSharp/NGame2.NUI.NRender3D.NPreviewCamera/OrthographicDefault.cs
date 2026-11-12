using UnityEngine;

namespace NGame2.NUI.NRender3D.NPreviewCamera;

public class OrthographicDefault : IPreviewCamera
{
	private struct CameraInfo
	{
		public float OrthoSize;

		public Vector3 Pos;

		public Vector3 Dir;
	}

	public float ZoomIn_ViewSize = 0.5f;

	public Vector3 ZoomIn_Offset = Vector3.zero;

	public float ZoomOut_ViewSize = 1.4f;

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

	void IPreviewCamera.AssignPreview(HeroPreview preview)
	{
		_preview = preview;
		_preview.MainCamera.orthographic = true;
		_preview.MainCamera.nearClipPlane = -50f;
		_preview.MainCamera.farClipPlane = 50f;
	}

	void IPreviewCamera.UpdateCamera()
	{
		if (!(_preview == null) && !(_preview.RenderFacade == null))
		{
			CameraInfo cameraInfo = CalcCameraInfo();
			_preview.MainCamera.orthographicSize = cameraInfo.OrthoSize;
			_preview.MainCamera.transform.position = cameraInfo.Pos;
			_preview.MainCamera.transform.rotation = Quaternion.LookRotation(cameraInfo.Dir);
			float num = cameraInfo.OrthoSize * 2f;
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
			OrthoSize = Mathf.Lerp(zoomInInfo.OrthoSize, zoomOutInfo.OrthoSize, ZoomOutRate),
			Pos = Vector3.Lerp(zoomInInfo.Pos, zoomOutInfo.Pos, ZoomOutRate),
			Dir = Vector3.Lerp(zoomInInfo.Dir, zoomOutInfo.Dir, ZoomOutRate)
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
			OrthoSize = ZoomOut_ViewSize,
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
			OrthoSize = ZoomIn_ViewSize,
			Pos = vector + ZoomIn_Offset,
			Dir = CalcLookDir()
		};
	}
}
