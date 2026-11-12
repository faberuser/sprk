using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GlobalAccountInfo
{
	public static GlobalAccountInfo Parse(string s)
	{
		return Parse(s, new GlobalAccountInfo());
	}

	public static GlobalAccountInfo Parse(string s, GlobalAccountInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GlobalAccountInfo Parse(IDictionary d)
	{
		return Parse(d, new GlobalAccountInfo());
	}

	public static GlobalAccountInfo Parse(IDictionary d, GlobalAccountInfo o)
	{
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		return o;
	}

	public static string EncodeJsonString(GlobalAccountInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GlobalAccountInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		return dictionary;
	}
}
