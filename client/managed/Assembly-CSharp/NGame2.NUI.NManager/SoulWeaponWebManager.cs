using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;
using NShared.ConfirmSoulStone;
using NShared.GetSoulStoneMileageReward;
using NShared.RestoreSoulStone;
using NShared.SoulBreak;
using NShared.SoulWeaponConfirmRenewOption;
using NShared.SoulWeaponEtherInjection;
using NShared.SoulWeaponReinforce;
using NShared.SoulWeaponRenewOption;
using NShared.SoulWeaponTransition;
using NShared.SoulWeaponUpgrade;
using NShared.SoulWeaponliberation;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class SoulWeaponWebManager : SingletonAuto<SoulWeaponWebManager>
{
	public interface IListener
	{
		void OnSoulWeaponLiberation(SoulWeaponInfo soulWeaponInfo);

		void OnSoulWeaponReinforce(SoulWeaponInfo soulWeaponInfo, SoulWeaponInfo resultSoulWeaponInfo, bool isSuccess);

		void OnSoulWeaponUpgrade(SoulWeaponInfo originSoulWeaponInfo, SoulWeaponInfo soulWeaponInfo, Action onClose);

		void OnSoulWeaponTransition(int equipItemSlotIndex, SoulWeaponInfo resultSoulWeaponInfo);

		void OnSoulWeaponInjection(SoulWeaponInfo soulWeaponInfo);

		void OnSoulWeaponRenewOption(SoulWeaponInfo soulWeaponInfo);

		void OnSoulWeaponConfirmRenewOption(SoulWeaponInfo soulWeaponInfo, SoulWeaponInfo resultSoulWeaponInfo);

		void OnRestoreSoulStone(RestoreSoulStoneRewardInfo rewardInfo);

		void OnGetBonusSoulStone(RestoreSoulStoneRewardInfo rewardInfo);

		void OnBreakSoulWeapon(ItemResultInfo[] itemResults);

		void FailSoulWeaponReinforce();

		void FailSoulWeaponLiberation();
	}

	private List<IListener> _listListener = new List<IListener>();

	public void AddListener(IListener listener)
	{
		_listListener.Remove(listener);
		_listListener.Add(listener);
	}

	public void RemoveListener(IListener listener)
	{
		_listListener.Remove(listener);
	}

	public void RequesetLiberation(int equipItemSlotIndex)
	{
		NShared.SoulWeaponliberation.Request request = new NShared.SoulWeaponliberation.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponliberation.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				_listListener.ForEach(delegate(IListener e)
				{
					e.FailSoulWeaponLiberation();
				});
			}
			else if (response.Result != NShared.SoulWeaponliberation.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				_listListener.ForEach(delegate(IListener e)
				{
					e.FailSoulWeaponLiberation();
				});
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(response.ResultSoulWeaponInfo);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnSoulWeaponLiberation(response.ResultSoulWeaponInfo);
				});
			}
		});
	}

	public void RequestInjection(int equipItemSlotIndex, int[] itemInfos)
	{
		NShared.SoulWeaponEtherInjection.Request request = new NShared.SoulWeaponEtherInjection.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex,
			InjectionItemInfos = itemInfos
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponEtherInjection.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SoulWeaponEtherInjection.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(response.ResultSoulWeaponInfo);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnSoulWeaponInjection(response.ResultSoulWeaponInfo);
				});
			}
		});
	}

	public void RequestReinforce(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo == null)
		{
			_listListener.ForEach(delegate(IListener e)
			{
				e.FailSoulWeaponReinforce();
			});
			return;
		}
		SoulWeaponInfo cloneSoulWeaponinfo = soulWeaponInfo.Clone();
		NShared.SoulWeaponReinforce.Request request = new NShared.SoulWeaponReinforce.Request
		{
			EquipItemSlotIndex = soulWeaponInfo.EquipItemSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponReinforce.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				_listListener.ForEach(delegate(IListener e)
				{
					e.FailSoulWeaponReinforce();
				});
			}
			else if (response.Result != NShared.SoulWeaponReinforce.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				_listListener.ForEach(delegate(IListener e)
				{
					e.FailSoulWeaponReinforce();
				});
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(response.ResultSoulWeaponInfo);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnSoulWeaponReinforce(cloneSoulWeaponinfo, response.ResultSoulWeaponInfo, response.ReinforceSuccess);
				});
			}
		});
	}

	public void RequestUpgrade(SoulWeaponInfo soulWeaponInfo, Action onClose)
	{
		if (soulWeaponInfo == null)
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
			return;
		}
		SoulWeaponInfo cloneSoulWeaponInfo = soulWeaponInfo.Clone();
		NShared.SoulWeaponUpgrade.Request request = new NShared.SoulWeaponUpgrade.Request
		{
			EquipItemSlotIndex = soulWeaponInfo.EquipItemSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponUpgrade.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onClose);
			}
			else if (response.Result != NShared.SoulWeaponUpgrade.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onClose);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(response.ResultSoulWeaponInfo);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnSoulWeaponUpgrade(cloneSoulWeaponInfo, response.ResultSoulWeaponInfo, onClose);
				});
			}
		});
	}

	public void RequestTransition(int beforeEquipItemSlotIndex, int AfterEquipItemSlotIndex)
	{
		NShared.SoulWeaponTransition.Request request = new NShared.SoulWeaponTransition.Request
		{
			EquipItemSlotIndex = beforeEquipItemSlotIndex,
			ReceiveEquipItemSlotIndex = AfterEquipItemSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponTransition.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SoulWeaponTransition.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.ApplyTransition(beforeEquipItemSlotIndex, response.ResultSoulWeaponInfo);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnSoulWeaponTransition(beforeEquipItemSlotIndex, response.ResultSoulWeaponInfo);
				});
			}
		});
	}

	public void RequestSoulWeaponRenewOption(SoulWeaponInfo soulWeaponInfo, bool useCoupon)
	{
		if (soulWeaponInfo == null)
		{
			return;
		}
		NGame2.NAccount.UserItemManager itemManager = GlobalAutoBehaviour<AccountManager>.instance.ItemManager;
		if (itemManager == null)
		{
			return;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SoulWeaponRenewTicketItemIndex");
		itemManager.GetItemCount(valueInt);
		NShared.SoulWeaponRenewOption.Request request = new NShared.SoulWeaponRenewOption.Request
		{
			EquipItemSlotIndex = soulWeaponInfo.EquipItemSlotIndex,
			CouponItemIndex = (useCoupon ? valueInt : 0)
		};
		request.OptionStat1 = soulWeaponInfo.GetBaseStat(ESoulWeaponStatType.Attack, isRenew: false);
		request.OptionStat2 = soulWeaponInfo.GetBaseStat(ESoulWeaponStatType.Hp, isRenew: false);
		request.BonusOptionStat1 = soulWeaponInfo.GetBonusStat(request.OptionStat1, ESoulWeaponStatType.Attack, isRenew: false);
		request.BonusOptionStat2 = soulWeaponInfo.GetBonusStat(request.OptionStat2, ESoulWeaponStatType.Hp, isRenew: false);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponRenewOption.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SoulWeaponRenewOption.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(response.ResultSoulWeaponInfo);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnSoulWeaponRenewOption(response.ResultSoulWeaponInfo);
				});
			}
		});
	}

	public void RequestSoulWeaponConfirmRenewOption(SoulWeaponInfo soulWeaponInfo, int equipItemSlotIndex, bool isRenew)
	{
		NShared.SoulWeaponConfirmRenewOption.Request request = new NShared.SoulWeaponConfirmRenewOption.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex,
			IsNew = isRenew
		};
		request.OptionStat1 = soulWeaponInfo.GetBaseStat(ESoulWeaponStatType.Attack, isRenew: false);
		request.OptionStat2 = soulWeaponInfo.GetBaseStat(ESoulWeaponStatType.Hp, isRenew: false);
		request.BonusOptionStat1 = soulWeaponInfo.GetBonusStat(request.OptionStat1, ESoulWeaponStatType.Attack, isRenew: false);
		request.BonusOptionStat2 = soulWeaponInfo.GetBonusStat(request.OptionStat2, ESoulWeaponStatType.Hp, isRenew: false);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponConfirmRenewOption.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SoulWeaponConfirmRenewOption.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				SoulWeaponInfo oldSoulWepaonInfo = soulWeaponInfo.Clone();
				GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(response.ResultSoulWeaponInfo);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnSoulWeaponConfirmRenewOption(oldSoulWepaonInfo, response.ResultSoulWeaponInfo);
				});
			}
		});
	}

	public void RequestRestoreSoulStone()
	{
		NShared.RestoreSoulStone.Request request = new NShared.RestoreSoulStone.Request
		{
			SoulStoneRestoreIndex = 1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RestoreSoulStone.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.RestoreSoulStone.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				int soulStoneMileage = response.SoulStoneMileage;
				long soulStoneMileage2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.SoulStoneMileage;
				int num = soulStoneMileage;
				long addSoulStoneMileage = checked(num - soulStoneMileage2);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
				int soulStoneMileage3 = response.SoulStoneMileage;
				userManager.SetSoulStoneMileage(soulStoneMileage3);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetRestoreSoulStoneIndices(response.SelectSoulStoneIndices);
				RestoreSoulStoneRewardInfo rewardInfo = new RestoreSoulStoneRewardInfo
				{
					CurrencyResults = response.CurrencyResults,
					ItemResults = response.ItemResults,
					EquipItemResults = response.EquipItemResults,
					AddSoulStoneMileage = addSoulStoneMileage,
					SelectSoulStoneIndices = response.SelectSoulStoneIndices,
					IsConfirmResponse = false
				};
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnRestoreSoulStone(rewardInfo);
				});
			}
		});
	}

	public void RequestSelectedSoulStone(int itemIndex)
	{
		NShared.ConfirmSoulStone.Request request = new NShared.ConfirmSoulStone.Request
		{
			SoulStoneItemIndex = itemIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ConfirmSoulStone.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ConfirmSoulStone.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ResultSoulStoneItemInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetRestoreSoulStoneIndices(null);
				RestoreSoulStoneRewardInfo restoreSoulStoneRewardInfo = new RestoreSoulStoneRewardInfo
				{
					CurrencyResults = null,
					ItemResults = new ItemResultInfo[1] { response.ResultSoulStoneItemInfo },
					EquipItemResults = null
				};
				int num = 0;
				restoreSoulStoneRewardInfo.AddSoulStoneMileage = num;
				restoreSoulStoneRewardInfo.SelectSoulStoneIndices = null;
				restoreSoulStoneRewardInfo.IsConfirmResponse = true;
				RestoreSoulStoneRewardInfo rewardInfo = restoreSoulStoneRewardInfo;
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnRestoreSoulStone(rewardInfo);
				});
			}
		});
	}

	public void RequestGetSoulStoneMileageReward()
	{
		NShared.GetSoulStoneMileageReward.Request request = new NShared.GetSoulStoneMileageReward.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetSoulStoneMileageReward.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetSoulStoneMileageReward.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				int soulStoneMileage = response.SoulStoneMileage;
				long soulStoneMileage2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.SoulStoneMileage;
				int num = soulStoneMileage;
				long addSoulStoneMileage = checked(num - soulStoneMileage2);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
				int soulStoneMileage3 = response.SoulStoneMileage;
				userManager.SetSoulStoneMileage(soulStoneMileage3);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetRestoreSoulStoneIndices(response.SelectSoulStoneIndices);
				RestoreSoulStoneRewardInfo rewardInfo = new RestoreSoulStoneRewardInfo
				{
					CurrencyResults = response.CurrencyResults,
					ItemResults = response.ItemResults,
					EquipItemResults = response.EquipItemResults,
					AddSoulStoneMileage = addSoulStoneMileage,
					IsGetBonus = true,
					SelectSoulStoneIndices = response.SelectSoulStoneIndices,
					IsConfirmResponse = false
				};
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnGetBonusSoulStone(rewardInfo);
				});
			}
		});
	}

	public void RequestBreakSoulWeapon(int[] equipItemSlotIndices)
	{
		NShared.SoulBreak.Request request = new NShared.SoulBreak.Request
		{
			EquipItemSlotIndices = equipItemSlotIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulBreak.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SoulBreak.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(equipItemSlotIndices); i++)
				{
					int key = equipItemSlotIndices[i];
					GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Remove(key);
				}
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnBreakSoulWeapon(response.ItemResults);
				});
			}
		});
	}
}
