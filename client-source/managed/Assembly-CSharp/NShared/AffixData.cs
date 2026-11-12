using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AffixData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public EAffixType AffixType { get; set; }

	[Key(3)]
	public TeamType TeamType { get; set; }

	[Key(4)]
	public int[] HeroIndices { get; set; }

	[Key(5)]
	public OperationType OptionType { get; set; }

	[Key(6)]
	public int OptionValue { get; set; }

	[Key(7)]
	public int Point { get; set; }

	[Key(8)]
	public bool IsPenalty { get; set; }

	[Key(9)]
	public string DescKey { get; set; }

	[Key(10)]
	public int BanRuleIndex { get; set; }

	[Key(11)]
	public EAffixShowType ShowType { get; set; }

	[Key(13)]
	public string[] AutoApplyConditionValues { get; set; }

	[Key(14)]
	public int InfinityIndex { get; set; }

	[Key(15)]
	public int InfinityStartLevel { get; set; }

	[Key(16)]
	public string IconPath { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int GetInfinityStep(int infinityLevel)
	{
		if (InfinityStartLevel == 0)
		{
			return infinityLevel;
		}
		return infinityLevel - InfinityStartLevel + 1;
	}
}
