namespace NShared.CheckConsume;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GemResultInfo GemResult { get; set; }

	public GoldResultInfo GoldResult { get; set; }

	public PvpCoinResultInfo PvpCoinResult { get; set; }

	public PlayerPurchaseTimeDurationInfo purchaseTimeDuration { get; set; }

	public PlayerFirstPurchaseInfo firstPurchase { get; set; }

	public PlayerProductPurchaseInfo PlayerProductPurchaseInfo { get; set; }

	public MileageResultInfo MileageResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; set; }

	public PurchaseMarketingResultInfo PurchaseMarketingResult { get; set; }
}
