namespace NShared.RefreshNode;

public enum ResultType
{
	Fail,
	Success,
	NotEnoughFreeRefresh,
	InvalidCost,
	NotEnoughGem,
	NotFoundSeasonData,
	MatchingDisabled,
	[Comment("시련의 전장 - 전투를 할수 있는 노드가 아닙니다.")]
	NotBattleNode,
	[Comment("시련의 전장 정보를 찾을 수 없습니다.")]
	NotFoundArenaInfo,
	InternalServerError,
	ContentsDisabled
}
