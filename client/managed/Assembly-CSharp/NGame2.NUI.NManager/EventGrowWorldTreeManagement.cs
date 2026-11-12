using System;
using NGame2.NAccount;
using NGame2.NUI.NEventWorldTree;
using NGame2.NUI.NHelper.NEventGrowWorldTree;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.GetEventStepInfo;
using NShared.GetEventStepReward;
using NShared.PutEventStep;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class EventGrowWorldTreeManagement : BaseManagement<EventGrowWorldTreeManagement>, EventGrowWorldTreeWindow.IListener
{
	private LocalAccountInt _prevWorldTreeStep = new LocalAccountInt(0, typeof(EventGrowWorldTreeManagement).FullName + "_prevWorldTreeStep");

	public int PrevWorldTreeStep
	{
		get
		{
			return _prevWorldTreeStep.Value;
		}
		set
		{
			_prevWorldTreeStep.Value = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		AddInstance<EventGrowWorldTreeWindow>("Resources/Prefab2/Window/EventGrowWorldTree/EventGrowWorldTreeWindow");
	}

	public void RequestEventStepInfo(Action<EventStepInfo> onSuccess, Action<string> onFailed)
	{
		if (!EventGrowWorldTreeHelper.IsOpenContents())
		{
			string i_argA = LocalizationManager.GetString(BaseResultType.Disabled);
			Vespa_System_Action_Extension_Method.fire(onFailed, i_argA);
			return;
		}
		NShared.GetEventStepInfo.Request request = new NShared.GetEventStepInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetEventStepInfo.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				string i_argA2 = LocalizationManager.GetString(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onFailed, i_argA2);
			}
			else if (response.Result != NShared.GetEventStepInfo.ResultType.Success)
			{
				string i_argA3 = LocalizationManager.GetString(response.Result);
				Vespa_System_Action_Extension_Method.fire(onFailed, i_argA3);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onSuccess, response.EventStepInfo);
			}
		});
	}

	public void Open()
	{
		if (!EventGrowWorldTreeHelper.IsOpenContents())
		{
			EventGrowWorldTreeHelper.OpenErrorPopup();
			return;
		}
		EventGrowWorldTreeWindow window = GetInstance<EventGrowWorldTreeWindow>();
		if (window == null)
		{
			VespaDebug.LogError($"[{GetType()}] EventGrowWorldTreeWindow is Null!!");
			return;
		}
		RequestEventStepInfo(delegate(EventStepInfo playerInfo)
		{
			NGame2.NAccount.UserEventGrowWorldTreeManager eventGrowWorldTreeManager = GlobalAutoBehaviour<AccountManager>.instance.EventGrowWorldTreeManager;
			if (eventGrowWorldTreeManager != null)
			{
				eventGrowWorldTreeManager.PlayerEventStepInfo = playerInfo;
				window.Open(this);
			}
		}, delegate(string errorMessage)
		{
			MessagePopup.OpenError(errorMessage);
		});
	}

	void EventGrowWorldTreeWindow.IListener.OnPutGrowPoint(CurrencyType type, Action<Action> onSuccess)
	{
		NShared.PutEventStep.Request request = new NShared.PutEventStep.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PutEventStep.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					EventGrowWorldTreeHelper.OpenErrorPopup(response.BaseResult.ToString());
				}
				else if (response.Result != NShared.PutEventStep.ResultType.Success)
				{
					EventGrowWorldTreeHelper.OpenErrorPopup(response.Result.ToString());
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.EventGrowWorldTreeManager.PlayerEventStepInfo = response.EventStepInfo;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
					Vespa_System_Action_Extension_Method.fire(onSuccess, delegate
					{
						ShowRewardPopup(response.CurrencyResults);
					});
				}
			}
		});
	}

	void EventGrowWorldTreeWindow.IListener.OnClickMissionReward(EventStepType type, Action onSuccess)
	{
		NShared.GetEventStepReward.Request request = new NShared.GetEventStepReward.Request
		{
			EventStepType = type
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetEventStepReward.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					EventGrowWorldTreeHelper.OpenErrorPopup(response.BaseResult.ToString());
				}
				else if (response.Result != NShared.GetEventStepReward.ResultType.Success)
				{
					EventGrowWorldTreeHelper.OpenErrorPopup(response.Result.ToString());
				}
				else
				{
					EventGrowWorldTreeHelper.SetRewardStep(type, response.RewardStep);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResultInfos);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
					Vespa_System_Action_Extension_Method.fire(onSuccess);
					ShowRewardPopup(response.CurrencyResults, response.StaminaResultInfos, response.ItemResults, response.EquipItemResults);
				}
			}
		});
	}

	private void ShowRewardPopup(CurrencyResultInfo3[] currencyResult, StaminaResultInfo[] staminaResultInfos = null, ItemResultInfo[] itemResultInfos = null, EquipItemInfo[] equipItemInfos = null)
	{
		BaseRewardPopup.PopupOpenData rewardData = new BaseRewardPopup.PopupOpenData();
		rewardData.SetCurrencyResultInfos3(currencyResult);
		rewardData.SetStaminaResultInfos(staminaResultInfos);
		rewardData.SetItemResultInfos(itemResultInfos);
		rewardData.SetEquipResultInfos(equipItemInfos);
		BaseRewardPopup baseRewardPopup = BaseRewardPopup.Create(isGlobal: false);
		Action<BaseRewardPopup> action = delegate(BaseRewardPopup window)
		{
			string stringDirect = LocalizationManager.GetStringDirect("EVENT_GROW_WORLD_TREE_TITLE");
			window.OpenWithData(rewardData, isGlobal: false);
			window.SetTitle(stringDirect);
		};
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			action(baseRewardPopup);
		}
		else
		{
			SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(baseRewardPopup, action);
		}
	}
}
