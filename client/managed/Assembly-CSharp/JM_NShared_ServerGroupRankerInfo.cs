using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ServerGroupRankerInfo
{
	public static ServerGroupRankerInfo Parse(string s)
	{
		return Parse(s, new ServerGroupRankerInfo());
	}

	public static ServerGroupRankerInfo Parse(string s, ServerGroupRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ServerGroupRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new ServerGroupRankerInfo());
	}

	public static ServerGroupRankerInfo Parse(IDictionary d, ServerGroupRankerInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.Score = BaseJsonMarshaler.ParseValueInt(d, "Score");
		return o;
	}

	public static string EncodeJsonString(ServerGroupRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ServerGroupRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("Score", o.Score.ToString());
		return dictionary;
	}
}
