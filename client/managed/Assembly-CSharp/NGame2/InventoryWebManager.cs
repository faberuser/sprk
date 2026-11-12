using System;
using NGame2.NUI.NWindow;
using NShared;
using NShared.UsePotionItem;
using NVespa;
using NVespa.NGlobal;

namespace NGame2;

public class InventoryWebManager : IWebManager<InventoryWebManager>
{
	InventoryWebManager IWebManager<InventoryWebManager>.Manager => this;

	public void RequestUsePotionItem(ItemData itemData, Action responseComplete)
	{
		NShared.UsePotionItem.Request request = new NShared.UsePotionItem.Request
		{
			ItemIndex = itemData.Index,
			ItemCount = 1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePotionItem.Response response)
		{
			if (response == null)
			{
				if (responseComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseComplete);
				}
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				if (responseComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseComplete);
				}
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				if (responseComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseComplete);
				}
			}
			else if (responseComplete != null)
			{
				Vespa_System_Action_Extension_Method.fire(responseComplete);
			}
		});
	}
}
