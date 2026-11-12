using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipPresetInfo
{
	public static EquipPresetInfo Parse(string s)
	{
		return Parse(s, new EquipPresetInfo());
	}

	public static EquipPresetInfo Parse(string s, EquipPresetInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipPresetInfo Parse(IDictionary d)
	{
		return Parse(d, new EquipPresetInfo());
	}

	public static EquipPresetInfo Parse(IDictionary d, EquipPresetInfo o)
	{
		o.EquipStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipStorageSlotIndex");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndex", BaseJsonMarshaler.CastToInt);
		o.EquipItemUid = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemUid", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(EquipPresetInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipPresetInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipStorageSlotIndex", o.EquipStorageSlotIndex.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("EquipItemSlotIndex", BaseJsonMarshaler.EncodeArray(o.EquipItemSlotIndex));
		dictionary.Add("EquipItemUid", BaseJsonMarshaler.EncodeArray(o.EquipItemUid));
		return dictionary;
	}
}
