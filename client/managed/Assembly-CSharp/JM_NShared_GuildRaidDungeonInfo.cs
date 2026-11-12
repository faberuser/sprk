using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidDungeonInfo
{
	public static GuildRaidDungeonInfo Parse(string s)
	{
		return Parse(s, new GuildRaidDungeonInfo());
	}

	public static GuildRaidDungeonInfo Parse(string s, GuildRaidDungeonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidDungeonInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidDungeonInfo());
	}

	public static GuildRaidDungeonInfo Parse(IDictionary d, GuildRaidDungeonInfo o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueInt(d, "GuildId");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.OpenedTime = BaseJsonMarshaler.ParseValueString(d, "OpenedTime");
		o.FinishMode = BaseJsonMarshaler.ParseValueInt(d, "FinishMode");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		o.ClearMemberId = BaseJsonMarshaler.ParseValueLong(d, "ClearMemberId");
		o.MonsterHp0 = BaseJsonMarshaler.ParseValueLong(d, "MonsterHp0");
		o.IsOngoing = BaseJsonMarshaler.ParseValueInt(d, "IsOngoing");
		o.BonusDungeonOpenStep = BaseJsonMarshaler.ParseValueInt(d, "BonusDungeonOpenStep");
		o.BonusChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "BonusChapterIndex");
		o.BonusDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "BonusDungeonIndex");
		o.IsBonusDungeon = BaseJsonMarshaler.ParseValueInt(d, "IsBonusDungeon");
		o.IsDummyDungeon = BaseJsonMarshaler.ParseValueInt(d, "IsDummyDungeon");
		return o;
	}

	public static string EncodeJsonString(GuildRaidDungeonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidDungeonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Step", o.Step.ToString());
		if (o.OpenedTime != null)
		{
			dictionary.Add("OpenedTime", o.OpenedTime);
		}
		dictionary.Add("FinishMode", o.FinishMode.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		dictionary.Add("ClearMemberId", o.ClearMemberId.ToString());
		dictionary.Add("MonsterHp0", o.MonsterHp0.ToString());
		dictionary.Add("IsOngoing", o.IsOngoing.ToString());
		dictionary.Add("BonusDungeonOpenStep", o.BonusDungeonOpenStep.ToString());
		dictionary.Add("BonusChapterIndex", o.BonusChapterIndex.ToString());
		dictionary.Add("BonusDungeonIndex", o.BonusDungeonIndex.ToString());
		dictionary.Add("IsBonusDungeon", o.IsBonusDungeon.ToString());
		dictionary.Add("IsDummyDungeon", o.IsDummyDungeon.ToString());
		return dictionary;
	}
}
