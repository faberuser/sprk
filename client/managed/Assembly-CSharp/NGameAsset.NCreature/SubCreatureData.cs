using System.Collections.Generic;
using JsonBin;
using NVespa;

namespace NGameAsset.NCreature;

public class SubCreatureData : ILoadJson, ISubCreatureData
{
	public bool EnableExtraTransform;

	public TransformData ExtraTransform = new TransformData();

	public Dictionary<AnimationKeyData, AnimationKeyData> AnimationRemap = new Dictionary<AnimationKeyData, AnimationKeyData>();

	public string CreatureCode { get; set; }

	public string PivotName { get; set; }

	public SubCreatureData()
	{
		CreatureCode = string.Empty;
		PivotName = string.Empty;
	}

	TransformData ISubCreatureData.GetExtraTransform()
	{
		if (!EnableExtraTransform)
		{
			return null;
		}
		return ExtraTransform;
	}

	AnimationKeyData ISubCreatureData.GetAnimationKey(AnimationKeyData parentKey)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(AnimationRemap, parentKey, parentKey);
	}

	bool ILoadJson.Load(JsonRead json)
	{
		CreatureCode = json["CreatureCode"].GETSTRING(CreatureCode);
		PivotName = json["PivotName"].GETSTRING(PivotName);
		EnableExtraTransform = json["EnableExtraTransform"].GETBOOLEAN(EnableExtraTransform);
		ExtraTransform.LOADFROMJSON(json["ExtraTransform"]);
		AnimationRemap.LoadDictionaryFromJson(json["AnimationRemap"]);
		return true;
	}

	public static void LoadList(JsonRead json, ref PoolingList<SubCreatureData> list)
	{
		list.Clear();
		if (!json.IsArray() || json.Count == 0)
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			SubCreatureData subCreatureData = new SubCreatureData();
			if (subCreatureData.LOADFROMJSON(fromArray))
			{
				list.Add(subCreatureData);
			}
		}
	}
}
