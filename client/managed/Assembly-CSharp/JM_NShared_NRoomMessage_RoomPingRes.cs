using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage;

public static class JM_NShared_NRoomMessage_RoomPingRes
{
	public static RoomPingRes Parse(string s)
	{
		return Parse(s, new RoomPingRes());
	}

	public static RoomPingRes Parse(string s, RoomPingRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomPingRes Parse(IDictionary d)
	{
		return Parse(d, new RoomPingRes());
	}

	public static RoomPingRes Parse(IDictionary d, RoomPingRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<RoomPingResType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(RoomPingRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomPingRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
