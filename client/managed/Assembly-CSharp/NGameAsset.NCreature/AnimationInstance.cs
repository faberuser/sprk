using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class AnimationInstance
{
	private Animation _component;

	private IAnimationOpenData _openData;

	private IncreaseDic<string, ClipMappingData> _clipMappingDic = new IncreaseDic<string, ClipMappingData>();

	private Dictionary<string, bool> _registedClip = new Dictionary<string, bool>();

	public AnimationInstance(Animation component, IAnimationOpenData openData)
	{
		_component = component;
		_component.playAutomatically = false;
		_component.cullingType = AnimationCullingType.AlwaysAnimate;
		_openData = openData;
	}

	public void ApplyTo(ICreature other)
	{
		for (int i = 0; i < _clipMappingDic.Count; i++)
		{
			ClipMappingData valueByIndex = _clipMappingDic.GetValueByIndex(i);
			other.SetAnimation(new AnimationStock
			{
				Key = valueByIndex.Key,
				IsMain = true,
				Frame = valueByIndex.Frame,
				Weight = valueByIndex.Weight
			});
		}
	}

	public void Close()
	{
		if (_component != null)
		{
			_component = null;
		}
		_openData = null;
		for (int i = 0; i < _clipMappingDic.Count; i++)
		{
			ClipMappingData valueByIndex = _clipMappingDic.GetValueByIndex(i);
			valueByIndex.ClipAsync = null;
			valueByIndex.Clip = null;
			valueByIndex.ClipName = null;
		}
		_clipMappingDic.Clear();
	}

	public void SetAnimation(AnimationKeyData animationKey, float frame, float weight)
	{
		ClipMappingData clipMapping = GetClipMapping(animationKey);
		if (clipMapping != null && (!Mathf.Approximately(clipMapping.Frame, frame) || !Mathf.Approximately(clipMapping.Weight, weight)))
		{
			clipMapping.Frame = frame;
			clipMapping.Weight = weight;
			SetClipAnimation(clipMapping);
		}
	}

	private void SetClipAnimation(ClipMappingData clipMapping)
	{
		if (String_Extension_Method.IsNullOrEmpty(clipMapping.ClipName) || clipMapping.Clip == null)
		{
			return;
		}
		if (clipMapping.Weight < Mathf.Epsilon)
		{
			if (clipMapping.Registered)
			{
				SetAnimationState(clipMapping, enabled: false);
			}
			else if (_component.GetClipCount() == 0)
			{
				_component.AddClip(clipMapping.Clip, clipMapping.ClipName);
				clipMapping.Registered = true;
				SetAnimationState(clipMapping, enabled: false);
			}
		}
		else
		{
			if (!clipMapping.Registered)
			{
				_component.AddClip(clipMapping.Clip, clipMapping.ClipName);
				clipMapping.Registered = true;
			}
			SetAnimationState(clipMapping, enabled: true);
		}
	}

	private void SetAnimationState(ClipMappingData clipMapping, bool enabled)
	{
		AnimationState animationState = _component[clipMapping.ClipName];
		if (!(animationState == null))
		{
			animationState.enabled = enabled;
			animationState.normalizedSpeed = 0f;
			animationState.normalizedTime = clipMapping.Frame;
			animationState.weight = clipMapping.Weight;
		}
	}

	private ClipMappingData GetClipMapping(AnimationKeyData key)
	{
		ClipMappingData clipMapping = _clipMappingDic.GETVALUE(key.PlayName, null);
		if (clipMapping != null)
		{
			return clipMapping;
		}
		clipMapping = new ClipMappingData();
		clipMapping.Key = key;
		clipMapping.ClipAsync = GetClipAsync(key.PlayName);
		_clipMappingDic.Add(key.PlayName, clipMapping);
		if (clipMapping.ClipAsync.ISDONE())
		{
			OnComplete(clipMapping);
		}
		else
		{
			AsyncManager.SingleEntry singleEntry = new AsyncManager.SingleEntry();
			singleEntry.AsyncLoad = clipMapping.ClipAsync;
			singleEntry.OnComplete = delegate
			{
				OnComplete(clipMapping);
			};
			SingletonUpdater<AsyncManager>.instance.Add(singleEntry);
		}
		return clipMapping;
	}

	private IAsyncLoad<AnimationClip> GetClipAsync(string name)
	{
		for (int i = 0; i < _openData.GetAnimationCount(); i++)
		{
			IAnimationData animationData = _openData.GetAnimationData(i);
			if (animationData != null && !(animationData.GetKey().PlayName != name))
			{
				return _openData.GetAnimationClip(i);
			}
		}
		return null;
	}

	private void OnComplete(ClipMappingData clipMapping)
	{
		if (clipMapping.ClipAsync != null)
		{
			if (!clipMapping.ClipAsync.ISVALID())
			{
				clipMapping.ClipAsync = null;
				return;
			}
			clipMapping.Clip = clipMapping.ClipAsync.Value;
			clipMapping.ClipName = clipMapping.Clip.name;
			clipMapping.ClipAsync = null;
			SetClipAnimation(clipMapping);
		}
	}

	public Dictionary<string, bool> GetRegistedClip()
	{
		_registedClip.Clear();
		foreach (KeyValuePair<string, ClipMappingData> item in (IEnumerable<KeyValuePair<string, ClipMappingData>>)_clipMappingDic)
		{
			_registedClip[item.Key] = item.Value.Registered;
		}
		return _registedClip;
	}
}
