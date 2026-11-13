using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RenewEquipOption;

public static class JM_NShared_RenewEquipOption_Request
{
	public static NShared.RenewEquipOption.Request Parse(string s)
	{
		return Parse(s, new NShared.RenewEquipOption.Request());
	}

	public static NShared.RenewEquipOption.Request Parse(string s, NShared.RenewEquipOption.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RenewEquipOption.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RenewEquipOption.Request());
	}

	public static NShared.RenewEquipOption.Request Parse(IDictionary d, NShared.RenewEquipOption.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.OptionSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "OptionSlotIndex");
		o.CouponItemIndex = BaseJsonMarshaler.ParseValueInt(d, "CouponItemIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RenewEquipOption.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RenewEquipOption.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("OptionSlotIndex", o.OptionSlotIndex.ToString());
		dictionary.Add("CouponItemIndex", o.CouponItemIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		SprkReforgeUntil.Encode(dictionary, o);
		return dictionary;
	}
}
