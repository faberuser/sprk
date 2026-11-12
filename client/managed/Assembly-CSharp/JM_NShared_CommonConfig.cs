using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CommonConfig
{
	public static CommonConfig Parse(string s)
	{
		return Parse(s, new CommonConfig());
	}

	public static CommonConfig Parse(string s, CommonConfig o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CommonConfig Parse(IDictionary d)
	{
		return Parse(d, new CommonConfig());
	}

	public static CommonConfig Parse(IDictionary d, CommonConfig o)
	{
		o.data_path = BaseJsonMarshaler.ParseValueString(d, "data_path");
		o.match_server_ip = BaseJsonMarshaler.ParseValueString(d, "match_server_ip");
		o.match_server_port = BaseJsonMarshaler.ParseValueInt(d, "match_server_port");
		o.redis = BaseJsonMarshaler.ParseObject(d, "redis", JM_NShared_CommonConfig_Redis.Parse);
		o.server = BaseJsonMarshaler.ParseArray(d, "server", JM_NShared_CommonConfig_Server.Parse);
		o.public_ip_command = BaseJsonMarshaler.ParseValueString(d, "public_ip_command");
		o.public_ip_command_args = BaseJsonMarshaler.ParseArrayCast(d, "public_ip_command_args", BaseJsonMarshaler.CastToString);
		o.private_ip_command = BaseJsonMarshaler.ParseValueString(d, "private_ip_command");
		o.private_ip_command_args = BaseJsonMarshaler.ParseArrayCast(d, "private_ip_command_args", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(CommonConfig o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CommonConfig o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.data_path != null)
		{
			dictionary.Add("data_path", o.data_path);
		}
		if (o.match_server_ip != null)
		{
			dictionary.Add("match_server_ip", o.match_server_ip);
		}
		dictionary.Add("match_server_port", o.match_server_port.ToString());
		dictionary.Add("redis", JM_NShared_CommonConfig_Redis.EncodeJsonObject(o.redis));
		dictionary.Add("server", BaseJsonMarshaler.EncodeArray(o.server, JM_NShared_CommonConfig_Server.EncodeJsonObject));
		if (o.public_ip_command != null)
		{
			dictionary.Add("public_ip_command", o.public_ip_command);
		}
		dictionary.Add("public_ip_command_args", BaseJsonMarshaler.EncodeArray(o.public_ip_command_args));
		if (o.private_ip_command != null)
		{
			dictionary.Add("private_ip_command", o.private_ip_command);
		}
		dictionary.Add("private_ip_command_args", BaseJsonMarshaler.EncodeArray(o.private_ip_command_args));
		return dictionary;
	}
}
