using System;
using System.Collections.Generic;
using NGame2.NUI.NGame3DWidget;
using NGameAsset;
using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NGame3DWidgetFromRes;

public class ContentsEffect : IContents
{
	private class EffectInstance
	{
		public Action OnComplete;

		public NGameAsset.NEffect.EffectInstance Value;
	}

	private UIGame3DWidgetFromRes _root;

	private OpenDataFromResource _openData;

	private List<EffectInstance> _instanceList = new List<EffectInstance>();

	public ContentsEffect(UIGame3DWidgetFromRes root, UnityEngine.Object resourceObject)
	{
		_root = root;
		_openData = GetOpenData(resourceObject);
		if (_openData != null)
		{
			_openData.Initialize();
		}
	}

	private OpenDataFromResource GetOpenData(UnityEngine.Object resourceObject)
	{
		GameObject gameObject = resourceObject as GameObject;
		if (gameObject != null)
		{
			return gameObject.GetComponent<OpenDataFromResource>();
		}
		OpenDataFromResource openDataFromResource = resourceObject as OpenDataFromResource;
		if (openDataFromResource != null)
		{
			return openDataFromResource;
		}
		return null;
	}

	void IContents.Play(TransformData extraTransform, ILocalTimer localTimer, Action onComplete)
	{
		if (!(_openData == null))
		{
			EffectInstance effectInstance = new EffectInstance();
			effectInstance.Value = _openData.CreateEffect(_root.gameObject.layer, extraTransform);
			if (effectInstance.Value != null)
			{
				Vespa_Common_Class_Extension_Method.Interface((IEffect)effectInstance.Value).SetParent(_root.transform);
				Vespa_Common_Class_Extension_Method.Interface((IEffect)effectInstance.Value).SetTimer(localTimer);
			}
			effectInstance.OnComplete = onComplete;
			_root.SetDirty();
			_instanceList.Add(effectInstance);
		}
	}

	void IContents.PlayPhase(string phaseName)
	{
		for (int i = 0; i < _instanceList.Count; i++)
		{
			EffectInstance effectInstance = _instanceList[i];
			if (effectInstance.Value != null)
			{
				Vespa_Common_Class_Extension_Method.Interface((IEffect)effectInstance.Value).PlayPhase(phaseName);
			}
		}
		_root.SetDirty();
	}

	void IContents.Stop(bool immediatelyClear)
	{
		for (int i = 0; i < _instanceList.Count; i++)
		{
			if (_instanceList[i].Value != null)
			{
				AssetUpdateManager.DelegateReleaseEffect(_instanceList[i].Value);
			}
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
			if (effectInstance.Value != null)
			{
				Vespa_Common_Class_Extension_Method.Interface((IEffect)effectInstance.Value).Update();
				if (Vespa_Common_Class_Extension_Method.Interface((IEffect)effectInstance.Value).IsEnd())
				{
					Vespa_System_Action_Extension_Method.fire(effectInstance.OnComplete);
					_instanceList.RemoveAt(i);
					i--;
					AssetUpdateManager.DelegateReleaseEffect(effectInstance.Value);
					flag = true;
				}
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
			_openData.Release();
		}
	}

	int IContents.GetInstanceCount()
	{
		return _instanceList.Count;
	}

	void IContents.UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor)
	{
		if (_instanceList.Count == 0)
		{
			return;
		}
		for (int i = 0; i < _instanceList.Count; i++)
		{
			EffectInstance effectInstance = _instanceList[i];
			if (effectInstance.Value != null)
			{
				Vespa_Common_Class_Extension_Method.Interface((IEffect)effectInstance.Value).GetRenderGroupModifier().SetExtraAlpha(panel.alpha);
				Vespa_Common_Class_Extension_Method.Interface((IEffect)effectInstance.Value).Visit(visitor);
			}
		}
	}
}
