using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipItemRuneInfo
{
	public static EquipItemRuneInfo Parse(string s)
	{
		return Parse(s, new EquipItemRuneInfo());
	}

	public static EquipItemRuneInfo Parse(string s, EquipItemRuneInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipItemRuneInfo Parse(IDictionary d)
	{
		return Parse(d, new EquipItemRuneInfo());
	}

	public static EquipItemRuneInfo Parse(IDictionary d, EquipItemRuneInfo o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.RunePage = BaseJsonMarshaler.ParseValueInt(d, "RunePage");
		o.RuneItemIndex1 = BaseJsonMarshaler.ParseValueInt(d, "RuneItemIndex1");
		o.RuneItemIndex2 = BaseJsonMarshaler.ParseValueInt(d, "RuneItemIndex2");
		o.RuneItemIndex3 = BaseJsonMarshaler.ParseValueInt(d, "RuneItemIndex3");
		return o;
	}

	public static string EncodeJsonString(EquipItemRuneInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipItemRuneInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("RunePage", o.RunePage.ToString());
		dictionary.Add("RuneItemIndex1", o.RuneItemIndex1.ToString());
		dictionary.Add("RuneItemIndex2", o.RuneItemIndex2.ToString());
		dictionary.Add("RuneItemIndex3", o.RuneItemIndex3.ToString());
		return dictionary;
	}
}
