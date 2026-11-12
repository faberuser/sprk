using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyPetIncubator;

public static class JM_NShared_BuyPetIncubator_Request
{
	public static NShared.BuyPetIncubator.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyPetIncubator.Request());
	}

	public static NShared.BuyPetIncubator.Request Parse(string s, NShared.BuyPetIncubator.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyPetIncubator.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyPetIncubator.Request());
	}

	public static NShared.BuyPetIncubator.Request Parse(IDictionary d, NShared.BuyPetIncubator.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.IncubatorIndex = BaseJsonMarshaler.ParseValueInt(d, "IncubatorIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyPetIncubator.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyPetIncubator.Request o)
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
