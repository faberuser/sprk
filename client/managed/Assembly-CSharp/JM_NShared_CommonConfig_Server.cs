using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CommonConfig_Server
{
	public static CommonConfig_Server Parse(string s)
	{
		return Parse(s, new CommonConfig_Server());
	}

	public static CommonConfig_Server Parse(string s, CommonConfig_Server o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CommonConfig_Server Parse(IDictionary d)
	{
		return Parse(d, new CommonConfig_Server());
	}

	public static CommonConfig_Server Parse(IDictionary d, CommonConfig_Server o)
	{
		o.name = BaseJsonMarshaler.ParseValueString(d, "name");
		o.type = BaseJsonMarshaler.ParseValueString(d, "type");
		o.group = BaseJsonMarshaler.ParseValueString(d, "group");
		o.public_ip = BaseJsonMarshaler.ParseValueString(d, "public_ip");
		o.private_ip = BaseJsonMarshaler.ParseValueString(d, "private_ip");
		o.port = BaseJsonMarshaler.ParseValueInt(d, "port");
		o.standalone = BaseJsonMarshaler.ParseValueBool(d, "standalone");
		o.is_default = BaseJsonMarshaler.ParseValueBool(d, "is_default");
		o.sensitive_debugging = BaseJsonMarshaler.ParseValueBool(d, "sensitive_debugging");
		o.no = BaseJsonMarshaler.ParseValueInt(d, "no");
		return o;
	}

	public static string EncodeJsonString(CommonConfig_Server o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CommonConfig_Server o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.name != null)
		{
			dictionary.Add("name", o.name);
		}
		if (o.type != null)
		{
			dictionary.Add("type", o.type);
		}
		if (o.group != null)
		{
			dictionary.Add("group", o.group);
		}
		if (o.public_ip != null)
		{
			dictionary.Add("public_ip", o.public_ip);
		}
		if (o.private_ip != null)
		{
			dictionary.Add("private_ip", o.private_ip);
		}
		dictionary.Add("port", o.port.ToString());
		dictionary.Add("standalone", o.standalone.ToString());
		dictionary.Add("is_default", o.is_default.ToString());
		dictionary.Add("sensitive_debugging", o.sensitive_debugging.ToString());
		dictionary.Add("no", o.no.ToString());
		return dictionary;
	}
}
