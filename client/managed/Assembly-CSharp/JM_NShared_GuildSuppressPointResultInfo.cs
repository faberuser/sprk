using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSuppressPointResultInfo
{
	public static GuildSuppressPointResultInfo Parse(string s)
	{
		return Parse(s, new GuildSuppressPointResultInfo());
	}

	public static GuildSuppressPointResultInfo Parse(string s, GuildSuppressPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSuppressPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildSuppressPointResultInfo());
	}

	public static GuildSuppressPointResultInfo Parse(IDictionary d, GuildSuppressPointResultInfo o)
	{
		o.NewValue = BaseJsonMarshaler.ParseValueInt(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(GuildSuppressPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSuppressPointResultInfo o)
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
