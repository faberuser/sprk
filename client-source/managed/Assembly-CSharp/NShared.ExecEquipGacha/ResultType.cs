namespace NShared.ExecEquipGacha;

public enum ResultType
{
	Fail,
	Success,
	GachaDataNotFound,
	[Comment("해당 장비소환이 닫혀있습니다.")]
	ClosedEquipGacha,
	[Comment("장비소환을 할수 없습니다.")]
	CannotEquipGacha,
	NotEnoughGold,
	NotEnoughGem,
	InvalidItemCode,
	InvalidEquipStarRate,
	InvalidGachaRequest,
	EquipItemFull,
	InvalidItemIndex,
	InvalidItemType,
	ItemNotOwned,
	InvalidGachaData,
	ContentsDisabled,
	CannotDiscountable,
	NotMatchedDiscountCost,
	AleadyDicounted,
	[Comment("소환 아이템 인덱스 정보가 비어있습니다.")]
	TicketItemIndexEmpty,
	[Comment("할인정보가 존재하지 않거나 진행중이 아닙니다.")]
	IsNoDiscount,
	[Comment("진행중인 소환 정보를 찾을 수 없습니다.")]
	EquipGachaInfoNotFound,
	[Comment("스탭업 소환의 보상 데이터를 찾을 수 없습니다.")]
	StepUpGachaRewardDataNotFound,
	[Comment("소환권 게임 데이터를 찾을 수 없습니다.")]
	GachaSelectDataNotFound,
	[Comment("해당 소환권에 허용되지 않은 소환타입 입니다.")]
	GacahTypeNotAllowed,
	[Comment("해당 소환권에 허용되지 않은 아이템 입니다.")]
	GacahIndexNotAllowed,
	StartPickupLimitOver,
	ShopItemDataNotFound,
	NotDailyEnded,
	ExceededPurchasableCount
}
