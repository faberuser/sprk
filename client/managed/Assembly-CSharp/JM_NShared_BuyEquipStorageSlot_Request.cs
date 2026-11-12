using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyEquipStorageSlot;

public static class JM_NShared_BuyEquipStorageSlot_Request
{
	public static NShared.BuyEquipStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyEquipStorageSlot.Request());
	}

	public static NShared.BuyEquipStorageSlot.Request Parse(string s, NShared.BuyEquipStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyEquipStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyEquipStorageSlot.Request());
	}

	public static NShared.BuyEquipStorageSlot.Request Parse(IDictionary d, NShared.BuyEquipStorageSlot.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyEquipStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyEquipStorageSlot.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
