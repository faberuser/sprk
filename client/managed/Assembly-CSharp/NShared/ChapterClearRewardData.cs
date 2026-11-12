using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class ChapterClearRewardData : IDataKeySelector<ChapterClearRewardKey>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int StepIndex { get; set; }

	[Key(2)]
	public RewardType RewardType { get; set; }

	[Key(3)]
	public int RewardCount { get; set; }

	[Key(4)]
	public string RewardItemCode { get; set; }

	[Key(5)]
	public int RewardReqStar { get; set; }

	ChapterClearRewardKey IDataKeySelector<ChapterClearRewardKey>.Key => new ChapterClearRewardKey(ChapterIndex, StepIndex);
}
