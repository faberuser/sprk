namespace NShared.CreateRaidRoom;

public enum ResultType
{
	Fail,
	Success,
	InvalidRoomInfo,
	AlreadyJoined,
	NoRaidData,
	[Comment("유효한 패널티/어드벤티지 값이 아닙니다.")]
	NotValidAffixIndex,
	[Comment("허용된 포인트 값을 벗어났습니다.")]
	NotAllowedPoint
}
