using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil;
using NShared;
using NShared.GetClearMissionReward;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NClearMission;

public class ClearMissionManager : SingletonUpdater<ClearMissionManager>
{
	private List<ClearMissionProductData> _clearMissionProductDataList = new List<ClearMissionProductData>();

	private List<ClearMissionProductMissionData> _clearMissionDataList = new List<ClearMissionProductMissionData>();

	public ClearMissionManager()
	{
		InitClearMissionProductData();
		InitClearMissionData();
	}

	public override void ChainUpdate()
	{
	}

	private void InitClearMissionProductData()
	{
		if (_clearMissionProductDataList == null)
		{
			_clearMissionProductDataList = new List<ClearMissionProductData>();
		}
		_clearMissionProductDataList.Clear();
		IList<ClearMissionProductData> allData = GameRoot.Instance.ClearMissionProductDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.COUNT(allData) != 0)
		{
			_clearMissionProductDataList.AddRange(allData);
		}
	}

	private void InitClearMissionData()
	{
		if (_clearMissionDataList == null)
		{
			_clearMissionDataList = new List<ClearMissionProductMissionData>();
		}
		_clearMissionDataList.Clear();
		IList<ClearMissionProductMissionData> allData = GameRoot.Instance.ClearMissionProductMissionDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.COUNT(allData) != 0)
		{
			_clearMissionDataList.AddRange(allData);
		}
	}

	public bool IsShowClearMissionIcon()
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				return false;
			}
			if (SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
			{
				return false;
			}
		}
		bool result = false;
		if (__Extension_Method_ICollection__.COUNT(_clearMissionProductDataList) == 0)
		{
			InitClearMissionProductData();
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_clearMissionProductDataList); i++)
		{
			ClearMissionProductData clearMissionProductData = _clearMissionProductDataList[i];
			if (clearMissionProductData != null)
			{
				bool flag = IsPurchasedProduct(clearMissionProductData);
				bool flag2 = IsPurchasableProduct(clearMissionProductData.MissionOpenCondition);
				if ((flag || flag2) && flag && IsRemainMissionReward(clearMissionProductData.ProductGroupIndex))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public bool IsPurchasableProduct(string[] condition)
	{
		return SingletonAuto<ConditionChecker>.instance.Check(condition);
	}

	public bool IsPurchasedProduct(ClearMissionProductData productData)
	{
		if (productData.PurchaseType == ClearMissionPurchaseType.PayShop && GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productData.PayShopProductIndex) == null)
		{
			return false;
		}
		return true;
	}

	public bool IsRemainMissionReward(int productGroupIndex)
	{
		bool result = false;
		if (__Extension_Method_ICollection__.COUNT(_clearMissionDataList) == 0)
		{
			InitClearMissionData();
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_clearMissionDataList); i++)
		{
			ClearMissionProductMissionData clearMissionProductMissionData = _clearMissionDataList[i];
			if (clearMissionProductMissionData != null && clearMissionProductMissionData.ProductGroupIndex == productGroupIndex && !GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.IsReceivedReward(clearMissionProductMissionData.Index))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool IsNewPromotion()
	{
		if (__Extension_Method_ICollection__.COUNT(_clearMissionProductDataList) == 0)
		{
			InitClearMissionProductData();
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_clearMissionProductDataList); i++)
		{
			ClearMissionProductData clearMissionProductData = _clearMissionProductDataList[i];
			if (clearMissionProductData != null && IsNewPromotion(clearMissionProductData.PayShopProductIndex))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNewPromotion(int productIndex)
	{
		NewPayShopProductInfo product = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetProduct(productIndex);
		if (product == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productIndex) != null)
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(product.BeginTime, out var o_dateTime))
		{
			return false;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("NewPromotionDay");
		DateTime dateTime = o_dateTime.AddDays(valueInt);
		return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() < dateTime;
	}

	public bool CanReceiveReward(bool checkAllMission, int productGroupIndex = 0)
	{
		bool result = false;
		if (__Extension_Method_ICollection__.COUNT(_clearMissionDataList) == 0)
		{
			InitClearMissionData();
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_clearMissionDataList); i++)
		{
			ClearMissionProductMissionData clearMissionProductMissionData = _clearMissionDataList[i];
			if (clearMissionProductMissionData != null && (checkAllMission || clearMissionProductMissionData.ProductGroupIndex == productGroupIndex) && CanReceiveRewardMission(clearMissionProductMissionData))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool CanReceiveRewardMission(ClearMissionProductMissionData missionData)
	{
		if (missionData == null)
		{
			return false;
		}
		ClearMissionInfo clearMissionInfo = GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.Get(missionData.Index);
		if (clearMissionInfo == null)
		{
			return false;
		}
		if (clearMissionInfo.LastStep > 0)
		{
			return false;
		}
		if (clearMissionInfo.Progress < missionData.ReqProgress)
		{
			return false;
		}
		return true;
	}

	public void BuyPayShopProduct(int payShopProductIndex)
	{
		if (payShopProductIndex != 0 && GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(payShopProductIndex) != null)
		{
			SceneAutoBehaviour<ClearMissionPopupManagement>.instance.CloseClearMissionPopup();
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Growth.ToString(), delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenPayShopProductInfoPopup(payShopProductIndex);
			});
		}
	}

	public void GetMissionReward(int missionIndex, string rewardTitle, Action onReceived, Action onRefreshTab)
	{
		NShared.GetClearMissionReward.Request request = new NShared.GetClearMissionReward.Request
		{
			MissionIndex = missionIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetClearMissionReward.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onReceived);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onReceived);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onReceived);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ClearMissionManager.Apply(response.ClearMissionResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				Vespa_System_Action_Extension_Method.fire(onRefreshTab);
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(rewardTitle).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults)
					.SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false, onReceived);
			}
		});
	}
}
