using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PingPartyDungeonRoom;

public static class JM_NShared_PingPartyDungeonRoom_Request
{
	public static NShared.PingPartyDungeonRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.PingPartyDungeonRoom.Request());
	}

	public static NShared.PingPartyDungeonRoom.Request Parse(string s, NShared.PingPartyDungeonRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PingPartyDungeonRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PingPartyDungeonRoom.Request());
	}

	public static NShared.PingPartyDungeonRoom.Request Parse(IDictionary d, NShared.PingPartyDungeonRoom.Request o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PingPartyDungeonRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PingPartyDungeonRoom.Request o)
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
