using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NBattleField;
using NGame2.NBattleView.NCamera;
using NGame2.NBattleView.NCreature.NHandler;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView;

public class CameraManager2 : SceneAutoBehaviour<CameraManager2>, GameCamera.IView, InitOnCreate, IInspectorGuiDrawer
{
	public CameraGroup CameraGroup = new CameraGroup();

	private Comparison<CameraBase> _cameraLessWeight;

	private OpeningCamera _openingCam;

	private WaveMoveCamera _waveMoveCam;

	private BattleCameraGroup _battleCameraGroup;

	private WaveEndCamera _waveEndCam;

	private FreezeCamera _freezeCam;

	private int _prevFrameCount;

	void InitOnCreate.InitializeSingleton()
	{
		_cameraLessWeight = (CameraBase lhs, CameraBase rhs) => lhs.Weight.Value.CompareTo(rhs.Weight.Value);
		_openingCam = new OpeningCamera();
		_waveEndCam = new WaveEndCamera();
		_waveMoveCam = new WaveMoveCamera();
		_freezeCam = new FreezeCamera();
		_battleCameraGroup = new BattleCameraGroup();
		CameraGroup.SetCam(_openingCam, 1f);
	}

	bool GameCamera.IView.GetCameraView(CameraStat.CameraStock cameraStock, out Vector3 posOnCameraSpace)
	{
		UpdateCamera(cameraStock);
		CameraStat cameraInfo = CameraGroup.GetCameraInfo(cameraStock);
		if (cameraInfo == null)
		{
			posOnCameraSpace = Vector3.zero;
			return false;
		}
		posOnCameraSpace = cameraInfo.GetPosOnCameraSpace(cameraStock);
		return true;
	}

	public void SwitchCam_Opening()
	{
		CameraGroup.SetCam(_openingCam, 1f);
	}

	public void SwitchCam_WaveMove(List<WaveReady> handlerList)
	{
		_waveMoveCam.SetHandlerList(handlerList);
		CameraGroup.SetCam(_waveMoveCam, 0f);
	}

	public void SwitchCam_Battle(int waveIndex = -1)
	{
		if (waveIndex == -1)
		{
			waveIndex = SingletonAuto<BattleFacade>.instance.GetBattleCache().WaveIndex;
		}
		_battleCameraGroup.SetWaveIndex(waveIndex);
		BattleCamera currentBattleCamera = _battleCameraGroup.GetCurrentBattleCamera();
		CameraGroup.SetCam(currentBattleCamera, 0f);
	}

	public void SwapBattleCamera()
	{
		BattleCamera nextBattleCamera = _battleCameraGroup.GetNextBattleCamera();
		CameraGroup.SetCam(nextBattleCamera, 0f);
	}

	public void SwitchCam_WaveEnd(int waveIndex = -1)
	{
		if (waveIndex == -1)
		{
			waveIndex = SingletonAuto<BattleFacade>.instance.GetBattleCache().WaveIndex;
		}
		if (CameraGroup.MainCamera != null && CameraGroup.MainCamera != _waveEndCam)
		{
			_waveEndCam.CopyFrom(CameraGroup.MainCamera);
			_waveEndCam.WaveIndex = waveIndex;
			CameraGroup.SetCam(_waveEndCam, 1f);
		}
	}

	public void SwitchCam_Freeze()
	{
		if (CameraGroup.MainCamera != null && CameraGroup.MainCamera != _freezeCam)
		{
			_freezeCam.CopyFrom(CameraGroup.MainCamera);
			CameraGroup.SetCam(_freezeCam, 1f);
		}
	}

	private void UpdateCamera(CameraStat.CameraStock cameraStock)
	{
		if (GlobalTimer.frameCount != _prevFrameCount)
		{
			_prevFrameCount = GlobalTimer.frameCount;
			CameraGroup.UpdateCamera();
		}
	}
}
