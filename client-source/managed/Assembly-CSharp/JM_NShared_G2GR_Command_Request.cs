using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2GR_Command;

public static class JM_NShared_G2GR_Command_Request
{
	public static NShared.G2GR_Command.Request Parse(string s)
	{
		return Parse(s, new NShared.G2GR_Command.Request());
	}

	public static NShared.G2GR_Command.Request Parse(string s, NShared.G2GR_Command.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2GR_Command.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2GR_Command.Request());
	}

	public static NShared.G2GR_Command.Request Parse(IDictionary d, NShared.G2GR_Command.Request o)
	{
		o.Host = BaseJsonMarshaler.ParseValueString(d, "Host");
		o.Namespace = BaseJsonMarshaler.ParseValueString(d, "Namespace");
		o.Path = BaseJsonMarshaler.ParseValueString(d, "Path");
		o.Service = BaseJsonMarshaler.ParseValueString(d, "Service");
		o.Method = BaseJsonMarshaler.ParseValueString(d, "Method");
		o.Argv = BaseJsonMarshaler.ParseArrayCast(d, "Argv", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(NShared.G2GR_Command.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2GR_Command.Request o)
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
		if (o.Namespace != null)
		{
			dictionary.Add("Namespace", o.Namespace);
		}
		if (o.Path != null)
		{
			dictionary.Add("Path", o.Path);
		}
		if (o.Service != null)
		{
			dictionary.Add("Service", o.Service);
		}
		if (o.Method != null)
		{
			dictionary.Add("Method", o.Method);
		}
		dictionary.Add("Argv", BaseJsonMarshaler.EncodeArray(o.Argv));
		return dictionary;
	}
}
