using System;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.NCostume;

public class WeaponCostumeRequester : CostumeRequesterBase, WeaponCostumeViewWidget.IListener
{
	public WeaponCostumeRequester(WebServiceRequest webRequest)
		: base(webRequest)
	{
	}

	void WeaponCostumeViewWidget.IListener.OnClickBuyCostume(HeroCustomizingDataGetter getter)
	{
		VespaDebug.LogError("OnClickBuyCostume");
		ApplyCustomizingCostumes(getter, delegate
		{
		});
	}

	void WeaponCostumeViewWidget.IListener.OnClickBuyHero(CreatureData creatureData)
	{
		GetCostumeManagementInstance<HeroCostumeView>().CloseAndDoAction(delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.BuyHeroShopItem(creatureData);
		});
	}

	void WeaponCostumeViewWidget.IListener.OnClickEquipCostume(HeroCustomizingDataGetter getter)
	{
		ApplyCustomizingCostumes(getter, delegate
		{
		});
	}

	void WeaponCostumeViewWidget.IListener.OnClickUnequipCostume(HeroCustomizingDataGetter getter)
	{
		ApplyCustomizingCostumes(getter, delegate
		{
		});
	}

	void WeaponCostumeViewWidget.IListener.RequestBuyHeroCostume(CostumeData costumeData, Action<CostumeData> OnComplete)
	{
		RequestBuyCostume(costumeData, OnComplete);
	}

	void WeaponCostumeViewWidget.IListener.OnClickSubStory(int heroIndex)
	{
		MoveToSubStory(heroIndex);
	}
}
