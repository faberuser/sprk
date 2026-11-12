using System;
using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildRaidDungeonClearedPopup : BaseWindow
{
	private enum ClearPopupType
	{
		GuildRaid,
		GuildSuppress
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildRaid/GuildRaidDungeonClearedPopup";

	public UIBaseGrid RewardInfoGrid;

	public UILabel BossNameLabel;

	public UILabel ClearDescLabel;

	private Action _onClose;

	private List<Component> _rewardComponentList = new List<Component>();

	private ClearPopupType _popupType;

	public static void Open(int chapterIndex, int dungeonIndex, int step, Action onClose)
	{
		WindowPrefab<GuildRaidDungeonClearedPopup> windowPrefab = new WindowPrefab<GuildRaidDungeonClearedPopup>("Resources/Prefab2/Window/GuildRaid/GuildRaidDungeonClearedPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(chapterIndex, dungeonIndex, step, ClearPopupType.GuildRaid, onClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(int chapterIndex, int dungeonIndex, int step, ClearPopupType popupType, Action onClose)
	{
		base.Open(delegate
		{
			InitGuildRaidClearedPopup(chapterIndex, dungeonIndex, step, popupType, onClose);
		});
	}

	private void InitGuildRaidClearedPopup(int chapterIndex, int dungeonIndex, int step, ClearPopupType popupType, Action onClose)
	{
		_onClose = onClose;
		_popupType = popupType;
		ClearRewardInfoComponentList();
		GuildRaidChapterData data = GameRoot.Instance.GuildRaidChapterDataContainer.GetData(chapterIndex);
		if (data == null)
		{
			return;
		}
		GuildRaidDungeonData guildRaidDungeonData = GetGuildRaidDungeonData(data, dungeonIndex, step);
		if (guildRaidDungeonData == null)
		{
			Close();
			return;
		}
		int killRewardIndex = guildRaidDungeonData.KillRewardIndex;
		SetRewardInfoGrid(killRewardIndex);
		CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, guildRaidDungeonData.Difficulty, scenario: false);
		string text = ((bossCreatureData != null) ? bossCreatureData.Name : "Unknown");
		UIHelper.SetLabelText(BossNameLabel, text);
		int chapterLastStep = GameRoot.Instance.GuildRaidDungeonDataContainer.GetChapterLastStep(chapterIndex);
		string text2 = LocalizationManager.GetString(GuildRaid.GUILDRAID_DUNGEON_CLEARED_POPUP_MESSAGE, text, step);
		if (data.IsBonusDungeon)
		{
			text2 = LocalizationManager.GetString(GuildRaid.GUILDRAID_BONUS_CLEARED_POPUP_MESSAGE, text);
		}
		else if (step >= chapterLastStep)
		{
			text2 = LocalizationManager.GetString(GuildRaid.GUILDRAID_DUNGEON_CLEARED_POPUP_MESSAGE_FINAL, text);
		}
		UIHelper.SetLabelText(ClearDescLabel, text2);
	}

	public static void OpenGuildSuppressClearedPopup(int raidIndex, int clearedSuppressDungeonIndex, int clearedLevel, Action onClose)
	{
		WindowPrefab<GuildRaidDungeonClearedPopup> windowPrefab = new WindowPrefab<GuildRaidDungeonClearedPopup>("Resources/Prefab2/Window/GuildRaid/GuildRaidDungeonClearedPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternalGuildSuppressClearedPopup(raidIndex, clearedSuppressDungeonIndex, clearedLevel, ClearPopupType.GuildSuppress, onClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternalGuildSuppressClearedPopup(int raidIndex, int clearedSuppressDungeonIndex, int clearedLevel, ClearPopupType popupType, Action onClose)
	{
		base.Open(delegate
		{
			InitGuildSuppressClearedPopup(raidIndex, clearedSuppressDungeonIndex, clearedLevel, popupType, onClose);
		});
	}

	private void InitGuildSuppressClearedPopup(int raidIndex, int clearedSuppressDungeonIndex, int clearedLevel, ClearPopupType popupType, Action onClose)
	{
		_onClose = onClose;
		_popupType = popupType;
		ClearRewardInfoComponentList();
		GuildSuppressDungeonData data = GameRoot.Instance.GuildSuppressDungeonDataContainer.GetData(clearedSuppressDungeonIndex);
		if (data != null)
		{
			RaidData data2 = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, clearedLevel);
			if (data2 != null)
			{
				int killRewardIndex = data.KillRewardIndex;
				SetRewardInfoGrid(killRewardIndex);
				CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(data2.ChapterIndex, data2.DungeonIndex, NShared.DungeonDifficultyType.Easy, scenario: false);
				string text = ((bossCreatureData != null) ? bossCreatureData.Name : "Unknown");
				UIHelper.SetLabelText(BossNameLabel, text);
				string stringDirect = LocalizationManager.GetStringDirect("GUILD_SUPPRESS_CLEARED_POPUP_MESSAGE", text);
				UIHelper.SetLabelText(ClearDescLabel, stringDirect);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearRewardInfoComponentList();
		_onClose = null;
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Vespa_System_Action_Extension_Method.fire(_onClose);
		_onClose = null;
	}

	private void ClearRewardInfoComponentList()
	{
		FactoryBase.ReleaseAndClearList(_rewardComponentList);
	}

	private void SetRewardInfoGrid(int killRewardIndex)
	{
		if (RewardInfoGrid != null)
		{
			RewardData data = GameRoot.Instance.RewardDataContainer.GetData(killRewardIndex);
			if (data != null)
			{
				List<Component> collection = BaseDungeonInfo.AddRewardItemToGrid(data, RewardInfoGrid, includeCurrency: true, null, BattleType.GuildRaid);
				_rewardComponentList.AddRange(collection);
			}
			RewardInfoGrid.RepositionGrid();
		}
	}

	private GuildRaidDungeonData GetGuildRaidDungeonData(GuildRaidChapterData guildRaidChapterData, int dungeonIndex, int step)
	{
		if (guildRaidChapterData == null)
		{
			return null;
		}
		if (guildRaidChapterData.IsBonusDungeon)
		{
			return GameRoot.Instance.GuildRaidBonusDungeonDataContainer.GetData(new GuildRaidBonusDungeonDataKey(guildRaidChapterData.ChapterIndex, dungeonIndex, guildRaidChapterData.BonusGroup));
		}
		if (guildRaidChapterData.IsDummyDungeon)
		{
			return null;
		}
		return GameRoot.Instance.GuildRaidDungeonDataContainer.GetData(new GuildRaidDungeonDataKey(guildRaidChapterData.ChapterIndex, dungeonIndex, step));
	}
}
