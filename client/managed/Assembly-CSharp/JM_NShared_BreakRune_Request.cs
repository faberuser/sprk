using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BreakRune;

public static class JM_NShared_BreakRune_Request
{
	public static NShared.BreakRune.Request Parse(string s)
	{
		return Parse(s, new NShared.BreakRune.Request());
	}

	public static NShared.BreakRune.Request Parse(string s, NShared.BreakRune.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BreakRune.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BreakRune.Request());
	}

	public static NShared.BreakRune.Request Parse(IDictionary d, NShared.BreakRune.Request o)
	{
		o.ItemIndices = BaseJsonMarshaler.ParseArrayCast(d, "ItemIndices", BaseJsonMarshaler.CastToInt);
		o.Counts = BaseJsonMarshaler.ParseArrayCast(d, "Counts", BaseJsonMarshaler.CastToInt);
		o.EquipItemSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BreakRune.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BreakRune.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndices", BaseJsonMarshaler.EncodeArray(o.ItemIndices));
		dictionary.Add("Counts", BaseJsonMarshaler.EncodeArray(o.Counts));
		dictionary.Add("EquipItemSlotIndices", BaseJsonMarshaler.EncodeArray(o.EquipItemSlotIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
