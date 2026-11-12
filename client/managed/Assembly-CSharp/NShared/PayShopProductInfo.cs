namespace NShared;

[JsonSerializable(Javascript = true)]
public class PayShopProductInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => Index;

	public int Index { get; set; }

	public PayShopCategory Category { get; set; }

	public PayShopSubCategory SubCategory { get; set; }

	public ProductType ProductType { get; set; }

	public string ProductName { get; set; }

	public string ProductDesc { get; set; }

	public string ProductDetailDesc { get; set; }

	public string InAppPurchaseId => string.Empty;

	public string AppleId { get; set; }

	public string GoogleId { get; set; }

	public string OneStoreId { get; set; }

	public string UDPStoreId { get; set; }

	public CurrencyType PriceType { get; set; }

	public long Price { get; set; }

	public string KRW { get; set; }

	public string USD { get; set; }

	public string THB { get; set; }

	public string VND { get; set; }

	public int HeroItemIndex { get; set; }

	public string[] UseItemCode { get; set; }

	public int[] Count { get; set; }

	public int Gem { get; set; }

	public int FirstGem { get; set; }

	public int BonusGem { get; set; }

	public bool IsPayBack { get; set; }

	public int Gold { get; set; }

	public int Mileage { get; set; }

	public int LuaPoint { get; set; }

	public int ShopEventPoint { get; set; }

	public string[] RewardAnyMiscTypes { get; set; }

	public int ShowIndex { get; set; }

	public string CardTitle { get; set; }

	public byte IsNew { get; set; }

	public string WithdrawTextKey { get; set; }

	public string IconPath { get; set; }

	public int CardType { get; set; }

	public int EffectOn { get; set; }

	public byte FirstPurchase { get; set; }

	public int Durational { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public int DailyGem { get; set; }

	public int DurationDay { get; set; }

	public bool ResetMidnight { get; set; }

	public byte PurchasableCount { get; set; }

	public ResetType ResetType { get; set; }

	public bool DailyReset { get; set; }

	public int[] WeeklyReset { get; set; }

	public int[] MonthlyResetDate { get; set; }

	public int PrePurchased { get; set; }

	public int[] StepProductGroup { get; set; }

	public string[] ExceptServer { get; set; }

	public int BuyGroup { get; set; }

	public int[] SelectAttendanceIndices { get; set; }

	public int BuyableDay { get; set; }

	public bool IsPurchaseMarketing { get; set; }

	public bool ShowProbability { get; set; }

	public bool IsNewbie { get; set; }

	public string[] BonusProductInfos { get; set; }

	public string ParentAnyMiscType { get; set; }

	public bool ShowGMTool { get; set; }

	public bool HideSoldOutCardPlate { get; set; }
}
