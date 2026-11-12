using System;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.NCostume;

public class AccessoryCostumeRequester : CostumeRequesterBase, AccessoryCostumeViewWidget.IListener
{
	public AccessoryCostumeRequester(WebServiceRequest webRequest)
		: base(webRequest)
	{
	}

	void AccessoryCostumeViewWidget.IListener.OnClickBuyCostume(HeroCustomizingDataGetter getter, Action onComplete)
	{
		ApplyCustomizingCostumes(getter, onComplete);
	}

	void AccessoryCostumeViewWidget.IListener.OnClickBuyHero(CreatureData creatureData)
	{
		GetCostumeManagementInstance<HeroCostumeView>().CloseAndDoAction(delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.BuyHeroShopItem(creatureData);
		});
	}

	void AccessoryCostumeViewWidget.IListener.OnClickEquipCostume(HeroCustomizingDataGetter getter)
	{
		ApplyCustomizingCostumes(getter, delegate
		{
		});
	}

	void AccessoryCostumeViewWidget.IListener.OnClickUnequipCostume(HeroCustomizingDataGetter getter)
	{
		ApplyCustomizingCostumes(getter, delegate
		{
		});
	}
}
