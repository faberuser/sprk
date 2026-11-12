using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NPCFriendlyPointData : IDataKeySelector<NPCFriendlyPointDataKey>
{
	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public int Step { get; set; }

	[Key(2)]
	public string EffectCode { get; set; }

	[Key(3)]
	public int ReqFriendlyPoint { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	[Key(5)]
	public string RewardDesc { get; set; }

	[Key(6)]
	public string[] RestrictionItemCode { get; set; }

	[Key(7)]
	public string RestrictionLocalKey { get; set; }

	[Key(8)]
	public bool LimitFriendlyPoint { get; set; }

	[Key(9)]
	public string[] UIColor { get; set; }

	[Key(10)]
	public int EndRewardRepeatCount { get; set; }

	[Key(11)]
	public string StampName { get; set; }

	NPCFriendlyPointDataKey IDataKeySelector<NPCFriendlyPointDataKey>.Key => new NPCFriendlyPointDataKey(HeroIndex, Step);

	public override string ToString()
	{
		return "NPCFriendlyPointData HeroIndex=" + HeroIndex + " Step=" + Step;
	}
}
