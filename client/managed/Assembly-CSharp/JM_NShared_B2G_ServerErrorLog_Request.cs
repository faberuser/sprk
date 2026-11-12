using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_ServerErrorLog;

public static class JM_NShared_B2G_ServerErrorLog_Request
{
	public static NShared.B2G_ServerErrorLog.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_ServerErrorLog.Request());
	}

	public static NShared.B2G_ServerErrorLog.Request Parse(string s, NShared.B2G_ServerErrorLog.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_ServerErrorLog.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_ServerErrorLog.Request());
	}

	public static NShared.B2G_ServerErrorLog.Request Parse(IDictionary d, NShared.B2G_ServerErrorLog.Request o)
	{
		o.ServerName = BaseJsonMarshaler.ParseValueString(d, "ServerName");
		o.PublicIp = BaseJsonMarshaler.ParseValueString(d, "PublicIp");
		o.Port = BaseJsonMarshaler.ParseValueInt(d, "Port");
		o.ServerVersion = BaseJsonMarshaler.ParseValueString(d, "ServerVersion");
		o.IsCheckServer = BaseJsonMarshaler.ParseValueBool(d, "IsCheckServer");
		o.ErrorMessage = BaseJsonMarshaler.ParseValueString(d, "ErrorMessage");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_ServerErrorLog.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_ServerErrorLog.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ServerName != null)
		{
			dictionary.Add("ServerName", o.ServerName);
		}
		if (o.PublicIp != null)
		{
			dictionary.Add("PublicIp", o.PublicIp);
		}
		dictionary.Add("Port", o.Port.ToString());
		if (o.ServerVersion != null)
		{
			dictionary.Add("ServerVersion", o.ServerVersion);
		}
		dictionary.Add("IsCheckServer", o.IsCheckServer.ToString());
		if (o.ErrorMessage != null)
		{
			dictionary.Add("ErrorMessage", o.ErrorMessage);
		}
		return dictionary;
	}
}
