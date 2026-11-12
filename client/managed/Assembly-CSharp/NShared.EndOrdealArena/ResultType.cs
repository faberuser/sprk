namespace NShared.EndOrdealArena;

public enum ResultType
{
	Fail,
	Success,
	MatchingDisabled,
	ContentsDisabled,
	[Comment("시련의 전장 - 시즌정보를 찾을 수 없습니다.")]
	NotFoundSeasonData,
	[Comment("시련의 전장 - 노드데이터를 찾을 수 없습니다.")]
	NotFoundNodeData,
	[Comment("시련의 전장 - 버프를 선택하지 않았습니다.")]
	NotSelectedBuff,
	[Comment("진행중인 시련의 전장을 찾을 수 없습니다.")]
	NotFoundOrdealNodeInfo,
	[Comment("시련의 전장 - 진행할 수 없는 노드입니다.")]
	NotAvailableNodeIndex,
	[Comment("시련의 전장 - 전투를 할수 있는 노드가 아닙니다.")]
	NotBattleNode,
	DungeonNotFound,
	InvalidHero,
	NotAvailableHero,
	ModulatedData,
	InternalServerError
}
