using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetCostumeStorageSlot;

public static class JM_NShared_SetCostumeStorageSlot_Request
{
	public static NShared.SetCostumeStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.SetCostumeStorageSlot.Request());
	}

	public static NShared.SetCostumeStorageSlot.Request Parse(string s, NShared.SetCostumeStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetCostumeStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetCostumeStorageSlot.Request());
	}

	public static NShared.SetCostumeStorageSlot.Request Parse(IDictionary d, NShared.SetCostumeStorageSlot.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.CostumeStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeStorageSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetCostumeStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetCostumeStorageSlot.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("CostumeStorageSlotIndex", o.CostumeStorageSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
