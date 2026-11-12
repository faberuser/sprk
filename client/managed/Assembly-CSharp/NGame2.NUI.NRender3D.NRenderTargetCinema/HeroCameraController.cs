using NGame2.NRender;

namespace NGame2.NUI.NRender3D.NRenderTargetCinema;

public class HeroCameraController : CameraController
{
	private CinemaCamera _cinemaCamera;

	private RenderFacade _renderFacade;

	public HeroCameraController(RenderTargetCinema cinema, int heroUid, CinemaCamera cinemaCamera, RenderFacade renderFacade)
		: base(cinema, heroUid)
	{
		_cinemaCamera = cinemaCamera;
		_renderFacade = renderFacade;
	}

	protected override RenderFacade GetRenderFacade()
	{
		return _renderFacade;
	}

	public override bool ChainLateUpdate()
	{
		if (_cinemaCamera == null)
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
		_cinemaCamera.CameraInfo.FieldOfView = base.FieldOfView;
		_cinemaCamera.CameraInfo.TargetPos = cameraInfo.Pos + ExtraOffsetByDistance * cameraInfo.Distance;
		_cinemaCamera.CameraInfo.LookDir = cameraInfo.Dir;
		_cinemaCamera.CameraInfo.TargetDistance = cameraInfo.Distance;
		_cinemaCamera.ChainUpdate();
		return true;
	}
}
