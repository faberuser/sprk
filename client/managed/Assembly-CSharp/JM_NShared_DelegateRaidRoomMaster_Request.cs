using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.DelegateRaidRoomMaster;

public static class JM_NShared_DelegateRaidRoomMaster_Request
{
	public static NShared.DelegateRaidRoomMaster.Request Parse(string s)
	{
		return Parse(s, new NShared.DelegateRaidRoomMaster.Request());
	}

	public static NShared.DelegateRaidRoomMaster.Request Parse(string s, NShared.DelegateRaidRoomMaster.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.DelegateRaidRoomMaster.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.DelegateRaidRoomMaster.Request());
	}

	public static NShared.DelegateRaidRoomMaster.Request Parse(IDictionary d, NShared.DelegateRaidRoomMaster.Request o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.PrevMasterId = BaseJsonMarshaler.ParseValueLong(d, "PrevMasterId");
		o.CurMasterId = BaseJsonMarshaler.ParseValueLong(d, "CurMasterId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.DelegateRaidRoomMaster.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.DelegateRaidRoomMaster.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("PrevMasterId", o.PrevMasterId.ToString());
		dictionary.Add("CurMasterId", o.CurMasterId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
