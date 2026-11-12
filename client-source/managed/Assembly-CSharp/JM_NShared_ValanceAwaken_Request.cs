using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ValanceAwaken;

public static class JM_NShared_ValanceAwaken_Request
{
	public static NShared.ValanceAwaken.Request Parse(string s)
	{
		return Parse(s, new NShared.ValanceAwaken.Request());
	}

	public static NShared.ValanceAwaken.Request Parse(string s, NShared.ValanceAwaken.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ValanceAwaken.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ValanceAwaken.Request());
	}

	public static NShared.ValanceAwaken.Request Parse(IDictionary d, NShared.ValanceAwaken.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ValanceAwaken.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ValanceAwaken.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			SprkTechnomagicAwaken.Encode(dictionary, o);
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		SprkTechnomagicAwaken.Encode(dictionary, o);
		return dictionary;
	}
}
