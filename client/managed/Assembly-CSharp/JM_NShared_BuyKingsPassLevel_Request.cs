using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyKingsPassLevel;

public static class JM_NShared_BuyKingsPassLevel_Request
{
	public static NShared.BuyKingsPassLevel.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyKingsPassLevel.Request());
	}

	public static NShared.BuyKingsPassLevel.Request Parse(string s, NShared.BuyKingsPassLevel.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyKingsPassLevel.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyKingsPassLevel.Request());
	}

	public static NShared.BuyKingsPassLevel.Request Parse(IDictionary d, NShared.BuyKingsPassLevel.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.ReqLevel = BaseJsonMarshaler.ParseValueInt(d, "ReqLevel");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyKingsPassLevel.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyKingsPassLevel.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("ReqLevel", o.ReqLevel.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
