using NGame2.NScene;
using NGameAsset.NSound;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public class BgmManager2 : SingletonAuto<BgmManager2>
{
	private class AssetBgm
	{
		public bool IsPlaying;

		public BgmData Bgm { get; private set; }

		public float Volume { get; set; }

		public IAsyncLoad<AudioClip> AsyncLoad { get; private set; }

		public void Set(BgmData bgm, float volume)
		{
			ReleaseBgm();
			Bgm = bgm;
			Volume = volume;
			if (Bgm != null)
			{
				AsyncLoad = SingletonAuto<AudioClipContainer>.instance.LockAudioClip(Bgm.Name, isVoice: false);
			}
			IsPlaying = false;
		}

		public void ReleaseBgm()
		{
			if (AsyncLoad != null)
			{
				AudioClipContainer.ReleaseAudioClip(AsyncLoad);
				AsyncLoad = null;
			}
			Bgm = null;
			IsPlaying = false;
		}
	}

	private Tuple<AudioClip, float> _clipFromResources = new Tuple<AudioClip, float>(null, 0f);

	private AssetBgm _assetBgm = new AssetBgm();

	private AssetBgm _eventAssetBgm = new AssetBgm();

	private bool _pendingRequest;

	private Tuple<IAsyncLoad<AudioClip>, string, float> _pendingBgm = new Tuple<IAsyncLoad<AudioClip>, string, float>(null, string.Empty, 0f);

	private Tuple<IAsyncLoad<AudioClip>, string, float> _pendingEventBgm = new Tuple<IAsyncLoad<AudioClip>, string, float>(null, string.Empty, 0f);

	public bool PendingRequest
	{
		get
		{
			return _pendingRequest;
		}
		set
		{
			_pendingRequest = value;
			if (!_pendingRequest)
			{
				FlushPendingBgm();
			}
		}
	}

	public BgmManager2()
	{
		SingletonUpdater<AssetUpdateManager>.CreateInstance();
	}

	public void ClearAll()
	{
		_assetBgm.Set(null, 0f);
		_eventAssetBgm.Set(null, 0f);
		_clipFromResources.Item1 = null;
	}

	public IAsyncLoad<AudioClip> ReserveLock(string code)
	{
		if (!VolatileOptionData.Sound_EnableBgm)
		{
			return null;
		}
		if (!SystemOptionData.Sound_EnableBgm)
		{
			return null;
		}
		BgmData bgmData = SingletonAuto<BgmDataContainer>.instance.GetBgmData(code);
		if (bgmData == null || String_Extension_Method.IsNullOrEmpty(bgmData.Name))
		{
			return null;
		}
		return SingletonAuto<AudioClipContainer>.instance.LockAudioClip(bgmData.Name, isVoice: false);
	}

	public string GetCurrentEventAssetBgmCode()
	{
		if (_eventAssetBgm != null && _eventAssetBgm.Bgm != null)
		{
			return _eventAssetBgm.Bgm.Code;
		}
		return null;
	}

	public void Request(string code, bool isEvent, float volume = 1f)
	{
		if (PendingRequest)
		{
			if (isEvent)
			{
				AudioClipContainer.ReleaseAudioClip(_pendingEventBgm.Item1);
				_pendingEventBgm.Item1 = ReserveLock(code);
				_pendingEventBgm.Item2 = ((_pendingEventBgm.Item1 != null) ? code : string.Empty);
				_pendingEventBgm.Item3 = volume;
			}
			else
			{
				AudioClipContainer.ReleaseAudioClip(_pendingBgm.Item1);
				_pendingBgm.Item1 = ReserveLock(code);
				_pendingBgm.Item2 = ((_pendingBgm.Item1 != null) ? code : string.Empty);
				_pendingBgm.Item3 = volume;
			}
		}
		else if (isEvent)
		{
			PlayEvent(code, volume);
		}
		else
		{
			Play(code, volume);
		}
	}

	public void SetVolume(float volume)
	{
		if (!String_Extension_Method.IsNullOrEmpty(_pendingEventBgm.Item2))
		{
			_pendingEventBgm.Item3 = volume;
		}
		else if (!String_Extension_Method.IsNullOrEmpty(_pendingBgm.Item2))
		{
			_pendingBgm.Item3 = volume;
		}
		else if (_eventAssetBgm.Bgm != null)
		{
			_eventAssetBgm.Volume = volume;
		}
		else if (_assetBgm.Bgm != null)
		{
			_assetBgm.Volume = volume;
		}
	}

	public void SetAssetBgmVolume(float volume)
	{
		if (_assetBgm.Bgm != null)
		{
			_assetBgm.Volume = volume;
		}
	}

	private void FlushPendingBgm()
	{
		if (!String_Extension_Method.IsNullOrEmpty(_pendingBgm.Item2))
		{
			Play(_pendingBgm.Item2, _pendingBgm.Item3);
			_pendingBgm.Item2 = string.Empty;
		}
		if (!String_Extension_Method.IsNullOrEmpty(_pendingEventBgm.Item2))
		{
			PlayEvent(_pendingEventBgm.Item2, _pendingEventBgm.Item3);
			_pendingEventBgm.Item2 = string.Empty;
		}
		if (_pendingBgm.Item1 != null)
		{
			AudioClipContainer.ReleaseAudioClip(_pendingBgm.Item1);
			_pendingBgm.Item1 = null;
		}
		if (_pendingEventBgm.Item1 != null)
		{
			AudioClipContainer.ReleaseAudioClip(_pendingEventBgm.Item1);
			_pendingEventBgm.Item1 = null;
		}
	}

	private void Play(string code, float volume = 1f)
	{
		if (VolatileOptionData.Sound_EnableBgm && SystemOptionData.Sound_EnableBgm)
		{
			BgmData bgmData = SingletonAuto<BgmDataContainer>.instance.GetBgmData(code);
			if (bgmData != null)
			{
				_clipFromResources.Item1 = null;
				_assetBgm.Set(bgmData, volume);
			}
		}
	}

	private void PlayEvent(string code, float volume)
	{
		if (VolatileOptionData.Sound_EnableBgm && SystemOptionData.Sound_EnableBgm)
		{
			BgmData bgmData = SingletonAuto<BgmDataContainer>.instance.GetBgmData(code);
			if (bgmData != null)
			{
				_eventAssetBgm.Set(bgmData, volume);
			}
		}
	}

	public void StopEvent(string code)
	{
		if (_eventAssetBgm.Bgm != null && _eventAssetBgm.Bgm.Code == code)
		{
			_eventAssetBgm.ReleaseBgm();
		}
		else if (!String_Extension_Method.IsNullOrEmpty(_pendingEventBgm.Item2) && _pendingEventBgm.Item2 == code)
		{
			_pendingEventBgm.Item2 = string.Empty;
			AudioClipContainer.ReleaseAudioClip(_pendingEventBgm.Item1);
			_pendingEventBgm.Item1 = null;
		}
	}

	public void OnSceneChanged()
	{
		if (_eventAssetBgm.Bgm != null)
		{
			if (_eventAssetBgm.IsPlaying)
			{
				_assetBgm.Set(_eventAssetBgm.Bgm, _eventAssetBgm.Volume);
			}
			_eventAssetBgm.ReleaseBgm();
		}
	}

	public void PlayFromResources(AudioClip audio, float volume)
	{
		audio = SprkLoginPresentation.FilterMusic(audio);
		if (VolatileOptionData.Sound_EnableBgm && SystemOptionData.Sound_EnableBgm && !(audio == null))
		{
			_assetBgm.Set(null, 0f);
			_clipFromResources.Item1 = audio;
			_clipFromResources.Item2 = volume;
		}
	}

	public void ChainUpdate()
	{
		if (LoadingManager.LoadingLock <= 0)
		{
			UpdateInternal();
			SingletonAuto<BgmPlayer>.instance.ChainUpdate();
		}
	}

	private void UpdateInternal()
	{
		if (_eventAssetBgm.Bgm != null)
		{
			if (_eventAssetBgm.AsyncLoad != null && _eventAssetBgm.AsyncLoad.IsDone)
			{
				if (!_eventAssetBgm.AsyncLoad.IsValid)
				{
					_eventAssetBgm.Set(null, 0f);
					return;
				}
				_eventAssetBgm.IsPlaying = true;
				SingletonAuto<BgmPlayer>.instance.Play(_eventAssetBgm.AsyncLoad.Value, _eventAssetBgm.Volume * ((float)_eventAssetBgm.Bgm.Volume * 0.001f), _eventAssetBgm.AsyncLoad);
			}
		}
		else if (_assetBgm.Bgm != null)
		{
			if (_assetBgm.AsyncLoad != null && _assetBgm.AsyncLoad.IsDone)
			{
				if (!_assetBgm.AsyncLoad.IsValid)
				{
					_assetBgm.Set(null, 0f);
					return;
				}
				_assetBgm.IsPlaying = true;
				SingletonAuto<BgmPlayer>.instance.Play(_assetBgm.AsyncLoad.Value, _assetBgm.Volume * ((float)_assetBgm.Bgm.Volume * 0.001f), _assetBgm.AsyncLoad);
			}
		}
		else if (_clipFromResources.Item1 != null)
		{
			SingletonAuto<BgmPlayer>.instance.Play(_clipFromResources.Item1, _clipFromResources.Item2, null);
		}
	}
}
