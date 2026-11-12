using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_ChangePartyDungeonRoomInfo
{
	public static ChangePartyDungeonRoomInfo Parse(string s)
	{
		return Parse(s, new ChangePartyDungeonRoomInfo());
	}

	public static ChangePartyDungeonRoomInfo Parse(string s, ChangePartyDungeonRoomInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangePartyDungeonRoomInfo Parse(IDictionary d)
	{
		return Parse(d, new ChangePartyDungeonRoomInfo());
	}

	public static ChangePartyDungeonRoomInfo Parse(IDictionary d, ChangePartyDungeonRoomInfo o)
	{
		o.DungeonType = BaseJsonMarshaler.ParseEnum<PartyDungeonType>(d, "DungeonType");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseEnum<PartyDungeonDifficultyType>(d, "Difficulty");
		o.IsResetSquard = BaseJsonMarshaler.ParseValueBool(d, "IsResetSquard");
		return o;
	}

	public static string EncodeJsonString(ChangePartyDungeonRoomInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangePartyDungeonRoomInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("IsResetSquard", o.IsResetSquard.ToString());
		return dictionary;
	}
}
