using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetEquipStorageSlot;

public static class JM_NShared_ResetEquipStorageSlot_Request
{
	public static NShared.ResetEquipStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetEquipStorageSlot.Request());
	}

	public static NShared.ResetEquipStorageSlot.Request Parse(string s, NShared.ResetEquipStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetEquipStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetEquipStorageSlot.Request());
	}

	public static NShared.ResetEquipStorageSlot.Request Parse(IDictionary d, NShared.ResetEquipStorageSlot.Request o)
	{
		o.EquipStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipStorageSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetEquipStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetEquipStorageSlot.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipStorageSlotIndex", o.EquipStorageSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
