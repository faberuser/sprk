using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NEventForge;
using NShared;
using NShared.EventExchangeEquip;
using NShared.EventForge;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NEvent;

public class EventForgeManagement : BaseManagement<EventForgeManagement>, EventForgeWindow.IListener, ExchangeRandomRewardPopup.IListener, EventForgeBreakConfirmPopup.IListener
{
	private EventForgeWebManager _eventForgeWebManager;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<EventForgeWindow>("Resources/Prefab2/Window/EventForge/EventForgeWindow");
		AddInstance<ExchangeRandomRewardPopup>("Resources/Prefab2/Window/EventForge/EventForgeItemChangeMessagePopup");
		AddInstance<EventForgePercentageInfoPopup>("Resources/Prefab2/Window/EventForge/EventForgeProbabilityPopup");
		AddInstance<EventForgeBreakConfirmPopup>("Resources/Prefab2/Window/EventForge/EventForgeMessagePopup");
		AddInstance<EventForgeReinforcementResultPopup>("Resources/Prefab2/Window/EventForge/BaseEventForgeResultPopup");
		_eventForgeWebManager = new EventForgeWebManager();
	}

	public void OpenEventForgeWindow()
	{
		EventForgeWindow eventForgeWindow = GetInstance<EventForgeWindow>();
		if (!(eventForgeWindow == null))
		{
			eventForgeWindow.Open(new EventForgeWindow.Info
			{
				IListener = this,
				CheckEnoughCompleteSuccessMaterialCountFunc = CheckPossibleCompleteSuccessReinforcement
			});
		}
	}

	private void OpenExchangeRandomRewardPopup(int slotIndex, EventForgeData eventForgeData, Action resultAction)
	{
		ExchangeRandomRewardPopup exchangeRandomRewardPopup = GetInstance<ExchangeRandomRewardPopup>();
		if (!(exchangeRandomRewardPopup == null))
		{
			exchangeRandomRewardPopup.Open(new ExchangeRandomRewardPopup.Info
			{
				IListener = this,
				SlotIndex = slotIndex,
				EventForgeData = eventForgeData,
				ResultAction = resultAction
			});
		}
	}

	private void OpenEventForgePercentageInfoPopup()
	{
		EventForgePercentageInfoPopup eventForgePercentageInfoPopup = GetInstance<EventForgePercentageInfoPopup>();
		if (!(eventForgePercentageInfoPopup == null))
		{
			eventForgePercentageInfoPopup.Open();
		}
	}

	private void OpenEventForgeBreakConfirmPopup(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction)
	{
		EventForgeBreakConfirmPopup eventForgeBreakConfirmPopup = GetInstance<EventForgeBreakConfirmPopup>();
		if (!(eventForgeBreakConfirmPopup == null))
		{
			eventForgeBreakConfirmPopup.Open(new EventForgeBreakConfirmPopup.Info
			{
				IListener = this,
				SlotIndex = slotIndex,
				EventForgeData = eventForgeData,
				ResultAction = resultAction,
				CheckEnoughCompleteSuccessMaterialCountFunc = CheckPossibleCompleteSuccessReinforcement
			});
		}
	}

	private void OpenEventForgeReinforcementResultPopup(EventForgeData eventForgeData, NShared.EventForge.Response resultInfo)
	{
		EventForgeReinforcementResultPopup eventForgeReinforcementResultPopup = GetInstance<EventForgeReinforcementResultPopup>();
		if (eventForgeReinforcementResultPopup == null)
		{
			return;
		}
		Action failResultAction = delegate
		{
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("EVENT_FORGE_GAIN_BREAKITEM")).SetItemResultInfos(resultInfo.ItemResults), isGlobal: false, delegate
			{
				CloseEventForgeReinforcementResultPopup();
			});
		};
		eventForgeReinforcementResultPopup.Open(new EventForgeReinforcementResultPopup.Info
		{
			EventForgeData = eventForgeData,
			ResultInfo = resultInfo,
			FailResultAction = failResultAction
		});
	}

	private void CloseExchangeRandomRewardPopup()
	{
		ExchangeRandomRewardPopup exchangeRandomRewardPopup = GetInstance<ExchangeRandomRewardPopup>();
		if (!(exchangeRandomRewardPopup == null))
		{
			exchangeRandomRewardPopup.Close();
		}
	}

	private void CloseEventForgeBreakConfirmPopup()
	{
		EventForgeBreakConfirmPopup eventForgeBreakConfirmPopup = GetInstance<EventForgeBreakConfirmPopup>();
		if (!(eventForgeBreakConfirmPopup == null))
		{
			eventForgeBreakConfirmPopup.Close();
		}
	}

	private void CloseEventForgeReinforcementResultPopup()
	{
		EventForgeReinforcementResultPopup eventForgeReinforcementResultPopup = GetInstance<EventForgeReinforcementResultPopup>();
		if (!(eventForgeReinforcementResultPopup == null))
		{
			eventForgeReinforcementResultPopup.Close();
		}
	}

	private bool CheckPossibleReinforcement(int reinforcementLevel, int groupIndex = 1)
	{
		EventForgeData dataByLevel = GameRoot.Instance.EventForgeDataContainer.GetDataByLevel(groupIndex, reinforcementLevel);
		if (dataByLevel == null)
		{
			return false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.ItemManager == null)
		{
			return false;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(dataByLevel.RequireItem);
		if (dataByCode == null)
		{
			return false;
		}
		return dataByLevel.RequireItemCount <= GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
	}

	private bool CheckPossibleCompleteSuccessReinforcement(int reinforcementLevel, int groupIndex = 1)
	{
		if (!CheckPossibleReinforcement(reinforcementLevel, groupIndex))
		{
			return false;
		}
		EventForgeData dataByLevel = GameRoot.Instance.EventForgeDataContainer.GetDataByLevel(groupIndex, reinforcementLevel);
		if (dataByLevel == null)
		{
			return false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.ItemManager == null)
		{
			return false;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(dataByLevel.SuccessfulItemCode);
		if (dataByCode == null)
		{
			return false;
		}
		return dataByLevel.SuccessfulItemCount <= GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
	}

	private void RequestReinforceEventItem(int slotIndex, bool isCompleteSuccess, int reinforcementLevel, Action<NShared.EventForge.Response> resultAction)
	{
		if (_eventForgeWebManager != null)
		{
			_eventForgeWebManager.RequestReinforceEventItem(slotIndex, isCompleteSuccess, reinforcementLevel, delegate(NShared.EventForge.Response response)
			{
				Vespa_System_Action_Extension_Method.fire(resultAction, response);
			});
		}
	}

	void EventForgeWindow.IListener.ProgressReinforceItem(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction)
	{
		int num = 1;
		if (eventForgeData != null)
		{
			num = eventForgeData.EnchantLevel + 1;
		}
		EventForgeData dataByLevel = GameRoot.Instance.EventForgeDataContainer.GetDataByLevel(eventForgeData?.EnchatItemGroup ?? 1, num);
		if (dataByLevel != null)
		{
			if (dataByLevel.IsBreakPopup)
			{
				OpenEventForgeBreakConfirmPopup(slotIndex, eventForgeData, resultAction);
			}
			else
			{
				RequestReinforceEventItem(slotIndex, isCompleteSuccess: false, num, resultAction);
			}
		}
	}

	void EventForgeWindow.IListener.ProgressCompleteSuccessReinforceItem(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction)
	{
		int reinforcementLevel = 1;
		if (eventForgeData != null)
		{
			reinforcementLevel = eventForgeData.EnchantLevel + 1;
		}
		RequestReinforceEventItem(slotIndex, isCompleteSuccess: true, reinforcementLevel, delegate(NShared.EventForge.Response response)
		{
			OpenEventForgeReinforcementResultPopup(eventForgeData, response);
			Vespa_System_Action_Extension_Method.fire(resultAction, response);
		});
	}

	void EventForgeWindow.IListener.ExchangeRandomReward(int slotIndex, EventForgeData eventForgeData, Action resultAction)
	{
		if (eventForgeData != null)
		{
			OpenExchangeRandomRewardPopup(slotIndex, eventForgeData, resultAction);
		}
	}

	void EventForgeWindow.IListener.ShowReinforcementPercentageInfo()
	{
		OpenEventForgePercentageInfoPopup();
	}

	void ExchangeRandomRewardPopup.IListener.ExchangeRandomReward(int slotIndex, Action resultAction)
	{
		if (_eventForgeWebManager != null)
		{
			_eventForgeWebManager.RequestExchangeRandomReward(slotIndex, delegate(NShared.EventExchangeEquip.Response response)
			{
				CloseExchangeRandomRewardPopup();
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("COMMON_REWARD")).SetItemResultInfos(response.ItemResults), isGlobal: false);
				Vespa_System_Action_Extension_Method.fire(resultAction);
			});
		}
	}

	void EventForgeBreakConfirmPopup.IListener.ProgressReinforceItem(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction)
	{
		int num = 1;
		if (eventForgeData != null)
		{
			num = eventForgeData.EnchantLevel + 1;
		}
		EventForgeData nextEventForgeData = GameRoot.Instance.EventForgeDataContainer.GetDataByLevel((eventForgeData == null) ? 1 : eventForgeData.EnchatItemGroup, num);
		if (nextEventForgeData == null)
		{
			return;
		}
		RequestReinforceEventItem(slotIndex, isCompleteSuccess: false, num, delegate(NShared.EventForge.Response response)
		{
			CloseEventForgeBreakConfirmPopup();
			if (nextEventForgeData.IsHighEnchant)
			{
				OpenEventForgeReinforcementResultPopup(eventForgeData, response);
			}
			Vespa_System_Action_Extension_Method.fire(resultAction, response);
		});
	}

	void EventForgeBreakConfirmPopup.IListener.ProgressCompleteSuccessReinforceItem(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction)
	{
		int reinforcementLevel = 1;
		if (eventForgeData != null)
		{
			reinforcementLevel = eventForgeData.EnchantLevel + 1;
		}
		RequestReinforceEventItem(slotIndex, isCompleteSuccess: true, reinforcementLevel, delegate(NShared.EventForge.Response response)
		{
			CloseEventForgeBreakConfirmPopup();
			OpenEventForgeReinforcementResultPopup(eventForgeData, response);
			Vespa_System_Action_Extension_Method.fire(resultAction, response);
		});
	}
}
