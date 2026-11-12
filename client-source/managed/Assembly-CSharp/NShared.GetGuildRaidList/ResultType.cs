namespace NShared.GetGuildRaidList;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드아이디를 찾을수 없습니다.")]
	GuildIdNotFound,
	[Comment("길드레이드 초기화가 진행중입니다. 잠시후 다시 요청해주세요.")]
	GuildRaidInitializing
}
