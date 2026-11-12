using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerKingsPassExp
{
	public static PlayerKingsPassExp Parse(string s)
	{
		return Parse(s, new PlayerKingsPassExp());
	}

	public static PlayerKingsPassExp Parse(string s, PlayerKingsPassExp o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerKingsPassExp Parse(IDictionary d)
	{
		return Parse(d, new PlayerKingsPassExp());
	}

	public static PlayerKingsPassExp Parse(IDictionary d, PlayerKingsPassExp o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.Exp = BaseJsonMarshaler.ParseValueInt(d, "Exp");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(PlayerKingsPassExp o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerKingsPassExp o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("Exp", o.Exp.ToString());
		dictionary.Add("Level", o.Level.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
