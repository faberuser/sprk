using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LeaveRaidRoom;

public static class JM_NShared_LeaveRaidRoom_Request
{
	public static NShared.LeaveRaidRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.LeaveRaidRoom.Request());
	}

	public static NShared.LeaveRaidRoom.Request Parse(string s, NShared.LeaveRaidRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LeaveRaidRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LeaveRaidRoom.Request());
	}

	public static NShared.LeaveRaidRoom.Request Parse(IDictionary d, NShared.LeaveRaidRoom.Request o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.RaidRoomDestroyed = BaseJsonMarshaler.ParseValueBool(d, "RaidRoomDestroyed");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LeaveRaidRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LeaveRaidRoom.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("RaidRoomDestroyed", o.RaidRoomDestroyed.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
