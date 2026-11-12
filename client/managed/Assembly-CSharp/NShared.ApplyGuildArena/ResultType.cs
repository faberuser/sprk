namespace NShared.ApplyGuildArena;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드전이 진행중이 아닙니다.")]
	GuildArenaSessionNotFound,
	[Comment("진행중이지 않은 길드전입니다.")]
	InvalidGuildArenaSessionIndex,
	[Comment("참가할 수 없습니다.")]
	NotGuildArenaAppliable,
	[Comment("길드전 데이터가 존재하지 않습니다.")]
	GuildArenaSessionDataNotFound,
	[Comment("이미 참가했습니다.")]
	AlreadyAppliedGuildArena,
	[Comment("가입된 길드가 존재하지 않습니다.")]
	GuildIdNotFound,
	[Comment("길드 회원정보를 찾을 수 없습니다.")]
	GuildMemberNotFound,
	[Comment("길드 마스터 혹은 관리자가 아닙니다.")]
	InvalidRank
}
