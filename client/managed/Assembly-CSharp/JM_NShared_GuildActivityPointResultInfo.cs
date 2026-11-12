using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildActivityPointResultInfo
{
	public static GuildActivityPointResultInfo Parse(string s)
	{
		return Parse(s, new GuildActivityPointResultInfo());
	}

	public static GuildActivityPointResultInfo Parse(string s, GuildActivityPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildActivityPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildActivityPointResultInfo());
	}

	public static GuildActivityPointResultInfo Parse(IDictionary d, GuildActivityPointResultInfo o)
	{
		o.NewValue = BaseJsonMarshaler.ParseValueInt(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(GuildActivityPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildActivityPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
