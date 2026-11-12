using System;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NCutScene;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NTutorial;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class MainStoryManagement : BaseManagement<MainStoryManagement>, BattlePartySetting.Listener, ShakmehPartySetting.Listener, MainStoryChapterList.Listener, MainStoryList.Listener, MainStoryPopup.Listener
{
	private MainStoryChapterData currentChapterData;

	private MainStoryData currentStoryData;

	public static bool IsPlayMainStoryMode { get; set; }

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<ShakmehPartySetting>("Resources/Prefab2/Window/ShakmehDungeon/ShakmehPartySetting");
		AddInstance<MainStoryChapterList>("Resources/Prefab2/Window/MainStory/MainStoryChapterList");
		AddInstance<MainStoryList>("Resources/Prefab2/Window/MainStory/MainStoryList");
		AddInstance<MainStoryPopup>("Resources/Prefab2/Window/MainStory/MainStoryPopup");
	}

	public void OpenChapterList(int focusingChapterIndex)
	{
		GetInstance<MainStoryChapterList>().Open(this, focusingChapterIndex);
	}

	public void OpenStoryList(MainStoryChapterData data, bool showList = true)
	{
		currentChapterData = data;
		MainStoryList mainStoryList = GetInstance<MainStoryList>();
		mainStoryList.Open(this, data, 0);
		mainStoryList.ShowStoryList(showList);
	}

	public void OpenPartySetting(int chapterIndex, int dungeonIndex)
	{
		EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, NShared.DungeonDifficultyType.Normal, isScenario: true);
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex).BattleType == BattleType.Shakmeh)
		{
			OpenShakmehPartySetting(enterDungeon);
		}
		else
		{
			OpenBattlePartySetting(enterDungeon);
		}
	}

	private void OpenShakmehPartySetting(EnterDungeonData enterDungeon)
	{
		ShakmehDungeonData dataByDungeonIndex = GameRoot.Instance.ShakmehDungeonDataContainer.GetDataByDungeonIndex(enterDungeon.OriginalDungeon);
		if (dataByDungeonIndex == null)
		{
			VespaDebug.LogError("Missing shakmehDungeonData. chapter=" + enterDungeon.Chapter + " dungeon=" + enterDungeon.OriginalDungeon);
			return;
		}
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.RemoveShakmehBuffInfo();
		BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(dataByDungeonIndex.BanRuleIndex);
		string localStorageKey = enterDungeon.LinkedDungeonData.LocalStorageKey;
		object obj = Battle.START_BATTLE;
		BanRuleData banRuleData = data;
		object obj2 = obj;
		int num = 0;
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(localStorageKey, StaminaType.Empty, num, (Enum)obj2, enterDungeon, banRuleData);
		GetInstance<ShakmehPartySetting>().Open(ShakmehBossType.Final, this, dataProvider);
	}

	private void OpenBattlePartySetting(EnterDungeonData enterDungeon)
	{
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(enterDungeon.LinkedDungeonData.LocalStorageKey, StaminaType.Chicken, 0, Battle.START_BATTLE, enterDungeon);
		GetInstance<BattlePartySetting>().Open(this, dataProvider);
	}

	public void OpenSubStory(int chapterIndex, int dungeonIndex)
	{
		if (GameRoot.Instance.SubStoryDataContainer.GetData(chapterIndex, dungeonIndex) == null)
		{
			VespaDebug.LogError("Missing subStoryData index. chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
			return;
		}
		Action onCallback = delegate
		{
			LobbyScene2.ListReturnState.Add(new MainStoryReturnState(null, currentChapterData, currentStoryData));
		};
		SceneAutoBehaviour<SubStoryManagement>.instance.StartSubStoryBattle(chapterIndex, dungeonIndex, onCallback);
	}

	public void OpenMainStoryPopup()
	{
		if (currentStoryData != null)
		{
			int chapterIndex = currentStoryData.ChapterIndex;
			int storyNo = currentStoryData.StoryNo + 1;
			MainStoryData data = GameRoot.Instance.MainStoryDataContainer.GetData(chapterIndex, storyNo);
			GetInstance<MainStoryPopup>().Open(this, data, delegate
			{
				GetInstance<MainStoryList>().ShowStoryList(show: true);
			});
		}
	}

	public void OnClickStory(MainStoryData data)
	{
		if (data == null)
		{
			return;
		}
		currentStoryData = data;
		EventTriggerCommand command = currentStoryData.Command;
		switch (command)
		{
		case EventTriggerCommand.StartTutorial:
		{
			int valueInt5 = ArgParser.GetValueInt(data.Argument, 0);
			if (valueInt5 == 0)
			{
				VespaDebug.LogError("Missing tutorial index. chapter=" + data.ChapterIndex + " story=" + data.StoryNo);
				break;
			}
			bool isVideo = ArgParser.GetValueBool(data.Argument, 1);
			GetInstance<MainStoryList>().ShowStoryList(show: false);
			SingletonUpdater<TutorialManager>.instance.StartTutorial(valueInt5, delegate
			{
				if (isVideo)
				{
					SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(delegate
					{
						ReturnToLastStory(currentChapterData, currentStoryData);
					});
				}
			}, delegate
			{
				if (!isVideo)
				{
					LobbyScene2.ListReturnState.Add(new MainStoryReturnState(null, currentChapterData, currentStoryData));
				}
			});
			break;
		}
		case EventTriggerCommand.StartCutScene:
		{
			string valueString = ArgParser.GetValueString(data.Argument, 0);
			if (String_Extension_Method.IsNullOrEmpty(valueString))
			{
				VespaDebug.LogError("Missing cutscene path. chapter=" + data.ChapterIndex + " story=" + data.StoryNo);
			}
			GetInstance<MainStoryList>().ShowStoryList(show: false);
			SceneAutoBehaviour<CutSceneManager>.instance.Open(valueString, canSkip: true, delegate
			{
				OpenMainStoryPopup();
			});
			break;
		}
		case EventTriggerCommand.StartDungeon:
		{
			int valueInt3 = ArgParser.GetValueInt(data.Argument, 0);
			int valueInt4 = ArgParser.GetValueInt(data.Argument, 1);
			OpenPartySetting(valueInt3, valueInt4);
			break;
		}
		case EventTriggerCommand.StartSubStory:
		{
			int valueInt = ArgParser.GetValueInt(data.Argument, 0);
			int valueInt2 = ArgParser.GetValueInt(data.Argument, 1);
			OpenSubStory(valueInt, valueInt2);
			break;
		}
		default:
			VespaDebug.LogError("not implemented command: " + command.ToString() + " chapter=" + data.ChapterIndex + " story=" + data.StoryNo);
			break;
		}
	}

	public void ReturnToLastStory(MainStoryChapterData lastChapterData, MainStoryData lastStoryData)
	{
		currentChapterData = lastChapterData;
		currentStoryData = lastStoryData;
		IsPlayMainStoryMode = false;
		OpenChapterList(0);
		OpenStoryList(lastChapterData, showList: false);
		OpenMainStoryPopup();
	}

	public bool IsOpenChapterStory(int StoryIndex)
	{
		MainStoryChapterData data = GameRoot.Instance.MainStoryChapterDataContainer.GetData(StoryIndex);
		if (data == null)
		{
			return false;
		}
		MainStoryData firstStoryData = GetFirstStoryData(data.ChapterIndex);
		if (firstStoryData == null)
		{
			return false;
		}
		string[] condition = firstStoryData.Condition;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(condition) && !SingletonAuto<ConditionChecker>.instance.Check(condition))
		{
			return false;
		}
		return true;
	}

	private MainStoryData GetFirstStoryData(int chapterIndex)
	{
		MainStoryData result = null;
		foreach (MainStoryData allDatum in GameRoot.Instance.MainStoryDataContainer.GetAllData())
		{
			if (allDatum.ChapterIndex == chapterIndex && allDatum.StoryNo == 1)
			{
				result = allDatum;
			}
		}
		return result;
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		CloseInstance<BattlePartySetting>();
		NShared.BeginCampaign.Request request = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			LinkedDungeonIndex = enterDungeon.LinkedDungeon,
			DungeonDifficulty = (int)enterDungeon.Difficulty,
			ScenarioDungeon = enterDungeon.IsScenario,
			HeroIndices = battlePartyInfo.heroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds,
			AffixIndices = battlePartyInfo.AffixIndices,
			Repeat = false
		};
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(battlePartyInfo.subHeroIndices))
		{
			request.AiHeroIndices = battlePartyInfo.subHeroIndices;
		}
		SingletonAuto<BattleContext>.instance.BeginScenarioReplay(request, delegate
		{
			IsPlayMainStoryMode = true;
			LobbyScene2.ListReturnState.Add(new MainStoryReturnState(enterDungeon, currentChapterData, currentStoryData));
		});
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting caller)
	{
		GetInstance<MainStoryList>().ShowStoryList(show: true);
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	void ShakmehPartySetting.Listener.StartBattle(ShakmehBossType shakmehBossType, EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		CloseInstance<ShakmehPartySetting>();
		NShared.BeginCampaign.Request request = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			LinkedDungeonIndex = enterDungeon.LinkedDungeon,
			DungeonDifficulty = (int)enterDungeon.Difficulty,
			ScenarioDungeon = enterDungeon.IsScenario,
			HeroIndices = battlePartyInfo.heroIndices,
			GroupHeroIndices = battlePartyInfo.subHeroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds,
			AffixIndices = battlePartyInfo.AffixIndices,
			Repeat = false
		};
		SingletonAuto<BattleContext>.instance.BeginScenarioReplay(request, delegate
		{
			IsPlayMainStoryMode = true;
			LobbyScene2.ListReturnState.Add(new MainStoryReturnState(enterDungeon, currentChapterData, currentStoryData));
		});
	}

	void ShakmehPartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool ShakmehPartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	void MainStoryChapterList.Listener.OnSelectChapter(MainStoryChapterData data)
	{
		if (data != null)
		{
			OpenStoryList(data);
		}
	}

	void MainStoryList.Listener.OnSelectStory(MainStoryData data)
	{
		OnClickStory(data);
	}

	void MainStoryList.Listener.OnPrevChapter()
	{
		if (currentChapterData != null)
		{
			MainStoryChapterData mainStoryChapterData = null;
			int key = currentChapterData.StoryIndex - 1;
			mainStoryChapterData = GameRoot.Instance.MainStoryChapterDataContainer.GetData(key);
			OpenStoryList(mainStoryChapterData);
		}
	}

	void MainStoryList.Listener.OnNextChapter()
	{
		if (currentChapterData != null)
		{
			MainStoryChapterData mainStoryChapterData = null;
			int key = currentChapterData.StoryIndex + 1;
			mainStoryChapterData = GameRoot.Instance.MainStoryChapterDataContainer.GetData(key);
			OpenStoryList(mainStoryChapterData);
		}
	}

	void MainStoryPopup.Listener.OnSelectNextStory()
	{
		if (currentStoryData != null)
		{
			int chapterIndex = currentStoryData.ChapterIndex;
			int storyNo = currentStoryData.StoryNo + 1;
			MainStoryData data = GameRoot.Instance.MainStoryDataContainer.GetData(chapterIndex, storyNo);
			if (data == null)
			{
				VespaDebug.LogError("chapter=" + chapterIndex + " story=" + storyNo);
				return;
			}
			GetInstance<MainStoryPopup>().CloseImmediately();
			OnClickStory(data);
		}
	}

	void MainStoryPopup.Listener.OnSelectReplayStory()
	{
		if (currentStoryData != null)
		{
			GetInstance<MainStoryPopup>().CloseImmediately();
			OnClickStory(currentStoryData);
		}
	}
}
