namespace NShared.GetGuildArenaRankerRecord;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드전이 진행중이 아닙니다.")]
	GuildArenaSessionNotFound,
	[Comment("가입된 길드가 없습니다.")]
	NoGuild,
	[Comment("길드전 랭킹기록을 찾을수 없습니다.")]
	GuildArenaSessionRecordInfoNotFound
}
