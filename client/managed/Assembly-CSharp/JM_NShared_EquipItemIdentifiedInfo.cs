using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipItemIdentifiedInfo
{
	public static EquipItemIdentifiedInfo Parse(string s)
	{
		return Parse(s, new EquipItemIdentifiedInfo());
	}

	public static EquipItemIdentifiedInfo Parse(string s, EquipItemIdentifiedInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipItemIdentifiedInfo Parse(IDictionary d)
	{
		return Parse(d, new EquipItemIdentifiedInfo());
	}

	public static EquipItemIdentifiedInfo Parse(IDictionary d, EquipItemIdentifiedInfo o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.Identified = BaseJsonMarshaler.ParseValueInt(d, "Identified");
		o.ExtraOptionIndex1 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionIndex1");
		o.ExtraOptionStep1 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionStep1");
		o.ExtraOptionRenewCount1 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionRenewCount1");
		o.IsRenewedExtraOption1 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedExtraOption1");
		o.ExtraOptionIndex2 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionIndex2");
		o.ExtraOptionStep2 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionStep2");
		o.ExtraOptionRenewCount2 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionRenewCount2");
		o.IsRenewedExtraOption2 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedExtraOption2");
		return o;
	}

	public static string EncodeJsonString(EquipItemIdentifiedInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipItemIdentifiedInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("Identified", o.Identified.ToString());
		dictionary.Add("ExtraOptionIndex1", o.ExtraOptionIndex1.ToString());
		dictionary.Add("ExtraOptionStep1", o.ExtraOptionStep1.ToString());
		dictionary.Add("ExtraOptionRenewCount1", o.ExtraOptionRenewCount1.ToString());
		dictionary.Add("IsRenewedExtraOption1", o.IsRenewedExtraOption1.ToString());
		dictionary.Add("ExtraOptionIndex2", o.ExtraOptionIndex2.ToString());
		dictionary.Add("ExtraOptionStep2", o.ExtraOptionStep2.ToString());
		dictionary.Add("ExtraOptionRenewCount2", o.ExtraOptionRenewCount2.ToString());
		dictionary.Add("IsRenewedExtraOption2", o.IsRenewedExtraOption2.ToString());
		return dictionary;
	}
}
