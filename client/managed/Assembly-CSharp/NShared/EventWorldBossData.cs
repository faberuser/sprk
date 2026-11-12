using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EventWorldBossData : IDataKeySelector<EventWorldBossDataKey>, IRaidData
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int BossLevel { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public bool IsOpen { get; set; }

	[Key(4)]
	public int NextOpenMinute { get; set; }

	[Key(5)]
	public int ChapterIndex { get; set; }

	[Key(6)]
	public int DungeonIndex { get; set; }

	[Key(7)]
	public int MaxHp { get; set; }

	[Key(8)]
	public int ParticipantRewardIndex { get; set; }

	[Key(9)]
	public int KillRewardIndex { get; set; }

	[Key(10)]
	public int EndRewardIndex { get; set; }

	[Key(11)]
	public string PreviewBg { get; set; }

	[Key(12)]
	public string Tactic { get; set; }

	[Key(13)]
	public int[] RecommendedHeroIndex { get; set; }

	[Key(14)]
	public string BossInfoImage { get; set; }

	[Key(15)]
	public int[] ShowInfoSkill { get; set; }

	[Key(16)]
	public string[] EventBossDesc { get; set; }

	[Key(17)]
	public string ButtonHighlightResource { get; set; }

	int IRaidData.Difficulty => 0;

	int IRaidData.RaidMultiInfoGroupIndex => 0;

	EventWorldBossDataKey IDataKeySelector<EventWorldBossDataKey>.Key => new EventWorldBossDataKey(Index, BossLevel);

	public override string ToString()
	{
		return "[Index=" + Index + " BossLevel=" + BossLevel + "]";
	}
}
