namespace NShared.GetGuildList;

public enum ResultType
{
	Fail,
	Success,
	[Comment("더 이상 다른 길드를 찾을 수 없습니다.")]
	GuildListSearchNotFound
}
