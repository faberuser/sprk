using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PhaseStepRewardData : IDataKeySelector<PhaseStepRewardDataKey>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public PhaseStepRewardType Type { get; set; }

	[Key(3)]
	public int Step { get; set; }

	[Key(4)]
	public PhaseStepReqType ReqType { get; set; }

	[Key(5)]
	public int ReqValue { get; set; }

	[Key(6)]
	public int RewardIndex { get; set; }

	PhaseStepRewardDataKey IDataKeySelector<PhaseStepRewardDataKey>.Key => new PhaseStepRewardDataKey(ChapterIndex, DungeonIndex, Type, Step);
}
