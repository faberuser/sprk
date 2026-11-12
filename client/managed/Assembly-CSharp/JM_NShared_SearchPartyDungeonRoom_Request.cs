using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SearchPartyDungeonRoom;

public static class JM_NShared_SearchPartyDungeonRoom_Request
{
	public static NShared.SearchPartyDungeonRoom.Request Parse(string s)
	{
		return Parse(s, new NShared.SearchPartyDungeonRoom.Request());
	}

	public static NShared.SearchPartyDungeonRoom.Request Parse(string s, NShared.SearchPartyDungeonRoom.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SearchPartyDungeonRoom.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SearchPartyDungeonRoom.Request());
	}

	public static NShared.SearchPartyDungeonRoom.Request Parse(IDictionary d, NShared.SearchPartyDungeonRoom.Request o)
	{
		o.DungeonType = BaseJsonMarshaler.ParseEnum<PartyDungeonType>(d, "DungeonType");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseEnum<PartyDungeonDifficultyType>(d, "DungeonDifficulty");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SearchPartyDungeonRoom.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SearchPartyDungeonRoom.Request o)
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
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
