using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UnsetChest;

public static class JM_NShared_UnsetChest_Request
{
	public static NShared.UnsetChest.Request Parse(string s)
	{
		return Parse(s, new NShared.UnsetChest.Request());
	}

	public static NShared.UnsetChest.Request Parse(string s, NShared.UnsetChest.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UnsetChest.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UnsetChest.Request());
	}

	public static NShared.UnsetChest.Request Parse(IDictionary d, NShared.UnsetChest.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndex", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UnsetChest.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UnsetChest.Request o)
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
