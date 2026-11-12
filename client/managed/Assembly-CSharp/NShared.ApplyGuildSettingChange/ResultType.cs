namespace NShared.ApplyGuildSettingChange;

public enum ResultType
{
	Success,
	Fail,
	InvalidRank,
	InvalidGuildIntroduce,
	TooLongGuildIntroduce,
	[Comment("길드 설명에 비속어가 포함되어 있습니다.")]
	CensoredGuildIntroduce,
	InvalidGuildNotice,
	TooLongGuildNotice,
	[Comment("길드 공지사항에 비속어가 포함되어 있습니다.")]
	CensoredGuildNotice,
	GuildJoinRequestExist,
	[Comment("다른 언어를 사용할 수 없습니다.")]
	InvalidNoticeInThisServer,
	[Comment("국가코드를 변경할 수 없는 기간입니다.")]
	CountryCodeChangeIsNotPossible
}
