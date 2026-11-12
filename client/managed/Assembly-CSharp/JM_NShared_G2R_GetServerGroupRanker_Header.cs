using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_GetServerGroupRanker;

public static class JM_NShared_G2R_GetServerGroupRanker_Header
{
	public static Header Parse(string s)
	{
		return Parse(s, new Header());
	}

	public static Header Parse(string s, Header o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Header Parse(IDictionary d)
	{
		return Parse(d, new Header());
	}

	public static Header Parse(IDictionary d, Header o)
	{
		o.Host = BaseJsonMarshaler.ParseValueString(d, "Host");
		o.Type = BaseJsonMarshaler.ParseValueString(d, "Type");
		o.Group = BaseJsonMarshaler.ParseValueString(d, "Group");
		return o;
	}

	public static string EncodeJsonString(Header o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Header o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Host != null)
		{
			dictionary.Add("Host", o.Host);
		}
		if (o.Type != null)
		{
			dictionary.Add("Type", o.Type);
		}
		if (o.Group != null)
		{
			dictionary.Add("Group", o.Group);
		}
		return dictionary;
	}
}
