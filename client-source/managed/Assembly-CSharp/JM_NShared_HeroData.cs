using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroData
{
	public static HeroData Parse(string s)
	{
		return Parse(s, new HeroData());
	}

	public static HeroData Parse(string s, HeroData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroData Parse(IDictionary d)
	{
		return Parse(d, new HeroData());
	}

	public static HeroData Parse(IDictionary d, HeroData o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.TagType = BaseJsonMarshaler.ParseValueInt(d, "TagType");
		o.TranscendSkillIndices = BaseJsonMarshaler.ParseArrayCast(d, "TranscendSkillIndices", BaseJsonMarshaler.CastToInt);
		o.EquipSetItem = BaseJsonMarshaler.ParseValueString(d, "EquipSetItem");
		o.EquipItemOption = BaseJsonMarshaler.ParseValueString(d, "EquipItemOption");
		o.EquipTreasureItemIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipTreasureItemIndex");
		return o;
	}

	public static string EncodeJsonString(HeroData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("TagType", o.TagType.ToString());
		dictionary.Add("TranscendSkillIndices", BaseJsonMarshaler.EncodeArray(o.TranscendSkillIndices));
		if (o.EquipSetItem != null)
		{
			dictionary.Add("EquipSetItem", o.EquipSetItem);
		}
		if (o.EquipItemOption != null)
		{
			dictionary.Add("EquipItemOption", o.EquipItemOption);
		}
		dictionary.Add("EquipTreasureItemIndex", o.EquipTreasureItemIndex.ToString());
		return dictionary;
	}
}
