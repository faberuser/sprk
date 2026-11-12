using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NCreature;

public class AnimationBlendData : ILoadJson, IAnimationBlendData
{
	public class BlendData : ILoadJson
	{
		public Dictionary<AnimationKeyData, float> Dic = new Dictionary<AnimationKeyData, float>(AnimationKeyData.Comparer.Default);

		bool ILoadJson.Load(JsonRead json)
		{
			Dic.LoadDictionaryFromJson(json["Dic"]);
			return true;
		}
	}

	private LazyDictionary<AnimationKeyData, BlendData> _fromToBlendDic = new LazyDictionary<AnimationKeyData, BlendData>(AnimationKeyData.Comparer.Default);

	private LazyDictionary<AnimationKeyData, float> _toOnlyBlend = new LazyDictionary<AnimationKeyData, float>(AnimationKeyData.Comparer.Default);

	private LazyDictionary<AnimationBlendDefault.BlendKey, float> _typeFromToDic = new LazyDictionary<AnimationBlendDefault.BlendKey, float>(AnimationBlendDefault.BlendKey.Comparer.Default);

	private LazyDictionary<DefineCreature.EMotion, float> _typeToOnlyDic;

	private bool _allEmpty;

	bool IAnimationBlendData.TryBlendTime(AnimationKeyData motionFrom, AnimationKeyData motionTo, out float blendTime)
	{
		if (_allEmpty)
		{
			blendTime = 0f;
			return false;
		}
		BlendData vALUE = _fromToBlendDic.GETVALUE(motionFrom, null);
		if (vALUE != null && vALUE.Dic.TryGetValue(motionTo, out blendTime))
		{
			return true;
		}
		if (_toOnlyBlend.TryGetValue(motionTo, out blendTime))
		{
			return true;
		}
		AnimationBlendDefault.BlendKey key = new AnimationBlendDefault.BlendKey(motionFrom.MotionType, motionTo.MotionType);
		if (_typeFromToDic.TryGetValue(key, out blendTime))
		{
			return true;
		}
		return _typeToOnlyDic.TryGetValue(motionTo.MotionType, out blendTime);
	}

	bool ILoadJson.Load(JsonRead json)
	{
		_fromToBlendDic.LoadFromJson(json["FromToBlendDic"]);
		_toOnlyBlend.LoadFromJson(json["ToOnlyBlend"]);
		_typeFromToDic.LoadFromJson(json["TypeFromToDic"]);
		_typeToOnlyDic.LoadFromJson(json["TypeToOnlyDic"]);
		_allEmpty = _fromToBlendDic.IsNullOrEmpty() && _toOnlyBlend.IsNullOrEmpty() && _typeFromToDic.IsNullOrEmpty() && _typeToOnlyDic.IsNullOrEmpty();
		return true;
	}
}
