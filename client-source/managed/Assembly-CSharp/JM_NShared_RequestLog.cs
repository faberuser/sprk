using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RequestLog
{
	public static RequestLog Parse(string s)
	{
		return Parse(s, new RequestLog());
	}

	public static RequestLog Parse(string s, RequestLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RequestLog Parse(IDictionary d)
	{
		return Parse(d, new RequestLog());
	}

	public static RequestLog Parse(IDictionary d, RequestLog o)
	{
		o.Service = BaseJsonMarshaler.ParseValueString(d, "Service");
		o.Argument = BaseJsonMarshaler.ParseValueString(d, "Argument");
		o.Time = BaseJsonMarshaler.ParseValueString(d, "Time");
		return o;
	}

	public static string EncodeJsonString(RequestLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RequestLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Service != null)
		{
			dictionary.Add("Service", o.Service);
		}
		if (o.Argument != null)
		{
			dictionary.Add("Argument", o.Argument);
		}
		if (o.Time != null)
		{
			dictionary.Add("Time", o.Time);
		}
		return dictionary;
	}
}
