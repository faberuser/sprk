using System;
using NGame2.NUI.NGame3DWidget;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NGame3DWidgetFromRes;

public class ContentsSound : IContents
{
	private class SoundInstance
	{
		public Action OnComplete;

		public AudioManager.AudioInstance Value;
	}

	private UIGame3DWidgetFromRes _root;

	private AudioClip _audioClip;

	private float _delayTime;

	private bool _isPlaying;

	private bool _isWaiting;

	private float _waitingTime;

	public ContentsSound(UIGame3DWidgetFromRes root, UnityEngine.Object resourceObject, float delayTime)
	{
		_root = root;
		_audioClip = resourceObject as AudioClip;
		_delayTime = delayTime;
	}

	void IContents.Play(TransformData extraTransform, ILocalTimer localTimer, Action onComplete)
	{
		if (SystemOptionData.Sound_EnableGameSound && !(_audioClip == null))
		{
			_isPlaying = true;
			if (_delayTime < Mathf.Epsilon)
			{
				SingletonUpdater<AudioManager>.instance.Play(_audioClip, 1f, EAudioMixerFlag.None);
				_isWaiting = false;
			}
			else
			{
				_isWaiting = true;
				_waitingTime = 0f;
			}
		}
	}

	void IContents.PlayPhase(string phaseName)
	{
	}

	void IContents.Stop(bool immediatelyClear)
	{
		if (_isPlaying)
		{
			_isPlaying = false;
		}
	}

	void IContents.ChainUpdate()
	{
		if (_isPlaying && _isWaiting)
		{
			_waitingTime += UnitySystemTimer.deltaTime;
			if (_waitingTime >= _delayTime)
			{
				_isWaiting = false;
				SingletonUpdater<AudioManager>.instance.Play(_audioClip, 1f, EAudioMixerFlag.None);
			}
		}
	}

	void IContents.ReleaseAll()
	{
		Vespa_Common_Class_Extension_Method.Interface((IContents)this).Stop(immediatelyClear: true);
		_audioClip = null;
	}

	int IContents.GetInstanceCount()
	{
		return 0;
	}

	void IContents.UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor)
	{
	}
}
