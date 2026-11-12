namespace NShared.BeginOrdealArena;

public enum ResultType
{
	Fail,
	Success,
	[Comment("시련의 전장 - 시즌정보를 찾을 수 없습니다.")]
	NotFoundSeasonData,
	[Comment("시련의 전장 - 노드데이터를 찾을 수 없습니다.")]
	NotFoundNodeData,
	[Comment("시련의 전장 - 오픈되지 않은 시즌입니다.")]
	MatchingDisabled,
	[Comment("시련의 전장 - 보유하지 않은 영웅이 포함 되어 있습니다.")]
	HeroNotFound,
	[Comment("선택할 수 있는 버프를 찾을 수 없습니다.")]
	NotFoundSelectBuff,
	[Comment("시련의 전장 - 진행할 수 없는 노드입니다.")]
	NotAvailableNodeIndex,
	InvalidHero,
	NotAvailableHero,
	InternalServerError,
	ContentsDisabled
}
