using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipItemBoostInfo
{
	public static EquipItemBoostInfo Parse(string s)
	{
		return Parse(s, new EquipItemBoostInfo());
	}

	public static EquipItemBoostInfo Parse(string s, EquipItemBoostInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipItemBoostInfo Parse(IDictionary d)
	{
		return Parse(d, new EquipItemBoostInfo());
	}

	public static EquipItemBoostInfo Parse(IDictionary d, EquipItemBoostInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.BoostPercent = BaseJsonMarshaler.ParseValueInt(d, "BoostPercent");
		return o;
	}

	public static string EncodeJsonString(EquipItemBoostInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipItemBoostInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("BoostPercent", o.BoostPercent.ToString());
		return dictionary;
	}
}
