using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ExtendPunishmentRuneStorageSlot;

public static class JM_NShared_ExtendPunishmentRuneStorageSlot_Request
{
	public static NShared.ExtendPunishmentRuneStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.ExtendPunishmentRuneStorageSlot.Request());
	}

	public static NShared.ExtendPunishmentRuneStorageSlot.Request Parse(string s, NShared.ExtendPunishmentRuneStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ExtendPunishmentRuneStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ExtendPunishmentRuneStorageSlot.Request());
	}

	public static NShared.ExtendPunishmentRuneStorageSlot.Request Parse(IDictionary d, NShared.ExtendPunishmentRuneStorageSlot.Request o)
	{
		o.ReqGem = BaseJsonMarshaler.ParseValueInt(d, "ReqGem");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ExtendPunishmentRuneStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ExtendPunishmentRuneStorageSlot.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ReqGem", o.ReqGem.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
