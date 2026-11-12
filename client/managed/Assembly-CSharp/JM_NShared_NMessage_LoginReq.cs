using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;
using NVespa;
using NVespa.NWeb;

public static class JM_NShared_NMessage_LoginReq
{
	public static LoginReq Parse(string s)
	{
		return Parse(s, new LoginReq());
	}

	public static LoginReq Parse(string s, LoginReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LoginReq Parse(IDictionary d)
	{
		return Parse(d, new LoginReq());
	}

	public static LoginReq Parse(IDictionary d, LoginReq o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.FriendIds = BaseJsonMarshaler.ParseArrayCast(d, "FriendIds", BaseJsonMarshaler.CastToLong);
		o.ChannelNo = BaseJsonMarshaler.ParseValueInt(d, "ChannelNo");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(LoginReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LoginReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("FriendIds", BaseJsonMarshaler.EncodeArray(o.FriendIds));
		dictionary.Add("ChannelNo", o.ChannelNo.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		dictionary.Add("SessionKey", SingletonUpdater<WebServiceRequester>.instance.SessionKey);
		return dictionary;
	}
}
