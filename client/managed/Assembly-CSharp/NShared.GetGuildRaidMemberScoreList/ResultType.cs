namespace NShared.GetGuildRaidMemberScoreList;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드아이디를 찾을수 없습니다.")]
	GuildIdNotFound
}
