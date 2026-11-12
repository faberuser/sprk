using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage;

public static class JM_NShared_NRoomMessage_RoomLoginReq
{
	public static RoomLoginReq Parse(string s)
	{
		return Parse(s, new RoomLoginReq());
	}

	public static RoomLoginReq Parse(string s, RoomLoginReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomLoginReq Parse(IDictionary d)
	{
		return Parse(d, new RoomLoginReq());
	}

	public static RoomLoginReq Parse(IDictionary d, RoomLoginReq o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.RoomId = BaseJsonMarshaler.ParseValueLong(d, "RoomId");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(RoomLoginReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomLoginReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("RoomId", o.RoomId.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
