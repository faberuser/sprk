using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SaveCostumeStorageSlot;

public static class JM_NShared_SaveCostumeStorageSlot_Request
{
	public static NShared.SaveCostumeStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.SaveCostumeStorageSlot.Request());
	}

	public static NShared.SaveCostumeStorageSlot.Request Parse(string s, NShared.SaveCostumeStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SaveCostumeStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SaveCostumeStorageSlot.Request());
	}

	public static NShared.SaveCostumeStorageSlot.Request Parse(IDictionary d, NShared.SaveCostumeStorageSlot.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.CostumeStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeStorageSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SaveCostumeStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SaveCostumeStorageSlot.Request o)
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
