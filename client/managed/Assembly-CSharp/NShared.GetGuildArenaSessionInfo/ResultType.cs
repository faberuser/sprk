namespace NShared.GetGuildArenaSessionInfo;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	[Comment("길드전이 진행중이 아닙니다.")]
	GuildArenaSessionNotFound,
	[Comment("가입된 길드가 없습니다.")]
	NoGuild,
	[Comment("길드 회원정보를 찾을 수 없습니다.")]
	GuildMemberNotFound,
	[Comment("길드전 기록을 찾을수 없습니다.")]
	GuildArenaScoreInfoNotFound,
	[Comment("길드전 자동신청 기록을 찾을수 없습니다.")]
	GuildArenaAutoApplyInfoNotFound
}
