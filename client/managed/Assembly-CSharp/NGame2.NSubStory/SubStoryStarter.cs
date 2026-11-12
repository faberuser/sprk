using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWorldMap.NMapLayout;
using NShared;
using NShared.BeginCampaign;
using NShared.EndCampaign;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NSubStory;

public class SubStoryStarter
{
	public interface Listener
	{
		void OnEndEventRewardPopup();
	}

	public enum CheckStartRetunType
	{
		HeroRequireFail,
		DungeonRequireFail,
		IsNotSubStoryBattleType,
		SubStoryDataNotExsit,
		CanStart
	}

	private Listener _listener;

	private EnterDungeonData _lastEnterDungeon;

	public SubStoryStarter(Listener listener)
	{
		_listener = listener;
	}

	private EnterDungeonData GetEnterDungeon()
	{
		return new EnterDungeonData(NGame2.NAccount.UserCampaignManager.ChapterIndex, NGame2.NAccount.UserCampaignManager.DungeonIndex, NGame2.NAccount.UserCampaignManager.DungeonIndex, NShared.DungeonDifficultyType.Easy);
	}

	private EnterDungeonData GetEnterDungeon(int chpaterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficultyType = NShared.DungeonDifficultyType.Easy)
	{
		return new EnterDungeonData(chpaterIndex, dungeonIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy);
	}

	public CheckStartRetunType CheckStart(NodeFunctionType functionType)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		if (functionType == NodeFunctionType.SubStoryBattle && !SubStoryUtillity.IsSubStoryBattleType(chapterIndex, dungeonIndex))
		{
			VespaDebug.LogError("chapter[" + chapterIndex + "] dungeon[" + dungeonIndex + "] CampaignDungeonTable BattleType is not SubStory");
			return CheckStartRetunType.IsNotSubStoryBattleType;
		}
		SubStoryData data = GameRoot.Instance.SubStoryDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("chapter[" + chapterIndex + "] dungeon[" + dungeonIndex + "] subStoryData is null");
			return CheckStartRetunType.SubStoryDataNotExsit;
		}
		if (functionType != NodeFunctionType.SubStoryBoss)
		{
			return CheckStartRetunType.CanStart;
		}
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(data.CreatureIndex);
		if (heroInfo == null || heroInfo.Star < data.ReqStar || heroInfo.Transcended < data.ReqTranscended || heroInfo.Level < data.ReqLevel)
		{
			return CheckStartRetunType.HeroRequireFail;
		}
		bool num = data.ReqChapterIndex != 0 && data.ReqDungeonIndex != 0;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(data.ReqChapterIndex, data.ReqDungeonIndex);
		if (num && !flag)
		{
			return CheckStartRetunType.DungeonRequireFail;
		}
		return CheckStartRetunType.CanStart;
	}

	public void StartSubStoryBattle(int chapterIndex, int dungeonIndex, Action onCallback = null)
	{
		_lastEnterDungeon = GetEnterDungeon(chapterIndex, dungeonIndex);
		StartBattleInternal(onCallback);
	}

	public void StartBattle()
	{
		_lastEnterDungeon = GetEnterDungeon();
		StartBattleInternal(delegate
		{
			LobbyScene2.ListReturnState.Add(new CampaignReturnState(_lastEnterDungeon));
		});
	}

	private void StartBattleInternal(Action onCallback = null)
	{
		NShared.BeginCampaign.Request request = CreateBeginCampaignRequest(_lastEnterDungeon);
		if (__Extension_Method_ICollection__.COUNT(request.HeroIndices) == 0)
		{
			VespaDebug.LogError("request.HeroIndices.COUNT() == 0");
			return;
		}
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			SingletonAuto<CampaignContext>.instance.SetForcedUserBattleCreatureCreateInfos(SubStoryUtillity.GetBattleCreatureInfos(_lastEnterDungeon).ToArray());
			Vespa_System_Action_Extension_Method.fire(onCallback);
		});
	}

	private NShared.BeginCampaign.Request CreateBeginCampaignRequest(EnterDungeonData enterDungeon)
	{
		SubStoryData data = GameRoot.Instance.SubStoryDataContainer.GetData(enterDungeon.Chapter, enterDungeon.OriginalDungeon);
		if (data == null)
		{
			return null;
		}
		BattlePartySetting.BattlePartyInfo battlePartyInfo = new BattlePartySetting.BattlePartyInfo();
		List<ForcedHeroInfo> forcedHeroInfoList = data.GetForcedHeroInfoList();
		battlePartyInfo.heroIndices = new int[__Extension_Method_ICollection__.COUNT(forcedHeroInfoList)];
		long[] array = new long[__Extension_Method_ICollection__.COUNT(forcedHeroInfoList)];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(forcedHeroInfoList); i++)
		{
			if (i == 0)
			{
				battlePartyInfo.leaderHeroIndex = forcedHeroInfoList[i].CachedHeroInfo.HeroIndex;
			}
			battlePartyInfo.heroIndices[i] = forcedHeroInfoList[i].CachedHeroInfo.HeroIndex;
			array[i] = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		}
		return new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			LinkedDungeonIndex = enterDungeon.LinkedDungeon,
			DungeonDifficulty = (int)enterDungeon.Difficulty,
			ScenarioDungeon = enterDungeon.IsScenario,
			HeroIndices = battlePartyInfo.heroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = array.ToArray(),
			Repeat = false,
			WorldMapEventIndex = 0,
			WorldMapNodeIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex,
			AffixIndices = battlePartyInfo.AffixIndices
		};
	}

	public void StartEvent()
	{
		_lastEnterDungeon = GetEnterDungeon();
		NShared.BeginCampaign.Request request = CreateBeginCampaignRequest(_lastEnterDungeon);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BeginCampaign.Response response)
		{
			if (response == null || response.BaseResult != BaseResultType.Success || response.Result != NShared.BeginCampaign.ResultType.Success)
			{
				BeginCampaignOnErrorCallback(request, response, null);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ApplyBeginCampaignResponse(response);
				SubStoryData data = GameRoot.Instance.SubStoryDataContainer.GetData(NGame2.NAccount.UserCampaignManager.ChapterIndex, NGame2.NAccount.UserCampaignManager.DungeonIndex);
				SingletonAuto<SubStoryEventManager>.instance.StartEventDirect(data.SubStoryEventIndex, delegate
				{
					SendEndCampaignEvent();
				});
			}
		});
	}

	private void SendEndCampaignEvent()
	{
		NShared.EndCampaign.Request request = new NShared.EndCampaign.Request
		{
			ChapterIndex = _lastEnterDungeon.Chapter,
			DungeonIndex = _lastEnterDungeon.OriginalDungeon,
			DungeonDifficulty = (int)_lastEnterDungeon.Difficulty,
			ScenarioDungeon = _lastEnterDungeon.IsScenario,
			Completed = true,
			Star = 3
		};
		bool wasCompleted = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(request.ChapterIndex, request.DungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EndCampaign.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.EndCampaign.ResultType.Success)
			{
				if (!wasCompleted)
				{
					_ = (object)null;
					_ = 7;
					_ = 1;
					_ = 0;
					_ = new object[2] { request.ChapterIndex, request.DungeonIndex };
				}
				if (response.CampaignResults != null)
				{
					ChapterDungeonInfo[] campaignResults = response.CampaignResults;
					foreach (ChapterDungeonInfo chapterDungeonInfo in campaignResults)
					{
						GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.CheckNewClearNode(chapterDungeonInfo.ChapterIndex, chapterDungeonInfo.DungeonIndex);
					}
				}
				GlobalAutoBehaviour<AccountManager>.instance.ApplyEndCampaignResponse(response);
				if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
				{
					GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.StartEffect();
				}
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("REWARD_SUBSTORY_TITLE")).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemInfos)
					.SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false, delegate
				{
					if (_listener != null)
					{
						_listener.OnEndEventRewardPopup();
					}
				});
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReservedContentsOpenConditionPopup();
				SceneAutoBehaviour<WorldMapManagement>.instance.ProcessChapterFlow(NGame2.NAccount.UserCampaignManager.ChapterIndex, NGame2.NAccount.UserCampaignManager.DungeonIndex, ChapterFlowManager.ConditionKeyType.CompleteSubStory);
			}
		});
	}

	private void BeginCampaignOnErrorCallback(NShared.BeginCampaign.Request request, NShared.BeginCampaign.Response response, Action<NShared.BeginCampaign.Response> onErrorCallback)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(request.ChapterIndex, request.DungeonIndex);
		Vespa_System_Action_Extension_Method.fire(onErrorCallback, response);
		if (data != null)
		{
			if (response.Result == NShared.BeginCampaign.ResultType.NotEnoughStamina || response.Result == NShared.BeginCampaign.ResultType.NotEnoughPlayKey || response.Result == NShared.BeginCampaign.ResultType.NotEnoughHideoutDungeonCount || response.Result == NShared.BeginCampaign.ResultType.NotEnoughConquestDungeonCount)
			{
				Action onSuccess = null;
				if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<EndBattleReward>())
				{
					onSuccess = SceneAutoBehaviour<EndCampaign>.instance.ProcessBattleRepeat;
				}
				UIHelper.ResolveRecoverStamina(isNotEnough: true, null, data.ReqStaminaType, data.ChapterIndex, onSuccess);
				return;
			}
			if (response.Result == NShared.BeginCampaign.ResultType.MaxDailyCount)
			{
				LobbyCampaignDungeon.OpenConfirmResetEventDungeon(request.ChapterIndex, request.DungeonIndex, null);
				return;
			}
			if (response.Result == NShared.BeginCampaign.ResultType.EquipItemFull)
			{
				if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
					MessagePopup.OpenEnabled(title, message, delegate
					{
						SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
					}, null, "MOVE_BAG", bYesEnabled: true);
				}
				else
				{
					string title2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message2 = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
					MessagePopup.OpenEnabled(title2, message2, SceneAutoBehaviour<EndCampaign>.instance.ExecReturnToLobbyAndOpenInventory, null, "MOVE_BAG", bYesEnabled: true);
				}
				return;
			}
		}
		MessagePopup.OpenError(LocalizationManager.GetResultString(response));
	}
}
