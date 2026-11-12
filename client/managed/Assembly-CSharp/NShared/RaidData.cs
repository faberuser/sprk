using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class RaidData : IDataKeySelector<RaidDataKey2>, IRaidData
{
	RaidDataKey2 IDataKeySelector<RaidDataKey2>.Key => new RaidDataKey2(Index, Level);

	[Key(0)]
	public string Name { get; set; }

	[Key(1)]
	public RaidType Type { get; set; }

	[Key(2)]
	public string Step_Name { get; set; }

	[Key(3)]
	public string Step_HelpDesc { get; set; }

	[Key(4)]
	public int ChapterIndex { get; set; }

	[Key(5)]
	public int DungeonIndex { get; set; }

	[Key(6)]
	public int OccurChapterIndex { get; set; }

	[Key(7)]
	public string PreviewBg { get; set; }

	[Key(8)]
	public int PlayerCount { get; set; }

	[Key(9)]
	public int DeckCount { get; set; }

	[Key(10)]
	public int DeckCountPerPlayer { get; set; }

	[Key(11)]
	public int AliveDuration { get; set; }

	[Key(12)]
	public string NodeTreePath { get; set; }

	[Key(13)]
	public string NodeTreeCode { get; set; }

	[Key(14)]
	public int RaidPoint { get; set; }

	[Key(15)]
	public string Tactic { get; set; }

	[Key(16)]
	public int[] RecommendedHeroIndex { get; set; }

	[Key(17)]
	public int[] ShowInfoSkill { get; set; }

	[Key(18)]
	public int SingleRaidIndex { get; set; }

	[Key(19)]
	public int MainPartyCount { get; set; }

	[Key(21)]
	public int SubPartyCount { get; set; }

	[Key(23)]
	public bool IsOnlineSingle { get; set; }

	[Key(24)]
	public int Index { get; set; }

	[Key(25)]
	public int Level { get; set; }

	[Key(26)]
	public int FilterValue { get; set; }

	[Key(27)]
	public bool IsOpen { get; set; }

	[Key(28)]
	public int[] OpenCondition { get; set; }

	[Key(29)]
	public int ReqHeroLevel { get; set; }

	[Key(30)]
	public int ResetGem { get; set; }

	[Key(31)]
	public int IndividualReward { get; set; }

	[Key(32)]
	public int ClearRaidIndex { get; set; }

	[Key(33)]
	public int BanIndex { get; set; }

	[Key(34)]
	public string RaidDesc { get; set; }

	[Key(35)]
	public int RaidMultiInfoGroupIndex { get; set; }

	[Key(36)]
	public bool HideRaidInfoSkillTab { get; set; }

	[Key(37)]
	public string RaidInfoCustomIdle { get; set; }

	int IRaidData.Difficulty => 0;

	public override string ToString()
	{
		return "[Index=" + Index + " Level=" + Level + "]";
	}

	public bool IsGuildSuppressContents()
	{
		if (Type != RaidType.GuildSuppress)
		{
			return Type == RaidType.GuildSuppressSingle;
		}
		return true;
	}
}
