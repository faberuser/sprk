using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage.NProtocol;

public static class JM_NShared_NRoomMessage_NProtocol_RoomMicState
{
	public static RoomMicState Parse(string s)
	{
		return Parse(s, new RoomMicState());
	}

	public static RoomMicState Parse(string s, RoomMicState o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomMicState Parse(IDictionary d)
	{
		return Parse(d, new RoomMicState());
	}

	public static RoomMicState Parse(IDictionary d, RoomMicState o)
	{
		o.UseMic = BaseJsonMarshaler.ParseValueBool(d, "UseMic");
		return o;
	}

	public static string EncodeJsonString(RoomMicState o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomMicState o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("UseMic", o.UseMic.ToString());
		return dictionary;
	}
}
