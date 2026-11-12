using System.Collections.Generic;
using NGameAsset;
using NGameAsset.NSound;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UIPlayAssetSound : MonoBehaviour, IInspectorGuiDrawer
{
	private class UILocalTimer : ILocalTimer
	{
		int ILocalTimer.deltaTick => GlobalTimer.deltaTick;

		int ILocalTimer.playbackSpeed
		{
			get
			{
				if (!GlobalTimer.pause)
				{
					return 1000;
				}
				return 0;
			}
		}

		bool ILocalTimer.pause => GlobalTimer.pause;

		ETimerEventFlag ILocalTimer.TotalEventFlag => (ETimerEventFlag)0;

		EAudioMixerFlag ILocalTimer.TotalAudioMixerFlag => EAudioMixerFlag.None;

		int ILocalTimer.GetPlaybackSpeedRecursively(int depth)
		{
			if (!GlobalTimer.pause)
			{
				return 1000;
			}
			return 0;
		}

		bool ILocalTimer.GetPauseRecursively(int depth)
		{
			return GlobalTimer.pause;
		}
	}

	private static UILocalTimer UITimer = new UILocalTimer();

	[HideInInspector]
	public string Code = string.Empty;

	private List<IAsyncLoad<AudioClip>> _clipAsyncList = new List<IAsyncLoad<AudioClip>>();

	private EventDelegate _playEventDelegate;

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit && !__Extension_Method_ICollection__.IsNullOrEmpty(_clipAsyncList))
		{
			for (int i = 0; i < _clipAsyncList.Count; i++)
			{
				AudioClipContainer.ReleaseAudioClip(_clipAsyncList[i]);
			}
			_clipAsyncList.Clear();
		}
	}

	private void Awake()
	{
		if (_playEventDelegate == null)
		{
			_playEventDelegate = new EventDelegate(Play);
		}
		UIButton component = GetComponent<UIButton>();
		if (component != null)
		{
			component.onClick.Add(_playEventDelegate);
		}
		UIToggle component2 = GetComponent<UIToggle>();
		if (component2 != null)
		{
			component2.onChange.Add(_playEventDelegate);
		}
		_clipAsyncList = SingletonAuto<SoundContainer>.instance.ReserveSound(Code, _clipAsyncList);
	}

	public void Play()
	{
		SoundStock stock = new SoundStock
		{
			Code = Code,
			LocalTimer = UITimer
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}
}
