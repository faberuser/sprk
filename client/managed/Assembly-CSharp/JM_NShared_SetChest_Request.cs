using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetChest;

public static class JM_NShared_SetChest_Request
{
	public static NShared.SetChest.Request Parse(string s)
	{
		return Parse(s, new NShared.SetChest.Request());
	}

	public static NShared.SetChest.Request Parse(string s, NShared.SetChest.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetChest.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetChest.Request());
	}

	public static NShared.SetChest.Request Parse(IDictionary d, NShared.SetChest.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndex", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetChest.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetChest.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", BaseJsonMarshaler.EncodeArray(o.EquipItemSlotIndex));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
