using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_VersionedHost
{
	public static VersionedHost Parse(string s)
	{
		return Parse(s, new VersionedHost());
	}

	public static VersionedHost Parse(string s, VersionedHost o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static VersionedHost Parse(IDictionary d)
	{
		return Parse(d, new VersionedHost());
	}

	public static VersionedHost Parse(IDictionary d, VersionedHost o)
	{
		o.Type = BaseJsonMarshaler.ParseValueString(d, "Type");
		o.Description = BaseJsonMarshaler.ParseValueString(d, "Description");
		o.AppVersion = BaseJsonMarshaler.ParseArray(d, "AppVersion", JM_NShared_AppVersionInfo.Parse);
		o.Server = BaseJsonMarshaler.ParseArray(d, "Server", JM_NShared_AppServerInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(VersionedHost o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(VersionedHost o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Type != null)
		{
			dictionary.Add("Type", o.Type);
		}
		if (o.Description != null)
		{
			dictionary.Add("Description", o.Description);
		}
		dictionary.Add("AppVersion", BaseJsonMarshaler.EncodeArray(o.AppVersion, JM_NShared_AppVersionInfo.EncodeJsonObject));
		dictionary.Add("Server", BaseJsonMarshaler.EncodeArray(o.Server, JM_NShared_AppServerInfo.EncodeJsonObject));
		return dictionary;
	}
}
