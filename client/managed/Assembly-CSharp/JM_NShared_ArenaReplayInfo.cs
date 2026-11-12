using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ArenaReplayInfo
{
	public static ArenaReplayInfo Parse(string s)
	{
		return Parse(s, new ArenaReplayInfo());
	}

	public static ArenaReplayInfo Parse(string s, ArenaReplayInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ArenaReplayInfo Parse(IDictionary d)
	{
		return Parse(d, new ArenaReplayInfo());
	}

	public static ArenaReplayInfo Parse(IDictionary d, ArenaReplayInfo o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseEnum<DungeonDifficultyType>(d, "Difficulty");
		o.LeftTeam = BaseJsonMarshaler.ParseObject(d, "LeftTeam", JM_NShared_ReplayTeamInfo.Parse);
		o.RightTeam = BaseJsonMarshaler.ParseObject(d, "RightTeam", JM_NShared_ReplayTeamInfo.Parse);
		o.WinTeam = BaseJsonMarshaler.ParseEnum<TeamType>(d, "WinTeam");
		o.WinMatchScore = BaseJsonMarshaler.ParseValueInt(d, "WinMatchScore");
		o.LoseMatchScore = BaseJsonMarshaler.ParseValueInt(d, "LoseMatchScore");
		o.PlayTimeMs = BaseJsonMarshaler.ParseValueInt(d, "PlayTimeMs");
		o.RecordedTime = BaseJsonMarshaler.ParseValueString(d, "RecordedTime");
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		return o;
	}

	public static string EncodeJsonString(ArenaReplayInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ArenaReplayInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("LeftTeam", JM_NShared_ReplayTeamInfo.EncodeJsonObject(o.LeftTeam));
		dictionary.Add("RightTeam", JM_NShared_ReplayTeamInfo.EncodeJsonObject(o.RightTeam));
		dictionary.Add("WinTeam", o.WinTeam.ToString());
		dictionary.Add("WinMatchScore", o.WinMatchScore.ToString());
		dictionary.Add("LoseMatchScore", o.LoseMatchScore.ToString());
		dictionary.Add("PlayTimeMs", o.PlayTimeMs.ToString());
		if (o.RecordedTime != null)
		{
			dictionary.Add("RecordedTime", o.RecordedTime);
		}
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		return dictionary;
	}
}
