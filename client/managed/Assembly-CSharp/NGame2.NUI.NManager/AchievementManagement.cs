using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NContentsDefine;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil;
using NGame2.NUtil.NModifyChecker;
using NShared;
using NShared.NLocalization;
using NShared.Ping;
using NShared.RewardAchievement;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using NVespa.NWeb;

namespace NGame2.NUI.NManager;

public class AchievementManagement : BaseManagement<AchievementManagement>, HeroFullSceneViewBase.Listener, HeroDetailView.Listener
{
	private float RecvPingMinInterval = 30f;

	private WebServiceRequest _currentServiceRequest;

	private DateTime requestedTime = DateTime.UtcNow;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<AchievementList>("Resources/Prefab2/Window/Achievement/AchievementList");
		AddInstance<HeroAchievementList>("Resources/Prefab2/Window/Achievement/HeroAchievementList");
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
	}

	public void OpenAchievementList()
	{
		AddTask(OpenAchievementListAsync());
	}

	private ITask OpenAchievementListAsync()
	{
		WebServiceTask<NShared.Ping.Request, NShared.Ping.Response> onSuccess = new WebServiceTask<NShared.Ping.Request, NShared.Ping.Response>(new NShared.Ping.Request(), (NShared.Ping.Request request, NShared.Ping.Response response) => (response.Result != NShared.Ping.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : new DelegateTask(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Set(response.AchievementInfos);
		}));
		return new SequenceTask().ADDTASK(new BranchTask().ConditionTask(() => (DateTime.UtcNow - requestedTime).TotalSeconds > (double)RecvPingMinInterval).OnSuccessTask(onSuccess)).ADDTASK(base.TaskFactory.Open(delegate(AchievementList window)
		{
			window.Init();
		}));
	}

	public ITask OpenHeroAchievementListAsync(int HeroIndex)
	{
		return base.TaskFactory.Open(delegate(HeroAchievementList window)
		{
			window.Init(HeroIndex);
		});
	}

	public void RequestRewardAchievements(List<int> AchievementIndexList)
	{
		HashSet<CurrencyType> hashSet = new HashSet<CurrencyType>();
		foreach (int AchievementIndex in AchievementIndexList)
		{
			int lastStepOnAchievementInfo = AchievementChecker.GetLastStepOnAchievementInfo(AchievementIndex);
			AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(AchievementIndex, lastStepOnAchievementInfo);
			if (data == null)
			{
				continue;
			}
			HashSet<CurrencyType> currencyTypesFromAchievementData = CommonMission.GetCurrencyTypesFromAchievementData(data);
			if (currencyTypesFromAchievementData == null || currencyTypesFromAchievementData.Count == 0)
			{
				continue;
			}
			foreach (CurrencyType item in currencyTypesFromAchievementData)
			{
				hashSet.Add(item);
			}
		}
		CurrencyMaxCap.WarningOrExecute(hashSet, delegate
		{
			AddTask(RequestRewardAchievementTask(AchievementIndexList, OnSuccessRewardAchievement, isHeroAchievement: true));
		});
	}

	public void OpenHeroDetailView(int heroIndex)
	{
		if (GameRoot.Instance.CreatureDataContainer.GetData(heroIndex) != null)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
			if (cachedHeroInfo == null)
			{
				cachedHeroInfo = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(heroIndex);
			}
			GetInstance<HeroDetailView>().Open(cachedHeroInfo, this);
		}
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
		HeroDetailView heroDetailView = GetInstance<HeroDetailView>();
		if (heroDetailView != null)
		{
			if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
			{
				LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
				heroDetailView.ChangeLayerRecursive(layerInfo);
			}
			else
			{
				LayerInfo layerInfo2 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI);
				heroDetailView.ChangeLayerRecursive(layerInfo2);
			}
		}
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
	}

	void HeroDetailView.Listener.OnTryBattle(HeroDetailView sender)
	{
		if (HeroDetailView.TryBattle(sender.HeroInfo))
		{
			LobbyScene2.ListReturnState.Add(new AchivementTryBattleReturnState());
		}
	}

	public void RestoreHeroDetailView()
	{
		AddTask(OpenAchievementListAsync(), OpenHeroAchievementListAsync(HeroDetailView.HeroIndex), base.TaskFactory.Run(delegate
		{
			SceneAutoBehaviour<AchievementManagement>.instance.OpenHeroDetailView(HeroDetailView.HeroIndex);
		}));
	}

	public void RestoreHeroDetailView(int HeroIndex)
	{
		AddTask(OpenAchievementListAsync(), OpenHeroAchievementListAsync(HeroIndex));
	}

	public void ResponseSuccessAchievementReward(NShared.RewardAchievement.Request request, NShared.RewardAchievement.Response response, bool isHeroAchievement, Action afterReward)
	{
		BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Quest.COMPLETE_QUEST_MSG)).SetExpResult(response.expResult).SetItemResultInfos(response.itemResults)
			.SetEquipResultInfos(response.equipItemInfos)
			.SetFriendshipPointResults(response.FriendshipPointResults)
			.AddCurrencyResultInfos3(response.CurrencyResults);
		if (response.staminaResults != null)
		{
			popupOpenData.SetStaminaResultInfos(response.staminaResults);
		}
		BaseRewardPopup.OpenPopup(popupOpenData, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI(), afterReward);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(request.AchievementIndices); i++)
		{
			SingletonAuto<AchievementNotiChecker>.instance.RewardAchievement(request.AchievementIndices[i]);
		}
	}

	private void OnSuccessRewardAchievement(NShared.RewardAchievement.Request request, NShared.RewardAchievement.Response response, bool isHeroAchievement, Action afterReward)
	{
		ResponseSuccessAchievementReward(request, response, isHeroAchievement, afterReward);
		if (!IsOpened<AchievementList>())
		{
			return;
		}
		if (isHeroAchievement && IsOpened<AchievementList>())
		{
			if (IsOpened<AchievementList>())
			{
				AchievementData achievementData = null;
				int num = 0;
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(request.AchievementIndices); i++)
				{
					achievementData = GameRoot.Instance.AchievementDataContainer.GetData(request.AchievementIndices[i], 1);
					num = ((achievementData != null) ? Vespa_String_Extension_Method.TOINT(achievementData.ReqValue, 0) : 0);
					GetInstance<AchievementList>().UpdateAchievementHero(num);
				}
			}
			if (IsOpened<HeroAchievementList>())
			{
				GetInstance<HeroAchievementList>().SetDirty();
			}
		}
		else
		{
			GetInstance<AchievementList>().SetDirty();
		}
	}

	public ITask RequestRewardAchievementTask(List<int> indexList, Action<NShared.RewardAchievement.Request, NShared.RewardAchievement.Response, bool, Action> onSuccess, bool isHeroAchievement = false)
	{
		SingletonAuto<ModifyChecker>.instance.CheckStart<ClearAchievement>(new string[1] { indexList.ToArray().ToString() });
		return new BranchTask().If(() => AchievementChecker.Cleared(Vespa_Generic_IList_Extension_Method.GetLast(indexList)) && !AchievementChecker.Completed(Vespa_Generic_IList_Extension_Method.GetLast(indexList))).Then(new WebServiceTask<NShared.RewardAchievement.Request, NShared.RewardAchievement.Response>(new NShared.RewardAchievement.Request
		{
			AchievementIndices = indexList.ToArray(),
			Steps = GetStepArray(indexList)
		}, delegate(NShared.RewardAchievement.Request request, NShared.RewardAchievement.Response response)
		{
			if (response.Result != NShared.RewardAchievement.ResultType.Success)
			{
				return base.TaskFactory.Run(delegate
				{
					Action onOk = null;
					if (response.Result == NShared.RewardAchievement.ResultType.RewardTimeEnded)
					{
						onOk = delegate
						{
							SceneAutoBehaviour<MissionManagement>.instance.OnRewardTimeEnded();
						};
					}
					MessagePopup.OpenError(response.Result, onOk);
				});
			}
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.expResult);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.staminaResults);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.itemResults);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResults);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.equipItemInfos);
			GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(response.ItemTimeDurationInfo);
			Vespa_System_Action_Extension_Method.fire(onSuccess, request, response, isHeroAchievement, delegate
			{
				if (SingletonAuto<ModifyChecker>.instance.CheckResult<ClearAchievement>(new string[1] { indexList.ToArray().ToString() }))
				{
					string[] modifyCondition = SingletonAuto<ModifyChecker>.instance.GetModifyCondition<ClearAchievement>(new string[1] { indexList.ToArray().ToString() });
					if (GameRoot.Instance.AppReviewConditionDataContainer.IsCheck(modifyCondition))
					{
						AppReviewPopup.Open();
					}
				}
				SceneAutoBehaviour<MissionManagement>.instance.RefreshQuestList();
			});
			return (ITask)null;
		}));
	}

	private int[] GetStepArray(List<int> indexList)
	{
		List<int> list = new List<int>();
		list.Clear();
		for (int i = 0; i < indexList.Count; i++)
		{
			list.Add(AchievementChecker.GetLastStepOnAchievementInfo(indexList[i]));
		}
		return list.ToArray();
	}

	public void SelectAchievementHero(int HeroIndex)
	{
		AddTask(OpenHeroAchievementListAsync(HeroIndex));
	}
}
