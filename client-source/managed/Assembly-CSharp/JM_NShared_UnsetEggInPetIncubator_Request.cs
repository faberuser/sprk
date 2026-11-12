using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UnsetEggInPetIncubator;

public static class JM_NShared_UnsetEggInPetIncubator_Request
{
	public static NShared.UnsetEggInPetIncubator.Request Parse(string s)
	{
		return Parse(s, new NShared.UnsetEggInPetIncubator.Request());
	}

	public static NShared.UnsetEggInPetIncubator.Request Parse(string s, NShared.UnsetEggInPetIncubator.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UnsetEggInPetIncubator.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UnsetEggInPetIncubator.Request());
	}

	public static NShared.UnsetEggInPetIncubator.Request Parse(IDictionary d, NShared.UnsetEggInPetIncubator.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.IncubatorIndex = BaseJsonMarshaler.ParseValueInt(d, "IncubatorIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UnsetEggInPetIncubator.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UnsetEggInPetIncubator.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("IncubatorIndex", o.IncubatorIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
