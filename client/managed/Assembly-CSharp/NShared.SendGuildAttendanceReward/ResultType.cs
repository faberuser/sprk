namespace NShared.SendGuildAttendanceReward;

public enum ResultType
{
	Fail,
	Success,
	NoGuild,
	InvalidValue,
	InfoError,
	ActionNotReady,
	InvalidActionValue,
	[Comment("초기화 중 입니다.")]
	Initializing,
	[Comment("초기화 실패 입니다.")]
	InitializingFail
}
