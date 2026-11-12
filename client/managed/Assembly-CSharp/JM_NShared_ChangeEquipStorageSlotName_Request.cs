using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangeEquipStorageSlotName;

public static class JM_NShared_ChangeEquipStorageSlotName_Request
{
	public static NShared.ChangeEquipStorageSlotName.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangeEquipStorageSlotName.Request());
	}

	public static NShared.ChangeEquipStorageSlotName.Request Parse(string s, NShared.ChangeEquipStorageSlotName.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangeEquipStorageSlotName.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangeEquipStorageSlotName.Request());
	}

	public static NShared.ChangeEquipStorageSlotName.Request Parse(IDictionary d, NShared.ChangeEquipStorageSlotName.Request o)
	{
		o.EquipStorageSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipStorageSlotIndex");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangeEquipStorageSlotName.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangeEquipStorageSlotName.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipStorageSlotIndex", o.EquipStorageSlotIndex.ToString());
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
