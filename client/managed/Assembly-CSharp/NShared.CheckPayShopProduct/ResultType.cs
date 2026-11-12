namespace NShared.CheckPayShopProduct;

public enum ResultType
{
	Fail,
	Success,
	[Comment("요청한 Platform 정보가 없습니다.")]
	NotCorrectPlatform,
	NotSameProductId
}
