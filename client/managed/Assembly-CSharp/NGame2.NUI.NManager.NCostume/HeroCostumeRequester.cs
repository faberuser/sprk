using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NCustomizing;
using NShared;
using NShared.SetCostume;
using NShared.UnsetCostume;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.NCostume;

public class HeroCostumeRequester : CostumeRequesterBase, HeroCostumeViewWidget.IListener
{
	public HeroCostumeRequester(WebServiceRequest webRequest)
		: base(webRequest)
	{
	}

	void HeroCostumeViewWidget.IListener.OnClickEquipCostume(HeroCostumeViewWidget caller)
	{
		if (WebServiceRequest.IsProgressing(_currentServiceRequest) || caller.CurrentCostumeData == null)
		{
			return;
		}
		NShared.SetCostume.Request request = new NShared.SetCostume.Request
		{
			HeroIndex = caller.CurrentCostumeData.HeroIndex,
			CostumeIndex = caller.CurrentCostumeData.CostumeIndex
		};
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetCostume.Response response)
		{
			if (response.Result != NShared.SetCostume.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				HeroCostumeResultInfo heroCostumeResult = response.HeroCostumeResult;
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetCostumeIndex(heroCostumeResult.HeroIndex, heroCostumeResult.CostumeIndex);
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetHairCostumeIndex(heroCostumeResult.HeroIndex, heroCostumeResult.HairCostumeIndex);
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetWeaponCostumeIndex(heroCostumeResult.HeroIndex, heroCostumeResult.WeaponCostumeIndex);
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetHideUniqueWeapon(heroCostumeResult.HeroIndex, heroCostumeResult.HideUniqueWeapon);
			}
		});
	}

	void HeroCostumeViewWidget.IListener.OnClickUnequipCostume(HeroCostumeViewWidget caller)
	{
		if (WebServiceRequest.IsProgressing(_currentServiceRequest) || caller.HeroInfo.CostumeIndex == 0)
		{
			return;
		}
		NShared.UnsetCostume.Request request = new NShared.UnsetCostume.Request
		{
			HeroIndex = caller.CurrentCostumeData.HeroIndex
		};
		_currentServiceRequest = SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UnsetCostume.Response response)
		{
			if (response.Result != NShared.UnsetCostume.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetCostumeIndex(caller.HeroInfo.HeroIndex, 0);
			}
		});
	}

	void HeroCostumeViewWidget.IListener.OnClickBuyCostume(HeroCostumeViewWidget caller)
	{
		RequestBuyCostume(caller.CurrentCostumeData, null);
	}

	void HeroCostumeViewWidget.IListener.OnClickBuyHero(HeroCostumeViewWidget caller)
	{
		CreatureData creatureData = caller.CreatureData;
		GetCostumeManagementInstance<HeroCostumeView>().CloseAndDoAction(delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.BuyHeroShopItem(creatureData);
		});
	}

	void HeroCostumeViewWidget.IListener.OnClickSubStory(HeroCostumeViewWidget caller)
	{
		MoveToSubStory(caller.HeroInfo.HeroIndex);
	}
}
