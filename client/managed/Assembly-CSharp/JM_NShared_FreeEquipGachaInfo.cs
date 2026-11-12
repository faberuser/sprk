using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_FreeEquipGachaInfo
{
	public static FreeEquipGachaInfo Parse(string s)
	{
		return Parse(s, new FreeEquipGachaInfo());
	}

	public static FreeEquipGachaInfo Parse(string s, FreeEquipGachaInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static FreeEquipGachaInfo Parse(IDictionary d)
	{
		return Parse(d, new FreeEquipGachaInfo());
	}

	public static FreeEquipGachaInfo Parse(IDictionary d, FreeEquipGachaInfo o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.LastGachaTime = BaseJsonMarshaler.ParseValueString(d, "LastGachaTime");
		o.LastGachaResetTime = BaseJsonMarshaler.ParseValueString(d, "LastGachaResetTime");
		o.RemainDayGachaChance = BaseJsonMarshaler.ParseValueByte(d, "RemainDayGachaChance");
		o.ResetRemainTime = BaseJsonMarshaler.ParseValueInt(d, "ResetRemainTime");
		return o;
	}

	public static string EncodeJsonString(FreeEquipGachaInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(FreeEquipGachaInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		if (o.LastGachaTime != null)
		{
			dictionary.Add("LastGachaTime", o.LastGachaTime);
		}
		if (o.LastGachaResetTime != null)
		{
			dictionary.Add("LastGachaResetTime", o.LastGachaResetTime);
		}
		dictionary.Add("RemainDayGachaChance", o.RemainDayGachaChance.ToString());
		dictionary.Add("ResetRemainTime", o.ResetRemainTime.ToString());
		return dictionary;
	}
}
