namespace NShared.AutoApplyGuildArena;

public enum ResultType
{
	Fail,
	Success,
	[Comment("가입된 길드가 존재하지 않습니다.")]
	GuildIdNotFound,
	[Comment("길드 회원정보를 찾을 수 없습니다.")]
	GuildMemberNotFound,
	[Comment("길드 마스터가 아닙니다.")]
	InvalidRank,
	[Comment("길드정보가 존재하지 않습니다.")]
	GuildInfoNotFound
}
