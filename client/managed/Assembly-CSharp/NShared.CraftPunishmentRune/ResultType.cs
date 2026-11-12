namespace NShared.CraftPunishmentRune;

public enum ResultType
{
	Fail,
	Success,
	[Comment("Request body 값 유효성 검증 에러.")]
	RequestBodyValidationFail,
	[Comment("유저가 충분한 재료 아이템을 가지지 않았습니다.")]
	PlayerNotHaveEnoughItem,
	[Comment("재료 룬이 충분하지 않습니다.(게이지가 맥스 게이지를 못 넘겼을 때)")]
	NotEnoughRuneProvided,
	[Comment("존재하지 않는 신벌의 룬 인덱스 입니다.")]
	InvalidPunishmentRuneIndex,
	[Comment("재료 룬이 존재하지 않습니다.")]
	IngredientRuneNotExist,
	[Comment("신벌의 룬 제작에 사용할 수 없는 룬 입니다.")]
	InvalidIngredientRune,
	[Comment("더이상 신벌의 룬 제작할 수 없습니다.")]
	CannotCraftAnyMore,
	[Comment("테이블 유효성 검증 에러.")]
	TableValidationError,
	[Comment("차단된 컨텐츠입니다.")]
	ContentsOff
}
