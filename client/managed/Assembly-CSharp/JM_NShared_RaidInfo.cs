using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RaidInfo
{
	public static RaidInfo Parse(string s)
	{
		return Parse(s, new RaidInfo());
	}

	public static RaidInfo Parse(string s, RaidInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RaidInfo Parse(IDictionary d)
	{
		return Parse(d, new RaidInfo());
	}

	public static RaidInfo Parse(IDictionary d, RaidInfo o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.BattleScore = BaseJsonMarshaler.ParseValueInt(d, "BattleScore");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		return o;
	}

	public static string EncodeJsonString(RaidInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RaidInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("BattleScore", o.BattleScore.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		return dictionary;
	}
}
