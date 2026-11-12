using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.FeedThePet;

public static class JM_NShared_FeedThePet_Request
{
	public static NShared.FeedThePet.Request Parse(string s)
	{
		return Parse(s, new NShared.FeedThePet.Request());
	}

	public static NShared.FeedThePet.Request Parse(string s, NShared.FeedThePet.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.FeedThePet.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.FeedThePet.Request());
	}

	public static NShared.FeedThePet.Request Parse(IDictionary d, NShared.FeedThePet.Request o)
	{
		o.PetIndex = BaseJsonMarshaler.ParseValueInt(d, "PetIndex");
		o.FeedCount = BaseJsonMarshaler.ParseValueInt(d, "FeedCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.FeedThePet.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.FeedThePet.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PetIndex", o.PetIndex.ToString());
		dictionary.Add("FeedCount", o.FeedCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
