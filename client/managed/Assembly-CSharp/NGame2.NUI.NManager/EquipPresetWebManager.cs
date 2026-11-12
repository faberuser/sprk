using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.AddEquipStorageSlot;
using NShared.BuyEquipStorageSlot;
using NShared.ChangeEquipStorageSlotName;
using NShared.ResetEquipStorageSlot;
using NShared.SetEquipStorageSlot;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class EquipPresetWebManager : SingletonAuto<EquipPresetWebManager>
{
	public void RequestBuyEquipPresetStorageSlot(Action showNotEnoughGemPopup)
	{
		NShared.BuyEquipStorageSlot.Request request = new NShared.BuyEquipStorageSlot.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyEquipStorageSlot.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.SetEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result == NShared.BuyEquipStorageSlot.ResultType.NotEnoughGem)
			{
				Vespa_System_Action_Extension_Method.fire(showNotEnoughGemPopup);
			}
			else if (response.Result != NShared.BuyEquipStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.Apply(response.EquipPresetInfo);
			}
		});
	}

	public void RequestSaveEquipPresetStorageSlot(int heroIndex, int slotIndex, int[] partIndices, int[] equipItemSlotIndices)
	{
		NShared.AddEquipStorageSlot.Request request = new NShared.AddEquipStorageSlot.Request
		{
			HeroIndex = heroIndex,
			EquipStorageSlotIndex = slotIndex,
			HeroPartIndex = partIndices,
			EquipItemSlotIndex = equipItemSlotIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.AddEquipStorageSlot.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.AddEquipStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.Apply(response.EquipPresetInfo);
			}
		});
	}

	public void RequestLoadEquipPresetStorageSlot(int heroIndex, int slotIndex, Action SuccessAction)
	{
		NShared.SetEquipStorageSlot.Request request = new NShared.SetEquipStorageSlot.Request
		{
			EquipStorageSlotIndex = slotIndex,
			HeroIndex = heroIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetEquipStorageSlot.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.SetEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SetEquipStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				PresetStorageHelper.ApplyUnEquipItems(response.UnEquippedSlotIndices);
				PresetStorageHelper.ApplyEquipPresetStorage(response.HeroInfo);
				Vespa_System_Action_Extension_Method.fire(SuccessAction);
			}
		});
	}

	public void RequestChangeNameEquipPresetStorage(int slotIndex, string name)
	{
		NShared.ChangeEquipStorageSlotName.Request request = new NShared.ChangeEquipStorageSlotName.Request
		{
			EquipStorageSlotIndex = slotIndex,
			Name = name
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ChangeEquipStorageSlotName.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.ChangeEquipStorageSlotName.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ChangeEquipStorageSlotName.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.ApplyName(response.EquipStorageSlotIndex, response.NewName);
			}
		});
	}

	public void RequestResetEquipPresetStorage(int slotIndex)
	{
		NShared.ResetEquipStorageSlot.Request request = new NShared.ResetEquipStorageSlot.Request
		{
			EquipStorageSlotIndex = slotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResetEquipStorageSlot.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.ResetEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ResetEquipStorageSlot.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.ApplyReset(response.EquipPresetInfo);
			}
		});
	}
}
