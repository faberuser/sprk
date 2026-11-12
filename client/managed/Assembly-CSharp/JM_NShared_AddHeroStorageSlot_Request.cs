using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AddHeroStorageSlot;

public static class JM_NShared_AddHeroStorageSlot_Request
{
	public static NShared.AddHeroStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.AddHeroStorageSlot.Request());
	}

	public static NShared.AddHeroStorageSlot.Request Parse(string s, NShared.AddHeroStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AddHeroStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AddHeroStorageSlot.Request());
	}

	public static NShared.AddHeroStorageSlot.Request Parse(IDictionary d, NShared.AddHeroStorageSlot.Request o)
	{
		o.StorageKey = BaseJsonMarshaler.ParseValueString(d, "StorageKey");
		o.StorageName = BaseJsonMarshaler.ParseValueString(d, "StorageName");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AddHeroStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AddHeroStorageSlot.Request o)
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
		if (o.StorageName != null)
		{
			dictionary.Add("StorageName", o.StorageName);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
