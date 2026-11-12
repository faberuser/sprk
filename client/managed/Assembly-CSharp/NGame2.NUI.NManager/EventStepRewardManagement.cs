using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NShared.GiveGiftItem;
using NShared.NLocalization;
using NShared.RecvRewardFriendlyPoint;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class EventStepRewardManagement : BaseManagement<EventStepRewardManagement>, ITouchDisabler, EventStepRewardView.Listener
{
	public bool IsValid => true;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<EventStepRewardView>("Resources/Prefab2/Window/EventStepReward/EventStepRewardView");
	}

	public void OpenEventStepRewardView(string npcCode, int colLimit, string componentTypeVal, int shopNpcIndex)
	{
		NPCData data = GameRoot.Instance.NPCDataContainer.GetData(npcCode);
		if (data != null)
		{
			GetInstance<EventStepRewardView>().Open(data, colLimit, this, componentTypeVal, shopNpcIndex);
		}
	}

	void EventStepRewardView.Listener.OnClickConfrimPresent(int heroIndex, int itemIndex, int itemCount, Action action)
	{
		NShared.GiveGiftItem.Request request = new NShared.GiveGiftItem.Request();
		request.HeroIndex = heroIndex;
		request.ItemIndices = new int[1] { itemIndex };
		request.ItemCount = new int[1] { itemCount };
		NShared.GiveGiftItem.Request request2 = request;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2, delegate(NShared.GiveGiftItem.Response response)
		{
			if (response.Result != NShared.GiveGiftItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Apply(response.FriendlyInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.EventGiftPointResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				if (action != null)
				{
					Vespa_System_Action_Extension_Method.fire(action);
				}
			}
		});
	}

	void EventStepRewardView.Listener.OnClickGetReward(int heroIndex, int step, NpcFriendlyType type, Action action)
	{
		NShared.RecvRewardFriendlyPoint.Request request = new NShared.RecvRewardFriendlyPoint.Request
		{
			HeroIndex = heroIndex,
			Step = step
		};
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RecvRewardFriendlyPoint.Response response)
		{
			GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
			if (response.Result != NShared.RecvRewardFriendlyPoint.ResultType.Success)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("EVENT_REWARD_GET_NOTICE_MESSAGE"));
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Apply(response.FriendlyInfo);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroInfos, response.ExpResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResultInfos);
				string title = LocalizationManager.GetString(NPCRecruit.NPC_TAKE_FRIENDLY_REWARD_TITLE);
				if (type == NpcFriendlyType.EventGodKing || type == NpcFriendlyType.EventGiftItem)
				{
					title = LocalizationManager.GetStringDirect("GUILD_RAID_CONTRIBUTE_REWARD");
				}
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(title).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults)
					.SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false, action);
			}
		});
	}
}
