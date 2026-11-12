using System.Collections.Generic;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NCreature;

public class AnimationLoadData
{
	private AssetDataAsync<AnimationClip, AnimationData> _clipAsync;

	public AnimationData Data { get; private set; }

	public AssetDataAsync<AnimationClip, AnimationData> ClipAsync
	{
		get
		{
			LoadClip();
			return _clipAsync;
		}
	}

	public bool IsLoadStart => _clipAsync != null;

	public AnimationLoadData(AnimationData data, bool preload)
	{
		Data = data;
		if (preload)
		{
			LoadClip();
		}
	}

	public void Close()
	{
		if (_clipAsync != null)
		{
			_clipAsync.Close();
			_clipAsync = null;
		}
	}

	public bool IsLoaded()
	{
		if (_clipAsync == null)
		{
			return false;
		}
		return _clipAsync.ISVALID();
	}

	public void LoadClip()
	{
		if (_clipAsync == null)
		{
			_clipAsync = Data.CreateAssetAsync();
		}
	}

	public static bool AsyncIsDone(List<AnimationLoadData> loadList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(loadList); i++)
		{
			AnimationLoadData animationLoadData = loadList[i];
			if (animationLoadData._clipAsync != null && !animationLoadData._clipAsync.ISDONE())
			{
				return false;
			}
		}
		return true;
	}

	public static bool AsyncIsValid(List<AnimationLoadData> loadList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(loadList); i++)
		{
			AnimationLoadData animationLoadData = loadList[i];
			if (animationLoadData._clipAsync != null && !animationLoadData._clipAsync.ISVALID())
			{
				return false;
			}
		}
		return true;
	}
}
