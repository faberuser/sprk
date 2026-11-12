using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LeavePartyDungeonRoom;

public static class JM_NShared_LeavePartyDungeonRoom_Request
{
	public static NShared.LeavePartyDungeonRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.LeavePartyDungeonRoom.Request());
	}

	public static NShared.LeavePartyDungeonRoom.Request Parse(string s, NShared.LeavePartyDungeonRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LeavePartyDungeonRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LeavePartyDungeonRoom.Request());
	}

	public static NShared.LeavePartyDungeonRoom.Request Parse(IDictionary d, NShared.LeavePartyDungeonRoom.Request o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.PartyDungeonRoomDestroyed = BaseJsonMarshaler.ParseValueBool(d, "PartyDungeonRoomDestroyed");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LeavePartyDungeonRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LeavePartyDungeonRoom.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("PartyDungeonRoomDestroyed", o.PartyDungeonRoomDestroyed.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
