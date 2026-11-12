using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangePartyDungeonRoomInfo;

public static class JM_NShared_ChangePartyDungeonRoomInfo_Request
{
	public static NShared.ChangePartyDungeonRoomInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangePartyDungeonRoomInfo.Request());
	}

	public static NShared.ChangePartyDungeonRoomInfo.Request Parse(string s, NShared.ChangePartyDungeonRoomInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangePartyDungeonRoomInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangePartyDungeonRoomInfo.Request());
	}

	public static NShared.ChangePartyDungeonRoomInfo.Request Parse(IDictionary d, NShared.ChangePartyDungeonRoomInfo.Request o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.DungeonType = BaseJsonMarshaler.ParseEnum<PartyDungeonType>(d, "DungeonType");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseEnum<PartyDungeonDifficultyType>(d, "DungeonDifficulty");
		o.Status = BaseJsonMarshaler.ParseValueByte(d, "Status");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangePartyDungeonRoomInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangePartyDungeonRoomInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("Status", o.Status.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
