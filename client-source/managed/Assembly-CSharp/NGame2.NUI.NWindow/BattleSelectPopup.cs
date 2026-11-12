using System;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class BattleSelectPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/BattleSelectPopup";

	public UIWidget RootWidget;

	public UIGrid Grid;

	public GameObject Button_Once;

	public GameObject Button_Repeat;

	public GameObject Button_Next;

	public GameObject Button_RepeatNext;

	private BattlePartySetting.IDataProvider _dataProvider;

	private Action _onClose;

	public static void Open(BattlePartySetting.IDataProvider dataProvider)
	{
		BattleSelectPopup battleSelectPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<BattleSelectPopup>();
		if ((bool)battleSelectPopup)
		{
			battleSelectPopup.OpenInternal(dataProvider);
			return;
		}
		WindowPrefab<BattleSelectPopup> windowPrefab = new WindowPrefab<BattleSelectPopup>("Resources/Prefab2/Window/MessagePopup/BattleSelectPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(dataProvider);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(BattlePartySetting.IDataProvider dataProvider)
	{
		base.Open();
		_dataProvider = dataProvider;
		SetMenuButtons(dataProvider);
	}

	public void SetMenuButtons(BattlePartySetting.IDataProvider dataProvider)
	{
		if (dataProvider != null)
		{
			bool flag = BattlePartySetting.AutoRepeatable(dataProvider.GetEnterDungeon());
			NShared.DungeonDifficultyType difficulty = dataProvider.GetEnterDungeon().Difficulty;
			BattleType battleType = BattleType.None;
			bool isBranch = false;
			if (dataProvider.GetEnterDungeon().LinkedDungeonData != null)
			{
				battleType = dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType;
				isBranch = dataProvider.GetEnterDungeon().LinkedDungeonData.IsBranch;
			}
			UIHelper.EnableGameObject(Button_Repeat, CheckRepeat(battleType, flag));
			UIHelper.EnableGameObject(Button_RepeatNext, enable: false);
			UIHelper.EnableGameObject(Button_Next, CheckNext(battleType, difficulty, flag, isBranch));
			Grid.Reposition();
		}
	}

	public static bool IsMultipleMenu(BattlePartySetting.IDataProvider dataProvider)
	{
		if (dataProvider == null)
		{
			return false;
		}
		bool flag = BattlePartySetting.AutoRepeatable(dataProvider.GetEnterDungeon());
		NShared.DungeonDifficultyType difficulty = dataProvider.GetEnterDungeon().Difficulty;
		BattleType battleType = BattleType.None;
		bool isBranch = false;
		if (dataProvider.GetEnterDungeon().LinkedDungeonData != null)
		{
			battleType = dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType;
			isBranch = dataProvider.GetEnterDungeon().LinkedDungeonData.IsBranch;
		}
		if (CheckNext(battleType, difficulty, flag, isBranch))
		{
			return true;
		}
		if (CheckRepeat(battleType, flag))
		{
			return true;
		}
		if (CheckRepeatNext(battleType))
		{
			return true;
		}
		return false;
	}

	public static bool CheckRepeat(BattleType battleType, bool canRepeat)
	{
		if (battleType == BattleType.None || battleType == BattleType.SubStory || battleType == BattleType.SubStoryBoss || battleType == BattleType.Trial || battleType == BattleType.GuildRaid || battleType == BattleType.Raid || battleType == BattleType.RaidSingle || battleType == BattleType.HardRaid || battleType == BattleType.ChallengeRaidSingle || battleType == BattleType.GatewaytoTrial || battleType == BattleType.GodkingTrial || battleType == BattleType.FieldRaid || battleType == BattleType.GargoriaRaid || battleType == BattleType.SeigfriedRaid || battleType == BattleType.AscalonRaid || battleType == BattleType.ArcdimRaid || battleType == BattleType.ArcdimHardRaid || battleType == BattleType.TreasureHouse || battleType == BattleType.PunishmentRaid || battleType == BattleType.KarmaRaid)
		{
			return false;
		}
		return canRepeat;
	}

	public static bool CheckRepeatNext(BattleType battleType)
	{
		if (battleType == BattleType.Hideout)
		{
			return true;
		}
		return false;
	}

	public static bool CheckNext(BattleType battleType, NShared.DungeonDifficultyType difficulty, bool canNext, bool isBranch)
	{
		if (battleType == BattleType.None || (battleType == BattleType.Campaign && difficulty <= NShared.DungeonDifficultyType.Normal && !canNext) || (battleType == BattleType.Boss && difficulty <= NShared.DungeonDifficultyType.Normal && !canNext) || (battleType == BattleType.Boss && isBranch) || battleType == BattleType.Hideout || battleType == BattleType.UnderPrison || battleType == BattleType.TreasureHouse || battleType == BattleType.SubStory || battleType == BattleType.SubStoryBoss || battleType == BattleType.WorldBoss || battleType == BattleType.Trial || battleType == BattleType.GuildRaid || battleType == BattleType.Raid || battleType == BattleType.RaidSingle || battleType == BattleType.HardRaid || battleType == BattleType.ChallengeRaid || battleType == BattleType.ChallengeRaidSingle || battleType == BattleType.EventDungeon || battleType == BattleType.GuildSuppress || battleType == BattleType.Arena || battleType == BattleType.ArenaBanPick || battleType == BattleType.SquareSideQuest || battleType == BattleType.Conquest || battleType == BattleType.GatewaytoTrial || battleType == BattleType.GodkingTrial || battleType == BattleType.FieldRaid || battleType == BattleType.Shakmeh || battleType == BattleType.PurchaseDungeon || battleType == BattleType.GargoriaRaid || battleType == BattleType.SeigfriedRaid || battleType == BattleType.AscalonRaid || battleType == BattleType.OrdealArena || battleType == BattleType.ArcdimRaid || battleType == BattleType.ArcdimHardRaid || battleType == BattleType.PunishmentRaid || battleType == BattleType.KarmaRaid)
		{
			return false;
		}
		return true;
	}

	public override void Reset()
	{
		base.Reset();
		_onClose = null;
	}

	public void OnClickStartBattleOnce()
	{
		switch (_dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType)
		{
		case BattleType.FieldRaid:
		case BattleType.GargoriaRaid:
		case BattleType.SeigfriedRaid:
		case BattleType.AscalonRaid:
			GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<RaidPartySetting>().TryStartBattle(null);
			break;
		case BattleType.Shakmeh:
			GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<ShakmehPartySetting>().TryStartBattle(null);
			break;
		default:
			GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<BattlePartySetting>().TryStartBattle(null);
			break;
		}
		Close();
	}

	public void OnClickStartBattleRepeat()
	{
		if (_dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType == BattleType.Shakmeh)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<ShakmehPartySetting>().OnClickRepeatBattle();
		}
		else
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<BattlePartySetting>().OnClickRepeatBattle();
		}
		Close();
	}

	public void OnClickStartBattleNext()
	{
		if (_dataProvider == null)
		{
			return;
		}
		_ = _dataProvider.GetEnterDungeon().Difficulty;
		BattleType battleType = _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType;
		string stringDirect = LocalizationManager.GetStringDirect("TOWER_SELECT_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("CHAPTER_CONTINUOUS_BATTLE_DETAIL");
		LocalizationManager.GetStringDirect("START_BATTLE");
		LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_BUTTON");
		string confirmAutoContent = LocalizationManager.GetStringDirect("CHAPTER_CONTINUOUS_BATTLE_MESSAGE");
		switch (battleType)
		{
		case BattleType.Campaign:
			stringDirect2 = LocalizationManager.GetStringDirect("CHAPTER_CONTINUOUS_BATTLE_DETAIL");
			confirmAutoContent = LocalizationManager.GetStringDirect("CHAPTER_CONTINUOUS_BATTLE_MESSAGE");
			break;
		case BattleType.Hideout:
			stringDirect2 = LocalizationManager.GetStringDirect("HIDEOUT_CONTINUOUS_BATTLE_DETAIL");
			confirmAutoContent = LocalizationManager.GetStringDirect("CONTINUOUS_BATTLE_MESSAGE2");
			break;
		case BattleType.Conquest:
			stringDirect2 = LocalizationManager.GetStringDirect("HUNTING_CONTINUOUS_BATTLE_DETAIL");
			confirmAutoContent = LocalizationManager.GetStringDirect("CONTINUOUS_BATTLE_MESSAGE2");
			break;
		}
		MessagePopup.OpenEnabled(stringDirect, stringDirect2, delegate
		{
			MessagePopup.OpenEnabled(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), confirmAutoContent, delegate
			{
				BattlePartySetting.ExtraOptionData extraOption = new BattlePartySetting.ExtraOptionData
				{
					AutoNext = true
				};
				GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<BattlePartySetting>().TryStartBattle(extraOption);
			}, null, "COMMON_CONFIRM", bYesEnabled: true);
		}, null, "COMMON_CONFIRM", bYesEnabled: true);
		Close();
	}

	public void OnClickClose()
	{
		Close();
		Vespa_System_Action_Extension_Method.fire(_onClose);
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnShowTooltip()
	{
		DescTooltip.Open(RootWidget, new TooltipTextInfo
		{
			Desc = new StringProxy
			{
				Value = LocalizationManager.GetStringDirect("TOOLTIP_INFO_TEXT_BATTLE_SELECT")
			},
			Name = new StringProxy
			{
				Value = string.Empty
			}
		});
	}

	public void OnHideTooltip()
	{
		DescTooltip.Hide(RootWidget);
	}
}
