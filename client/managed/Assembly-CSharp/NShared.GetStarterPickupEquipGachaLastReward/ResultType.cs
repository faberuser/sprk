namespace NShared.GetStarterPickupEquipGachaLastReward;

public enum ResultType
{
	Fail,
	Success,
	[Comment("이미 받았습니다.")]
	AlreadyTakenReward,
	[Comment("아직 받을수 없습니다.")]
	NotTakableReward,
	[Comment("잘못된 타입 입니다.")]
	InvalidEquipGachaType
}
