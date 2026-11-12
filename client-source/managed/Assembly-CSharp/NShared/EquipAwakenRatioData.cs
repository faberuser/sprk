using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipAwakenRatioData : IDataKeySelector<EquipAwakenRatioDataKey>
{
	[Key(0)]
	public int Star { get; set; }

	[Key(1)]
	public int AwakenPoint { get; set; }

	[Key(2)]
	public int SuccessRatio { get; set; }

	[Key(3)]
	public int FailBonusRatio { get; set; }

	EquipAwakenRatioDataKey IDataKeySelector<EquipAwakenRatioDataKey>.Key => new EquipAwakenRatioDataKey(Star, AwakenPoint);

	public override string ToString()
	{
		return "Star=" + Star + " AwakenPoint=" + AwakenPoint + " SuccessRatio=" + SuccessRatio + " FailBonusRatio=" + FailBonusRatio;
	}
}
