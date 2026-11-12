using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_GetWorldRankerAllTier;

public static class JM_NShared_G2R_GetWorldRankerAllTier_Request
{
	public static NShared.G2R_GetWorldRankerAllTier.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetWorldRankerAllTier.Request());
	}

	public static NShared.G2R_GetWorldRankerAllTier.Request Parse(string s, NShared.G2R_GetWorldRankerAllTier.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetWorldRankerAllTier.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetWorldRankerAllTier.Request());
	}

	public static NShared.G2R_GetWorldRankerAllTier.Request Parse(IDictionary d, NShared.G2R_GetWorldRankerAllTier.Request o)
	{
		o.StartRank = BaseJsonMarshaler.ParseValueInt(d, "StartRank");
		o.EndRank = BaseJsonMarshaler.ParseValueInt(d, "EndRank");
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_GetWorldRankerAllTier.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_GetWorldRankerAllTier.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("StartRank", o.StartRank.ToString());
		dictionary.Add("EndRank", o.EndRank.ToString());
		return dictionary;
	}
}
