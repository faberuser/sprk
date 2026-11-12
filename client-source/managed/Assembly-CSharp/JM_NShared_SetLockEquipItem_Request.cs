using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetLockEquipItem;

public static class JM_NShared_SetLockEquipItem_Request
{
	public static NShared.SetLockEquipItem.Request Parse(string s)
	{
		return Parse(s, new NShared.SetLockEquipItem.Request());
	}

	public static NShared.SetLockEquipItem.Request Parse(string s, NShared.SetLockEquipItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetLockEquipItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetLockEquipItem.Request());
	}

	public static NShared.SetLockEquipItem.Request Parse(IDictionary d, NShared.SetLockEquipItem.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.Locked = BaseJsonMarshaler.ParseValueByte(d, "Locked");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetLockEquipItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetLockEquipItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("Locked", o.Locked.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
