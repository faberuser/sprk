using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NEventCraft;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.EventCraftItem;
using NShared.RewardEventRoulette;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class EventCraftManagement : BaseManagement<EventCraftManagement>, EventCraftWindow.IListener, EventRouletteWindow.IListener
{
	protected override void Awake()
	{
		base.Awake();
		AddInstance<EventCraftWindow>("Resources/Prefab2/Window/EventCraft/EventCraftWindow");
		AddInstance<EventRouletteWindow>("Resources/Prefab2/Window/EventCraft/EventRouletteWindow");
	}

	public void OpenEventCraft()
	{
		if (!IsOpenContents(EventBonusType.EventCraft))
		{
			OpenErrorPopup();
			return;
		}
		EventCraftWindow eventCraftWindow = GetInstance<EventCraftWindow>();
		if (eventCraftWindow != null)
		{
			eventCraftWindow.Open(this);
		}
	}

	public void OpenEventRoulette()
	{
		if (!IsOpenContents(EventBonusType.EventRoulette))
		{
			OpenErrorPopup();
			return;
		}
		EventRouletteWindow eventRouletteWindow = GetInstance<EventRouletteWindow>();
		if (eventRouletteWindow != null)
		{
			eventRouletteWindow.Open(this);
		}
	}

	private void OpenErrorPopup()
	{
		string stringDirect = LocalizationManager.GetStringDirect("EVENT_HIDDEN_DUNGEON_INFO");
		string stringDirect2 = LocalizationManager.GetStringDirect("EVENT_OPHELIA_WORKSHOP_END_POPUP_DESC_2");
		MessagePopup.Open(stringDirect, stringDirect2);
	}

	private bool IsOpenContents(EventBonusType bonusType)
	{
		bool result = false;
		List<DropBonusEvent> effectiveEvents = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(bonusType);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(effectiveEvents))
		{
			return result;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(effectiveEvents); i++)
		{
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(effectiveEvents[i].EndTime, out var o_dateTime) && (int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds > 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	void EventCraftWindow.IListener.OnCraft(EventCraftData eventCraftData, int craftCount)
	{
		NShared.EventCraftItem.Request request = new NShared.EventCraftItem.Request
		{
			CraftIndex = eventCraftData.Index,
			CraftCount = craftCount
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EventCraftItem.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.EventCraftItem.ResultType.Success)
			{
				if (res.Result == NShared.EventCraftItem.ResultType.MissingBonusEventCondition)
				{
					string stringDirect = LocalizationManager.GetStringDirect("EVENT_HIDDEN_DUNGEON_INFO");
					string stringDirect2 = LocalizationManager.GetStringDirect("EVENT_OPHELIA_WORKSHOP_END_POPUP_DESC");
					MessagePopup.Open(stringDirect, stringDirect2);
				}
				else
				{
					MessagePopup.OpenError(res.Result);
				}
			}
			else
			{
				_ = res.ItemResults;
				DbRewardResult rewardResult = res.RewardResult;
				EventCraftWindow window = GetInstance<EventCraftWindow>();
				if (!(window == null) && rewardResult != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.ApplyDbRewardResult(res.RewardResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResults);
					GlobalAutoBehaviour<AccountManager>.instance.EventCraftManager.Apply(res.EventCraftItemInfo);
					bool isSpecialResult = CheckSpecialResults(rewardResult);
					Action onComplete = delegate
					{
						BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
						popupOpenData.AddItemResultInfos(rewardResult.ItemResults);
						popupOpenData.AddEquipResultInfos(rewardResult.EquipItemResults);
						popupOpenData.AddCurrencyResultInfos3(rewardResult.CurrencyResults);
						string stringDirect3 = LocalizationManager.GetStringDirect("COMMON_REWARD");
						popupOpenData.SetTitle(stringDirect3);
						BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
						window.UpdateCraftCount(eventCraftData);
					};
					window.PlaySuccessAction(isSpecialResult, null, onComplete);
				}
			}
		});
	}

	private bool CheckSpecialResults(DbRewardResult rewardResult)
	{
		if (rewardResult == null)
		{
			return false;
		}
		ItemResultInfo[] itemResults = rewardResult.ItemResults;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(itemResults))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemResults); i++)
			{
				ItemResultInfo itemResultInfo = itemResults[i];
				if (itemResultInfo != null && GameRoot.Instance.EventCraftEffectDataContainer.GetData(itemResultInfo.ItemIndex) != null)
				{
					return true;
				}
			}
		}
		EquipItemInfo[] equipItemResults = rewardResult.EquipItemResults;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(equipItemResults))
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(equipItemResults); j++)
			{
				EquipItemInfo equipItemInfo = equipItemResults[j];
				if (equipItemInfo != null && GameRoot.Instance.EventCraftEffectDataContainer.GetData(equipItemInfo.ItemIndex) != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	void EventRouletteWindow.IListener.OnClickRun(EventRouletteData eventRouletteData, long rouletteCount)
	{
		int index = eventRouletteData.Index;
		NShared.RewardEventRoulette.Request request = new NShared.RewardEventRoulette.Request
		{
			RouletteIndex = index,
			RouletteCount = checked((int)rouletteCount)
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RewardEventRoulette.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.RewardEventRoulette.ResultType.Success)
			{
				if (res.Result == NShared.RewardEventRoulette.ResultType.MissingBonusEventCondition)
				{
					string stringDirect = LocalizationManager.GetStringDirect("EVENT_HIDDEN_DUNGEON_INFO");
					string stringDirect2 = LocalizationManager.GetStringDirect("EVENT_OPHELIA_WORKSHOP_END_POPUP_DESC");
					MessagePopup.Open(stringDirect, stringDirect2);
				}
				else
				{
					MessagePopup.OpenError(res.Result);
				}
			}
			else
			{
				DbRewardResult rewardResult = res.RewardResult;
				EventRouletteWindow eventRouletteWindow = GetInstance<EventRouletteWindow>();
				if (!(eventRouletteWindow == null))
				{
					Action onComplete = delegate
					{
						string stringDirect3 = LocalizationManager.GetStringDirect("COMMON_REWARD");
						BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
						popupOpenData.SetTitle(stringDirect3);
						popupOpenData.SetItemResultInfos(rewardResult.ItemResults);
						popupOpenData.SetEquipResultInfos(rewardResult.EquipItemResults);
						popupOpenData.SetCurrencyResultInfos3(rewardResult.CurrencyResults);
						BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
						GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResults);
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResults);
						GlobalAutoBehaviour<AccountManager>.instance.ApplyDbRewardResult(res.RewardResult);
					};
					eventRouletteWindow.PlaySuccessAction(eventRouletteData, null, onComplete);
				}
			}
		});
	}
}
