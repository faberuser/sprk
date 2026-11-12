using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RaidRoomInfo
{
	public static RaidRoomInfo Parse(string s)
	{
		return Parse(s, new RaidRoomInfo());
	}

	public static RaidRoomInfo Parse(string s, RaidRoomInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RaidRoomInfo Parse(IDictionary d)
	{
		return Parse(d, new RaidRoomInfo());
	}

	public static RaidRoomInfo Parse(IDictionary d, RaidRoomInfo o)
	{
		o.RoomNo = BaseJsonMarshaler.ParseValueLong(d, "RoomNo");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.RaidMonsterLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidMonsterLevel");
		o.GoalIndex = BaseJsonMarshaler.ParseValueByte(d, "GoalIndex");
		o.DungeonType = BaseJsonMarshaler.ParseEnum<PartyDungeonType>(d, "DungeonType");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseEnum<PartyDungeonDifficultyType>(d, "DungeonDifficulty");
		o.Opened = BaseJsonMarshaler.ParseValueByte(d, "Opened");
		o.MasterAccountId = BaseJsonMarshaler.ParseValueLong(d, "MasterAccountId");
		o.MasterNick = BaseJsonMarshaler.ParseValueString(d, "MasterNick");
		o.CurPartyMember = BaseJsonMarshaler.ParseValueByte(d, "CurPartyMember");
		o.IsAutoRoot = BaseJsonMarshaler.ParseValueBool(d, "IsAutoRoot");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(RaidRoomInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RaidRoomInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RoomNo", o.RoomNo.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("RaidMonsterLevel", o.RaidMonsterLevel.ToString());
		dictionary.Add("GoalIndex", o.GoalIndex.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("Opened", o.Opened.ToString());
		dictionary.Add("MasterAccountId", o.MasterAccountId.ToString());
		if (o.MasterNick != null)
		{
			dictionary.Add("MasterNick", o.MasterNick);
		}
		dictionary.Add("CurPartyMember", o.CurPartyMember.ToString());
		dictionary.Add("IsAutoRoot", o.IsAutoRoot.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		return dictionary;
	}
}
