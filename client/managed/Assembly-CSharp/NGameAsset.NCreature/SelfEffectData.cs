using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NCreature;

public class SelfEffectData : ILoadJson
{
	public enum EPlayType
	{
		Always,
		EnableList,
		DisableList
	}

	public string Code = string.Empty;

	public TransformData ExtraTransform = new TransformData();

	public EPlayType PlayType;

	public List<AnimationKeyData> KeyList = new List<AnimationKeyData>();

	public bool ReleaseImmediately;

	private HashSet<AnimationKeyData> _keySet;

	public bool ContainsKey(AnimationKeyData key)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(KeyList))
		{
			return false;
		}
		if (_keySet == null)
		{
			_keySet = new HashSet<AnimationKeyData>(AnimationKeyData.Comparer.Default);
			for (int i = 0; i < KeyList.Count; i++)
			{
				_keySet.Add(KeyList[i]);
			}
		}
		return _keySet.Contains(key);
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Code = json["Code"].GETSTRING(Code);
		ExtraTransform.LOADFROMJSON(json["ExtraTransform"]);
		PlayType = json["PlayType"].GETENUM(PlayType);
		KeyList.LoadListFromJson(json["KeyList"]);
		ReleaseImmediately = json["ReleaseImmediately"].GETBOOLEAN(ReleaseImmediately);
		return true;
	}
}
