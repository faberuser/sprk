namespace NShared.ApplyGuildSuppress;

public enum ResultType
{
	Fail,
	Success,
	[Comment("토벌전이 진행중이 아닙니다.")]
	SessionNotFound,
	[Comment("진행중이지 않은 토벌전입니다.")]
	InvalidSessionIndex,
	[Comment("참가할 수 없습니다.")]
	NotAppliable,
	[Comment("토벌전 데이터가 존재하지 않습니다.")]
	SessionDataNotFound,
	[Comment("이미 참가했습니다.")]
	AlreadyApplied,
	[Comment("가입된 길드가 존재하지 않습니다.")]
	GuildIdNotFound,
	[Comment("길드 마스터 혹은 관리자가 아닙니다.")]
	InvalidRank
}
