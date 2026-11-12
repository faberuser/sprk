using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NEventTrigger;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NCommonImpl;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NPunish;
using NShared;
using NShared.BeginCampaign;
using NShared.BeginContent;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class PunishManagment : BaseManagement<PunishManagment>, PunishMainWindow.IListenenr, PunishDungeonTypeSelectWindow.IListener, PunishmentRaidWindow.IListener, PunishScenarioListComponent.IListener, BattlePartySetting.Listener, PunishmentRaidPartySetting.Listener, PunishmentLevelSelectPopup.IListenenr, PunishShardSelectPopup.IListenenr
{
	private float _battleRequestedTime;

	private RaidData _raidData;

	private PunishmentGroupData _scenarioGroupData;

	protected override void Awake()
	{
		base.Awake();
		AddInstanceWindow();
	}

	private void AddInstanceWindow()
	{
		AddInstance<PunishMainWindow>(PunishMainWindow.PrefabPath);
		AddInstance<PunishDungeonTypeSelectWindow>(PunishDungeonTypeSelectWindow.PrefabPath);
		AddInstance<PunishScenarioListWindow>(PunishScenarioListWindow.PrefabPath);
		AddInstance<PunishmentRaidWindow>("Resources/Prefab2/Window/Raid/PunishmentRaidWindow");
		AddInstance<PunishmentRaidPartySetting>("Resources/Prefab2/Window/Raid/PunishmentRaidPartySetting");
		AddInstance<BattleDungeonInfo>("Resources/Prefab2/Window/Dungeon/BattleDungeonInfo");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<PunishmentLevelSelectPopup>(PunishmentLevelSelectPopup.PrefabPath);
		AddInstance<PunishmentRaidRewardInfoPopup>("Resources/Prefab2/Window/Raid/PunishmentRaidRewardInfoPopup");
		AddInstance<PunishShardSelectPopup>(PunishShardSelectPopup.PrefabPath);
		AddInstance<KarmaCardSelectWindow>(KarmaCardSelectWindow.PrefabPath);
		AddInstance<PunishmentShardGaugeWindow>(PunishmentShardGaugeWindow.PrefabPath);
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("PunishManagement", this, delegate
		{
			using (new BlockManager().Horizontal())
			{
				if (GUILayout.Button("Karma Open"))
				{
					KarmaCardSelectWindow karmaCardSelectWindow = GetInstance<KarmaCardSelectWindow>();
					if (karmaCardSelectWindow != null && !karmaCardSelectWindow.IsOpen())
					{
						karmaCardSelectWindow.Openbase(null);
					}
				}
				if (GUILayout.Button("Karma Close"))
				{
					KarmaCardSelectWindow karmaCardSelectWindow2 = GetInstance<KarmaCardSelectWindow>();
					if (karmaCardSelectWindow2 != null && karmaCardSelectWindow2.IsOpen())
					{
						karmaCardSelectWindow2.Close();
					}
				}
			}
			using (new BlockManager().Horizontal())
			{
				if (GUILayout.Button("Shard Open"))
				{
					PunishmentShardGaugeWindow punishmentShardGaugeWindow = GetInstance<PunishmentShardGaugeWindow>();
					if (punishmentShardGaugeWindow != null && !punishmentShardGaugeWindow.IsOpen())
					{
						punishmentShardGaugeWindow.Open(70101, 1, BattleType.KarmaRaid, new BattleInstanceExtraData
						{
							FlaskItemIndex = 40238,
							FlaskItemCount = 1
						});
					}
				}
				if (GUILayout.Button("Shard Close"))
				{
					PunishmentShardGaugeWindow punishmentShardGaugeWindow2 = GetInstance<PunishmentShardGaugeWindow>();
					if (punishmentShardGaugeWindow2 != null && punishmentShardGaugeWindow2.IsOpen())
					{
						punishmentShardGaugeWindow2.Close();
					}
				}
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Kill : " + PunishmentShardGaugeWindow.EndKillCount);
				GUILayout.Label("Wave : " + PunishmentShardGaugeWindow.ClearWaveIndex);
				if (GUILayout.Button("AddKill"))
				{
					PunishmentShardGaugeWindow.EndKillCount++;
				}
				if (GUILayout.Button("RemoveKill"))
				{
					PunishmentShardGaugeWindow.EndKillCount--;
				}
				if (GUILayout.Button("AddWave"))
				{
					PunishmentShardGaugeWindow.ClearWaveIndex++;
					PunishmentShardGaugeWindow.EndKillCount = 0;
				}
			}
		});
	}

	public void OpenPunishMainWindow(int groupIndex = 0)
	{
		if (RestoredPortal.ApocalypsionAccess())
		{
			PunishMainWindow punishMainWindow = GetInstance<PunishMainWindow>();
			if (!(punishMainWindow == null) && !punishMainWindow.IsOpen())
			{
				punishMainWindow.Open(this, groupIndex);
			}
		}
	}

	public void OpenPunishDungeonTypeSelectWindow(PunishmentGroupData bossData)
	{
		PunishDungeonTypeSelectWindow punishDungeonTypeSelectWindow = GetInstance<PunishDungeonTypeSelectWindow>();
		if (!(punishDungeonTypeSelectWindow == null) && !punishDungeonTypeSelectWindow.IsOpen())
		{
			punishDungeonTypeSelectWindow.Open(bossData, this);
		}
	}

	public void OpenPunishScenarioListWindow(PunishmentGroupData scenarioData)
	{
		_scenarioGroupData = scenarioData;
		PunishScenarioListWindow punishScenarioListWindow = GetInstance<PunishScenarioListWindow>();
		if (!(punishScenarioListWindow == null) && !punishScenarioListWindow.IsOpen())
		{
			punishScenarioListWindow.Open(scenarioData, this);
		}
	}

	public void OpenPunishRaidWindow(PunishmentGroupData bossData, int level)
	{
		if (bossData != null)
		{
			PunishmentRaidWindow punishmentRaidWindow = GetInstance<PunishmentRaidWindow>();
			if (!(punishmentRaidWindow == null) && !punishmentRaidWindow.IsOpen())
			{
				punishmentRaidWindow.Open(bossData.GroupIndex, level, this);
			}
		}
	}

	public void OpenPunishmentRewardPopup(int groupIndex, int level)
	{
		PunishmentRaidRewardInfoPopup punishmentRaidRewardInfoPopup = GetInstance<PunishmentRaidRewardInfoPopup>();
		if (!(punishmentRaidRewardInfoPopup == null) && !punishmentRaidRewardInfoPopup.IsOpen())
		{
			punishmentRaidRewardInfoPopup.Open(groupIndex, level);
		}
	}

	public void OpenCustom()
	{
		PunishmentRaidWindow punishmentRaidWindow = GetInstance<PunishmentRaidWindow>();
		if (!(punishmentRaidWindow == null) && !punishmentRaidWindow.IsOpen())
		{
			punishmentRaidWindow.Open(101002, this);
		}
	}

	public void OpenPunishmentRaidPartySetting(RaidData raidData, PunishmentGroupData punishmentGroupData, int[] affixIndices = null)
	{
		if (raidData == null)
		{
			return;
		}
		PunishmentRaidPartySetting punishmentRaidPartySetting = GetInstance<PunishmentRaidPartySetting>();
		if (!(punishmentRaidPartySetting == null))
		{
			_raidData = raidData;
			int chapterIndex = raidData.ChapterIndex;
			int dungeonIndex = raidData.DungeonIndex;
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data == null)
			{
				VespaDebug.LogError("Raid DungeonData null : raid=" + raidData?.ToString() + " chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
			}
			else
			{
				NShared.DungeonDifficultyType difficulty = NShared.DungeonDifficultyType.Easy;
				string localStorageKey = data.LocalStorageKey;
				EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, difficulty);
				PunishmentBattlePartySettingProvider dataProvider = new PunishmentBattlePartySettingProvider(localStorageKey, enterDungeonData, raidData, punishmentGroupData, affixIndices);
				punishmentRaidPartySetting.Open(this, dataProvider);
			}
		}
	}

	public void OpenPunishmentLevelSelectPopup(int groupIndex)
	{
		PunishmentLevelSelectPopup punishmentLevelSelectPopup = GetInstance<PunishmentLevelSelectPopup>();
		if (!(punishmentLevelSelectPopup == null) && !punishmentLevelSelectPopup.IsOpen())
		{
			punishmentLevelSelectPopup.Open(this, groupIndex);
		}
	}

	public void OpenShardSelectPopup(PunishmentGroupData groupData, PunishmentRaidPartySetting.IDataProvider dataProvider)
	{
		PunishShardSelectPopup punishShardSelectPopup = GetInstance<PunishShardSelectPopup>();
		if (!(punishShardSelectPopup == null) && !punishShardSelectPopup.IsOpen())
		{
			punishShardSelectPopup.Open(this, groupData, dataProvider);
		}
	}

	private void OpenNeedStaminaPopup()
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Potion);
		List<Tuple<ItemData, int>> list = new List<Tuple<ItemData, int>>();
		foreach (ItemInfo item in itemsByType)
		{
			ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(item.ItemIndex);
			if (data != null && (data.SubData.ActionType == ItemActionType.RecoverStamina || data.SubData.ActionType == ItemActionType.RecoverStaminaPercent))
			{
				if (!EnumTools.TryParse<StaminaType>(data.SubData.ActionSubValue, out var o_value))
				{
					o_value = StaminaType.Chicken;
				}
				if (o_value == StaminaType.Chicken)
				{
					list.Add(new Tuple<ItemData, int>(data, item.Count));
				}
			}
		}
		if (list.Count > 0)
		{
			UIHelper.ShowSelectItemMessagePopup(isNotEnough: true, StaminaType.Chicken, list, 0);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("COMMON.INFORM");
		string stringDirect2 = LocalizationManager.GetStringDirect("COMMON_REQ_STAMINA");
		MessagePopup3Button.Open(stringDirect, stringDirect2, null, delegate
		{
			PunishmentLevelSelectPopup punishmentLevelSelectPopup = GetInstance<PunishmentLevelSelectPopup>();
			if (punishmentLevelSelectPopup.IsOpen())
			{
				punishmentLevelSelectPopup.Close();
			}
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Stamina.ToString());
		}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
	}

	public void OpenByScenarioReturnState(PunishmentGroupData punishmentGroupData)
	{
		int groupIndex = punishmentGroupData.GroupIndex;
		PunishmentGroupData bossData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(groupIndex, EPunishmentDungeonType.Boss);
		PunishmentGroupData scenarioData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(groupIndex, EPunishmentDungeonType.Scenario);
		if (bossData != null && scenarioData != null)
		{
			new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishMainWindow(punishmentGroupData.GroupIndex);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishDungeonTypeSelectWindow(bossData);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishScenarioListWindow(scenarioData);
			}))
				.WrapperTouchControl()
				.DisableTouch()
				.Commit();
		}
	}

	public void OpenByKarmaFailReturnState(PunishmentGroupData punishmentGroupData, RaidData raidData)
	{
		int groupIndex = punishmentGroupData.GroupIndex;
		PunishmentGroupData bossData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(groupIndex, EPunishmentDungeonType.Boss);
		PunishmentGroupData karmaData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(groupIndex, EPunishmentDungeonType.Karma);
		if (bossData != null && karmaData != null)
		{
			new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishMainWindow(punishmentGroupData.GroupIndex);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishDungeonTypeSelectWindow(bossData);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishmentRaidPartySetting(raidData, karmaData);
			}))
				.WrapperTouchControl()
				.DisableTouch()
				.Commit();
		}
	}

	public void OpenByBossClearReturnState(PunishmentGroupData punishmentGroupData, RaidData raidData, int openLevel)
	{
		int groupIndex = punishmentGroupData.GroupIndex;
		PunishmentGroupData bossData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(groupIndex, EPunishmentDungeonType.Boss);
		if (bossData != null)
		{
			new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishMainWindow(punishmentGroupData.GroupIndex);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishDungeonTypeSelectWindow(bossData);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishRaidWindow(bossData, openLevel);
			}))
				.WrapperTouchControl()
				.DisableTouch()
				.Commit();
		}
	}

	public void OpenByBossFailReturnState(PunishmentGroupData punishmentGroupData, RaidData raidData, int openLevel, int[] affixIndices)
	{
		int groupIndex = punishmentGroupData.GroupIndex;
		PunishmentGroupData bossData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(groupIndex, EPunishmentDungeonType.Boss);
		if (bossData != null)
		{
			new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishMainWindow(punishmentGroupData.GroupIndex);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishDungeonTypeSelectWindow(bossData);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				OpenPunishRaidWindow(bossData, openLevel);
			}))
				.ADDTASK(base.TaskFactory.Run(delegate
				{
					OpenPunishmentRaidPartySetting(raidData, punishmentGroupData, affixIndices);
				}))
				.WrapperTouchControl()
				.DisableTouch()
				.Commit();
		}
	}

	private void RequestOpenPunishmentDungeon(PunishmentGroupData data, int level)
	{
		if (data.PunishmentDungeonType == EPunishmentDungeonType.Karma)
		{
			NGame2.NUI.NWindow.NPunish.Helper.RequestOpenKarmaDungeon(data.GroupIndex, RefreshPunishDungeonTypeSelectWindow);
			return;
		}
		Action a = RefreshPunishDungeonTypeSelectWindow;
		a = (Action)Delegate.Combine(a, (Action)delegate
		{
			OpenPunishRaidWindow(data, level);
		});
		NGame2.NUI.NWindow.NPunish.Helper.RequestOpenBoss(data.GroupIndex, level, a);
	}

	private void OpenRaidOpenPopup(PunishmentGroupData data, int level = 1)
	{
		int openStamina = NGame2.NUI.NWindow.NPunish.Helper.GetOpenStamina(data);
		long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.Chicken);
		long num = currentStamina;
		int num2 = openStamina;
		if (num2 > num)
		{
			OpenNeedStaminaPopup();
			return;
		}
		Action onYes = delegate
		{
			RequestOpenPunishmentDungeon(data, level);
		};
		string stringDirect = LocalizationManager.GetStringDirect("PUNISHMENTRAID_OPEN_CONFIRM");
		string stringDirect2 = LocalizationManager.GetStringDirect("POPUP_KARMA_OPEN_DESC", openStamina);
		MessagePopup.Open(stringDirect, stringDirect2, onYes, null);
	}

	private void OpenRaidResetPopup(PunishmentGroupData data)
	{
		Action onYes = delegate
		{
			if (!NGame2.NUI.NWindow.NPunish.Helper.IsOpenPunishRaid(GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(data.GroupIndex, data.PunishmentDungeonType)))
			{
				string stringDirect3 = LocalizationManager.GetStringDirect("COMMON.INFORM");
				string stringDirect4 = LocalizationManager.GetStringDirect("RESULT_TYPE.CLOSED_DUNGEON");
				MessagePopup.Open(stringDirect3, stringDirect4);
			}
			else
			{
				Action onSuccess = RefreshPunishDungeonTypeSelectWindow;
				NGame2.NUI.NWindow.NPunish.Helper.RequestResetBoss(data.GroupIndex, onSuccess);
			}
		};
		string stringDirect = LocalizationManager.GetStringDirect("PUNISHMENTRAID_RESET_CONFIRM");
		string stringDirect2 = LocalizationManager.GetStringDirect("PUNISHMENTRAID_RESET_MESSAGE");
		MessagePopup.Open(stringDirect, stringDirect2, onYes, null);
	}

	private void RefreshPunishDungeonTypeSelectWindow()
	{
		if (IsOpened<PunishmentLevelSelectPopup>())
		{
			GetInstance<PunishmentLevelSelectPopup>().Close();
		}
		PunishDungeonTypeSelectWindow punishDungeonTypeSelectWindow = GetInstance<PunishDungeonTypeSelectWindow>();
		if (!(punishDungeonTypeSelectWindow == null))
		{
			punishDungeonTypeSelectWindow.Refresh();
		}
	}

	public void CloseByExpireDungeon()
	{
		if (IsOpened<PunishmentRaidPartySetting>())
		{
			GetInstance<PunishmentRaidPartySetting>().Close();
		}
		if (IsOpened<PunishmentRaidWindow>())
		{
			GetInstance<PunishmentRaidWindow>().Close();
		}
		if (IsOpened<PunishDungeonTypeSelectWindow>())
		{
			GetInstance<PunishDungeonTypeSelectWindow>().Close();
		}
		if (!IsOpened<PunishMainWindow>())
		{
			OpenPunishMainWindow();
		}
	}

	void PunishMainWindow.IListenenr.OnClickEnter(PunishmentGroupData data)
	{
		EnterPunishmentDungeonTypeSelect enterPunishmentDungeonTypeSelect = new EnterPunishmentDungeonTypeSelect();
		enterPunishmentDungeonTypeSelect.GroupIndex = data.GroupIndex;
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(enterPunishmentDungeonTypeSelect);
		OpenPunishDungeonTypeSelectWindow(data);
	}

	void PunishDungeonTypeSelectWindow.IListener.OnClickEnterScenario(PunishmentGroupData data)
	{
		EnterPunishScenarioList enterPunishScenarioList = new EnterPunishScenarioList();
		enterPunishScenarioList.GroupIndex = data.GroupIndex;
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(enterPunishScenarioList);
		OpenPunishScenarioListWindow(data);
	}

	void PunishDungeonTypeSelectWindow.IListener.OnClickOpenKarma(PunishmentGroupData data)
	{
		OpenRaidOpenPopup(data);
	}

	void PunishDungeonTypeSelectWindow.IListener.OnClickEnterKarma(PunishmentGroupData data)
	{
		RaidData data2 = GameRoot.Instance.RaidDataContainer.GetData(data.KarmaRaidIndex, 1);
		OpenPunishmentRaidPartySetting(data2, data);
	}

	void PunishDungeonTypeSelectWindow.IListener.OnClickOpenBoss(PunishmentGroupData data)
	{
		OpenPunishmentLevelSelectPopup(data.GroupIndex);
	}

	void PunishDungeonTypeSelectWindow.IListener.OnClickEnterBoss(PunishmentGroupData data)
	{
		if (data != null)
		{
			OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(data.GroupIndex, data.PunishmentDungeonType);
			if (openPunishmentRaidInfo != null)
			{
				OpenPunishRaidWindow(data, openPunishmentRaidInfo.OpenLevel);
			}
		}
	}

	void PunishDungeonTypeSelectWindow.IListener.OnClickResetBoss(PunishmentGroupData data)
	{
		OpenRaidResetPopup(data);
	}

	void PunishDungeonTypeSelectWindow.IListener.OnClickBossRewardPopup(PunishmentGroupData data)
	{
		if (data != null)
		{
			int groupIndex = data.GroupIndex;
			int level = 0;
			OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(groupIndex, EPunishmentDungeonType.Boss);
			if (openPunishmentRaidInfo != null)
			{
				level = ((openPunishmentRaidInfo.IsOpen != 1) ? (openPunishmentRaidInfo.ClearLevel + 1) : openPunishmentRaidInfo.OpenLevel);
			}
			OpenPunishmentRewardPopup(groupIndex, level);
		}
	}

	void PunishmentLevelSelectPopup.IListenenr.OnClickOpenBoss(PunishmentGroupData data, int level)
	{
		OpenRaidOpenPopup(data, level);
	}

	void PunishScenarioListComponent.IListener.OnClickEnterDungeon(int chapterIndex, int dungeonIndex)
	{
		EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex);
		BattleDungeonInfoListener battleDungeonInfoListener = new BattleDungeonInfoListener();
		battleDungeonInfoListener.EnterDungeon = enterDungeonData;
		battleDungeonInfoListener.OnReadyBattle = OnReadyBattle;
		EventDungeonRemainTimer eventDungeonRemainTimer = new EventDungeonRemainTimer(null);
		eventDungeonRemainTimer.SetShowTimer(showTimer: false);
		battleDungeonInfoListener.DungeonRemainTimer = eventDungeonRemainTimer;
		GetInstance<BattleDungeonInfo>().Open(enterDungeonData, battleDungeonInfoListener);
	}

	private void OnReadyBattle(EnterDungeonData enterDungeon)
	{
		DefaultBattlePartySettingDataProdiver dataProdiver = GetDataProdiver(enterDungeon);
		GetInstance<BattlePartySetting>().Open(this, dataProdiver);
	}

	private DefaultBattlePartySettingDataProdiver GetDataProdiver(EnterDungeonData enterDungeon)
	{
		int chapter = enterDungeon.Chapter;
		int originalDungeon = enterDungeon.OriginalDungeon;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapter, originalDungeon);
		_ = data.ReqStaminaType;
		int staminaOfDifficulty = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), enterDungeon.Difficulty);
		return DataProviderBuilder.Build(data.LocalStorageKey, data.ReqStaminaType, staminaOfDifficulty, Battle.START_BATTLE, enterDungeon, null);
	}

	void PunishmentRaidWindow.IListener.OpenRaidRewardPopup(int raidGroupIndex)
	{
		int level = 0;
		OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(raidGroupIndex, EPunishmentDungeonType.Boss);
		if (openPunishmentRaidInfo != null)
		{
			level = ((openPunishmentRaidInfo.IsOpen != 1) ? (openPunishmentRaidInfo.ClearLevel + 1) : openPunishmentRaidInfo.OpenLevel);
		}
		OpenPunishmentRewardPopup(raidGroupIndex, level);
	}

	void PunishShardSelectPopup.IListenenr.ApplySelectItem(int itemIndex, long itemCount)
	{
		if (IsOpened<PunishShardSelectPopup>())
		{
			GetInstance<PunishShardSelectPopup>().Close();
		}
		PunishmentRaidPartySetting punishmentRaidPartySetting = GetInstance<PunishmentRaidPartySetting>();
		if (punishmentRaidPartySetting.IsOpen())
		{
			punishmentRaidPartySetting.RefreshShardSelectWidget();
		}
	}

	void PunishmentRaidPartySetting.Listener.OnClickShardSelect(PunishmentGroupData groupData, PunishmentRaidPartySetting.IDataProvider dataProvider)
	{
		OpenShardSelectPopup(groupData, dataProvider);
	}

	void PunishmentRaidPartySetting.Listener.StartPunishmentBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, PunishmentRaidPartySetting.ShardPlaskInfo shardPlaskInfo, PunishmentGroupData punishmentGroupData)
	{
		if (Time.time - _battleRequestedTime < 10f || enterDungeon == null || shardPlaskInfo == null || punishmentGroupData == null)
		{
			return;
		}
		OpenPunishmentRaidInfo openpunishmentInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(punishmentGroupData.GroupIndex, punishmentGroupData.PunishmentDungeonType);
		if (openpunishmentInfo != null && openpunishmentInfo.IsOpen != 0)
		{
			_battleRequestedTime = Time.time;
			NShared.BeginContent.Request request = new NShared.BeginContent.Request();
			request.ChapterIndex = enterDungeon.Chapter;
			request.DungeonIndex = enterDungeon.OriginalDungeon;
			request.DungeonDifficulty = (int)enterDungeon.Difficulty;
			request.HeroIndices = battlePartyInfo.heroIndices;
			request.AiHeroIndices = battlePartyInfo.subHeroIndices;
			request.GroupIndex = punishmentGroupData.GroupIndex;
			request.DungeonType = punishmentGroupData.PunishmentDungeonType;
			request.FlaskItemIndex = shardPlaskInfo.shardItemIndex;
			request.FlaskItemCount = shardPlaskInfo.shardItemCount;
			request.AffixIndices = battlePartyInfo.AffixIndices;
			if (punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss)
			{
				request.Level = openpunishmentInfo.OpenLevel;
			}
			long[] accountIds = battlePartyInfo.accountIds;
			SingletonAuto<BattleContext>.instance.RequestBeginContent(request, accountIds, delegate
			{
				LobbyScene2.ListReturnState.Add(new PunishmentRaidReturnState
				{
					AffixIndices = battlePartyInfo.AffixIndices,
					RaidData = _raidData,
					PunishmentGroupData = punishmentGroupData,
					OpenLevel = openpunishmentInfo.OpenLevel
				});
			}, delegate
			{
				_battleRequestedTime = 0f;
			});
		}
	}

	void PunishmentRaidPartySetting.Listener.OnClickRaidInfo(PunishmentRaidPartySetting caller)
	{
		SceneAutoBehaviour<RaidManagement>.instance.OpenRaidInfo(caller.RaidData);
	}

	void PunishmentRaidPartySetting.Listener.OnClickOpenHeroInfo()
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		CachedHeroInfo cachedHeroInfo = ((__Extension_Method_ICollection__.COUNT(heroInfoList) > 0) ? heroInfoList[0] : null);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfoList); i++)
		{
			if (heroInfoList[i] != null)
			{
				cachedHeroInfo = heroInfoList[i];
				break;
			}
		}
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("Cannot select hero.");
		}
		else
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(cachedHeroInfo);
		}
	}

	void PunishmentRaidPartySetting.Listener.OnClickOpenInventoryInfo()
	{
		SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
	}

	void PunishmentRaidPartySetting.Listener.OnClickStartPunishmentBattle(PunishmentRaidPartySetting caller)
	{
		caller.OnClickStartBattle();
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (Time.time - _battleRequestedTime < 10f || enterDungeon == null)
		{
			return;
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(enterDungeon.Chapter, enterDungeon.OriginalDungeon);
		if (data == null)
		{
			return;
		}
		_battleRequestedTime = Time.time;
		NShared.BeginCampaign.Request request = new NShared.BeginCampaign.Request();
		request.ChapterIndex = enterDungeon.Chapter;
		request.DungeonIndex = enterDungeon.OriginalDungeon;
		request.LinkedDungeonIndex = enterDungeon.LinkedDungeon;
		request.DungeonDifficulty = (int)enterDungeon.Difficulty;
		request.ScenarioDungeon = enterDungeon.IsScenario;
		request.HeroIndices = battlePartyInfo.heroIndices;
		request.LeaderHeroIndex = battlePartyInfo.leaderHeroIndex;
		request.AccountIds = battlePartyInfo.accountIds;
		request.Repeat = false;
		int onetimeBoosterCount = battlePartyInfo.OnetimeBoosterCount;
		request.ItemBoosterCount = onetimeBoosterCount;
		request.DeltaHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(battlePartyInfo.DeltaHeroIndexList)) ? battlePartyInfo.DeltaHeroIndexList.ToArray() : null);
		request.ZetaHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(battlePartyInfo.ZetaHeroIndexList)) ? battlePartyInfo.ZetaHeroIndexList.ToArray() : null);
		request.AffixIndices = battlePartyInfo.AffixIndices;
		if (CampaignContext.CheckEachWaveEachTeam(data.BattleType))
		{
			request.GroupHeroIndices = battlePartyInfo.subHeroIndices;
		}
		else
		{
			request.AiHeroIndices = battlePartyInfo.subHeroIndices;
		}
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			if (extraOption != null && extraOption.AutoRepeat)
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
			}
			LobbyScene2.ListReturnState.Add(new PunishmentRaidReturnState
			{
				AffixIndices = battlePartyInfo.AffixIndices,
				RaidData = null,
				PunishmentGroupData = _scenarioGroupData,
				OpenLevel = 0
			});
		}, delegate
		{
			_battleRequestedTime = 0f;
		});
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}
}
