using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage;

public static class JM_NShared_NRoomMessage_RoomMessageRes
{
	public static RoomMessageRes Parse(string s)
	{
		return Parse(s, new RoomMessageRes());
	}

	public static RoomMessageRes Parse(string s, RoomMessageRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomMessageRes Parse(IDictionary d)
	{
		return Parse(d, new RoomMessageRes());
	}

	public static RoomMessageRes Parse(IDictionary d, RoomMessageRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<RoomMessageResType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(RoomMessageRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomMessageRes o)
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
