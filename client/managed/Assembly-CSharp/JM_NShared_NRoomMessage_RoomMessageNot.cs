using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage;

public static class JM_NShared_NRoomMessage_RoomMessageNot
{
	public static RoomMessageNot Parse(string s)
	{
		return Parse(s, new RoomMessageNot());
	}

	public static RoomMessageNot Parse(string s, RoomMessageNot o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomMessageNot Parse(IDictionary d)
	{
		return Parse(d, new RoomMessageNot());
	}

	public static RoomMessageNot Parse(IDictionary d, RoomMessageNot o)
	{
		o.SenderId = BaseJsonMarshaler.ParseValueLong(d, "SenderId");
		o.Type = BaseJsonMarshaler.ParseValueString(d, "Type");
		o.Content = BaseJsonMarshaler.ParseValueString(d, "Content");
		return o;
	}

	public static string EncodeJsonString(RoomMessageNot o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomMessageNot o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SenderId", o.SenderId.ToString());
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
