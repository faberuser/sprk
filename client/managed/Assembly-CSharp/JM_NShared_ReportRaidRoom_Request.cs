using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ReportRaidRoom;

public static class JM_NShared_ReportRaidRoom_Request
{
	public static NShared.ReportRaidRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.ReportRaidRoom.Request());
	}

	public static NShared.ReportRaidRoom.Request Parse(string s, NShared.ReportRaidRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReportRaidRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReportRaidRoom.Request());
	}

	public static NShared.ReportRaidRoom.Request Parse(IDictionary d, NShared.ReportRaidRoom.Request o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.Reason = BaseJsonMarshaler.ParseValueInt(d, "Reason");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ReportRaidRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ReportRaidRoom.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("Reason", o.Reason.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
