namespace NShared.StartDispatch;

public enum ResultType
{
	Fail,
	Success,
	[Comment("배틀서버를 찾을 수 없습니다.")]
	BattleServerNotFound,
	[Comment("던전정보를 찾을 수 없습니다.")]
	DungeonNotFound,
	[Comment("더이상 파견을 보낼수 없습니다.")]
	AlreadyMaxBattleCount,
	[Comment("영웅수가 부족합니다.")]
	NotEnoughHeroCount,
	[Comment("보유 영웅이 아닙니다.")]
	NotOwnHero,
	[Comment("이미 파견중인 영웅입니다.")]
	AlreadyOnBattleHero,
	[Comment("재화가 충분하지 않습니다.")]
	NotEnoughCurrency
}
