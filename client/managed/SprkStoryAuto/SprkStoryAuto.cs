using System;
using System.Collections.Generic;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NCutScene;
using NGame2.NCutScene.NScript;
using NGame2.NScene;
using NGame2.NSubStory;
using NGame2.NTutorial;
using NGame2.NTutorial.NRuntime;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NCutScene;
using NGame2.NUI.NWindow.NSelectRewardPopup;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.BeginCampaign;
using NShared.CompleteTutorial;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SprkStoryAuto
{
	private static BattlePartySetting waitingParty;

	private static bool waitingForEvent;

	private static bool continuousStory;

	private static EnterDungeonData pendingNext;

	private static bool movingToNext;

	private static bool openingNextParty;

	private static float resumeAfter;

	private static float lastProgress;

	private static float nextTick;

	private static float retryAfter;

	private static float nextScriptedSkill;

	private static int scriptedSkillCursor;

	private static SprkStoryAutoRunner runner;

	private static TutorialData lastTutorialStep;

	private static SubStoryEventData lastSubstoryStep;

	private static bool caveCompletionRequested;

	private static bool clauseCompletionRequested;

	private static float clauseResumeAfter;

	private static int[] storyMainParty;

	private static int[] storySubParty;

	private static readonly FieldInfo currentSquard = typeof(BattlePartySetting).GetField("_currentSquard", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo autoStarted = typeof(BattlePartySetting).GetField("_isAutoCampaign", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo cutsceneSkippable = typeof(CutSceneManager).GetField("_canSkip", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo narrationIndex = typeof(ForegroundTextList).GetField("CurrentIndex", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo partyProvider = typeof(BattlePartySetting).GetField("_dataProvider", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo checkNext = typeof(EndCampaign).GetMethod("CheckNextDungeon", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo skillUseResult = typeof(BaseSkillComponent).GetMethod("GetSyncSkillUseResult", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo rewardCallback = typeof(BaseRewardPopup).GetField("_onClose", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo rewardData = typeof(BaseSelectRewardPopup).GetField("_selectRewardData", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly int[] aeginaRoute = new int[24]
	{
		1, 20, 2, 21, 3, 4, 22, 23, 5, 24,
		25, 6, 7, 26, 27, 8, 28, 29, 9, 210,
		30, 31, 11, 12
	};

	private static AutoNextBattle pendingStop;

	private static int stopReadyFrame = -1;

	private static BattlePartySetting.ExtraOptionData pendingEntryOption;

	private static readonly FieldInfo repeatEnabled = typeof(AutoBattle).GetField("_repeatEnabled", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo autoElapsed = typeof(BattlePartySetting).GetField("_autoCampaignElapsedTime", BindingFlags.Instance | BindingFlags.NonPublic);

	public static bool AutoSkills(bool enabled)
	{
		return enabled;
	}

	private static bool ScriptedAutoSkills()
	{
		BattleManagement instance = SceneAutoBehaviour<BattleManagement>.instance;
		if (instance == null)
		{
			return false;
		}
		if (instance.IsOpened<SystemButtonAuto>())
		{
			return instance.GetInstance<SystemButtonAuto>().SkillReserveAuto;
		}
		if (instance.IsOpened<SystemButtonAutoFastContinue>())
		{
			return instance.GetInstance<SystemButtonAutoFastContinue>().SkillReserveAuto;
		}
		if (instance.IsOpened<SystemButtonAutoSimple>())
		{
			return instance.GetInstance<SystemButtonAutoSimple>().SkillReserveAuto;
		}
		return false;
	}

	public static BattleType ContinueType(BattleType type)
	{
		if (!Active() || type != BattleType.Shakmeh)
		{
			return type;
		}
		return BattleType.Campaign;
	}

	public static void SaveStoryPosition(NGame2.NAccount.UserCampaignManager manager, int chapter, int dungeon)
	{
		manager.SaveLatelyBattleNodePos(chapter, (Active() && chapter == 50000 && dungeon == 601) ? 11 : dungeon);
	}

	public static void PrepareStoryRequest(NShared.BeginCampaign.Request request)
	{
		if (Active() && request != null && request.ChapterIndex == 50000 && request.DungeonIndex == 601 && request.ScenarioDungeon)
		{
			request.SelectedRewardItemCodes = NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.GetSelectedRewardToArray();
			request.GroupHeroIndices = request.AiHeroIndices;
			request.AiHeroIndices = null;
		}
	}

	public static DefaultBattlePartySettingDataProdiver StoryProvider(EnterDungeonData enter)
	{
		if (enter == null || enter.Chapter != 50000 || enter.OriginalDungeon != 601 || !IsStory(enter))
		{
			return null;
		}
		CampaignDungeonData linkedDungeonData = enter.LinkedDungeonData;
		ShakmehDungeonData dataByDungeonIndex = GameRoot.Instance.ShakmehDungeonDataContainer.GetDataByDungeonIndex(601);
		if (dataByDungeonIndex == null)
		{
			return null;
		}
		string localStorageKey = linkedDungeonData.LocalStorageKey;
		StaminaType reqStaminaType = linkedDungeonData.ReqStaminaType;
		int staminaOfDifficulty = linkedDungeonData.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), enter.Difficulty, enter.IsScenario);
		object obj = Battle.START_BATTLE;
		BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(dataByDungeonIndex.BanRuleIndex);
		object obj2 = obj;
		int num = staminaOfDifficulty;
		return new DefaultBattlePartySettingDataProdiver(localStorageKey, reqStaminaType, num, (Enum)obj2, enter, data);
	}

	public static bool IsStory(EnterDungeonData enter)
	{
		if (IsStoryRoute(enter) && GlobalAutoBehaviour<AccountManager>.isValidInstance && AccountManager.IsLogined())
		{
			return !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(enter.Chapter, enter.OriginalDungeon);
		}
		return false;
	}

	public static bool IsStoryRoute(EnterDungeonData enter)
	{
		if (enter != null && IsAeginaStage(enter.Chapter, enter.OriginalDungeon))
		{
			if (enter.Difficulty == (NShared.DungeonDifficultyType)Helper.GetMinDifficulty(enter.Chapter, enter.LinkedDungeon) && enter.LinkedDungeonData != null)
			{
				if (!IsStoryType(enter.LinkedDungeonData.BattleType) && enter.LinkedDungeonData.BattleType != BattleType.SubStory)
				{
					if (enter.OriginalDungeon == 601 && enter.IsScenario)
					{
						return enter.LinkedDungeonData.BattleType == BattleType.Shakmeh;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		if (enter != null && IsRequiredSubstory(enter.Chapter, enter.OriginalDungeon))
		{
			if ((enter.Difficulty == NShared.DungeonDifficultyType.Easy || enter.Difficulty == NShared.DungeonDifficultyType.Normal) && enter.LinkedDungeonData != null)
			{
				return enter.LinkedDungeonData.BattleType == BattleType.SubStory;
			}
			return false;
		}
		if (enter != null && (enter.Difficulty == NShared.DungeonDifficultyType.Normal || (IsCaveBattle(enter.Chapter, enter.OriginalDungeon) && enter.Difficulty == NShared.DungeonDifficultyType.Easy)) && enter.LinkedDungeonData != null)
		{
			return IsStoryType(enter.LinkedDungeonData.BattleType);
		}
		return false;
	}

	private static bool IsCaveBattle(int chapter, int dungeon)
	{
		if (chapter == 117)
		{
			if (dungeon != 3)
			{
				return dungeon == 4;
			}
			return true;
		}
		return false;
	}

	private static bool IsScarlet2(int chapter, int dungeon)
	{
		if (chapter == 9)
		{
			return dungeon == 51;
		}
		return false;
	}

	private static bool IsChapter10Finale(int chapter, int dungeon)
	{
		if (chapter == 10 && dungeon >= 30)
		{
			return dungeon <= 41;
		}
		return false;
	}

	private static bool IsAeginaStage(int chapter, int dungeon)
	{
		if (chapter == 50000)
		{
			if (Array.IndexOf(aeginaRoute, dungeon) < 0)
			{
				return dungeon == 601;
			}
			return true;
		}
		return false;
	}

	private static int MapDungeon(EnterDungeonData enter)
	{
		if (enter.Chapter != 50000 || enter.OriginalDungeon != 601)
		{
			return enter.OriginalDungeon;
		}
		return 11;
	}

	private static CampaignDungeonData NextAegina(EnterDungeonData enter)
	{
		int num = Array.IndexOf(aeginaRoute, MapDungeon(enter));
		if (num < 0)
		{
			return null;
		}
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		while (++num < aeginaRoute.Length)
		{
			int num2 = aeginaRoute[num];
			if (!campaignManager.IsCompleted(50000, (num2 == 11) ? 601 : num2))
			{
				return GameRoot.Instance.CampaignDungeonDataContainer.GetData(50000, num2);
			}
		}
		return null;
	}

	private static bool WaitingForAeginaUnlock(CampaignDungeonData next)
	{
		if (next == null || next.ChapterIndex != 50000)
		{
			return false;
		}
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		if (next.DungeonIndex != 30 || !campaignManager.IsCompleted(50000, 210) || campaignManager.IsCompleted(50000, 10))
		{
			if (next.DungeonIndex == 12 && campaignManager.IsCompleted(50000, 601))
			{
				return !campaignManager.IsCompleted(50000, 11);
			}
			return false;
		}
		return true;
	}

	private static bool IsRequiredSubstory(int chapter, int dungeon)
	{
		if (!IsScarlet2(chapter, dungeon) && !IsChapter10Finale(chapter, dungeon))
		{
			if (chapter == 50000)
			{
				if (dungeon != 210 && dungeon != 12)
				{
					if (dungeon >= 20)
					{
						return dungeon <= 31;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private static bool IsStoryEvent(int chapter, int dungeon)
	{
		if (chapter != 10 || (dungeon != 30 && dungeon != 39 && dungeon != 41))
		{
			if (chapter == 50000)
			{
				if (dungeon != 12)
				{
					if (dungeon >= 20)
					{
						return dungeon <= 31;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static NShared.DungeonDifficultyType NextDifficulty(NShared.DungeonDifficultyType difficulty, CampaignDungeonData next)
	{
		if (Active() && next != null && IsAeginaStage(next.ChapterIndex, next.DungeonIndex))
		{
			return (NShared.DungeonDifficultyType)Helper.GetMinDifficulty(50000, (next.DungeonIndex == 11) ? 601 : next.DungeonIndex);
		}
		if (!Active() || next == null)
		{
			return difficulty;
		}
		if (!IsCaveBattle(next.ChapterIndex, next.DungeonIndex) && !IsRequiredSubstory(next.ChapterIndex, next.DungeonIndex))
		{
			return NShared.DungeonDifficultyType.Normal;
		}
		return NShared.DungeonDifficultyType.Easy;
	}

	public static bool IsStoryType(BattleType type)
	{
		if (type != BattleType.Campaign)
		{
			return type == BattleType.Boss;
		}
		return true;
	}

	public static bool Active()
	{
		if (continuousStory && GlobalAutoBehaviour<AccountManager>.isValidInstance && AccountManager.IsLogined() && AutoNextBattle.IsValidInstance && IsStoryType(AutoNextBattle.instance.AutoNextBattleType))
		{
			if (AutoNextBattle.instance.DungeonDifficulty != NShared.DungeonDifficultyType.Normal)
			{
				return AutoNextBattle.instance.DungeonDifficulty == NShared.DungeonDifficultyType.Easy;
			}
			return true;
		}
		return false;
	}

	public static bool ShowChoice(EnterDungeonData enter)
	{
		if (pendingStop != null)
		{
			return true;
		}
		if (enter != null && IsRequiredSubstory(enter.Chapter, enter.OriginalDungeon))
		{
			return false;
		}
		if (!IsStory(enter))
		{
			return false;
		}
		BattlePartySetting party = null;
		BattlePartySetting[] array = UnityEngine.Object.FindObjectsOfType<BattlePartySetting>();
		foreach (BattlePartySetting battlePartySetting in array)
		{
			if (battlePartySetting.IsOpen())
			{
				party = battlePartySetting;
				break;
			}
		}
		if (party == null)
		{
			return false;
		}
		MessagePopup3Button.Open("Story battle", "Play once, or advance through available story battles and unlocked chapters automatically. Continuous Story skips skippable dialogue and stops on defeat or when entry requires your attention.", delegate
		{
			party.TryStartBattle(null);
		}, delegate
		{
			if (!(party == null) && party.IsOpen())
			{
				_ = SingletonUpdater<TutorialManager>.instance;
				if (runner == null)
				{
					GameObject gameObject = new GameObject("SprkStoryAuto");
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
					runner = gameObject.AddComponent<SprkStoryAutoRunner>();
				}
				runner.gameObject.SetActive(value: true);
				Application.runInBackground = true;
				CaptureParty(party, (BattlePartySetting.IDataProvider)partyProvider.GetValue(party));
				BattleType battleType = ((enter.Chapter == 50000) ? BattleType.Campaign : enter.LinkedDungeonData.BattleType);
				NShared.DungeonDifficultyType difficulty = enter.Difficulty;
				int num = 0;
				AutoNextBattle.OpenGlobal(battleType, difficulty, num);
				continuousStory = true;
				pendingNext = null;
				lastProgress = Time.realtimeSinceStartup;
				waitingParty = party;
				waitingForEvent = true;
				retryAfter = lastProgress + 0.5f;
			}
		}, "Battle Once", "Continuous Story");
		return true;
	}

	public static bool ChooseForParty(BattlePartySetting.IDataProvider provider, BattlePartySetting.Listener listener)
	{
		if (!Active() && listener is LobbyCampaignDungeon && provider != null)
		{
			return ShowChoice(provider.GetEnterDungeon());
		}
		return false;
	}

	public static void BeforeStart(BattlePartySetting party, BattlePartySetting.IDataProvider provider)
	{
		if (!Active())
		{
			return;
		}
		if (provider == null || !IsStory(provider.GetEnterDungeon()))
		{
			Stop();
			return;
		}
		CaptureParty(party, provider);
		if (waitingParty != party)
		{
			lastProgress = Time.realtimeSinceStartup;
		}
		waitingParty = party;
		waitingForEvent = false;
		EnterDungeonData enterDungeon = provider.GetEnterDungeon();
		if (enterDungeon.Chapter == 117 && enterDungeon.OriginalDungeon == 4)
		{
			QueueNext(GameRoot.Instance.CampaignDungeonDataContainer.GetData(7, 7));
		}
		retryAfter = Time.realtimeSinceStartup + 1f;
		autoStarted.SetValue(party, true);
	}

	public static bool EventRaised(bool raised)
	{
		if (raised && Active())
		{
			waitingForEvent = true;
		}
		return raised;
	}

	public static void StopForEntryEvent(AutoBattle battle, bool send, PartyRoomRepeat.ResultType result)
	{
		if (!Active() || !waitingForEvent)
		{
			battle.OnClickStop(send, result);
		}
	}

	public static bool EntryChecked(bool allowed)
	{
		if (!allowed && Active())
		{
			Stop();
		}
		return allowed;
	}

	public static bool CanNext()
	{
		return NextRoute(SingletonAuto<CampaignContext>.instance.EnterDungeon) != null;
	}

	private static CampaignDungeonData NextRoute(EnterDungeonData enter)
	{
		if (!IsStoryRoute(enter) || MainStoryManagement.IsPlayMainStoryMode)
		{
			return null;
		}
		if (enter.Chapter == 9 && enter.OriginalDungeon == 23)
		{
			return null;
		}
		CampaignDungeonData originalDungeonData = enter.OriginalDungeonData;
		if (originalDungeonData == null || originalDungeonData.NextDungeonIndex == originalDungeonData.DungeonIndex)
		{
			return null;
		}
		CampaignDungeonData campaignDungeonData = null;
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		if (enter.Chapter == 50000)
		{
			campaignDungeonData = NextAegina(enter);
		}
		else if (enter.Chapter == 10 && enter.OriginalDungeon >= 29 && enter.OriginalDungeon <= 41)
		{
			campaignDungeonData = ((enter.OriginalDungeon < 41) ? GameRoot.Instance.CampaignDungeonDataContainer.GetData(10, enter.OriginalDungeon + 1) : GameRoot.Instance.CampaignDungeonDataContainer.GetData(11, 1));
		}
		else if (enter.Chapter == 9 && enter.OriginalDungeon == 13)
		{
			campaignDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(9, campaignManager.IsCompleted(9, 51) ? 14 : 51);
		}
		else if (enter.Chapter == 9 && enter.OriginalDungeon == 51)
		{
			campaignDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(9, 14);
		}
		else if (enter.Chapter == 7 && enter.OriginalDungeon == 6)
		{
			campaignDungeonData = ((!campaignManager.IsCompleted(117, 3)) ? GameRoot.Instance.CampaignDungeonDataContainer.GetData(117, 3) : ((!campaignManager.IsCompleted(117, 4)) ? GameRoot.Instance.CampaignDungeonDataContainer.GetData(117, 4) : GameRoot.Instance.CampaignDungeonDataContainer.GetData(7, 7)));
		}
		else if (IsCaveBattle(enter.Chapter, enter.OriginalDungeon))
		{
			campaignDungeonData = ((enter.OriginalDungeon == 3 && !campaignManager.IsCompleted(117, 4)) ? GameRoot.Instance.CampaignDungeonDataContainer.GetData(117, 4) : GameRoot.Instance.CampaignDungeonDataContainer.GetData(7, 7));
		}
		else if (originalDungeonData.NextDungeonIndex > 0)
		{
			campaignDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(enter.Chapter, originalDungeonData.NextDungeonIndex);
		}
		else
		{
			CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(enter.Chapter + 1);
			if (enter.ChapterData == null || enter.ChapterData.Type != ChapterType.WorldMap || data == null || data.Type != ChapterType.WorldMap)
			{
				return null;
			}
			campaignDungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(enter.Chapter + 1, 1);
		}
		if (campaignDungeonData == null || (!IsStoryType(campaignDungeonData.BattleType) && (!IsRequiredSubstory(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex) || campaignDungeonData.BattleType != BattleType.SubStory) && (campaignDungeonData.ChapterIndex != 50000 || campaignDungeonData.DungeonIndex != 11 || campaignDungeonData.BattleType != BattleType.Shakmeh)))
		{
			return null;
		}
		if (!campaignManager.CanGoToChapter(campaignDungeonData.ChapterIndex) || (!campaignManager.CanVisit(campaignDungeonData.ChapterIndex, campaignDungeonData.DungeonIndex) && !IsCaveExit(campaignDungeonData) && !WaitingForAeginaUnlock(campaignDungeonData)))
		{
			return null;
		}
		return campaignDungeonData;
	}

	private static bool IsCaveExit(CampaignDungeonData next)
	{
		if (next != null && next.ChapterIndex == 7 && next.DungeonIndex == 7)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(117, 3);
		}
		return false;
	}

	private static void QueueNext(CampaignDungeonData next)
	{
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		if (next.ChapterIndex == 50000 && next.DungeonIndex == 11)
		{
			next = GameRoot.Instance.CampaignDungeonDataContainer.GetData(50000, 601);
		}
		pendingNext = new EnterDungeonData(next.ChapterIndex, next.DungeonIndex, campaignManager.GetLinkedDungeonIndex(next.ChapterIndex, next.DungeonIndex), NextDifficulty(NShared.DungeonDifficultyType.Normal, next), campaignManager.CanPlayScenario(next.ChapterIndex, next.DungeonIndex) && !campaignManager.IsCompleted(next.ChapterIndex, next.DungeonIndex));
		movingToNext = (openingNextParty = false);
		lastProgress = Time.realtimeSinceStartup;
	}

	public static bool Advance(EndCampaign end)
	{
		if (!Active())
		{
			return false;
		}
		CampaignDungeonData campaignDungeonData = NextRoute(SingletonAuto<CampaignContext>.instance.EnterDungeon);
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		bool flag = IsCaveExit(campaignDungeonData) && !campaignManager.CanVisit(7, 7);
		if (campaignDungeonData == null || (!flag && !WaitingForAeginaUnlock(campaignDungeonData) && !(bool)checkNext.Invoke(end, new object[1] { campaignDungeonData })))
		{
			Stop();
			return true;
		}
		QueueNext(campaignDungeonData);
		LobbyScene2.ReturnState.OpenFlag &= ~LobbyScene2.ReturnStateOpenFlag.OpenNextDungeon;
		end.ReturnToLobby();
		return true;
	}

	private static void ResumeNext()
	{
		if (pendingNext == null || SceneManager.GetActiveScene().name != "Lobby" || GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading || !SceneAutoBehaviour<WorldMapManagement>.isValidInstance || !SceneAutoBehaviour<LobbyCampaignDungeon>.isValidInstance)
		{
			return;
		}
		WorldMapManagement instance = SceneAutoBehaviour<WorldMapManagement>.instance;
		if (instance.IsMapLoading)
		{
			return;
		}
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			GetNewHero2 instance2 = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<GetNewHero2>();
			if (instance2 != null && (instance2.ReservedOpenTask || instance2.IsOpen()))
			{
				return;
			}
		}
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		if (RecoverSkippedClause())
		{
			return;
		}
		if ((IsChapter10Finale(pendingNext.Chapter, pendingNext.OriginalDungeon) || pendingNext.Chapter == 50000) && campaignManager.IsCompleted(pendingNext.Chapter, pendingNext.OriginalDungeon))
		{
			CampaignDungeonData campaignDungeonData = NextRoute(pendingNext);
			if (campaignDungeonData == null)
			{
				Stop();
				return;
			}
			QueueNext(campaignDungeonData);
		}
		if (pendingNext.Chapter == 117 && pendingNext.OriginalDungeon == 4 && campaignManager.IsCompleted(117, 4))
		{
			QueueNext(GameRoot.Instance.CampaignDungeonDataContainer.GetData(7, 7));
		}
		if (IsScarlet2(pendingNext.Chapter, pendingNext.OriginalDungeon) && campaignManager.IsCompleted(pendingNext.Chapter, pendingNext.OriginalDungeon))
		{
			QueueNext(GameRoot.Instance.CampaignDungeonDataContainer.GetData(9, 14));
		}
		if (!campaignManager.CanGoToChapter(pendingNext.Chapter) || GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			Stop();
		}
		else if (!campaignManager.CanVisit(pendingNext.Chapter, MapDungeon(pendingNext)))
		{
			if (!WaitingForAeginaUnlock(pendingNext.OriginalDungeonData))
			{
				if (pendingNext.Chapter == 7 && pendingNext.OriginalDungeon == 7 && campaignManager.IsCompleted(117, 3))
				{
					CompleteSkippedCave();
				}
				else
				{
					Stop();
				}
			}
		}
		else if (!movingToNext)
		{
			movingToNext = true;
			resumeAfter = Time.realtimeSinceStartup + 2.5f;
			lastProgress = Time.realtimeSinceStartup;
			if (IsChapter10Finale(pendingNext.Chapter, pendingNext.OriginalDungeon))
			{
				instance.IsPossibleMove = true;
			}
			instance.MovePlayerToDungeon(pendingNext.Chapter, MapDungeon(pendingNext), DisableTouch: false, warp: true);
		}
		else
		{
			if (NGame2.NAccount.UserCampaignManager.ChapterIndex != pendingNext.Chapter || Time.realtimeSinceStartup < resumeAfter)
			{
				return;
			}
			if (IsRequiredSubstory(pendingNext.Chapter, pendingNext.OriginalDungeon))
			{
				if (!openingNextParty && NGame2.NAccount.UserCampaignManager.DungeonIndex == pendingNext.OriginalDungeon && SceneAutoBehaviour<LobbyManagement>.isValidInstance)
				{
					openingNextParty = true;
					lastProgress = Time.realtimeSinceStartup;
					SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(null);
				}
				return;
			}
			if (pendingNext.Chapter == 117 && pendingNext.OriginalDungeon == 4 && NGame2.NAccount.UserCampaignManager.DungeonIndex == 4 && Helper.HasEventTrigger(117, 4) && SceneAutoBehaviour<LobbyManagement>.isValidInstance)
			{
				QueueNext(GameRoot.Instance.CampaignDungeonDataContainer.GetData(7, 7));
				SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(null);
				return;
			}
			BattlePartySetting[] array = UnityEngine.Object.FindObjectsOfType<BattlePartySetting>();
			foreach (BattlePartySetting battlePartySetting in array)
			{
				if (!battlePartySetting.IsOpen())
				{
					continue;
				}
				BattlePartySetting.IDataProvider dataProvider = (BattlePartySetting.IDataProvider)partyProvider.GetValue(battlePartySetting);
				EnterDungeonData enterDungeonData = dataProvider?.GetEnterDungeon();
				if (enterDungeonData != null && enterDungeonData.Chapter == pendingNext.Chapter && enterDungeonData.OriginalDungeon == pendingNext.OriginalDungeon)
				{
					if (!RestoreParty(battlePartySetting, dataProvider))
					{
						Stop();
						GlobalCenterMessage.Open("Continuous Story stopped: your party needs attention for this stage.");
					}
					else if (!SingletonUpdater<TutorialManager>.instance.IsProgressing && !GlobalAutoBehaviour<PopupManegement>.instance.AnyPopupExist())
					{
						pendingNext = null;
						autoStarted.SetValue(battlePartySetting, true);
						battlePartySetting.OnClickStartBattle();
					}
					return;
				}
			}
			if (!openingNextParty)
			{
				openingNextParty = true;
				lastProgress = Time.realtimeSinceStartup;
				SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenPartySetting(pendingNext);
			}
		}
	}

	private static bool HasScriptedParty(BattlePartySetting.IDataProvider provider)
	{
		EnterDungeonData enterDungeon = provider.GetEnterDungeon();
		if (IsRequiredSubstory(enterDungeon.Chapter, enterDungeon.OriginalDungeon))
		{
			return true;
		}
		int[] forcedPartyMemberHeroIndices = provider.GetForcedPartyMemberHeroIndices();
		if (forcedPartyMemberHeroIndices == null || forcedPartyMemberHeroIndices.Length == 0)
		{
			return Helper.CheckFixedSetHero(enterDungeon.Chapter, enterDungeon.LinkedDungeon, enterDungeon.Difficulty, enterDungeon.IsScenario);
		}
		return true;
	}

	private static void CaptureParty(BattlePartySetting party, BattlePartySetting.IDataProvider provider)
	{
		if (provider != null && !HasScriptedParty(provider))
		{
			Squard squard = (Squard)currentSquard.GetValue(party);
			if (squard != null && squard.GetMainSquardMyHeroIndexList().Count != 0)
			{
				storyMainParty = squard.GetMainSquardMyHeroIndexList().ToArray();
				List<int> subSquardMyHeroIndexList = squard.GetSubSquardMyHeroIndexList();
				storySubParty = ((subSquardMyHeroIndexList == null) ? new int[0] : subSquardMyHeroIndexList.ToArray());
			}
		}
	}

	private static bool SameParty(List<int> current, int[] selected)
	{
		if (current == null)
		{
			return selected.Length == 0;
		}
		if (current.Count != selected.Length)
		{
			return false;
		}
		foreach (int item in selected)
		{
			if (!current.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	private static bool RestoreParty(BattlePartySetting party, BattlePartySetting.IDataProvider provider)
	{
		if (storyMainParty == null || HasScriptedParty(provider))
		{
			return true;
		}
		Squard squard = (Squard)currentSquard.GetValue(party);
		if (squard == null)
		{
			return false;
		}
		EnterDungeonData enterDungeon = provider.GetEnterDungeon();
		if (enterDungeon.Chapter == 50000 && enterDungeon.OriginalDungeon == 601 && enterDungeon.IsScenario)
		{
			return RestoreAeginaTeams(party, squard);
		}
		return SetParty(party, squard, storyMainParty, storySubParty);
	}

	private static bool SetParty(BattlePartySetting party, Squard squad, int[] mainParty, int[] subParty)
	{
		if (mainParty.Length > squad.GetMainSquardSlotCount() || subParty.Length > squad.GetSubSquardSlotCount())
		{
			return false;
		}
		if (SameParty(squad.GetMainSquardMyHeroIndexList(), mainParty) && SameParty(squad.GetSubSquardMyHeroIndexList(), subParty))
		{
			return true;
		}
		squad.SwichingSquard(isOnMainSquard: true);
		int[] array = squad.GetMainSquardMyHeroIndexList().ToArray();
		foreach (int num in array)
		{
			HeroPortraitComponent deckHeroPortrait = party.GetDeckHeroPortrait(num, 0L);
			if (deckHeroPortrait == null)
			{
				return false;
			}
			((HeroPortraitComponent.Listener)party).OnClick(deckHeroPortrait);
			if (squad.GetMainSquardMyHeroIndexList().Contains(num))
			{
				return false;
			}
		}
		List<int> subSquardMyHeroIndexList = squad.GetSubSquardMyHeroIndexList();
		if (subSquardMyHeroIndexList != null && subSquardMyHeroIndexList.Count > 0)
		{
			squad.SwichingSquard(isOnMainSquard: false);
			array = subSquardMyHeroIndexList.ToArray();
			foreach (int num2 in array)
			{
				HeroPortraitComponent deckHeroPortrait2 = party.GetDeckHeroPortrait(num2, 0L);
				if (deckHeroPortrait2 == null)
				{
					return false;
				}
				((HeroPortraitComponent.Listener)party).OnClick(deckHeroPortrait2);
				if (squad.GetSubSquardMyHeroIndexList().Contains(num2))
				{
					return false;
				}
			}
		}
		squad.SwichingSquard(isOnMainSquard: true);
		array = mainParty;
		foreach (int num3 in array)
		{
			HeroPortraitComponent deckHeroPortrait3 = party.GetDeckHeroPortrait(num3, 0L);
			if (deckHeroPortrait3 == null)
			{
				return false;
			}
			((HeroPortraitComponent.Listener)party).OnClick(deckHeroPortrait3);
			if (!squad.GetMainSquardMyHeroIndexList().Contains(num3))
			{
				return false;
			}
		}
		if (subParty.Length != 0)
		{
			squad.SwichingSquard(isOnMainSquard: false);
			array = subParty;
			foreach (int num4 in array)
			{
				HeroPortraitComponent deckHeroPortrait4 = party.GetDeckHeroPortrait(num4, 0L);
				if (deckHeroPortrait4 == null)
				{
					return false;
				}
				((HeroPortraitComponent.Listener)party).OnClick(deckHeroPortrait4);
				if (!squad.GetSubSquardMyHeroIndexList().Contains(num4))
				{
					return false;
				}
			}
			squad.SwichingSquard(isOnMainSquard: true);
		}
		if (SameParty(squad.GetMainSquardMyHeroIndexList(), mainParty))
		{
			return SameParty(squad.GetSubSquardMyHeroIndexList(), subParty);
		}
		return false;
	}

	private static bool RestoreAeginaTeams(BattlePartySetting party, Squard squad)
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		heroInfoList.Sort(delegate(CachedHeroInfo left, CachedHeroInfo right)
		{
			int num2 = right.Level.CompareTo(left.Level);
			return (num2 == 0) ? left.HeroIndex.CompareTo(right.HeroIndex) : num2;
		});
		List<int> list = new List<int>(storyMainParty);
		list.AddRange(storySubParty);
		foreach (CachedHeroInfo item in heroInfoList)
		{
			if (!list.Contains(item.HeroIndex))
			{
				list.Add(item.HeroIndex);
			}
		}
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		foreach (int item2 in list)
		{
			HeroPortraitComponent deckHeroPortrait = party.GetDeckHeroPortrait(item2, 0L);
			if (!(deckHeroPortrait == null) && deckHeroPortrait.CreatureData != null && deckHeroPortrait.IsControllerable && !deckHeroPortrait.IsBanned && !deckHeroPortrait.IsDispatched && !party.IsBannedHero(deckHeroPortrait))
			{
				CreatureDPSType dPSType = deckHeroPortrait.CreatureData.GetDPSType();
				List<int> list4 = ((dPSType == CreatureDPSType.Melee) ? list2 : list3);
				int num = ((dPSType == CreatureDPSType.Melee) ? squad.GetMainSquardSlotCount() : squad.GetSubSquardSlotCount());
				if ((dPSType == CreatureDPSType.Melee || dPSType == CreatureDPSType.Range) && list4.Count < num && !list4.Contains(item2))
				{
					list4.Add(item2);
				}
			}
		}
		if (!SetParty(party, squad, list2.ToArray(), list3.ToArray()))
		{
			return false;
		}
		if (list2.Count > 0)
		{
			return list3.Count > 0;
		}
		return false;
	}

	private static void CompleteSkippedCave()
	{
		AccountManager account = GlobalAutoBehaviour<AccountManager>.instance;
		if (caveCompletionRequested || !account.IsTutorialSkip || !account.CampaignManager.IsCompleted(117, 3) || !account.CampaignManager.IsCompleted(117, 4) || account.TutorialManager.IsComplete(70001))
		{
			return;
		}
		caveCompletionRequested = true;
		EnterDungeonData continuation = pendingNext;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(new NShared.CompleteTutorial.Request
		{
			TutorialIndex = 70001
		}, delegate(NShared.CompleteTutorial.Response response)
		{
			if (GlobalAutoBehaviour<AccountManager>.isValidInstance && (object)account == GlobalAutoBehaviour<AccountManager>.instance)
			{
				bool flag = response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.CompleteTutorial.ResultType.Success && response.Info != null && response.Info.TutorialIndex == 70001;
				if (flag)
				{
					account.TutorialManager.Apply(response.Info);
					account.CampaignManager.Apply(response.DungeonInfos);
				}
				if (Active() && pendingNext == continuation)
				{
					if (flag)
					{
						lastProgress = Time.realtimeSinceStartup;
					}
					else
					{
						Stop();
					}
				}
			}
		});
	}

	private static bool RecoverSkippedClause()
	{
		if (Time.realtimeSinceStartup < clauseResumeAfter)
		{
			return true;
		}
		AccountManager account = GlobalAutoBehaviour<AccountManager>.instance;
		if (!account.IsTutorialSkip || !account.CampaignManager.IsCompleted(1, 20) || account.TutorialManager.IsComplete(20002))
		{
			return false;
		}
		if (clauseCompletionRequested)
		{
			return true;
		}
		int index = (account.TutorialManager.IsComplete(20001) ? 20002 : 20001);
		clauseCompletionRequested = true;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(new NShared.CompleteTutorial.Request
		{
			TutorialIndex = index
		}, delegate(NShared.CompleteTutorial.Response response)
		{
			clauseCompletionRequested = false;
			if (GlobalAutoBehaviour<AccountManager>.isValidInstance && (object)account == GlobalAutoBehaviour<AccountManager>.instance)
			{
				if (response == null || response.BaseResult != BaseResultType.Success || response.Result != NShared.CompleteTutorial.ResultType.Success || response.Info == null || response.Info.TutorialIndex != index)
				{
					if (Active())
					{
						Stop();
					}
				}
				else
				{
					account.TutorialManager.Apply(response.Info);
					account.UserManager.Apply(response.CurrencyResults);
					account.CampaignManager.Apply(response.DungeonInfos);
					account.ItemManager.Apply(response.ItemResults);
					account.EquipItemManager.Apply(response.EquipItemResults);
					account.ApplyAddHeroForTutorial(response.HeroInfos, response.TeamExpResultInfos, reserveWindow: false);
					account.UserManager.Apply(response.StaminaResultInfos);
					clauseResumeAfter = Time.realtimeSinceStartup + 0.5f;
					if (Active())
					{
						lastProgress = Time.realtimeSinceStartup;
					}
				}
			}
		});
		return true;
	}

	public static bool LowerDifficultyCleared(NGame2.NAccount.UserCampaignManager manager, int chapter, int dungeon, NShared.DungeonDifficultyType difficulty)
	{
		if (!Active() || difficulty != NShared.DungeonDifficultyType.Easy)
		{
			return manager.IsCompleted(chapter, dungeon, difficulty);
		}
		return true;
	}

	private static bool AcknowledgeBattleTutorial(TutorialManager tutorial, TutorialWindow window)
	{
		if (SceneManager.GetActiveScene().name != "Battle2" || !(tutorial.CurrentTutorialRuntime is WaitTouch) || window == null || !window.gameObject.activeInHierarchy)
		{
			return false;
		}
		TutorialWindow.ActionInfo firstTouchActionInfo = window.GetFirstTouchActionInfo();
		if (firstTouchActionInfo == null || firstTouchActionInfo.Action == null || !firstTouchActionInfo.Oneshot || firstTouchActionInfo.IsLongTouch || firstTouchActionInfo.ProcessTouch || firstTouchActionInfo.ProcessBack || firstTouchActionInfo.TargetGo != null)
		{
			return false;
		}
		window.ProcessTouchAction(firstTouchActionInfo);
		return true;
	}

	private static bool AcknowledgeStoryNarration(CutSceneManager cutscene, bool hideOnly = true)
	{
		ScriptGroup currentScriptGroup = cutscene.CurrentScriptGroup;
		CutSceneForeground cutSceneForeground = cutscene.CutSceneForeground;
		if (currentScriptGroup == null || (hideOnly && currentScriptGroup.SkipType != ScriptGroup.ESkipType.StopThisAndHide) || currentScriptGroup.TouchType != ScriptGroup.ETouchType.PassToScript || cutSceneForeground == null || !cutSceneForeground.IsOpen() || !cutSceneForeground.gameObject.activeInHierarchy)
		{
			return false;
		}
		ForegroundTextList foregroundTextList = null;
		foreach (ScriptHolder script in currentScriptGroup.ScriptList)
		{
			IScript value = script.Value;
			if (value is ForegroundTextList)
			{
				if (foregroundTextList != null)
				{
					return false;
				}
				foregroundTextList = (ForegroundTextList)value;
			}
			else if (value != null && !value.IsEnd && value is IClickScript)
			{
				return false;
			}
		}
		if (foregroundTextList == null)
		{
			return false;
		}
		int num = (int)narrationIndex.GetValue(foregroundTextList);
		cutscene.OnClickWindow();
		if (!currentScriptGroup.IsEnd)
		{
			return num != (int)narrationIndex.GetValue(foregroundTextList);
		}
		return true;
	}

	private static bool AcknowledgeStoryDialogue(CutSceneManager cutscene)
	{
		ScriptGroup currentScriptGroup = cutscene.CurrentScriptGroup;
		CutSceneWindow cutSceneWindow = cutscene.CutSceneWindow;
		if (currentScriptGroup == null || currentScriptGroup.SkipType != ScriptGroup.ESkipType.StopThisAndHide || cutSceneWindow == null || !cutSceneWindow.IsOpen() || !cutSceneWindow.gameObject.activeInHierarchy || (currentScriptGroup.TouchType != ScriptGroup.ETouchType.Touch && currentScriptGroup.TouchType != ScriptGroup.ETouchType.TouchAndTouch && currentScriptGroup.TouchType != ScriptGroup.ETouchType.TimeoutOrTouch))
		{
			return false;
		}
		foreach (ScriptHolder script in currentScriptGroup.ScriptList)
		{
			if (script.Value is DisplayText)
			{
				cutscene.OnClickWindow();
				return currentScriptGroup.IsEnd;
			}
		}
		return false;
	}

	private static bool AcknowledgeForegroundCard(CutSceneManager cutscene)
	{
		ScriptGroup currentScriptGroup = cutscene.CurrentScriptGroup;
		CutSceneForeground cutSceneForeground = cutscene.CutSceneForeground;
		if (currentScriptGroup == null || currentScriptGroup.SkipType != ScriptGroup.ESkipType.StopThisAndHide || currentScriptGroup.TouchType != ScriptGroup.ETouchType.Touch || cutSceneForeground == null || !cutSceneForeground.IsOpen() || !cutSceneForeground.gameObject.activeInHierarchy)
		{
			return false;
		}
		bool flag = false;
		foreach (ScriptHolder script in currentScriptGroup.ScriptList)
		{
			IScript value = script.Value;
			if (value is ForegroundText)
			{
				flag = true;
			}
			else if (value != null && !value.IsEnd && value is IClickScript)
			{
				return false;
			}
		}
		if (!flag)
		{
			return false;
		}
		cutscene.OnClickWindow();
		return currentScriptGroup.IsEnd;
	}

	private static bool AcknowledgeCampaignTouch(CutSceneManager cutscene)
	{
		string name = SceneManager.GetActiveScene().name;
		if (name != "Lobby" && name != "Battle2")
		{
			return false;
		}
		EnterDungeonData enterDungeon = pendingNext;
		if (enterDungeon == null && name == "Battle2")
		{
			enterDungeon = SingletonAuto<CampaignContext>.instance.EnterDungeon;
		}
		if (enterDungeon == null || (!IsScarlet2(enterDungeon.Chapter, enterDungeon.OriginalDungeon) && enterDungeon.Chapter != 10 && !IsAeginaStage(enterDungeon.Chapter, enterDungeon.OriginalDungeon)))
		{
			return false;
		}
		ScriptGroup currentScriptGroup = cutscene.CurrentScriptGroup;
		if (currentScriptGroup == null)
		{
			return false;
		}
		if (currentScriptGroup.TouchType == ScriptGroup.ETouchType.PassToScript)
		{
			return AcknowledgeStoryNarration(cutscene, hideOnly: false);
		}
		if (currentScriptGroup.TouchType != ScriptGroup.ETouchType.Touch && currentScriptGroup.TouchType != ScriptGroup.ETouchType.TouchAndTouch && currentScriptGroup.TouchType != ScriptGroup.ETouchType.TimeoutOrTouch)
		{
			return false;
		}
		CutSceneWindow cutSceneWindow = cutscene.CutSceneWindow;
		CutSceneForeground cutSceneForeground = cutscene.CutSceneForeground;
		if ((!(cutSceneWindow != null) || !cutSceneWindow.IsOpen() || !cutSceneWindow.gameObject.activeInHierarchy) && (!(cutSceneForeground != null) || !cutSceneForeground.IsOpen() || !cutSceneForeground.gameObject.activeInHierarchy))
		{
			return false;
		}
		foreach (ScriptHolder script in currentScriptGroup.ScriptList)
		{
			if (script.Value != null && !script.Value.IsEnd && script.Value is IClickScript)
			{
				return false;
			}
		}
		cutscene.OnClickWindow();
		return currentScriptGroup.IsEnd;
	}

	private static void AcknowledgeStoryReward()
	{
		if (pendingNext == null || !openingNextParty || !IsStoryEvent(pendingNext.Chapter, pendingNext.OriginalDungeon) || SceneManager.GetActiveScene().name != "Lobby" || !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(pendingNext.Chapter, pendingNext.OriginalDungeon))
		{
			return;
		}
		BaseRewardPopup[] array = UnityEngine.Object.FindObjectsOfType<BaseRewardPopup>();
		foreach (BaseRewardPopup baseRewardPopup in array)
		{
			Action action = (Action)rewardCallback.GetValue(baseRewardPopup);
			if (baseRewardPopup.IsOpen() && action != null && !(action.Method.DeclaringType == null) && action.Method.DeclaringType.FullName.StartsWith("NGame2.NSubStory.SubStoryStarter"))
			{
				baseRewardPopup.OnClickBackground();
			}
		}
	}

	private static bool AcknowledgeAeginaSummary(TutorialManager tutorial)
	{
		if (pendingNext == null || pendingNext.Chapter != 50000 || tutorial.CurrentTutorialData == null || tutorial.CurrentTutorialData.Command != "SkipEnd" || SceneManager.GetActiveScene().name != "Lobby")
		{
			return false;
		}
		TutorialSkipPopup[] array = UnityEngine.Object.FindObjectsOfType<TutorialSkipPopup>();
		foreach (TutorialSkipPopup tutorialSkipPopup in array)
		{
			if (tutorialSkipPopup.IsOpen())
			{
				tutorialSkipPopup.Close();
				return true;
			}
		}
		return false;
	}

	private static bool WaitingForAeginaReward()
	{
		if (waitingParty == null || !waitingParty.IsOpen())
		{
			return false;
		}
		EnterDungeonData enterDungeonData = ((BattlePartySetting.IDataProvider)partyProvider.GetValue(waitingParty))?.GetEnterDungeon();
		if (enterDungeonData == null || enterDungeonData.Chapter != 50000 || enterDungeonData.OriginalDungeon != 601 || !enterDungeonData.IsScenario || GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(50000, 601))
		{
			return false;
		}
		PreSelectRewardPopup[] array = UnityEngine.Object.FindObjectsOfType<PreSelectRewardPopup>();
		foreach (PreSelectRewardPopup preSelectRewardPopup in array)
		{
			SelectRewardData selectRewardData = (SelectRewardData)rewardData.GetValue(preSelectRewardPopup);
			if (preSelectRewardPopup.IsOpen() && selectRewardData != null && selectRewardData.ChapterIndex == 50000 && selectRewardData.DungeonIndex == 601 && selectRewardData.SelectCount == 1)
			{
				return true;
			}
		}
		return false;
	}

	private static void PlayScriptedParty()
	{
		if (!ScriptedAutoSkills())
		{
			return;
		}
		EnterDungeonData enterDungeon = SingletonAuto<CampaignContext>.instance.EnterDungeon;
		if (enterDungeon == null || enterDungeon.Chapter != 10 || enterDungeon.LinkedDungeonData == null || !SingletonAuto<BattleFacade>.isValidInstance || Time.realtimeSinceStartup < nextScriptedSkill)
		{
			return;
		}
		bool flag = enterDungeon.OriginalDungeon == 23 && enterDungeon.IsScenario && enterDungeon.LinkedDungeonData.ScenarioBattleDefine == "Crow_Chapter10";
		bool flag2 = enterDungeon.OriginalDungeon == 29 && enterDungeon.IsScenario && enterDungeon.LinkedDungeonData.ScenarioBattleDefine == "Clause_Chapter10";
		bool flag3 = (enterDungeon.OriginalDungeon == 36 || enterDungeon.OriginalDungeon == 38) && enterDungeon.LinkedDungeonData.BattleType == BattleType.SubStory && enterDungeon.LinkedDungeonData.BattleDefineCode == (HashString)"Campaign_SubStory10_2";
		if (!flag && !flag2 && !flag3)
		{
			return;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache == null || !battleCache.IsBattleStarted || battleCache.IsBattleEnded || !battleCache.IsBattleEnabled)
		{
			return;
		}
		List<NormalSkillComponent> list = new List<NormalSkillComponent>();
		PartySlotComponent2[] array = UnityEngine.Object.FindObjectsOfType<PartySlotComponent2>();
		foreach (PartySlotComponent2 partySlotComponent in array)
		{
			BattleFacade.CreatureCacheData creatureCache = partySlotComponent.CreatureCache;
			if (!partySlotComponent.gameObject.activeInHierarchy || creatureCache == null || creatureCache.Uid == 0 || creatureCache.IsDeadOrResting() || creatureCache.CreateInfo == null || creatureCache.CreateInfo.HeroInfo == null || (flag && creatureCache.CreateInfo.HeroInfo.HeroIndex != 52) || creatureCache.Player == null || !creatureCache.Player.IsMe)
			{
				continue;
			}
			NormalSkillComponent[] componentsInChildren = partySlotComponent.GetComponentsInChildren<NormalSkillComponent>();
			foreach (NormalSkillComponent normalSkillComponent in componentsInChildren)
			{
				if (normalSkillComponent.gameObject.activeInHierarchy && (!flag || normalSkillComponent.SkillSlotIndex == 3) && !normalSkillComponent.SkillInfo.IsLocked && creatureCache.ReservedSkillSlotIndex != normalSkillComponent.SkillSlotIndex && creatureCache.UsingSkillSlotIndex != normalSkillComponent.SkillSlotIndex)
				{
					SkillUseResult skillUseResult = (SkillUseResult)SprkStoryAuto.skillUseResult.Invoke(normalSkillComponent, null);
					if (skillUseResult == SkillUseResult.Success || skillUseResult == SkillUseResult.EnhancedSuccess)
					{
						list.Add(normalSkillComponent);
					}
				}
			}
		}
		if (list.Count != 0)
		{
			list[scriptedSkillCursor++ % list.Count].OnClickSkill();
			nextScriptedSkill = Time.realtimeSinceStartup + 0.5f;
		}
	}

	public static void Tick()
	{
		if (FinishPendingStop() || Time.realtimeSinceStartup < nextTick)
		{
			return;
		}
		nextTick = Time.realtimeSinceStartup + 0.25f;
		if (!Active())
		{
			if (!continuousStory || !(Time.realtimeSinceStartup - lastProgress < 3f))
			{
				Cancelled();
			}
			return;
		}
		try
		{
			bool flag = false;
			TutorialManager instance = SingletonUpdater<TutorialManager>.instance;
			if (instance.IsProgressing)
			{
				flag = true;
				if (lastTutorialStep != instance.CurrentTutorialData)
				{
					lastTutorialStep = instance.CurrentTutorialData;
					lastProgress = Time.realtimeSinceStartup;
				}
				TutorialWindow instance2 = TutorialWindow.instance;
				if (instance2 != null && instance2.NeedShowSkipButton())
				{
					instance2.OnClickSkip();
				}
				else if (AcknowledgeAeginaSummary(instance))
				{
					lastProgress = Time.realtimeSinceStartup;
				}
				else if (AcknowledgeBattleTutorial(instance, instance2))
				{
					lastProgress = Time.realtimeSinceStartup;
				}
			}
			if (pendingNext != null && IsStoryEvent(pendingNext.Chapter, pendingNext.OriginalDungeon) && SingletonAuto<SubStoryEventManager>.isValidInstance)
			{
				SubStoryEventManager instance3 = SingletonAuto<SubStoryEventManager>.instance;
				if (instance3.IsProgressing)
				{
					flag = true;
					if (lastSubstoryStep != instance3.CurrentTutorialData)
					{
						lastSubstoryStep = instance3.CurrentTutorialData;
						lastProgress = Time.realtimeSinceStartup;
					}
					TutorialWindow instance4 = TutorialWindow.instance;
					if (instance4 != null && instance4.NeedShowSkipButton())
					{
						instance4.OnClickSkip();
					}
				}
			}
			CutSceneManager[] array = UnityEngine.Object.FindObjectsOfType<CutSceneManager>();
			foreach (CutSceneManager cutSceneManager in array)
			{
				if (!cutSceneManager.IsPlaying && !cutSceneManager.IsLoading)
				{
					continue;
				}
				flag = true;
				if (!cutSceneManager.IsPlaying || !cutSceneManager.IsShowing())
				{
					continue;
				}
				if (!(bool)cutsceneSkippable.GetValue(cutSceneManager))
				{
					if (AcknowledgeCampaignTouch(cutSceneManager))
					{
						lastProgress = Time.realtimeSinceStartup;
					}
					continue;
				}
				CutSceneMenu cutSceneMenu = cutSceneManager.CutSceneMenu;
				if (cutSceneMenu == null || !cutSceneMenu.IsOpen() || cutSceneMenu.SkipButton == null)
				{
					continue;
				}
				if (!cutSceneMenu.SkipButton.gameObject.activeInHierarchy)
				{
					if (AcknowledgeStoryNarration(cutSceneManager) || AcknowledgeStoryDialogue(cutSceneManager) || AcknowledgeForegroundCard(cutSceneManager) || AcknowledgeCampaignTouch(cutSceneManager))
					{
						lastProgress = Time.realtimeSinceStartup;
					}
				}
				else if (cutSceneMenu.SkipButton.isEnabled)
				{
					int currentScript = cutSceneManager.CurrentScript;
					cutSceneMenu.OnClickSkip();
					if (!cutSceneManager.IsPlaying || cutSceneManager.CurrentScript != currentScript)
					{
						lastProgress = Time.realtimeSinceStartup;
					}
				}
			}
			bool flag2 = SceneManager.GetActiveScene().name == "Battle2";
			bool flag3 = false;
			NGame2.NUI.NWindow.NBattle.EndBattleReward[] array2 = UnityEngine.Object.FindObjectsOfType<NGame2.NUI.NWindow.NBattle.EndBattleReward>();
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i].IsOpen())
				{
					flag3 = true;
					break;
				}
			}
			if (flag2 && !flag && !flag3 && !GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
			{
				PlayScriptedParty();
				lastProgress = Time.realtimeSinceStartup;
				waitingParty = null;
				waitingForEvent = false;
			}
			if (waitingForEvent && Time.realtimeSinceStartup >= retryAfter && !flag && !GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading && waitingParty != null && waitingParty.IsOpen() && !GlobalAutoBehaviour<PopupManegement>.instance.AnyPopupExist())
			{
				waitingForEvent = false;
				waitingParty.TryStartBattle(new BattlePartySetting.ExtraOptionData
				{
					AutoNext = true
				});
			}
			if (!flag)
			{
				if (WaitingForAeginaReward())
				{
					lastProgress = Time.realtimeSinceStartup;
				}
				AcknowledgeStoryReward();
			}
			if (!flag && !GlobalAutoBehaviour<PopupManegement>.instance.AnyPopupExist())
			{
				ResumeNext();
			}
			if (Time.realtimeSinceStartup - lastProgress > 90f)
			{
				Stop();
				GlobalCenterMessage.Open("Continuous Story stopped: an event or entry requires your attention.");
			}
		}
		catch (Exception ex)
		{
			Stop();
			Debug.LogError("[SPRK Story Auto] Stopped: " + ex);
		}
	}

	public static void TrackEntryOption(BattlePartySetting.ExtraOptionData option)
	{
		if (continuousStory)
		{
			pendingEntryOption = option;
		}
	}

	public static bool DeferOpeningStop(AutoNextBattle window)
	{
		if (window == null)
		{
			return false;
		}
		if ((object)pendingStop == window)
		{
			return true;
		}
		if (!continuousStory || (!window.ReservedOpenTask && window.WindowState == BaseWindow.EState.OpenedShow))
		{
			return false;
		}
		pendingStop = window;
		stopReadyFrame = -1;
		repeatEnabled.SetValue(window, false);
		UIHelper.EnableButton(window.StopButton, enable: false);
		Cancelled();
		return true;
	}

	private static bool FinishPendingStop()
	{
		if (pendingStop == null)
		{
			pendingStop = null;
			return false;
		}
		if (pendingStop.ReservedOpenTask || pendingStop.WindowState == BaseWindow.EState.AnimateShow || pendingStop.WindowState == BaseWindow.EState.OpenedHide)
		{
			return true;
		}
		if (stopReadyFrame < 0)
		{
			stopReadyFrame = Time.frameCount;
			return true;
		}
		if (Time.frameCount <= stopReadyFrame + 1)
		{
			return true;
		}
		AutoNextBattle autoNextBattle = pendingStop;
		pendingStop = null;
		autoNextBattle.OnClickStopWithoutMessage();
		return true;
	}

	public static void Stop()
	{
		bool flag = continuousStory || Active();
		if (!flag || !DeferOpeningStop(AutoNextBattle.instance))
		{
			Cancelled();
			if (flag)
			{
				AutoNextBattle.instance.OnClickStopWithoutMessage();
			}
		}
	}

	public static void Cancelled()
	{
		continuousStory = false;
		if (pendingEntryOption != null)
		{
			pendingEntryOption.AutoNext = false;
		}
		pendingEntryOption = null;
		if (waitingParty != null && waitingParty.IsOpen())
		{
			autoStarted.SetValue(waitingParty, false);
			autoElapsed.SetValue(waitingParty, 0f);
		}
		waitingParty = null;
		waitingForEvent = false;
		pendingNext = null;
		movingToNext = (openingNextParty = false);
		lastTutorialStep = null;
		lastSubstoryStep = null;
		caveCompletionRequested = false;
		nextScriptedSkill = 0f;
		scriptedSkillCursor = 0;
		storyMainParty = (storySubParty = null);
		if (runner != null)
		{
			runner.gameObject.SetActive(pendingStop != null);
		}
	}
}
