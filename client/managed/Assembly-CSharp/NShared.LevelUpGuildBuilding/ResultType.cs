namespace NShared.LevelUpGuildBuilding;

public enum ResultType
{
	Fail,
	Success,
	[Comment("관리자 등급이 아닙니다.")]
	InvalidRank,
	[Comment("길드 레벨이 부족합니다.")]
	InvalidGuildLevel,
	[Comment("잘못된 건물 레벨을 요청하였습니다.")]
	InvalidBuildingLevel,
	[Comment("최대 건물 레벨이어서 건물 레벨업을 할 수 없습니다.")]
	MaxGuildBuildingLevel,
	[Comment("목재가 부족합니다.")]
	NotEnoughWood,
	[Comment("석재가 부족합니다.")]
	NotEnoughStone,
	[Comment("금속이 부족합니다.")]
	NotEnoughMetal
}
