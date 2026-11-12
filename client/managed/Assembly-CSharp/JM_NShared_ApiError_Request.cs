using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ApiError;

public static class JM_NShared_ApiError_Request
{
	public static NShared.ApiError.Request Parse(string s)
	{
		return Parse(s, new NShared.ApiError.Request());
	}

	public static NShared.ApiError.Request Parse(string s, NShared.ApiError.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ApiError.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ApiError.Request());
	}

	public static NShared.ApiError.Request Parse(IDictionary d, NShared.ApiError.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.LoginIds = BaseJsonMarshaler.ParseArrayCast(d, "LoginIds", BaseJsonMarshaler.CastToString);
		o.Versions = BaseJsonMarshaler.ParseArrayCast(d, "Versions", BaseJsonMarshaler.CastToString);
		o.Seqs = BaseJsonMarshaler.ParseArrayCast(d, "Seqs", BaseJsonMarshaler.CastToLong);
		o.PreApis = BaseJsonMarshaler.ParseArrayCast(d, "PreApis", BaseJsonMarshaler.CastToString);
		o.Apis = BaseJsonMarshaler.ParseArrayCast(d, "Apis", BaseJsonMarshaler.CastToString);
		o.HttpErrors = BaseJsonMarshaler.ParseArrayCast(d, "HttpErrors", BaseJsonMarshaler.CastToString);
		o.ServerErrors = BaseJsonMarshaler.ParseArrayCast(d, "ServerErrors", BaseJsonMarshaler.CastToString);
		o.Messages = BaseJsonMarshaler.ParseArrayCast(d, "Messages", BaseJsonMarshaler.CastToString);
		o.ServerMessages = BaseJsonMarshaler.ParseArrayCast(d, "ServerMessages", BaseJsonMarshaler.CastToString);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ApiError.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ApiError.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("LoginIds", BaseJsonMarshaler.EncodeArray(o.LoginIds));
		dictionary.Add("Versions", BaseJsonMarshaler.EncodeArray(o.Versions));
		dictionary.Add("Seqs", BaseJsonMarshaler.EncodeArray(o.Seqs));
		dictionary.Add("PreApis", BaseJsonMarshaler.EncodeArray(o.PreApis));
		dictionary.Add("Apis", BaseJsonMarshaler.EncodeArray(o.Apis));
		dictionary.Add("HttpErrors", BaseJsonMarshaler.EncodeArray(o.HttpErrors));
		dictionary.Add("ServerErrors", BaseJsonMarshaler.EncodeArray(o.ServerErrors));
		dictionary.Add("Messages", BaseJsonMarshaler.EncodeArray(o.Messages));
		dictionary.Add("ServerMessages", BaseJsonMarshaler.EncodeArray(o.ServerMessages));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
