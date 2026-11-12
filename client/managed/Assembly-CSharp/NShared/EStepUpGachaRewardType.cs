using System;

namespace NShared;

[Flags]
public enum EStepUpGachaRewardType
{
	None = 0,
	Empty = 1,
	Discount = 2,
	RewardItem = 4,
	Buff = 8
}
