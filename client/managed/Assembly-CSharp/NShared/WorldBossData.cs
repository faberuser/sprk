using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class WorldBossData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public int OpenDow { get; set; }

	[Key(3)]
	public int ReqMaxHeroLevel { get; set; }

	[Key(4)]
	public int DealBattleDuration { get; set; }

	[Key(5)]
	public int NormalBattleDuration { get; set; }

	[Key(6)]
	public int CoolTimeDuration { get; set; }

	[Key(7)]
	public int ChapterIndex { get; set; }

	[Key(8)]
	public int DungeonIndex { get; set; }

	[Key(9)]
	public string Tactic { get; set; }

	[Key(10)]
	public int[] RecommendedHeroIndex { get; set; }

	[Key(11)]
	public string BossInfoImage { get; set; }

	[Key(12)]
	public string PreviewBg { get; set; }

	[Key(13)]
	public int KillRewardIndex { get; set; }

	[Key(14)]
	public int[] ShowInfoSkill { get; set; }

	[Key(15)]
	public bool IsGlobal { get; set; }

	[Key(16)]
	public string PortalImagePath { get; set; }

	[Key(17)]
	public string AdventureNoticeImagePath { get; set; }

	[Key(18)]
	public int NextIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "[Index=" + Index + " Name=" + Name + "]";
	}
}
