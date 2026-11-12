using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RemoveHeroEquipStorageSlot;

public static class JM_NShared_RemoveHeroEquipStorageSlot_Request
{
	public static NShared.RemoveHeroEquipStorageSlot.Request Parse(string s)
	{
		return Parse(s, new NShared.RemoveHeroEquipStorageSlot.Request());
	}

	public static NShared.RemoveHeroEquipStorageSlot.Request Parse(string s, NShared.RemoveHeroEquipStorageSlot.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RemoveHeroEquipStorageSlot.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RemoveHeroEquipStorageSlot.Request());
	}

	public static NShared.RemoveHeroEquipStorageSlot.Request Parse(IDictionary d, NShared.RemoveHeroEquipStorageSlot.Request o)
	{
		o.EquipStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipStorageSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RemoveHeroEquipStorageSlot.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RemoveHeroEquipStorageSlot.Request o)
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
