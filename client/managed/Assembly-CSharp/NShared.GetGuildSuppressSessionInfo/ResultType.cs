namespace NShared.GetGuildSuppressSessionInfo;

public enum ResultType
{
	Fail,
	Success,
	[Comment("토벌전이 진행중이 아닙니다.")]
	SessionNotFound,
	[Comment("가입된 길드가 없습니다.")]
	NoGuild,
	[Comment("토벌전 플레이 기록을 찾을수 없습니다.")]
	GuildSuppressPlayInfoNotFound,
	[Comment("토벌전 자동신청 기록을 찾을수 없습니다.")]
	GuildSuppressAutoApplyInfoNotFound
}
