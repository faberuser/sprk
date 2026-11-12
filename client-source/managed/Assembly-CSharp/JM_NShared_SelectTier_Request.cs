using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SelectTier;

public static class JM_NShared_SelectTier_Request
{
	public static NShared.SelectTier.Request Parse(string s)
	{
		return Parse(s, new NShared.SelectTier.Request());
	}

	public static NShared.SelectTier.Request Parse(string s, NShared.SelectTier.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SelectTier.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SelectTier.Request());
	}

	public static NShared.SelectTier.Request Parse(IDictionary d, NShared.SelectTier.Request o)
	{
		o.Tier = BaseJsonMarshaler.ParseEnum<MatchTierType>(d, "Tier");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SelectTier.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SelectTier.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Tier", o.Tier.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
