using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangeRaidRoomInfo;

public static class JM_NShared_ChangeRaidRoomInfo_Request
{
	public static NShared.ChangeRaidRoomInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangeRaidRoomInfo.Request());
	}

	public static NShared.ChangeRaidRoomInfo.Request Parse(string s, NShared.ChangeRaidRoomInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangeRaidRoomInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangeRaidRoomInfo.Request());
	}

	public static NShared.ChangeRaidRoomInfo.Request Parse(IDictionary d, NShared.ChangeRaidRoomInfo.Request o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.RaidMonsterLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidMonsterLevel");
		o.GoalIndex = BaseJsonMarshaler.ParseValueByte(d, "GoalIndex");
		o.Status = BaseJsonMarshaler.ParseValueByte(d, "Status");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangeRaidRoomInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangeRaidRoomInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("RaidMonsterLevel", o.RaidMonsterLevel.ToString());
		dictionary.Add("GoalIndex", o.GoalIndex.ToString());
		dictionary.Add("Status", o.Status.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
