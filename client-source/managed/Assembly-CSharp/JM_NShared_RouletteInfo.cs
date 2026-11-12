using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RouletteInfo
{
	public static RouletteInfo Parse(string s)
	{
		return Parse(s, new RouletteInfo());
	}

	public static RouletteInfo Parse(string s, RouletteInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RouletteInfo Parse(IDictionary d)
	{
		return Parse(d, new RouletteInfo());
	}

	public static RouletteInfo Parse(IDictionary d, RouletteInfo o)
	{
		o.RouletteInfoIndex = BaseJsonMarshaler.ParseValueInt(d, "RouletteInfoIndex");
		o.RemainCount = BaseJsonMarshaler.ParseValueInt(d, "RemainCount");
		o.MaxCount = BaseJsonMarshaler.ParseValueInt(d, "MaxCount");
		return o;
	}

	public static string EncodeJsonString(RouletteInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RouletteInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RouletteInfoIndex", o.RouletteInfoIndex.ToString());
		dictionary.Add("RemainCount", o.RemainCount.ToString());
		dictionary.Add("MaxCount", o.MaxCount.ToString());
		return dictionary;
	}
}
