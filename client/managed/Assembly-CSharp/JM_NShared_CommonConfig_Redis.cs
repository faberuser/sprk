using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CommonConfig_Redis
{
	public static CommonConfig_Redis Parse(string s)
	{
		return Parse(s, new CommonConfig_Redis());
	}

	public static CommonConfig_Redis Parse(string s, CommonConfig_Redis o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CommonConfig_Redis Parse(IDictionary d)
	{
		return Parse(d, new CommonConfig_Redis());
	}

	public static CommonConfig_Redis Parse(IDictionary d, CommonConfig_Redis o)
	{
		o.player = BaseJsonMarshaler.ParseArray(d, "player", JM_NShared_CommonConfig_RedisHost.Parse);
		o.message = BaseJsonMarshaler.ParseArray(d, "message", JM_NShared_CommonConfig_RedisHost.Parse);
		return o;
	}

	public static string EncodeJsonString(CommonConfig_Redis o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CommonConfig_Redis o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("player", BaseJsonMarshaler.EncodeArray(o.player, JM_NShared_CommonConfig_RedisHost.EncodeJsonObject));
		dictionary.Add("message", BaseJsonMarshaler.EncodeArray(o.message, JM_NShared_CommonConfig_RedisHost.EncodeJsonObject));
		return dictionary;
	}
}
