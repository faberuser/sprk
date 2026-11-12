using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.EventExchangeEquip;
using NShared.EventForge;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NEvent;

public class EventForgeWebManager
{
	private bool CheckBaseResult(NShared.Response response)
	{
		if (response == null)
		{
			MessagePopup.OpenError(BaseResultType.Fail);
			return false;
		}
		if (response.BaseResult != BaseResultType.Success)
		{
			MessagePopup.OpenError(response.BaseResult);
			return false;
		}
		return true;
	}

	public void RequestReinforceEventItem(int slotIndex, bool isCompleteSuccess, int reinforcementLevel, Action<NShared.EventForge.Response> resultAction)
	{
		NShared.EventForge.Request request = new NShared.EventForge.Request
		{
			EquipItemSlotIndex = slotIndex,
			UpgradeLevel = reinforcementLevel,
			IsFixed = isCompleteSuccess
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EventForge.Response response)
		{
			if (CheckBaseResult(response))
			{
				if (response.Result != NShared.EventForge.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfos);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.ResultEquipItem);
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyRemoved(response.RemovedEquipItemSlotIndices);
					Vespa_System_Action_Extension_Method.fire(resultAction, response);
				}
			}
		});
	}

	public void RequestExchangeRandomReward(int slotIndex, Action<NShared.EventExchangeEquip.Response> resultAction)
	{
		NShared.EventExchangeEquip.Request request = new NShared.EventExchangeEquip.Request();
		request.EquipItemSlotIndices = new int[1] { slotIndex };
		NShared.EventExchangeEquip.Request request2 = request;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2, delegate(NShared.EventExchangeEquip.Response response)
		{
			if (CheckBaseResult(response))
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyRemoved(response.RemovedEquipItemSlotIndices);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				Vespa_System_Action_Extension_Method.fire(resultAction, response);
			}
		});
	}
}
