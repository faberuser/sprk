using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildLevelData
{
	public static GuildLevelData Parse(string s)
	{
		return Parse(s, new GuildLevelData());
	}

	public static GuildLevelData Parse(string s, GuildLevelData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildLevelData Parse(IDictionary d)
	{
		return Parse(d, new GuildLevelData());
	}

	public static GuildLevelData Parse(IDictionary d, GuildLevelData o)
	{
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.ActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "ActivityPoint");
		o.TotalActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "TotalActivityPoint");
		o.MaxMember = BaseJsonMarshaler.ParseValueInt(d, "MaxMember");
		o.MaxAdmin = BaseJsonMarshaler.ParseValueInt(d, "MaxAdmin");
		return o;
	}

	public static string EncodeJsonString(GuildLevelData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildLevelData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("ActivityPoint", o.ActivityPoint.ToString());
		dictionary.Add("TotalActivityPoint", o.TotalActivityPoint.ToString());
		dictionary.Add("MaxMember", o.MaxMember.ToString());
		dictionary.Add("MaxAdmin", o.MaxAdmin.ToString());
		return dictionary;
	}
}
