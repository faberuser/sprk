using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StepUpGachaRewardData : IDataKeySelector<StepupGachaRewardDataKey>
{
	StepupGachaRewardDataKey IDataKeySelector<StepupGachaRewardDataKey>.Key => new StepupGachaRewardDataKey(Index, Step);

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Step { get; set; }

	[Key(2)]
	public int Round { get; set; }

	[Key(3)]
	public int NextStep { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	[Key(5)]
	public int Discount { get; set; }

	[Key(6)]
	public string Buff { get; set; }

	public bool IsEmptyReward()
	{
		if (RewardIndex != 0)
		{
			return false;
		}
		if (Discount != 0)
		{
			return false;
		}
		return true;
	}

	public EStepUpGachaRewardType GetMainRewardType()
	{
		if (Discount != 0)
		{
			return EStepUpGachaRewardType.Discount;
		}
		if (RewardIndex != 0)
		{
			return EStepUpGachaRewardType.RewardItem;
		}
		if (!string.IsNullOrEmpty(Buff))
		{
			return EStepUpGachaRewardType.Buff;
		}
		return EStepUpGachaRewardType.Empty;
	}

	public EStepUpGachaRewardType GetAllRewardType()
	{
		EStepUpGachaRewardType eStepUpGachaRewardType = EStepUpGachaRewardType.None;
		if (Discount != 0)
		{
			eStepUpGachaRewardType |= EStepUpGachaRewardType.Discount;
		}
		if (RewardIndex != 0)
		{
			eStepUpGachaRewardType |= EStepUpGachaRewardType.RewardItem;
		}
		if (!string.IsNullOrEmpty(Buff))
		{
			eStepUpGachaRewardType |= EStepUpGachaRewardType.Buff;
		}
		if (eStepUpGachaRewardType == EStepUpGachaRewardType.None)
		{
			eStepUpGachaRewardType = EStepUpGachaRewardType.Empty;
		}
		return eStepUpGachaRewardType;
	}
}
