using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PingRaidRoom;

public static class JM_NShared_PingRaidRoom_Request
{
	public static NShared.PingRaidRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.PingRaidRoom.Request());
	}

	public static NShared.PingRaidRoom.Request Parse(string s, NShared.PingRaidRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PingRaidRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PingRaidRoom.Request());
	}

	public static NShared.PingRaidRoom.Request Parse(IDictionary d, NShared.PingRaidRoom.Request o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PingRaidRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PingRaidRoom.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
