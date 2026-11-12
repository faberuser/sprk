using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangeHeroStorageSlotName;

public static class JM_NShared_ChangeHeroStorageSlotName_Request
{
	public static NShared.ChangeHeroStorageSlotName.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangeHeroStorageSlotName.Request());
	}

	public static NShared.ChangeHeroStorageSlotName.Request Parse(string s, NShared.ChangeHeroStorageSlotName.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangeHeroStorageSlotName.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangeHeroStorageSlotName.Request());
	}

	public static NShared.ChangeHeroStorageSlotName.Request Parse(IDictionary d, NShared.ChangeHeroStorageSlotName.Request o)
	{
		o.StorageKey = BaseJsonMarshaler.ParseValueString(d, "StorageKey");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangeHeroStorageSlotName.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangeHeroStorageSlotName.Request o)
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
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
