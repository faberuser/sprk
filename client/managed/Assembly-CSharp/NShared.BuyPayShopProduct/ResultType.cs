namespace NShared.BuyPayShopProduct;

public enum ResultType
{
	Fail,
	Success,
	ShopItemDataNotFound,
	ItemDataNotFound,
	NotEnoughGem,
	NotEnoughGold,
	AlreadyPurchased,
	NotEnoughMileage,
	ExceededPurchasableCount,
	InvalidPayShopType,
	InvalidShopItemData,
	AlreadyHasHero,
	NotDailyEnded,
	NotEnoughLuaPoint,
	[Comment("신규 유저 혜택 팝업이 있지 않아 구매가 불가능합니다.")]
	NotOpenedNewbiePopup,
	[Comment("신규 유저 혜택 팝업 오픈시간이 종료되어 구매가 불가능합니다.")]
	NewbiePopupTimeEnded,
	[Comment("블랙 프라이 데이 포인트 부족.")]
	NotEnoughShopEventPoint,
	[Comment("점핑 토근샵 포인트 부족.")]
	NotEnoughLimitedShopEventPoint,
	[Comment("선택상점의 데이터가 존재하지 않습니다.")]
	SelectShopDataNotFound,
	[Comment("선택상점의 상품이 존재하지 않습니다.")]
	NotFoundGachaSelectCategoryIndex,
	[Comment("선택상점의 상품이 존재하지 않습니다.")]
	NotFoundSelectShopCategory,
	[Comment("진행중인 킹스패스 시즌이 존재하지 않습니다.")]
	NoCurrentKingsPassSeason,
	[Comment("킹스패스 시즌 게임데이터를 찾을 수 없습니다.")]
	NotFoundKingsPassSeasonData,
	[Comment("현재 킹스패스 시즌 상품과 일치하지 않습니다.")]
	NotMatchSeasonProductIndex,
	[Comment("구매유도 팝업 상품을 찾을 수 없습니다.")]
	NotFoundPurchaseMarketing,
	[Comment("구매유도 팝업 오픈시간이 종료되어 구매가 불가능합니다.")]
	PurchaseMarketingTimeEnded,
	[Comment("등록된 이벤트가 동작하지 않았습니다.")]
	NotExcutePostEvent,
	[Comment("구매 불가")]
	ShopNotBuyable,
	PtsUnSupported
}
