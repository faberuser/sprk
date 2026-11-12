using NGame2.NAccount;
using NShared;
using NShared.CompletePurchase;

namespace NGame2.IAP;

public class PurchaseApplyData
{
	private ApplyToAccountWrapper _applyWrapper = new ApplyToAccountWrapper();

	public string ProductName { get; private set; }

	public string OrderId { get; private set; }

	public string ProductId { get; private set; }

	public double Price { get; private set; }

	public string Currency { get; private set; }

	public CurrencyResultInfo3[] CurrencyResult => _applyWrapper.CurrencyResults;

	public ItemResultInfo[] ItemResults => _applyWrapper.ItemResults;

	public EquipItemInfo[] EquipItemInfos => _applyWrapper.EquipItemInfos;

	public HeroAddResultInfo[] HeroAddResults => _applyWrapper.HeroAddResults;

	public AttendanceInfo Attendance => _applyWrapper.AttendanceInfo;

	public AttendanceData[] AttendanceDatas => _applyWrapper.AttendanceDatas;

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo => _applyWrapper.ItemTimeDurationInfo;

	public PurchaseMarketingResultInfo PurchaseMarketingResult => _applyWrapper.PurchaseMarketingResult;

	public PlayerProductPurchaseInfo PlayerProductPurchaseInfo => _applyWrapper.PlayerProductPurchaseInfo;

	public NewbieMissionInfo[] NewbieMissionInfos => _applyWrapper.NewbieMissionInfos;

	public PurchaseApplyData(NShared.CompletePurchase.Request request, NShared.CompletePurchase.Response response)
	{
		OrderId = request.OrderId;
		ProductId = request.ProductId;
		Currency = request.CurrencyCode;
		float result = 0f;
		float.TryParse(request.Price, out result);
		Price = result;
		_applyWrapper.Set(response.CurrencyResults, response.ItemResults, response.EquipItemInfos, response.HeroAddResults, response.Attendance, response.AttendanceDatas, response.ItemTimeDurationInfo, response.PurchaseMarketingResult, response.PlayerProductPurchaseInfo, response.NewbieMissionInfos, response.PlayerAnyMiscs, response.OpendMissionCategories);
	}

	public PurchaseApplyData(string productName, string orderId, string productId, string currency, NShared.CompletePurchase.Response response)
	{
		ProductName = productName;
		OrderId = orderId;
		ProductId = productId;
		Currency = currency;
		_applyWrapper.Set(response.CurrencyResults, response.ItemResults, response.EquipItemInfos, response.HeroAddResults, response.Attendance, response.AttendanceDatas, response.ItemTimeDurationInfo, response.PurchaseMarketingResult, response.PlayerProductPurchaseInfo, response.NewbieMissionInfos, response.PlayerAnyMiscs, response.OpendMissionCategories);
	}

	public void ApplyToAccount()
	{
		_ = (object)null;
		_ = OrderId;
		_ = ProductId;
		_ = ProductName;
		_ = Price;
		_ = 1;
		_ = Currency;
		_ = string.Empty;
		if (AccountManager.IsLogined())
		{
			_applyWrapper.Apply();
		}
	}

	public void ExecuteAction()
	{
		if (AccountManager.IsLogined())
		{
			_applyWrapper.DoActionAfterApply();
		}
	}
}
