using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PcAvatarData : IDataKeySelector<PcAvatarDataKey>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int Index { get; set; }

	[Key(2)]
	public string ConstructorFactory { get; set; }

	[Key(3)]
	public string[] FactoryValues { get; set; }

	[Key(4)]
	public string[] Condition1 { get; set; }

	[Key(5)]
	public int[] MoveNodes { get; set; }

	[Key(6)]
	public float[] StayTimes { get; set; }

	[Key(7)]
	public float MoveSpeedRate { get; set; }

	[Key(8)]
	public bool RewardOpen { get; set; }

	[Key(9)]
	public int RewardCoolTime { get; set; }

	[Key(10)]
	public string[] TouchActions { get; set; }

	[Key(11)]
	public string[] TouchText { get; set; }

	[IgnoreMember]
	public PcAvatarDataKey Key => new PcAvatarDataKey(ChapterIndex, Index);
}
