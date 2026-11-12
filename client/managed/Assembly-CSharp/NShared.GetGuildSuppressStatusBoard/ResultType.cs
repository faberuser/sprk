namespace NShared.GetGuildSuppressStatusBoard;

public enum ResultType
{
	Fail,
	Success,
	[Comment("진행중인 길드 토벌전이 존재하지 않습니다.")]
	GuildSuppressSessionNotFound,
	[Comment("토벌전 준비 중입니다. 토벌전이 시작되면 다시 시도해주세요.")]
	GuildSuppressNotOpen,
	[Comment("길드 토벌전 세션테이블 데이터를 찾을 수 없습니다.")]
	GuildSuppressSessionDataNotFound,
	[Comment("길드 토벌전 매칭 그룹 정보를 찾을 수 없습니다.")]
	NotFoundGuildSuppressGroupInfo,
	[Comment("길드 토벌전 매칭 그룹 인덱스를 찾을 수 없습니다.")]
	NotFoundGuildSuppressGroupIndex
}
