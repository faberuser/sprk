using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_UpdatedEquipItemInfo
{
	public static UpdatedEquipItemInfo Parse(string s)
	{
		return Parse(s, new UpdatedEquipItemInfo());
	}

	public static UpdatedEquipItemInfo Parse(string s, UpdatedEquipItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static UpdatedEquipItemInfo Parse(IDictionary d)
	{
		return Parse(d, new UpdatedEquipItemInfo());
	}

	public static UpdatedEquipItemInfo Parse(IDictionary d, UpdatedEquipItemInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		return o;
	}

	public static string EncodeJsonString(UpdatedEquipItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(UpdatedEquipItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Level", o.Level.ToString());
		return dictionary;
	}
}
