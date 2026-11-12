using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GachaCeilingCountInfo
{
	public static GachaCeilingCountInfo Parse(string s)
	{
		return Parse(s, new GachaCeilingCountInfo());
	}

	public static GachaCeilingCountInfo Parse(string s, GachaCeilingCountInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GachaCeilingCountInfo Parse(IDictionary d)
	{
		return Parse(d, new GachaCeilingCountInfo());
	}

	public static GachaCeilingCountInfo Parse(IDictionary d, GachaCeilingCountInfo o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.GachaCount = BaseJsonMarshaler.ParseValueInt(d, "GachaCount");
		return o;
	}

	public static string EncodeJsonString(GachaCeilingCountInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GachaCeilingCountInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		dictionary.Add("GachaCount", o.GachaCount.ToString());
		return dictionary;
	}
}
