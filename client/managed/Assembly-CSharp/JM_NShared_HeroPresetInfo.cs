using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroPresetInfo
{
	public static HeroPresetInfo Parse(string s)
	{
		return Parse(s, new HeroPresetInfo());
	}

	public static HeroPresetInfo Parse(string s, HeroPresetInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroPresetInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroPresetInfo());
	}

	public static HeroPresetInfo Parse(IDictionary d, HeroPresetInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.ApplySkillPage = BaseJsonMarshaler.ParseValueInt(d, "ApplySkillPage");
		o.ApplyRunePage = BaseJsonMarshaler.ParseValueInt(d, "ApplyRunePage");
		o.Position = BaseJsonMarshaler.ParseValueInt(d, "Position");
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndex", BaseJsonMarshaler.CastToInt);
		o.EquipItemUid = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemUid", BaseJsonMarshaler.CastToString);
		o.EquipItemRunePage = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemRunePage", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(HeroPresetInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroPresetInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("ApplySkillPage", o.ApplySkillPage.ToString());
		dictionary.Add("ApplyRunePage", o.ApplyRunePage.ToString());
		dictionary.Add("Position", o.Position.ToString());
		dictionary.Add("EquipItemSlotIndex", BaseJsonMarshaler.EncodeArray(o.EquipItemSlotIndex));
		dictionary.Add("EquipItemUid", BaseJsonMarshaler.EncodeArray(o.EquipItemUid));
		dictionary.Add("EquipItemRunePage", BaseJsonMarshaler.EncodeArray(o.EquipItemRunePage));
		return dictionary;
	}
}
