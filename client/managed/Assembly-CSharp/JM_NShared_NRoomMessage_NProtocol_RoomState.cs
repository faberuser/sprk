using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage.NProtocol;

public static class JM_NShared_NRoomMessage_NProtocol_RoomState
{
	public static RoomState Parse(string s)
	{
		return Parse(s, new RoomState());
	}

	public static RoomState Parse(string s, RoomState o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RoomState Parse(IDictionary d)
	{
		return Parse(d, new RoomState());
	}

	public static RoomState Parse(IDictionary d, RoomState o)
	{
		o.IsBroadcasting = BaseJsonMarshaler.ParseValueBool(d, "IsBroadcasting");
		return o;
	}

	public static string EncodeJsonString(RoomState o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RoomState o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsBroadcasting", o.IsBroadcasting.ToString());
		return dictionary;
	}
}
