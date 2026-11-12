namespace NShared.GetSelectShopInfo;

public enum ResultType
{
	Fail,
	Success,
	[Comment("판매중인 선택상점이 없습니다.")]
	NotActiveSelectShop,
	[Comment("선택상점의 데이터가 존재하지 않습니다.")]
	SelectShopDataNotFound,
	[Comment("선택상점의 상품이 존재하지 않습니다.")]
	NotFoundGachaSelectCategoryIndex,
	[Comment("선택상점의 상품이 존재하지 않습니다.")]
	NotFoundSelectShopCategory
}
