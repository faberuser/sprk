using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyHeroStorageSlot;

public static class JM_NShared_BuyHeroStorageSlot_Request
{
	public static NShared.BuyHeroStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyHeroStorageSlot.Request());
	}

	public static NShared.BuyHeroStorageSlot.Request Parse(string s, NShared.BuyHeroStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyHeroStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyHeroStorageSlot.Request());
	}

	public static NShared.BuyHeroStorageSlot.Request Parse(IDictionary d, NShared.BuyHeroStorageSlot.Request o)
	{
		o.StorageKey = BaseJsonMarshaler.ParseValueString(d, "StorageKey");
		o.StorageName = BaseJsonMarshaler.ParseValueString(d, "StorageName");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyHeroStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyHeroStorageSlot.Request o)
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
