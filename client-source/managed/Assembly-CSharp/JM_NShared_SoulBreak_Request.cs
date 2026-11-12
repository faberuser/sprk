using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulBreak;

public static class JM_NShared_SoulBreak_Request
{
	public static NShared.SoulBreak.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulBreak.Request());
	}

	public static NShared.SoulBreak.Request Parse(string s, NShared.SoulBreak.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulBreak.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulBreak.Request());
	}

	public static NShared.SoulBreak.Request Parse(IDictionary d, NShared.SoulBreak.Request o)
	{
		o.EquipItemSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulBreak.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulBreak.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndices", BaseJsonMarshaler.EncodeArray(o.EquipItemSlotIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
