using System;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.NCostume;

public class HairCostumeRequester : CostumeRequesterBase, HairCostumeViewWidget.IListener
{
	public HairCostumeRequester(WebServiceRequest webRequest)
		: base(webRequest)
	{
	}

	void HairCostumeViewWidget.IListener.OnClickBuyCostume(HeroCustomizingDataGetter getter)
	{
		VespaDebug.LogError("OnClickBuyCostume");
		ApplyCustomizingCostumes(getter, delegate
		{
		});
	}

	void HairCostumeViewWidget.IListener.OnClickBuyHero(CreatureData creatureData)
	{
		GetCostumeManagementInstance<HeroCostumeView>().CloseAndDoAction(delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.BuyHeroShopItem(creatureData);
		});
	}

	void HairCostumeViewWidget.IListener.OnClickEquipCostume(HeroCustomizingDataGetter getter)
	{
		ApplyCustomizingCostumes(getter, delegate
		{
		});
	}

	void HairCostumeViewWidget.IListener.OnClickUnequipCostume(HeroCustomizingDataGetter getter)
	{
		ApplyCustomizingCostumes(getter, delegate
		{
		});
	}

	void HairCostumeViewWidget.IListener.RequestBuyHeroCostume(CostumeData costumeData, Action<CostumeData> OnComplete)
	{
		RequestBuyCostume(costumeData, OnComplete);
	}
}
