using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CreatePartyDungeonRoom;

public static class JM_NShared_CreatePartyDungeonRoom_Request
{
	public static NShared.CreatePartyDungeonRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.CreatePartyDungeonRoom.Request());
	}

	public static NShared.CreatePartyDungeonRoom.Request Parse(string s, NShared.CreatePartyDungeonRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CreatePartyDungeonRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CreatePartyDungeonRoom.Request());
	}

	public static NShared.CreatePartyDungeonRoom.Request Parse(IDictionary d, NShared.CreatePartyDungeonRoom.Request o)
	{
		o.DungeonType = BaseJsonMarshaler.ParseEnum<PartyDungeonType>(d, "DungeonType");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseEnum<PartyDungeonDifficultyType>(d, "DungeonDifficulty");
		o.Opened = BaseJsonMarshaler.ParseValueByte(d, "Opened");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CreatePartyDungeonRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CreatePartyDungeonRoom.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("Opened", o.Opened.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
