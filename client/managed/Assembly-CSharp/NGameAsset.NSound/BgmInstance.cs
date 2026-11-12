using NVespa;
using UnityEngine;

namespace NGameAsset.NSound;

public class BgmInstance
{
	public enum EStatus
	{
		Ready,
		FadeOut,
		Play,
		Stop
	}

	public float Volume;

	private float _fadeElaspedTime;

	public AudioClip Clip { get; private set; }

	public IAsyncLoad<AudioClip> AsyncLoad { get; private set; }

	public AudioManager.AudioInstance AudioInstance { get; private set; }

	public EStatus Status { get; private set; }

	public void Open(AudioClip clip, float volume, IAsyncLoad<AudioClip> asyncLoad)
	{
		Clip = clip;
		Volume = volume;
		if (asyncLoad != null)
		{
			AsyncLoad = SingletonAuto<AudioClipContainer>.instance.LockAudioClip(asyncLoad);
		}
		Status = EStatus.Ready;
		_fadeElaspedTime = 0f;
		AudioInstance = SingletonUpdater<AudioManager>.instance.CreateInstance(clip, EAudioMixerFlag.Bgm);
		if (AudioInstance != null)
		{
			AudioInstance.Volume = Volume;
		}
	}

	public void Close()
	{
		if (AudioInstance != null)
		{
			AudioManager.ReleaseInstance(AudioInstance);
			AudioInstance = null;
		}
		if (AsyncLoad != null)
		{
			AudioClipContainer.ReleaseAudioClip(AsyncLoad);
			AsyncLoad = null;
		}
		Status = EStatus.Stop;
		Volume = 0f;
	}

	public void Play()
	{
		if (AudioInstance != null)
		{
			AudioInstance.Volume = Volume;
			Status = EStatus.Play;
			AudioInstance.Play(isLoop: true);
		}
		else
		{
			Status = EStatus.Stop;
		}
	}

	public void Stop()
	{
		if (Status != EStatus.FadeOut)
		{
			Status = EStatus.FadeOut;
			_fadeElaspedTime = 0f;
			if (AudioInstance != null)
			{
				Volume = AudioInstance.Volume;
			}
		}
	}

	public void Update()
	{
		if (AudioInstance == null)
		{
			return;
		}
		_fadeElaspedTime += UnitySystemTimer.deltaTime;
		if (Status == EStatus.Ready)
		{
			return;
		}
		if (Status == EStatus.FadeOut)
		{
			float num = _fadeElaspedTime / GameSharedSetting.data.Sound.BgmFadeOutTime;
			float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSharedSetting.data.Sound.BgmFadeOutCurve, num, 1f - num);
			float num3 = Volume * num2;
			if (num3 <= 0f)
			{
				Status = EStatus.Stop;
				num3 = 0f;
			}
			AudioInstance.Volume = num3;
		}
		else if (Status == EStatus.Play)
		{
			if (AudioInstance.Volume != Volume)
			{
				AudioInstance.Volume = Volume;
			}
		}
		else
		{
			_ = Status;
			_ = 3;
		}
	}
}
