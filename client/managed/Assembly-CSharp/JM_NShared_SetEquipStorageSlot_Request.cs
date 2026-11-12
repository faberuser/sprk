using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetEquipStorageSlot;

public static class JM_NShared_SetEquipStorageSlot_Request
{
	public static NShared.SetEquipStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.SetEquipStorageSlot.Request());
	}

	public static NShared.SetEquipStorageSlot.Request Parse(string s, NShared.SetEquipStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetEquipStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetEquipStorageSlot.Request());
	}

	public static NShared.SetEquipStorageSlot.Request Parse(IDictionary d, NShared.SetEquipStorageSlot.Request o)
	{
		o.EquipStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipStorageSlotIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetEquipStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetEquipStorageSlot.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipStorageSlotIndex", o.EquipStorageSlotIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
