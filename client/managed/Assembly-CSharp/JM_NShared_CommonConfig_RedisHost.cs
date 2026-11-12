using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CommonConfig_RedisHost
{
	public static CommonConfig_RedisHost Parse(string s)
	{
		return Parse(s, new CommonConfig_RedisHost());
	}

	public static CommonConfig_RedisHost Parse(string s, CommonConfig_RedisHost o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CommonConfig_RedisHost Parse(IDictionary d)
	{
		return Parse(d, new CommonConfig_RedisHost());
	}

	public static CommonConfig_RedisHost Parse(IDictionary d, CommonConfig_RedisHost o)
	{
		o.type = BaseJsonMarshaler.ParseValueString(d, "type");
		o.ip = BaseJsonMarshaler.ParseValueString(d, "ip");
		o.port = BaseJsonMarshaler.ParseValueInt(d, "port");
		o.pass = BaseJsonMarshaler.ParseValueString(d, "pass");
		return o;
	}

	public static string EncodeJsonString(CommonConfig_RedisHost o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CommonConfig_RedisHost o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.type != null)
		{
			dictionary.Add("type", o.type);
		}
		if (o.ip != null)
		{
			dictionary.Add("ip", o.ip);
		}
		dictionary.Add("port", o.port.ToString());
		if (o.pass != null)
		{
			dictionary.Add("pass", o.pass);
		}
		return dictionary;
	}
}
