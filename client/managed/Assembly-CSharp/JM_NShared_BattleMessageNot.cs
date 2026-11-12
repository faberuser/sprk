using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleMessageNot
{
	public static BattleMessageNot Parse(string s)
	{
		return Parse(s, new BattleMessageNot());
	}

	public static BattleMessageNot Parse(string s, BattleMessageNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleMessageNot Parse(IDictionary d)
	{
		return Parse(d, new BattleMessageNot());
	}

	public static BattleMessageNot Parse(IDictionary d, BattleMessageNot o)
	{
		o.SenderId = BaseJsonMarshaler.ParseValueLong(d, "SenderId");
		o.GroupType = BaseJsonMarshaler.ParseEnum<MessageGroupType>(d, "GroupType");
		o.ReceiverIds = BaseJsonMarshaler.ParseArrayCast(d, "ReceiverIds", BaseJsonMarshaler.CastToLong);
		o.Type = BaseJsonMarshaler.ParseValueString(d, "Type");
		o.Content = BaseJsonMarshaler.ParseValueString(d, "Content");
		return o;
	}

	public static string EncodeJsonString(BattleMessageNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleMessageNot o)
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
