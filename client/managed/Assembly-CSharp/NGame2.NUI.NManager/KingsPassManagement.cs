using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BuyKingsPassLevel;
using NShared.GetActiveProductInfo;
using NShared.GetKingsPassAllRewards;
using NShared.GetKingsPassReward;
using NShared.GetKingsPassSeasonInfo;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class KingsPassManagement : BaseManagement<KingsPassManagement>, KingsPassWindow.Listener, KingsPassBuyLevelPopup.IListener, BuyPremiumePassPopup.IListener
{
	public KingsPassSeaonInfo KingsPassSeaonInfo;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<KingsPassWindow>("Resources/Prefab2/Window/KingsPass/KingsPassWindow");
		AddInstance<KingsPassBuyLevelPopup>("Resources/Prefab2/Window/KingsPass/KingsPassBuyLevelPopup");
		AddInstance<BuyPremiumePassPopup>("Resources/Prefab2/Window/MessagePopup/BuyPremiumePassPopup");
	}

	public void OpenKingsPassWindow()
	{
		NShared.GetKingsPassSeasonInfo.Request request = new NShared.GetKingsPassSeasonInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetKingsPassSeasonInfo.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result != NShared.GetKingsPassSeasonInfo.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Apply(response.KingsPassMissionInfos);
					GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Set(response.PlayerKingsPassExpInfo);
					GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Set(response.KingsPassSeaonInfo);
					KingsPassSeaonInfo = response.KingsPassSeaonInfo;
					OpenInternalKingspassWindow(KingsPassSeaonInfo);
				}
			}
		});
	}

	private void OpenInternalKingspassWindow(KingsPassSeaonInfo seasonInfo)
	{
		KingsPassWindow kingsPassWindow = GetInstance<KingsPassWindow>();
		if (kingsPassWindow != null)
		{
			kingsPassWindow.Open(seasonInfo, this);
		}
	}

	public void CloseKingsPassWindow()
	{
		KingsPassWindow kingsPassWindow = GetInstance<KingsPassWindow>();
		if (kingsPassWindow != null && kingsPassWindow.IsOpen())
		{
			kingsPassWindow.Close();
		}
	}

	public void OpenKingsPassBuyLevelPopup()
	{
		KingsPassBuyLevelPopup kingsPassBuyLevelPopup = GetInstance<KingsPassBuyLevelPopup>();
		if (kingsPassBuyLevelPopup != null)
		{
			kingsPassBuyLevelPopup.Open(this);
		}
	}

	public void CloseKingsPassBuyLevelPopup()
	{
		KingsPassBuyLevelPopup kingsPassBuyLevelPopup = GetInstance<KingsPassBuyLevelPopup>();
		if (kingsPassBuyLevelPopup != null && kingsPassBuyLevelPopup.IsOpen())
		{
			kingsPassBuyLevelPopup.Close();
		}
	}

	public void OpenBuyPremiumePassPopup()
	{
		BuyPremiumePassPopup buyPremiumePassPopup = GetInstance<BuyPremiumePassPopup>();
		if (!(buyPremiumePassPopup == null))
		{
			buyPremiumePassPopup.Open(this);
		}
	}

	public void CloseBuyPremiumePassPopup()
	{
		BuyPremiumePassPopup buyPremiumePassPopup = GetInstance<BuyPremiumePassPopup>();
		if (buyPremiumePassPopup != null && buyPremiumePassPopup.IsOpen())
		{
			buyPremiumePassPopup.Close();
		}
	}

	public bool IsOpenKingsPassPopup()
	{
		BuyPremiumePassPopup buyPremiumePassPopup = GetInstance<BuyPremiumePassPopup>();
		bool num = buyPremiumePassPopup != null && buyPremiumePassPopup.IsOpen();
		KingsPassBuyLevelPopup kingsPassBuyLevelPopup = GetInstance<KingsPassBuyLevelPopup>();
		bool flag = kingsPassBuyLevelPopup != null && kingsPassBuyLevelPopup.IsOpen();
		return num || flag;
	}

	public bool CanAllReceiveReward(bool checkPaidReward = true)
	{
		bool result = false;
		KingsPassMission[] values = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.GetValues();
		foreach (KingsPassMission kingsPassMission in values)
		{
			if (kingsPassMission.LastStep == 0 && kingsPassMission.Progress == 1)
			{
				KingsPassSeasonMissionData data = GameRoot.Instance.KingsPassSeasonMissionDataContainer.GetData(kingsPassMission.SeasonIndex, kingsPassMission.MissionIndex);
				if (data != null && (!checkPaidReward || String_Extension_Method.IsNullOrEmpty(data.AdditionalRewardPriceType)))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private void OpenRewardPopup(RewardResultInfo[] rewardResults)
	{
		if (rewardResults != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(rewardResults);
			BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("PET_GET_REWARD_TEXT"));
			Array.ForEach(rewardResults, delegate(RewardResultInfo e)
			{
				popupOpen.AddItemResultInfos(e.ItemResults).AddEquipResultInfos(e.EquipItemResults).AddCurrencyResultInfos3(e.CurrencyResults)
					.AddStaminaResultInfos(e.StaminaResultInfos)
					.AddPetInfos(e.PetInfos);
			});
			BaseRewardPopup.OpenPopup(popupOpen, isGlobal: false);
		}
	}

	private void RequestBuyKingsPass(int productIndex)
	{
		RequestCheckKingsPass(productIndex, delegate
		{
			RequestInternalBuyKingsPass(productIndex);
		});
	}

	private void RequestCheckKingsPass(int productIndex, Action onSuccess)
	{
		NShared.GetActiveProductInfo.Request request = new NShared.GetActiveProductInfo.Request
		{
			ProductIndex = productIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetActiveProductInfo.Response response)
		{
			if (response.BaseResult != BaseResultType.Success || response.Result != NShared.GetActiveProductInfo.ResultType.Success)
			{
				CloseBuyPremiumePassPopup();
				MessagePopup.OpenError(response.Result);
			}
			else if (productIndex != response.PayShopProductInfo.Index)
			{
				VespaDebug.LogError("productIndex != response.PayShopProductInfo.Index");
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	private void RequestInternalBuyKingsPass(int productIndex)
	{
		SceneAutoBehaviour<PayShopManagement>.instance.ExternalPurchaseProductProcess(productIndex, delegate
		{
			KingsPassWindow kingsPassWindow = GetInstance<KingsPassWindow>();
			if (kingsPassWindow != null && kingsPassWindow.IsOpen())
			{
				kingsPassWindow.SetKingsPassButton();
				kingsPassWindow.Refresh();
			}
		}, delegate
		{
			CloseBuyPremiumePassPopup();
		});
	}

	void KingsPassWindow.Listener.ReceivedRewarad(KingsPassMission kingsPassMission, KingsPassItemComponent comp, Action onSuccess)
	{
		if (kingsPassMission == null)
		{
			return;
		}
		NShared.GetKingsPassReward.Request request = new NShared.GetKingsPassReward.Request
		{
			SeasonIndex = kingsPassMission.SeasonIndex,
			MissionIndex = kingsPassMission.MissionIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetKingsPassReward.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result != NShared.GetKingsPassReward.ResultType.Success)
				{
					bool isWrongSeason = response.Result == NShared.GetKingsPassReward.ResultType.NoCurrentKingsPassSeason || response.Result == NShared.GetKingsPassReward.ResultType.NotInProgressKingsPassSeason;
					MessagePopup.OpenError(response.Result, delegate
					{
						if (isWrongSeason)
						{
							CloseKingsPassWindow();
						}
					});
				}
				else
				{
					RewardResultInfo rewardsResult = response.RewardsResult;
					if (rewardsResult != null)
					{
						OpenRewardPopup(new RewardResultInfo[1] { rewardsResult });
					}
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Apply(response.KingsPassMissionInfo);
					Vespa_System_Action_Extension_Method.fire(onSuccess);
				}
			}
		});
	}

	void KingsPassWindow.Listener.AllReceiveReward(KingsPassSeaonInfo kingsPassSeaonInfo, Action OnSucess)
	{
		if (kingsPassSeaonInfo == null)
		{
			return;
		}
		NShared.GetKingsPassAllRewards.Request request = new NShared.GetKingsPassAllRewards.Request
		{
			SeasonIndex = kingsPassSeaonInfo.SeasonIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetKingsPassAllRewards.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result != NShared.GetKingsPassAllRewards.ResultType.Success)
				{
					bool isWrongSeason = response.Result == NShared.GetKingsPassAllRewards.ResultType.NoCurrentKingsPassSeason || response.Result == NShared.GetKingsPassAllRewards.ResultType.NotInProgressKingsPassSeason;
					MessagePopup.OpenError(response.Result, delegate
					{
						if (isWrongSeason)
						{
							CloseKingsPassWindow();
						}
					});
				}
				else
				{
					RewardResultInfo[] rewardsResults = response.RewardsResults;
					if (rewardsResults != null)
					{
						OpenRewardPopup(rewardsResults);
					}
					GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Apply(response.KingsPassMissionInfos);
					Vespa_System_Action_Extension_Method.fire(OnSucess);
				}
			}
		});
	}

	void KingsPassWindow.Listener.BuyAdditionalReward(KingsPassMission kingsPassMission, KingsPassSeasonMissionData kingsPassSeasonMissionData, Action OnSucess)
	{
		if (kingsPassMission == null || kingsPassSeasonMissionData == null)
		{
			return;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(kingsPassSeasonMissionData.RewardIndex);
		if (data == null)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.ItemCode1);
		if (dataByCode == null)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("KINGSPASS_ADDITIONAL_REWARD_BUY_TEXT");
		MultipleBuyConfirmPopup.Open(kingsPassSeasonMissionData.AdditionalRewardPrice, kingsPassSeasonMissionData.AdditionalRewardPriceType, dataByCode, stringDirect, delegate
		{
			NShared.GetKingsPassReward.Request request = new NShared.GetKingsPassReward.Request
			{
				SeasonIndex = kingsPassMission.SeasonIndex,
				MissionIndex = kingsPassMission.MissionIndex
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetKingsPassReward.Response response)
			{
				if (response != null && response.BaseResult == BaseResultType.Success)
				{
					if (response.Result != NShared.GetKingsPassReward.ResultType.Success)
					{
						if (response.Result == NShared.GetKingsPassReward.ResultType.NotEnoughCost)
						{
							string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
							string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
							MessagePopup.Open(title, message, delegate
							{
								SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
							}, null);
						}
						else
						{
							bool isWrongSeason = response.Result == NShared.GetKingsPassReward.ResultType.NoCurrentKingsPassSeason || response.Result == NShared.GetKingsPassReward.ResultType.NotInProgressKingsPassSeason;
							MessagePopup.OpenError(response.Result, delegate
							{
								if (isWrongSeason)
								{
									CloseKingsPassWindow();
								}
							});
						}
					}
					else
					{
						RewardResultInfo rewardsResult = response.RewardsResult;
						if (rewardsResult != null)
						{
							OpenRewardPopup(new RewardResultInfo[1] { rewardsResult });
						}
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
						GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Apply(response.KingsPassMissionInfo);
						Vespa_System_Action_Extension_Method.fire(OnSucess);
					}
				}
			});
		});
	}

	void KingsPassWindow.Listener.BuyLevel()
	{
		OpenKingsPassBuyLevelPopup();
	}

	void KingsPassWindow.Listener.BuyKingsPass(int productIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex) == null)
		{
			VespaDebug.LogError("productInfo == null");
			return;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productIndex) != null)
		{
			VespaDebug.LogError("purchaseInfo != null");
			return;
		}
		RequestCheckKingsPass(productIndex, delegate
		{
			PlayerKingsPassExp playerKingsPassExp = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.PlayerKingsPassExp;
			if (playerKingsPassExp == null || playerKingsPassExp.Level == 0)
			{
				string stringDirect = LocalizationManager.GetStringDirect("KINGSPASS_BUY_POPUP_TITLE");
				string stringDirect2 = LocalizationManager.GetStringDirect("KINGSPASS_BUY_POPUP_DESC");
				Action onYes = delegate
				{
					RequestBuyKingsPass(productIndex);
				};
				MessagePopup.Open(stringDirect, stringDirect2, onYes, null, NShared.NLocalization.Common.BUY, NShared.NLocalization.Common.COMMON_CANCEL);
			}
			else
			{
				OpenBuyPremiumePassPopup();
			}
		});
	}

	void KingsPassBuyLevelPopup.IListener.BuyLevel(int seasonIndex, int level)
	{
		RequestBuyKingsPassLevel(seasonIndex, level);
	}

	private void RequestBuyKingsPassLevel(int seasonIndex, int level)
	{
		NShared.BuyKingsPassLevel.Request request = new NShared.BuyKingsPassLevel.Request
		{
			SeasonIndex = seasonIndex,
			ReqLevel = level
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyKingsPassLevel.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success)
			{
				if (response.Result != NShared.BuyKingsPassLevel.ResultType.Success)
				{
					bool isWrongSeason = response.Result == NShared.BuyKingsPassLevel.ResultType.NoCurrentKingsPassSeason || response.Result == NShared.BuyKingsPassLevel.ResultType.NotInProgressKingsPassSeason;
					MessagePopup.OpenError(response.Result, delegate
					{
						if (isWrongSeason)
						{
							CloseKingsPassBuyLevelPopup();
							CloseKingsPassWindow();
						}
					});
				}
				else
				{
					CloseKingsPassBuyLevelPopup();
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Set(response.PlayerKingsPassExpInfo);
					KingsPassWindow kingsPassWindow = GetInstance<KingsPassWindow>();
					if (kingsPassWindow != null && kingsPassWindow.IsOpen())
					{
						kingsPassWindow.SetLevelExpPoint();
						kingsPassWindow.Refresh();
					}
				}
			}
		});
	}

	void BuyPremiumePassPopup.IListener.BuyPremiumePass(int productIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex) != null)
		{
			RequestBuyKingsPass(productIndex);
		}
	}
}
