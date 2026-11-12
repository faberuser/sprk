namespace NShared;

[JsonSerializable]
public enum EShopSortType
{
	None,
	TimeLowToHigh,
	TimeHighToLow,
	PriceLowToHigh,
	PriceHighToLow,
	GradeLowToHigh,
	GradeHighToLow
}
