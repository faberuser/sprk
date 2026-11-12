using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AppServerInfo
{
	public static AppServerInfo Parse(string s)
	{
		return Parse(s, new AppServerInfo());
	}

	public static AppServerInfo Parse(string s, AppServerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AppServerInfo Parse(IDictionary d)
	{
		return Parse(d, new AppServerInfo());
	}

	public static AppServerInfo Parse(IDictionary d, AppServerInfo o)
	{
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Host = BaseJsonMarshaler.ParseValueString(d, "Host");
		o.Opened = BaseJsonMarshaler.ParseValueBool(d, "Opened");
		o.Https = BaseJsonMarshaler.ParseValueBool(d, "Https");
		o.CookieKeyword = BaseJsonMarshaler.ParseValueString(d, "CookieKeyword");
		o.Country = BaseJsonMarshaler.ParseArrayCast(d, "Country", BaseJsonMarshaler.CastToString);
		o.ShutdownMessage = BaseJsonMarshaler.ParseValueString(d, "ShutdownMessage");
		o.QueryServerStatusUrl = BaseJsonMarshaler.ParseValueString(d, "QueryServerStatusUrl");
		o.LoginServer = BaseJsonMarshaler.ParseValueString(d, "LoginServer");
		return o;
	}

	public static string EncodeJsonString(AppServerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AppServerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.Host != null)
		{
			dictionary.Add("Host", o.Host);
		}
		dictionary.Add("Opened", o.Opened.ToString());
		dictionary.Add("Https", o.Https.ToString());
		if (o.CookieKeyword != null)
		{
			dictionary.Add("CookieKeyword", o.CookieKeyword);
		}
		dictionary.Add("Country", BaseJsonMarshaler.EncodeArray(o.Country));
		if (o.ShutdownMessage != null)
		{
			dictionary.Add("ShutdownMessage", o.ShutdownMessage);
		}
		if (o.QueryServerStatusUrl != null)
		{
			dictionary.Add("QueryServerStatusUrl", o.QueryServerStatusUrl);
		}
		if (o.LoginServer != null)
		{
			dictionary.Add("LoginServer", o.LoginServer);
		}
		return dictionary;
	}
}
