using System;
using System.Collections.Generic;
using NGameAsset;
using NGameAsset.NSound;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NGame3DWidget;

public class ContentsSound : IContents
{
	private class SoundInstance
	{
		public Action OnComplete;

		public ISound Sound;
	}

	private UIGame3DWidget _root;

	private string _soundCode;

	private List<IAsyncLoad<AudioClip>> _audioClipList = new List<IAsyncLoad<AudioClip>>();

	private List<SoundInstance> _instanceList = new List<SoundInstance>();

	public ContentsSound(UIGame3DWidget root, string soundCode)
	{
		_root = root;
		_soundCode = soundCode;
		if (!String_Extension_Method.IsNullOrEmpty(_soundCode))
		{
			_audioClipList = SingletonAuto<SoundContainer>.instance.ReserveSound(_soundCode);
		}
	}

	void IContents.Play(ITransformData extraTransform, ILocalTimer localTimer, Action onComplete, bool playSync)
	{
		SoundStock stock = new SoundStock
		{
			Code = _soundCode,
			LocalTimer = localTimer
		};
		SoundInstance soundInstance = new SoundInstance();
		soundInstance.Sound = SingletonAuto<SoundContainer>.instance.CreateSound(stock);
		soundInstance.OnComplete = onComplete;
		_instanceList.Add(soundInstance);
	}

	void IContents.PlayPhase(string phaseName)
	{
	}

	void IContents.Stop(bool immediatelyClear)
	{
		for (int i = 0; i < _instanceList.Count; i++)
		{
			SoundContainer.ReleaseSound(_instanceList[i].Sound);
		}
		_instanceList.Clear();
	}

	void IContents.ChainUpdate()
	{
		for (int i = 0; i < _instanceList.Count; i++)
		{
			SoundInstance soundInstance = _instanceList[i];
			soundInstance.Sound.Update();
			if (soundInstance.Sound.IsEnd())
			{
				Vespa_System_Action_Extension_Method.fire(soundInstance.OnComplete);
				SoundContainer.ReleaseSound(soundInstance.Sound);
				_instanceList.RemoveAt(i);
				i--;
			}
		}
	}

	void IContents.ReleaseAll()
	{
		Vespa_Common_Class_Extension_Method.Interface((IContents)this).Stop(immediatelyClear: true);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_audioClipList))
		{
			for (int i = 0; i < _audioClipList.Count; i++)
			{
				AudioClipContainer.ReleaseAudioClip(_audioClipList[i]);
			}
			_audioClipList.Clear();
		}
	}

	int IContents.GetInstanceCount()
	{
		return _instanceList.Count;
	}

	void IContents.UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor)
	{
	}

	void IContents.UpdateAlpha(UIWidget widget)
	{
	}

	bool IContents.ISDONE()
	{
		return false;
	}
}
