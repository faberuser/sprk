using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_LoginRes
{
	public static LoginRes Parse(string s)
	{
		return Parse(s, new LoginRes());
	}

	public static LoginRes Parse(string s, LoginRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LoginRes Parse(IDictionary d)
	{
		return Parse(d, new LoginRes());
	}

	public static LoginRes Parse(IDictionary d, LoginRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<LoginResType>(d, "Result");
		o.WhisperChats = BaseJsonMarshaler.ParseArray(d, "WhisperChats", JM_NShared_NMessage_MessageNot.Parse);
		o.GuildChats = BaseJsonMarshaler.ParseArray(d, "GuildChats", JM_NShared_NMessage_MessageNot.Parse);
		o.ChannelNo = BaseJsonMarshaler.ParseValueInt(d, "ChannelNo");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(LoginRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LoginRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("WhisperChats", BaseJsonMarshaler.EncodeArray(o.WhisperChats, JM_NShared_NMessage_MessageNot.EncodeJsonObject));
		dictionary.Add("GuildChats", BaseJsonMarshaler.EncodeArray(o.GuildChats, JM_NShared_NMessage_MessageNot.EncodeJsonObject));
		dictionary.Add("ChannelNo", o.ChannelNo.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
