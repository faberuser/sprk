using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseDungeonInfo
{
	public static EclipseDungeonInfo Parse(string s)
	{
		return Parse(s, new EclipseDungeonInfo());
	}

	public static EclipseDungeonInfo Parse(string s, EclipseDungeonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseDungeonInfo Parse(IDictionary d)
	{
		return Parse(d, new EclipseDungeonInfo());
	}

	public static EclipseDungeonInfo Parse(IDictionary d, EclipseDungeonInfo o)
	{
		o.MatchIndex = BaseJsonMarshaler.ParseValueInt(d, "MatchIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		o.Status = BaseJsonMarshaler.ParseEnum<EclipseStatus>(d, "Status");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.StartWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "StartWaveIndex");
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		o.CurrentCreatureLevel = BaseJsonMarshaler.ParseValueInt(d, "CurrentCreatureLevel");
		o.DungeonList = BaseJsonMarshaler.ParseValueString(d, "DungeonList");
		o.GuildSkillData = BaseJsonMarshaler.ParseValueString(d, "GuildSkillData");
		o.AccountBuffData = BaseJsonMarshaler.ParseValueString(d, "AccountBuffData");
		o.ClassBuffData = BaseJsonMarshaler.ParseValueString(d, "ClassBuffData");
		o.ExtraStatDataBases = BaseJsonMarshaler.ParseValueString(d, "ExtraStatDataBases");
		o.PetStatDataBases = BaseJsonMarshaler.ParseValueString(d, "PetStatDataBases");
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		o.ExpireTime = BaseJsonMarshaler.ParseValueString(d, "ExpireTime");
		o.RewardedTime = BaseJsonMarshaler.ParseValueString(d, "RewardedTime");
		o.OnlineGameSpeedRatio = BaseJsonMarshaler.ParseValueInt(d, "OnlineGameSpeedRatio");
		o.EnterTicketCount = BaseJsonMarshaler.ParseValueInt(d, "EnterTicketCount");
		o.EndWaveIndices = BaseJsonMarshaler.ParseValueString(d, "EndWaveIndices");
		return o;
	}

	public static string EncodeJsonString(EclipseDungeonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseDungeonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchIndex", o.MatchIndex.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("WaveIndex", o.WaveIndex.ToString());
		dictionary.Add("Status", o.Status.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("StartWaveIndex", o.StartWaveIndex.ToString());
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		dictionary.Add("CurrentCreatureLevel", o.CurrentCreatureLevel.ToString());
		if (o.DungeonList != null)
		{
			dictionary.Add("DungeonList", o.DungeonList);
		}
		if (o.GuildSkillData != null)
		{
			dictionary.Add("GuildSkillData", o.GuildSkillData);
		}
		if (o.AccountBuffData != null)
		{
			dictionary.Add("AccountBuffData", o.AccountBuffData);
		}
		if (o.ClassBuffData != null)
		{
			dictionary.Add("ClassBuffData", o.ClassBuffData);
		}
		if (o.ExtraStatDataBases != null)
		{
			dictionary.Add("ExtraStatDataBases", o.ExtraStatDataBases);
		}
		if (o.PetStatDataBases != null)
		{
			dictionary.Add("PetStatDataBases", o.PetStatDataBases);
		}
		dictionary.Add("PlayTime", o.PlayTime.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		if (o.ExpireTime != null)
		{
			dictionary.Add("ExpireTime", o.ExpireTime);
		}
		if (o.RewardedTime != null)
		{
			dictionary.Add("RewardedTime", o.RewardedTime);
		}
		dictionary.Add("OnlineGameSpeedRatio", o.OnlineGameSpeedRatio.ToString());
		dictionary.Add("EnterTicketCount", o.EnterTicketCount.ToString());
		if (o.EndWaveIndices != null)
		{
			dictionary.Add("EndWaveIndices", o.EndWaveIndices);
		}
		return dictionary;
	}
}
