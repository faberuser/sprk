using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ValanceRenewOptionInfo
{
	public static ValanceRenewOptionInfo Parse(string s)
	{
		return Parse(s, new ValanceRenewOptionInfo());
	}

	public static ValanceRenewOptionInfo Parse(string s, ValanceRenewOptionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ValanceRenewOptionInfo Parse(IDictionary d)
	{
		return Parse(d, new ValanceRenewOptionInfo());
	}

	public static ValanceRenewOptionInfo Parse(IDictionary d, ValanceRenewOptionInfo o)
	{
		o.RenewOptionEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionEquipItemSlotIndex");
		o.RenewOptionResultIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionResultIndex");
		o.RenewOptionResultStep = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionResultStep");
		o.RenewOptionSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionSlotIndex");
		return o;
	}

	public static string EncodeJsonString(ValanceRenewOptionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ValanceRenewOptionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RenewOptionEquipItemSlotIndex", o.RenewOptionEquipItemSlotIndex.ToString());
		dictionary.Add("RenewOptionResultIndex", o.RenewOptionResultIndex.ToString());
		dictionary.Add("RenewOptionResultStep", o.RenewOptionResultStep.ToString());
		dictionary.Add("RenewOptionSlotIndex", o.RenewOptionSlotIndex.ToString());
		return dictionary;
	}
}
