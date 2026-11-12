using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ApiErrorLog
{
	public static ApiErrorLog Parse(string s)
	{
		return Parse(s, new ApiErrorLog());
	}

	public static ApiErrorLog Parse(string s, ApiErrorLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ApiErrorLog Parse(IDictionary d)
	{
		return Parse(d, new ApiErrorLog());
	}

	public static ApiErrorLog Parse(IDictionary d, ApiErrorLog o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.LoginId = BaseJsonMarshaler.ParseValueString(d, "LoginId");
		o.Version = BaseJsonMarshaler.ParseValueString(d, "Version");
		o.Seq = BaseJsonMarshaler.ParseValueLong(d, "Seq");
		o.PreApi = BaseJsonMarshaler.ParseValueString(d, "PreApi");
		o.Api = BaseJsonMarshaler.ParseValueString(d, "Api");
		o.HttpError = BaseJsonMarshaler.ParseValueString(d, "HttpError");
		o.ServerError = BaseJsonMarshaler.ParseValueString(d, "ServerError");
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		o.ServerMessage = BaseJsonMarshaler.ParseValueString(d, "ServerMessage");
		return o;
	}

	public static string EncodeJsonString(ApiErrorLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ApiErrorLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.LoginId != null)
		{
			dictionary.Add("LoginId", o.LoginId);
		}
		if (o.Version != null)
		{
			dictionary.Add("Version", o.Version);
		}
		dictionary.Add("Seq", o.Seq.ToString());
		if (o.PreApi != null)
		{
			dictionary.Add("PreApi", o.PreApi);
		}
		if (o.Api != null)
		{
			dictionary.Add("Api", o.Api);
		}
		if (o.HttpError != null)
		{
			dictionary.Add("HttpError", o.HttpError);
		}
		if (o.ServerError != null)
		{
			dictionary.Add("ServerError", o.ServerError);
		}
		if (o.Message != null)
		{
			dictionary.Add("Message", o.Message);
		}
		if (o.ServerMessage != null)
		{
			dictionary.Add("ServerMessage", o.ServerMessage);
		}
		return dictionary;
	}
}
