using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AwakenEquip;

public static class JM_NShared_AwakenEquip_Request
{
	public static NShared.AwakenEquip.Request Parse(string s)
	{
		return Parse(s, new NShared.AwakenEquip.Request());
	}

	public static NShared.AwakenEquip.Request Parse(string s, NShared.AwakenEquip.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AwakenEquip.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AwakenEquip.Request());
	}

	public static NShared.AwakenEquip.Request Parse(IDictionary d, NShared.AwakenEquip.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.MaterialSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "MaterialSlotIndices", BaseJsonMarshaler.CastToInt);
		o.MaterialItemInfors = BaseJsonMarshaler.ParseArrayCast(d, "MaterialItemInfors", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AwakenEquip.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AwakenEquip.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("MaterialSlotIndices", BaseJsonMarshaler.EncodeArray(o.MaterialSlotIndices));
		dictionary.Add("MaterialItemInfors", BaseJsonMarshaler.EncodeArray(o.MaterialItemInfors));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
