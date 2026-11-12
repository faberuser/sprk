using System;
using System.Collections.Generic;
using NGameAsset;
using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NGame3DWidget;

public class ContentsEffect : IContents
{
	private class EffectInstance
	{
		public Action OnComplete;

		public IAsyncLoad<IEffect> EffectAsync;

		public bool EffectLoadEnd;
	}

	private UIGame3DWidget _root;

	private string _effectCode;

	private OpenData _openData;

	private float _finalAlpha = 1f;

	private List<EffectInstance> _instanceList = new List<EffectInstance>();

	private bool _playSync;

	public ContentsEffect(UIGame3DWidget root, string effectCode)
	{
		_root = root;
		_effectCode = effectCode;
		if (!String_Extension_Method.IsNullOrEmpty(_effectCode))
		{
			Builder builder = SingletonAuto<BuilderContainer>.instance.Get(_effectCode);
			if (builder != null)
			{
				_openData = builder.CreateOpenData();
			}
		}
	}

	void IContents.Play(ITransformData extraTransform, ILocalTimer localTimer, Action onComplete, bool playSync)
	{
		if (_openData != null)
		{
			_playSync = playSync;
			EffectInstance effectInstance = new EffectInstance();
			IAsyncLoad<IEffect> asyncLoad = SingletonAuto<EffectContainer>.instance.CreateAsync(_effectCode, _root.RenderLayer, extraTransform);
			asyncLoad.Value.SetParent(_root.transform);
			asyncLoad.Value.SetTimer(localTimer);
			RenderGroupModifier2 renderGroupModifier = asyncLoad.Value.GetRenderGroupModifier();
			renderGroupModifier.SetGroundClip(enable: false);
			renderGroupModifier.SetExtraAlpha(_finalAlpha);
			effectInstance.EffectAsync = asyncLoad;
			effectInstance.OnComplete = onComplete;
			_instanceList.Add(effectInstance);
		}
	}

	void IContents.PlayPhase(string phaseName)
	{
		for (int i = 0; i < _instanceList.Count; i++)
		{
			_instanceList[i].EffectAsync.Value.PlayPhase(phaseName);
		}
		_root.SetDirty();
	}

	void IContents.Stop(bool immediatelyClear)
	{
		for (int i = 0; i < _instanceList.Count; i++)
		{
			AssetUpdateManager.DelegateReleaseEffect(_instanceList[i].EffectAsync, detachParent: true, immediatelyClear);
		}
		_instanceList.Clear();
		_root.SetDirty();
	}

	void IContents.ChainUpdate()
	{
		bool flag = false;
		for (int i = 0; i < _instanceList.Count; i++)
		{
			EffectInstance effectInstance = _instanceList[i];
			if (!_playSync)
			{
				effectInstance.EffectAsync.Value.Update();
			}
			if (!effectInstance.EffectLoadEnd && effectInstance.EffectAsync.ISDONE())
			{
				flag = true;
				effectInstance.EffectLoadEnd = true;
			}
			if (_playSync && effectInstance.EffectLoadEnd)
			{
				effectInstance.EffectAsync.Value.Update();
			}
			if (effectInstance.EffectAsync.Value.IsEnd())
			{
				EffectContainer.ReleaseEffect(effectInstance.EffectAsync, detachParent: true);
				_instanceList.RemoveAt(i);
				i--;
				Vespa_System_Action_Extension_Method.fire(effectInstance.OnComplete);
			}
		}
		if (flag)
		{
			_root.SetDirty();
		}
	}

	void IContents.ReleaseAll()
	{
		Vespa_Common_Class_Extension_Method.Interface((IContents)this).Stop(immediatelyClear: true);
		if (_openData != null)
		{
			_openData.Close();
			_openData = null;
		}
	}

	int IContents.GetInstanceCount()
	{
		return _instanceList.Count;
	}

	void IContents.UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor)
	{
		if (_instanceList.Count != 0)
		{
			for (int i = 0; i < _instanceList.Count; i++)
			{
				_instanceList[i].EffectAsync.Value.Visit(visitor);
			}
		}
	}

	void IContents.UpdateAlpha(UIWidget widget)
	{
		if (Mathf.Approximately(_finalAlpha, widget.finalAlpha))
		{
			return;
		}
		_finalAlpha = widget.finalAlpha;
		if (_instanceList.Count != 0)
		{
			for (int i = 0; i < _instanceList.Count; i++)
			{
				_instanceList[i].EffectAsync.Value.GetRenderGroupModifier().SetExtraAlpha(_finalAlpha);
			}
		}
	}

	bool IContents.ISDONE()
	{
		for (int i = 0; i < _instanceList.Count; i++)
		{
			if (!_instanceList[i].EffectAsync.ISDONE())
			{
				return false;
			}
		}
		return true;
	}
}
