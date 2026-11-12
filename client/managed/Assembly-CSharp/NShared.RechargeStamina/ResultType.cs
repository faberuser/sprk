namespace NShared.RechargeStamina;

public enum ResultType
{
	Fail,
	Success,
	NotEnoughGem,
	InvalidStaminaType,
	MaxStamina,
	[Comment("충전 제한 횟수를 초과하였습니다.")]
	ResetLimitExceeded
}
