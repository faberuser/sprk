using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage;

public static class JM_NShared_NRoomMessage_RoomPingReq
{
	public static RoomPingReq Parse(string s)
	{
		return Parse(s, new RoomPingReq());
	}

	public static RoomPingReq Parse(string s, RoomPingReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomPingReq Parse(IDictionary d)
	{
		return Parse(d, new RoomPingReq());
	}

	public static RoomPingReq Parse(IDictionary d, RoomPingReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(RoomPingReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomPingReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
