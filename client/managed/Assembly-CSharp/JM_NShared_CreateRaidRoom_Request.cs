using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CreateRaidRoom;

public static class JM_NShared_CreateRaidRoom_Request
{
	public static NShared.CreateRaidRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.CreateRaidRoom.Request());
	}

	public static NShared.CreateRaidRoom.Request Parse(string s, NShared.CreateRaidRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CreateRaidRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CreateRaidRoom.Request());
	}

	public static NShared.CreateRaidRoom.Request Parse(IDictionary d, NShared.CreateRaidRoom.Request o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.RaidMonsterLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidMonsterLevel");
		o.GoalIndex = BaseJsonMarshaler.ParseValueByte(d, "GoalIndex");
		o.Opened = BaseJsonMarshaler.ParseValueByte(d, "Opened");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CreateRaidRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CreateRaidRoom.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("RaidMonsterLevel", o.RaidMonsterLevel.ToString());
		dictionary.Add("GoalIndex", o.GoalIndex.ToString());
		dictionary.Add("Opened", o.Opened.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
