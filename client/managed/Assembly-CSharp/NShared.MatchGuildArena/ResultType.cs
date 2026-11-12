namespace NShared.MatchGuildArena;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드전이 진행중이 아닙니다.")]
	GuildArenaSessionNotFound,
	[Comment("진행중인 세션이 아닙니다.")]
	GuildArenaWrongSessionIndex,
	[Comment("길드전 상태가 전투중이 아닙니다.")]
	GuildArenaNotBattleState,
	[Comment("영웅을 보유하고 있지 않습니다.")]
	HeroNotExist,
	[Comment("길드전 입장티켓을 보유하고 있지 않습니다.")]
	NotEnoughGuildArenaKey,
	[Comment("가입된 길드가 없습니다.")]
	NoGuild,
	[Comment("가입된 길드가 존재하지 않습니다.")]
	GuildIdNotFound,
	[Comment("이미 전투에 참여한 영웅입니다.")]
	AlreadyUsedBattleHero,
	[Comment("전투중인 방어덱에 전투를 할 수 없습니다.")]
	CannotPlayInBattle,
	[Comment("길드전 세션참여 점수를 찾을 수 없습니다.")]
	GuildArenaSessionScoreNotFound,
	[Comment("매칭된 적을 찾을 수 없습니다.")]
	GuildArenaEnemyNotFound,
	[Comment("공격할 적이 매칭된 적이 아닙니다.")]
	GuildArenaEnemyNotCorrect,
	[Comment("해당 방어덱은 이미 클리어가 되었습니다.")]
	GuildArenaEnemyDeckAlreadyCompleted,
	[Comment("상대 방어덱 정보를 찾을 수 없습니다")]
	GuildArenaEnemyDeckNotFound,
	[Comment("상대 방어덱 정보를 찾을 수 없습니다")]
	GuildArenaMatchInfoNotFound
}
