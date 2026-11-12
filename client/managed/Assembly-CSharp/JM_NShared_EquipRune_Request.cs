using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EquipRune;

public static class JM_NShared_EquipRune_Request
{
	public static NShared.EquipRune.Request Parse(string s)
	{
		return Parse(s, new NShared.EquipRune.Request());
	}

	public static NShared.EquipRune.Request Parse(string s, NShared.EquipRune.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EquipRune.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EquipRune.Request());
	}

	public static NShared.EquipRune.Request Parse(IDictionary d, NShared.EquipRune.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.SlotNum = BaseJsonMarshaler.ParseValueInt(d, "SlotNum");
		o.RunePage = BaseJsonMarshaler.ParseValueInt(d, "RunePage");
		o.IsPreserve = BaseJsonMarshaler.ParseValueBool(d, "IsPreserve");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EquipRune.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EquipRune.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("SlotNum", o.SlotNum.ToString());
		dictionary.Add("RunePage", o.RunePage.ToString());
		dictionary.Add("IsPreserve", o.IsPreserve.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
