using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildPointResultInfo
{
	public static GuildPointResultInfo Parse(string s)
	{
		return Parse(s, new GuildPointResultInfo());
	}

	public static GuildPointResultInfo Parse(string s, GuildPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildPointResultInfo());
	}

	public static GuildPointResultInfo Parse(IDictionary d, GuildPointResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.AddDailyAccValue = BaseJsonMarshaler.ParseValueLong(d, "AddDailyAccValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		o.NewDailyAccValue = BaseJsonMarshaler.ParseValueLong(d, "NewDailyAccValue");
		return o;
	}

	public static string EncodeJsonString(GuildPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("AddDailyAccValue", o.AddDailyAccValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		dictionary.Add("NewDailyAccValue", o.NewDailyAccValue.ToString());
		return dictionary;
	}
}
