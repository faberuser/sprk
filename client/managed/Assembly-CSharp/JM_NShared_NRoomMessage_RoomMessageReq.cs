using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage;

public static class JM_NShared_NRoomMessage_RoomMessageReq
{
	public static RoomMessageReq Parse(string s)
	{
		return Parse(s, new RoomMessageReq());
	}

	public static RoomMessageReq Parse(string s, RoomMessageReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomMessageReq Parse(IDictionary d)
	{
		return Parse(d, new RoomMessageReq());
	}

	public static RoomMessageReq Parse(IDictionary d, RoomMessageReq o)
	{
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(RoomMessageReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomMessageReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Message != null)
		{
			dictionary.Add("Message", o.Message);
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
