using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_MessageReq
{
	public static MessageReq Parse(string s)
	{
		return Parse(s, new MessageReq());
	}

	public static MessageReq Parse(string s, MessageReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MessageReq Parse(IDictionary d)
	{
		return Parse(d, new MessageReq());
	}

	public static MessageReq Parse(IDictionary d, MessageReq o)
	{
		o.GroupType = BaseJsonMarshaler.ParseEnum<MessageGroupType>(d, "GroupType");
		o.ReceiverIds = BaseJsonMarshaler.ParseArrayCast(d, "ReceiverIds", BaseJsonMarshaler.CastToLong);
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(MessageReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MessageReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupType", o.GroupType.ToString());
		dictionary.Add("ReceiverIds", BaseJsonMarshaler.EncodeArray(o.ReceiverIds));
		if (o.Message != null)
		{
			dictionary.Add("Message", o.Message);
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
