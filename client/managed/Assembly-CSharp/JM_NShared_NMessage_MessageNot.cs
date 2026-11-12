using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_MessageNot
{
	public static MessageNot Parse(string s)
	{
		return Parse(s, new MessageNot());
	}

	public static MessageNot Parse(string s, MessageNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MessageNot Parse(IDictionary d)
	{
		return Parse(d, new MessageNot());
	}

	public static MessageNot Parse(IDictionary d, MessageNot o)
	{
		o.SenderId = BaseJsonMarshaler.ParseValueLong(d, "SenderId");
		o.GroupType = BaseJsonMarshaler.ParseEnum<MessageGroupType>(d, "GroupType");
		o.ReceiverIds = BaseJsonMarshaler.ParseArrayCast(d, "ReceiverIds", BaseJsonMarshaler.CastToLong);
		o.Type = BaseJsonMarshaler.ParseValueString(d, "Type");
		o.Content = BaseJsonMarshaler.ParseValueString(d, "Content");
		return o;
	}

	public static string EncodeJsonString(MessageNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MessageNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SenderId", o.SenderId.ToString());
		dictionary.Add("GroupType", o.GroupType.ToString());
		dictionary.Add("ReceiverIds", BaseJsonMarshaler.EncodeArray(o.ReceiverIds));
		if (o.Type != null)
		{
			dictionary.Add("Type", o.Type);
		}
		if (o.Content != null)
		{
			dictionary.Add("Content", o.Content);
		}
		return dictionary;
	}
}
