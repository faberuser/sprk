using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildMemberAccActivityPointResultInfo
{
	public static GuildMemberAccActivityPointResultInfo Parse(string s)
	{
		return Parse(s, new GuildMemberAccActivityPointResultInfo());
	}

	public static GuildMemberAccActivityPointResultInfo Parse(string s, GuildMemberAccActivityPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildMemberAccActivityPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildMemberAccActivityPointResultInfo());
	}

	public static GuildMemberAccActivityPointResultInfo Parse(IDictionary d, GuildMemberAccActivityPointResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueInt(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueInt(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(GuildMemberAccActivityPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildMemberAccActivityPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
