using NGame2.NRender;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NRenderTargetCinema;

public class CameraController
{
	protected struct CameraInfo
	{
		public Vector3 Pos;

		public Vector3 Dir;

		public float Distance;
	}

	public Vector3 ExtraOffsetByDistance;

	public float ZoomIn_Distance = 1.5f;

	public Vector3 ZoomIn_Offset = Vector3.zero;

	public float ZoomOut_Distance = 6f;

	public Vector3 ZoomOut_Offset = Vector3.zero;

	public float LookAngleY;

	public float LookAngleX;

	protected RenderTargetCinema _cinema;

	private int _heroUid;

	private float _zoomOutRate;

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

	private RenderFacade _renderFacade => GetRenderFacade();

	public CameraController(RenderTargetCinema cinema, int heroUid)
	{
		_cinema = cinema;
		_heroUid = heroUid;
	}

	public virtual void SetCameraInfo(int heroIndex, int costumeIndex)
	{
		ClientCreatureData clientCreatureDataByIndex = DataHelper.GetClientCreatureDataByIndex(heroIndex, costumeIndex);
		ZoomOutRate = 0f;
		ZoomIn_Offset = ((clientCreatureDataByIndex != null) ? Math.Vec3FromArray(clientCreatureDataByIndex.DetailViewZoomInOffset) : Vector3.zero);
		ZoomIn_Distance = clientCreatureDataByIndex?.DetailViewZoomInDistance ?? 2f;
		ZoomOut_Offset = ((clientCreatureDataByIndex != null) ? Math.Vec3FromArray(clientCreatureDataByIndex.DetailViewZoomOutOffset) : Vector3.zero);
		ZoomOut_Distance = clientCreatureDataByIndex?.DetailViewZoomOutDistance ?? 10f;
		LookAngleX = GameSetting.data.Ui.Hero3dView.CameraAngle;
	}

	public virtual bool ChainLateUpdate()
	{
		if (_cinema == null)
		{
			return false;
		}
		if (_cinema.CinemaCamera == null)
		{
			return false;
		}
		RenderFacade renderFacade = GetRenderFacade();
		if (renderFacade == null)
		{
			return false;
		}
		if (!renderFacade.AssetCreatureAsync.ISDONE())
		{
			return false;
		}
		CameraInfo cameraInfo = CalcCameraInfo();
		_cinema.CinemaCamera.CameraInfo.FieldOfView = FieldOfView;
		_cinema.CinemaCamera.CameraInfo.TargetPos = cameraInfo.Pos + ExtraOffsetByDistance * cameraInfo.Distance;
		_cinema.CinemaCamera.CameraInfo.LookDir = cameraInfo.Dir;
		_cinema.CinemaCamera.CameraInfo.TargetDistance = cameraInfo.Distance;
		_cinema.CinemaCamera.ChainUpdate();
		return true;
	}

	protected virtual RenderFacade GetRenderFacade()
	{
		return _cinema.GETRENDERFACADE(_heroUid);
	}

	protected CameraInfo CalcCameraInfo()
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

	protected Vector3 CalcLookDir()
	{
		return Quaternion.AngleAxis(LookAngleY, Vector3.up) * Quaternion.AngleAxis(LookAngleX, Vector3.right) * Vector3.forward;
	}

	protected virtual CameraInfo GetZoomOutInfo()
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

	protected virtual CameraInfo GetZoomInInfo()
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
		Bounds shapeBounds = renderFacade.ShapeBounds;
		Transform transform = renderFacade.Pivot.FindTransform("Bip001 Head");
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
