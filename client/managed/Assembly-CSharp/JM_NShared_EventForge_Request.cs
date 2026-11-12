using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EventForge;

public static class JM_NShared_EventForge_Request
{
	public static NShared.EventForge.Request Parse(string s)
	{
		return Parse(s, new NShared.EventForge.Request());
	}

	public static NShared.EventForge.Request Parse(string s, NShared.EventForge.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EventForge.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EventForge.Request());
	}

	public static NShared.EventForge.Request Parse(IDictionary d, NShared.EventForge.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.UpgradeLevel = BaseJsonMarshaler.ParseValueInt(d, "UpgradeLevel");
		o.IsFixed = BaseJsonMarshaler.ParseValueBool(d, "IsFixed");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EventForge.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EventForge.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("UpgradeLevel", o.UpgradeLevel.ToString());
		dictionary.Add("IsFixed", o.IsFixed.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
