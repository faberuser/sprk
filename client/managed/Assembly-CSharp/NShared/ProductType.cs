namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ProductType
{
	None,
	DailyGem,
	Gem,
	Gold,
	Item,
	Package,
	Booster,
	Attendance,
	Activation,
	GoogleGiftCardPromotion,
	GooglePlayPointReward,
	DailyItem,
	KingsPass
}
