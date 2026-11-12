using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NCreature;

public class AnimationData : ILoadJson, IAnimationData
{
	public AnimationKeyData Key;

	public string BundleFileName = string.Empty;

	public string AssetName = string.Empty;

	public float TimeLengthLoad;

	public EventNodeVisible NodeVisible;

	public List<EventSoundFx> SoundFxList;

	private LazyLoad<FacialAnimationData> _facialAnimation;

	public float TimeLength
	{
		get
		{
			if (TimeLengthLoad > Mathf.Epsilon)
			{
				return TimeLengthLoad;
			}
			return 1f;
		}
	}

	AnimationKeyData IAnimationData.GetKey()
	{
		return Key;
	}

	bool IAnimationData.GetNodeVisible(string nodeName, float frame, out bool isNullNode)
	{
		isNullNode = true;
		if (NodeVisible == null)
		{
			return true;
		}
		return NodeVisible.GetNodeVisible(nodeName, frame, out isNullNode);
	}

	FacialAnimationData IAnimationData.GetFacialAnimation()
	{
		return _facialAnimation.Value;
	}

	List<EventSoundFx> IAnimationData.GetSoundFxList()
	{
		return SoundFxList;
	}

	public AssetDataAsync<AnimationClip, AnimationData> CreateAssetAsync()
	{
		AssetDataAsync<AnimationClip, AnimationData> assetDataAsync = new AssetDataAsync<AnimationClip, AnimationData>();
		assetDataAsync.Open(this, BundleFileName, AssetName);
		return assetDataAsync;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Key = AnimationKeyData.CreateFromJson(json["Key"]);
		BundleFileName = json["BundleFileName"].GETSTRING(BundleFileName);
		AssetName = json["AssetName"].GETSTRING(AssetName);
		TimeLengthLoad = json["TimeLength"].GETFLOAT(TimeLengthLoad);
		bool flag = !String_Extension_Method.IsNullOrEmpty(Key.PlayName) && !String_Extension_Method.IsNullOrEmpty(AssetName);
		if (!flag)
		{
			return false;
		}
		JsonRead json2 = json["NodeVisible"];
		if (json2.IsArray())
		{
			NodeVisible = new EventNodeVisible();
			NodeVisible.LOADFROMJSON(json2);
		}
		JsonRead json3 = json["SoundFxList"];
		if (json3.IsArray())
		{
			SoundFxList = new List<EventSoundFx>();
			SoundFxList.LoadListFromJson(json3);
			SoundFxList.Sort((EventSoundFx lhs, EventSoundFx rhs) => lhs.Frame.CompareTo(rhs.Frame));
		}
		if (json.Contains("FacialAnimation"))
		{
			_facialAnimation.SetJsonRead(json["FacialAnimation"]);
		}
		return flag;
	}

	public static void LoadList(JsonRead json, List<AnimationData> animationList)
	{
		animationList.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			AnimationData animationData = new AnimationData();
			if (animationData.LOADFROMJSON(fromArray))
			{
				animationList.Add(animationData);
			}
		}
	}

	public static void LoadListToDictionary(JsonRead json, Dictionary<AnimationKeyData, AnimationData> animationDic)
	{
		animationDic.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			AnimationData animationData = new AnimationData();
			if (animationData.LOADFROMJSON(fromArray) && !animationDic.ContainsKey(animationData.Key))
			{
				animationDic.Add(animationData.Key, animationData);
			}
		}
	}

	public static List<AnimationData> GetAnimationDataList(List<AnimationData> animationDataList, ModelData modelData, CreatureStock.ELoadLevel loadLevel)
	{
		List<AnimationData> modelAnimationList = null;
		if (modelData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(modelData.AnimationList))
		{
			modelAnimationList = new List<AnimationData>(modelData.AnimationList);
		}
		List<AnimationData> list = new List<AnimationData>();
		AnimationData animationData = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(animationDataList); i++)
		{
			animationData = GetCompareAnimationData(modelData, animationDataList[i], ref modelAnimationList);
			if (animationData != null && DefineCreature.NeedLoadAnimation(loadLevel, animationData.Key.MotionType))
			{
				list.Add(animationData);
			}
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(modelAnimationList))
		{
			list.InsertRange(0, modelAnimationList);
		}
		return list;
	}

	private static AnimationData GetCompareAnimationData(ModelData modelData, AnimationData animationData, ref List<AnimationData> modelAnimationList)
	{
		if (animationData == null)
		{
			return null;
		}
		AnimationData result = animationData;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(modelData.AnimationList))
		{
			return result;
		}
		AnimationData animationData2 = null;
		for (int i = 0; i < modelData.AnimationList.Count; i++)
		{
			animationData2 = modelData.AnimationList[i];
			if (animationData2 != null && AnimationKeyData.Comparer.Default.Equals(animationData.Key, animationData2.Key))
			{
				result = animationData2;
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(modelAnimationList))
				{
					modelAnimationList.Remove(animationData2);
				}
				break;
			}
		}
		return result;
	}
}
