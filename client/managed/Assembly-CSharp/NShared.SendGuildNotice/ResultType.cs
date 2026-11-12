namespace NShared.SendGuildNotice;

public enum ResultType
{
	Success,
	Fail,
	[Comment("권한 등급이 아닙니다.")]
	InvalidRank
}
