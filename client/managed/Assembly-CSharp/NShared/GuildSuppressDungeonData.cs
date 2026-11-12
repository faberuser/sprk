using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildSuppressDungeonData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int DefaultRewardIndex { get; set; }

	[Key(2)]
	public int KillRewardIndex { get; set; }

	[Key(3)]
	public int KillActivityPoint { get; set; }

	[Key(4)]
	public int BuyableRewardIndex { get; set; }

	[Key(5)]
	public string Tactic { get; set; }

	[Key(6)]
	public int[] RecommendedHeroIndex { get; set; }

	[Key(7)]
	public int BanIndex { get; set; }

	[Key(8)]
	public string PortraitPath { get; set; }

	[Key(9)]
	public string PreviewBg { get; set; }

	[Key(10)]
	public long FinishModeRatio { get; set; }

	[Key(11)]
	public int[] ShowInfoSkill { get; set; }

	[Key(12)]
	public string IconPath { get; set; }

	[Key(13)]
	public string StatusBoardPortraitPath { get; set; }

	public override string ToString()
	{
		return "GuildSuppressChapterIndex=" + Index;
	}
}
