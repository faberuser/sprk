using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipStorageSlotInfo
{
	public static EquipStorageSlotInfo Parse(string s)
	{
		return Parse(s, new EquipStorageSlotInfo());
	}

	public static EquipStorageSlotInfo Parse(string s, EquipStorageSlotInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipStorageSlotInfo Parse(IDictionary d)
	{
		return Parse(d, new EquipStorageSlotInfo());
	}

	public static EquipStorageSlotInfo Parse(IDictionary d, EquipStorageSlotInfo o)
	{
		o.EquipStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipStorageSlotIndex");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.EquipItemSlotIndex1 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex1");
		o.EquipItemSlotIndex2 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex2");
		o.EquipItemSlotIndex3 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex3");
		o.EquipItemSlotIndex4 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex4");
		o.EquipItemSlotIndex5 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex5");
		o.EquipItemSlotIndex6 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex6");
		o.EquipItemSlotIndex7 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex7");
		o.EquipItemSlotIndex8 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex8");
		o.EquipItemSlotIndex9 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex9");
		o.EquipItemSlotIndex10 = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex10");
		o.LinkInfo = BaseJsonMarshaler.ParseValueString(d, "LinkInfo");
		return o;
	}

	public static string EncodeJsonString(EquipStorageSlotInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipStorageSlotInfo o)
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
		dictionary.Add("EquipItemSlotIndex1", o.EquipItemSlotIndex1.ToString());
		dictionary.Add("EquipItemSlotIndex2", o.EquipItemSlotIndex2.ToString());
		dictionary.Add("EquipItemSlotIndex3", o.EquipItemSlotIndex3.ToString());
		dictionary.Add("EquipItemSlotIndex4", o.EquipItemSlotIndex4.ToString());
		dictionary.Add("EquipItemSlotIndex5", o.EquipItemSlotIndex5.ToString());
		dictionary.Add("EquipItemSlotIndex6", o.EquipItemSlotIndex6.ToString());
		dictionary.Add("EquipItemSlotIndex7", o.EquipItemSlotIndex7.ToString());
		dictionary.Add("EquipItemSlotIndex8", o.EquipItemSlotIndex8.ToString());
		dictionary.Add("EquipItemSlotIndex9", o.EquipItemSlotIndex9.ToString());
		dictionary.Add("EquipItemSlotIndex10", o.EquipItemSlotIndex10.ToString());
		if (o.LinkInfo != null)
		{
			dictionary.Add("LinkInfo", o.LinkInfo);
		}
		return dictionary;
	}
}
