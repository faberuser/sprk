using System;
using System.IO;
using System.Linq;
using NGame2.NDevice;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NGameAsset.NPatch;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Video;

namespace NGame2.NUI.NWindow;

public class VideoManager : SingletonUpdater<VideoManager>
{
	public enum State
	{
		INIT,
		PREPARE,
		WAIT_OPENNING_SCENE_CHANGER,
		PLAYING,
		WAIT_CLOSING_SCENE_CHANGER,
		STOPPING,
		IDLE
	}

	public State _state = State.IDLE;

	public VideoPlayer VideoPlayer;

	private SceneChanger openSceneChanger;

	private SceneChanger closeSceneChanger;

	private LayerInfo GlobalUILayerInfo;

	private LayerInfo GlobalVideoLayerInfo;

	private int prepareMaxTryCount = 20;

	private int prepareTryCount;

	private int prepareCheckCount;

	private float elapsedTime;

	private int fps;

	private Action finishCallback;

	private bool isSkipped;

	private void SetNextState(State state)
	{
		_state = state;
	}

	public static void Open_Path(string videoRootPath)
	{
		SingletonUpdater<VideoManager>.instance.OpenInternal(videoRootPath);
	}

	public static void Open_VideoCode(string videoName)
	{
		SingletonUpdater<VideoManager>.instance.OpenInternal_VideoCode(videoName);
	}

	private void OpenInternal(string rootPath)
	{
		Init(rootPath, isWebUrl: false);
	}

	private void OpenInternal_VideoCode(string videoCode)
	{
		string videoPath = GameRoot.Instance.VideoDataContainer.GetVideoPath(videoCode);
		if (String_Extension_Method.IsNullOrEmpty(videoPath))
		{
			VespaDebug.LogError($"Invalid Video Code : {videoCode}");
		}
		else
		{
			Init(videoPath, isWebUrl: false);
		}
	}

	public static void OpenURL(string videoName)
	{
		SingletonUpdater<VideoManager>.instance.OpenURLInternal(videoName);
	}

	private void OpenURLInternal(string videoName)
	{
		Init(videoName, isWebUrl: true);
	}

	public static bool IsPlaying()
	{
		return SingletonUpdater<VideoManager>.instance._state != State.IDLE;
	}

	public static void SkipToClose(Action endCallback)
	{
		OnSkipButtonPressed(endCallback)();
	}

	private static Action OnSkipButtonPressed(Action endCallback)
	{
		return delegate
		{
			SingletonUpdater<VideoManager>.instance.finishCallback = endCallback;
			SingletonUpdater<VideoManager>.instance.isSkipped = true;
		};
	}

	private void EnableVideoCamera(bool enableVideoCamera)
	{
		UIHelper.EnableGameObject(GameUIRoot.instance.CameraGroup, !enableVideoCamera);
	}

	private void ArrangeDepth()
	{
		if (GlobalUILayerInfo != null && GlobalUILayerInfo.Camera != null && GlobalVideoLayerInfo != null && GlobalVideoLayerInfo.Camera != null)
		{
			GlobalUILayerInfo.Camera.clearFlags = CameraClearFlags.Depth;
			GlobalVideoLayerInfo.Camera.clearFlags = CameraClearFlags.Depth;
		}
	}

	private void ResetValues()
	{
		prepareTryCount = 0;
		prepareCheckCount = 0;
		elapsedTime = 0f;
		fps = 0;
		finishCallback = null;
		isSkipped = false;
	}

	public void Init(string videoName, bool isWebUrl)
	{
		if (_state == State.IDLE)
		{
			SetNextState(State.INIT);
			ResetValues();
			GlobalUILayerInfo = GlobalUIRoot.instance.GetLayerInfo(GlobalUIRoot.ELayerType.GlobalUI);
			GlobalVideoLayerInfo = GlobalUIRoot.instance.GetLayerInfo(GlobalUIRoot.ELayerType.GlobalVideo);
			VideoPlayer = GlobalVideoLayerInfo.Camera.gameObject.GetComponent<VideoPlayer>();
			if (VideoPlayer == null)
			{
				VideoPlayer = GlobalVideoLayerInfo.Camera.gameObject.AddComponent<VideoPlayer>();
			}
			openSceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(100f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
			WaitingPopup.OpenWithLayer(this, GlobalUIRoot.ELayerType.GlobalUI);
			VolatileOptionData.Sound_EnableBgm = false;
			string url = videoName;
			if (!isWebUrl)
			{
				url = ((DeviceHelper.GetAndroidSDKInt() < 29) ? ("file://" + GetVideoAssetPath(videoName)) : GetVideoAssetPath(videoName));
			}
			bool sound_EnableBgm = SystemOptionData.Sound_EnableBgm;
			float sound_BgmVolume = SystemOptionData.Sound_BgmVolume;
			VideoPlayer.source = VideoSource.Url;
			VideoPlayer.url = url;
			VideoPlayer.isLooping = false;
			VideoPlayer.playOnAwake = false;
			VideoPlayer.waitForFirstFrame = false;
			VideoPlayer.skipOnDrop = true;
			VideoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
			VideoPlayer.targetCamera = GlobalVideoLayerInfo.Camera;
			VideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
			VideoPlayer.controlledAudioTrackCount = 1;
			VideoPlayer.EnableAudioTrack(0, enabled: true);
			VideoPlayer.SetDirectAudioVolume(0, sound_BgmVolume);
			VideoPlayer.SetDirectAudioMute(0, !sound_EnableBgm);
			VideoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
			VideoPlayer.started += VideoPlayer_started;
			VideoPlayer.frameDropped += VideoPlayer_frameDropped;
			VideoPlayer.seekCompleted += VideoPlayer_seekCompleted;
			VideoPlayer.loopPointReached += VideoPlayer_loopPointReached;
			VideoPlayer.frameReady += VideoPlayer_frameReady;
			VideoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;
			VideoPlayer.errorReceived += VideoPlayer_errorReceived;
			VideoPlayer.clockResyncOccurred += VideoPlayer_clockResyncOccurred;
			_ = VideoPlayer.frameRate;
			GC.Collect();
			EnableVideoCamera(enableVideoCamera: true);
			VideoPlayer.Prepare();
			SetNextState(State.PREPARE);
		}
	}

	private void Prepare()
	{
		if (!VideoPlayer.isPrepared)
		{
			prepareCheckCount++;
			if (prepareCheckCount > 60)
			{
				prepareCheckCount = 0;
				VideoPlayer.Prepare();
				prepareTryCount++;
			}
			if (prepareTryCount > prepareMaxTryCount)
			{
				SetNextState(State.STOPPING);
			}
		}
		else if (VideoPlayer.isPrepared)
		{
			SetNextState(State.WAIT_OPENNING_SCENE_CHANGER);
		}
	}

	private void WaitOpenSceneChanger()
	{
		if (openSceneChanger == null || openSceneChanger.IsCompleteAnimation())
		{
			VideoPlayer.Play();
			ArrangeDepth();
			WaitingPopup.Close(this);
			if (openSceneChanger != null)
			{
				openSceneChanger.Close();
				openSceneChanger = null;
			}
			SetNextState(State.PLAYING);
		}
	}

	private void Play()
	{
		bool flag = false;
		ArrangeDepth();
		if ((float)VideoPlayer.frame >= (float)VideoPlayer.frameCount - 1f * (float)fps)
		{
			flag = true;
		}
		elapsedTime += Time.deltaTime;
		if ((double)elapsedTime > VideoPlayer.length - 1.0)
		{
			flag = true;
		}
		if (flag || isSkipped)
		{
			SetNextState(State.STOPPING);
		}
	}

	private void Stop()
	{
		if (openSceneChanger != null)
		{
			openSceneChanger.Close();
			openSceneChanger = null;
		}
		WaitingPopup.Close(this);
		closeSceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(10f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		SetNextState(State.WAIT_CLOSING_SCENE_CHANGER);
	}

	private void WaitCloseSceneChanger()
	{
		if (closeSceneChanger == null || closeSceneChanger.IsCompleteAnimation())
		{
			VideoPlayer.Stop();
			EnableVideoCamera(enableVideoCamera: false);
			Vespa_System_Action_Extension_Method.fire(finishCallback);
			VolatileOptionData.Sound_EnableBgm = true;
			ArrangeDepth();
			GlobalAutoBehaviour<GlobalCameraManager>.instance.AdjustCameraRenderAndClearFlags();
			if (closeSceneChanger != null)
			{
				closeSceneChanger.Close();
				closeSceneChanger = null;
			}
			SetNextState(State.IDLE);
		}
	}

	private void VideoPlayer_started(VideoPlayer source)
	{
	}

	private void VideoPlayer_frameDropped(VideoPlayer source)
	{
	}

	private void VideoPlayer_seekCompleted(VideoPlayer source)
	{
	}

	private void VideoPlayer_loopPointReached(VideoPlayer source)
	{
	}

	private void VideoPlayer_frameReady(VideoPlayer source, long frameIdx)
	{
	}

	private void VideoPlayer_prepareCompleted(VideoPlayer source)
	{
	}

	private void VideoPlayer_clockResyncOccurred(VideoPlayer source, double seconds)
	{
	}

	private void VideoPlayer_errorReceived(VideoPlayer source, string message)
	{
		VespaDebug.LogError(message);
	}

	public string GetVideoAssetPath(string path)
	{
		string subPath = String_Extension_Method.CombinePath(GetSpecPath(SystemOptionData.VideoSpec), path);
		string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, subPath);
		if (File.Exists(downloadedAssetPath))
		{
			return downloadedAssetPath;
		}
		return FindAnotherSpec(path);
	}

	public override void ChainUpdate()
	{
		switch (_state)
		{
		case State.IDLE:
			break;
		case State.INIT:
			break;
		case State.PREPARE:
			Prepare();
			break;
		case State.PLAYING:
			Play();
			break;
		case State.STOPPING:
			Stop();
			break;
		case State.WAIT_OPENNING_SCENE_CHANGER:
			WaitOpenSceneChanger();
			break;
		case State.WAIT_CLOSING_SCENE_CHANGER:
			WaitCloseSceneChanger();
			break;
		}
	}

	private string GetSpecPath(EVideoSpecType eType)
	{
		switch (eType)
		{
		case EVideoSpecType.Undefine:
		case EVideoSpecType.Low:
			return "Video/Low/";
		case EVideoSpecType.Mid:
			return "Video/Mid/";
		case EVideoSpecType.High:
			return "Video/High/";
		case EVideoSpecType.Original:
			return "Video/Origin/";
		default:
			return "";
		}
	}

	private string FindAnotherSpec(string strPath)
	{
		int num = Enum.GetValues(typeof(EVideoSpecType)).Cast<int>().Max();
		int num2 = Enum.GetValues(typeof(EVideoSpecType)).Cast<int>().Min();
		bool num3 = SystemOptionData.VideoSpec == (EVideoSpecType)num;
		int num4 = ((!num3) ? 1 : (-1));
		int num5 = (num3 ? num2 : (num + 1));
		for (int i = (num3 ? num : (num2 + 1)); i != num5; i += num4)
		{
			EVideoSpecType eVideoSpecType = (EVideoSpecType)i;
			if (eVideoSpecType != SystemOptionData.VideoSpec)
			{
				string subPath = String_Extension_Method.CombinePath(GetSpecPath(eVideoSpecType), strPath);
				string downloadedAssetPath = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, subPath);
				if (File.Exists(downloadedAssetPath))
				{
					return downloadedAssetPath;
				}
			}
		}
		Debug.LogError($"No File \"{strPath}\"");
		return string.Empty;
	}
}
