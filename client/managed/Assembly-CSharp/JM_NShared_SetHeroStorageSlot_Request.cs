using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetHeroStorageSlot;

public static class JM_NShared_SetHeroStorageSlot_Request
{
	public static NShared.SetHeroStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.SetHeroStorageSlot.Request());
	}

	public static NShared.SetHeroStorageSlot.Request Parse(string s, NShared.SetHeroStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetHeroStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetHeroStorageSlot.Request());
	}

	public static NShared.SetHeroStorageSlot.Request Parse(IDictionary d, NShared.SetHeroStorageSlot.Request o)
	{
		o.StorageKey = BaseJsonMarshaler.ParseValueString(d, "StorageKey");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetHeroStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetHeroStorageSlot.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.StorageKey != null)
		{
			dictionary.Add("StorageKey", o.StorageKey);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
