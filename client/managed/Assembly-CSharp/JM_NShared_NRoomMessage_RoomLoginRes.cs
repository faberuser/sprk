using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage;

public static class JM_NShared_NRoomMessage_RoomLoginRes
{
	public static RoomLoginRes Parse(string s)
	{
		return Parse(s, new RoomLoginRes());
	}

	public static RoomLoginRes Parse(string s, RoomLoginRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomLoginRes Parse(IDictionary d)
	{
		return Parse(d, new RoomLoginRes());
	}

	public static RoomLoginRes Parse(IDictionary d, RoomLoginRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<RoomLoginResType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(RoomLoginRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomLoginRes o)
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
