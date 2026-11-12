using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseBattleInfo
{
	public static EclipseBattleInfo Parse(string s)
	{
		return Parse(s, new EclipseBattleInfo());
	}

	public static EclipseBattleInfo Parse(string s, EclipseBattleInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseBattleInfo Parse(IDictionary d)
	{
		return Parse(d, new EclipseBattleInfo());
	}

	public static EclipseBattleInfo Parse(IDictionary d, EclipseBattleInfo o)
	{
		o.MatchIndex = BaseJsonMarshaler.ParseValueInt(d, "MatchIndex");
		o.DeckList = BaseJsonMarshaler.ParseArray(d, "DeckList", JM_NShared_EclipseDeckData.Parse);
		o.DungeonList = BaseJsonMarshaler.ParseArray(d, "DungeonList", JM_NShared_EclipseDungeonData.Parse);
		o.AdminLevel = BaseJsonMarshaler.ParseValueInt(d, "AdminLevel");
		o.GuildSkills = BaseJsonMarshaler.ParseArray(d, "GuildSkills", JM_NShared_GuildSkillDataBase.Parse);
		o.AccountBuffs = BaseJsonMarshaler.ParseArray(d, "AccountBuffs", JM_NShared_AccountBuffDataBase.Parse);
		o.ClassBuffDataBases = BaseJsonMarshaler.ParseArray(d, "ClassBuffDataBases", JM_NShared_ClassBuffDataBase.Parse);
		o.CurrentDeckIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentDeckIndex");
		o.CurrentChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentChapterIndex");
		o.CurrentDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentDungeonIndex");
		o.CurrentWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "CurrentWaveIndex");
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		o.CurrentCreatureLevel = BaseJsonMarshaler.ParseValueInt(d, "CurrentCreatureLevel");
		o.GameServerSessionKey = BaseJsonMarshaler.ParseValueString(d, "GameServerSessionKey");
		o.OnlineGameSpeedRatio = BaseJsonMarshaler.ParseValueInt(d, "OnlineGameSpeedRatio");
		o.ExtraStatDataBases = BaseJsonMarshaler.ParseArray(d, "ExtraStatDataBases", JM_NShared_DefaultExtraStatData.Parse);
		o.PetStatDataBases = BaseJsonMarshaler.ParseArray(d, "PetStatDataBases", JM_NShared_PetStatDataBase.Parse);
		return o;
	}

	public static string EncodeJsonString(EclipseBattleInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseBattleInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchIndex", o.MatchIndex.ToString());
		dictionary.Add("DeckList", BaseJsonMarshaler.EncodeArray(o.DeckList, JM_NShared_EclipseDeckData.EncodeJsonObject));
		dictionary.Add("DungeonList", BaseJsonMarshaler.EncodeArray(o.DungeonList, JM_NShared_EclipseDungeonData.EncodeJsonObject));
		dictionary.Add("AdminLevel", o.AdminLevel.ToString());
		dictionary.Add("GuildSkills", BaseJsonMarshaler.EncodeArray(o.GuildSkills, JM_NShared_GuildSkillDataBase.EncodeJsonObject));
		dictionary.Add("AccountBuffs", BaseJsonMarshaler.EncodeArray(o.AccountBuffs, JM_NShared_AccountBuffDataBase.EncodeJsonObject));
		dictionary.Add("ClassBuffDataBases", BaseJsonMarshaler.EncodeArray(o.ClassBuffDataBases, JM_NShared_ClassBuffDataBase.EncodeJsonObject));
		dictionary.Add("CurrentDeckIndex", o.CurrentDeckIndex.ToString());
		dictionary.Add("CurrentChapterIndex", o.CurrentChapterIndex.ToString());
		dictionary.Add("CurrentDungeonIndex", o.CurrentDungeonIndex.ToString());
		dictionary.Add("CurrentWaveIndex", o.CurrentWaveIndex.ToString());
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		dictionary.Add("CurrentCreatureLevel", o.CurrentCreatureLevel.ToString());
		if (o.GameServerSessionKey != null)
		{
			dictionary.Add("GameServerSessionKey", o.GameServerSessionKey);
		}
		dictionary.Add("OnlineGameSpeedRatio", o.OnlineGameSpeedRatio.ToString());
		dictionary.Add("ExtraStatDataBases", BaseJsonMarshaler.EncodeArray(o.ExtraStatDataBases, JM_NShared_DefaultExtraStatData.EncodeJsonObject));
		dictionary.Add("PetStatDataBases", BaseJsonMarshaler.EncodeArray(o.PetStatDataBases, JM_NShared_PetStatDataBase.EncodeJsonObject));
		return dictionary;
	}
}
