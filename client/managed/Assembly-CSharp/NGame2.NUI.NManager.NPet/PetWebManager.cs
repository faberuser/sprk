using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.AddEquipStorageSlot;
using NShared.ChangePetLayout;
using NShared.FeedThePet;
using NShared.GetPetHouseInfo;
using NShared.PetHappyGift;
using NShared.PetInteractive;
using NShared.SetEggInPetIncubator;
using NShared.UnsetEggInPetIncubator;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.NPet;

public class PetWebManager
{
	public void RequestGetPetHouseInfo(Action endAction)
	{
		NShared.GetPetHouseInfo.Request request = new NShared.GetPetHouseInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.GetPetHouseInfo.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(endAction);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(endAction);
			}
			else if (response.Result != NShared.GetPetHouseInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(endAction);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Apply(response.PetIncubatorSlotInfos);
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorManager.Apply(response.PetIncubatorInfos);
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetMiscInfos);
				Vespa_System_Action_Extension_Method.fire(endAction);
			}
		});
	}

	public void RequestChangePetLayout(int[] petIndices, Action successAction)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(petIndices))
		{
			return;
		}
		NShared.ChangePetLayout.Request request = new NShared.ChangePetLayout.Request
		{
			PetIndices = petIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ChangePetLayout.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ChangePetLayout.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetMiscInfos);
				if (successAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(successAction);
				}
			}
		});
	}

	public void RequestPetInteractive(int petIndex, PetActionType petActionType, int actionCount, Action<bool> endAction)
	{
		NShared.PetInteractive.Request request = new NShared.PetInteractive.Request
		{
			PetIndex = petIndex,
			ActionType = petActionType,
			ActionCount = actionCount
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PetInteractive.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction, i_argA: false);
				}
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction, i_argA: false);
				}
			}
			else if (response.Result != NShared.PetInteractive.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction, i_argA: false);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetResult, NGame2.NAccount.UserPetManager.EChangedPetInfoType.PetInterAction);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction, i_argA: true);
				}
			}
		});
	}

	public void RequestFeedThePet(int petIndex, int feedCount, Action<bool> endAction)
	{
		NShared.FeedThePet.Request request = new NShared.FeedThePet.Request
		{
			PetIndex = petIndex,
			FeedCount = feedCount
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.FeedThePet.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction, i_argA: false);
				}
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction, i_argA: false);
				}
			}
			else if (response.Result != NShared.FeedThePet.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction, i_argA: false);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetResultInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction, i_argA: true);
				}
			}
		});
	}

	public void RequestGetHappyGift(int petIndex, Action endAction)
	{
		NShared.PetHappyGift.Request request = new NShared.PetHappyGift.Request
		{
			PetIndex = petIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PetHappyGift.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction);
				}
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction);
				}
			}
			else if (response.Result != NShared.PetHappyGift.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetResult, NGame2.NAccount.UserPetManager.EChangedPetInfoType.PetInterAction);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(response.CurrencyResults);
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect(LocalizationManager.GetStringDirect("PET_FEEDMAX_REWARD_TITLE"))).SetItemResultInfos(response.ItemResults).SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false);
				if (endAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(endAction);
				}
			}
		});
	}

	public void RequestStartIncubating(int slotIndex, int incubatorIndex, int eggIndex, Action successAction)
	{
		NShared.SetEggInPetIncubator.Request request = new NShared.SetEggInPetIncubator.Request
		{
			SlotIndex = slotIndex,
			IncubatorIndex = incubatorIndex,
			ItemIndex = eggIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetEggInPetIncubator.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SetEggInPetIncubator.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Apply(response.PetIncubatorSlotInfo);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				if (successAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(successAction);
				}
			}
		});
	}

	public void RequestCancelIncubating(int slotIndex, int incubatorIndex, Action successAction)
	{
		NShared.UnsetEggInPetIncubator.Request request = new NShared.UnsetEggInPetIncubator.Request
		{
			SlotIndex = slotIndex,
			IncubatorIndex = incubatorIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UnsetEggInPetIncubator.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.UnsetEggInPetIncubator.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Apply(response.PetIncubatorSlotInfo);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				Vespa_System_Action_Extension_Method.fire(successAction);
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("COMMON_GET")).SetItemResultInfos(new ItemResultInfo[1] { response.ItemResult }), isGlobal: false);
			}
		});
	}
}
