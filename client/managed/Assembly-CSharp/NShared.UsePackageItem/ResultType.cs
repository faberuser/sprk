namespace NShared.UsePackageItem;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	InvalidItemCount,
	ItemDataNotFound,
	ItemTypeMismatch,
	PackageDataNotFound,
	[Comment("보유중인 장비가 너무 많아 아이템을 사용 할 수 없습니다.")]
	EquipItemFull
}
