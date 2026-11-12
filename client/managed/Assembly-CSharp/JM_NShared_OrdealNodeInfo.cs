using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_OrdealNodeInfo
{
	public static OrdealNodeInfo Parse(string s)
	{
		return Parse(s, new OrdealNodeInfo());
	}

	public static OrdealNodeInfo Parse(string s, OrdealNodeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OrdealNodeInfo Parse(IDictionary d)
	{
		return Parse(d, new OrdealNodeInfo());
	}

	public static OrdealNodeInfo Parse(IDictionary d, OrdealNodeInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Floor = BaseJsonMarshaler.ParseValueInt(d, "Floor");
		o.NodeType = BaseJsonMarshaler.ParseEnum<EOrdealArenaNodeType>(d, "NodeType");
		o.EventIndex = BaseJsonMarshaler.ParseValueInt(d, "EventIndex");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.AccountInfo = BaseJsonMarshaler.ParseObject(d, "AccountInfo", JM_NShared_CachedAccountInfo.Parse);
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		return o;
	}

	public static string EncodeJsonString(OrdealNodeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OrdealNodeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Floor", o.Floor.ToString());
		dictionary.Add("NodeType", o.NodeType.ToString());
		dictionary.Add("EventIndex", o.EventIndex.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("AccountInfo", JM_NShared_CachedAccountInfo.EncodeJsonObject(o.AccountInfo));
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		return dictionary;
	}
}
