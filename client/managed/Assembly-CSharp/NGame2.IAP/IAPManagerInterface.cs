using System;
using System.Collections.Generic;
using NShared;

namespace NGame2.IAP;

public interface IAPManagerInterface
{
	EStatus Status { get; }

	void Init(Action<EResultInit, string> callback);

	void Disable();

	void QueryInventory(EQueryInventoryType queryType, IList<string> productIdList, Action<EResultQueryInventory, string> callback);

	void RequestPurchase(bool silent, string productId, int productIndex, Action<EResultPurchase, PurchaseApplyData, string> callback);

	SkuInfo GetSkuInfo(string productId, NewPayShopProductInfo productInfo);
}
