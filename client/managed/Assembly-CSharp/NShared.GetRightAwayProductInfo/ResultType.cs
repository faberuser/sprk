namespace NShared.GetRightAwayProductInfo;

public enum ResultType
{
	Fail,
	Success,
	ShopItemDataNotFound,
	[Comment("상품이 매진되었습니다.")]
	SoldOutServerLimitedQuantity
}
