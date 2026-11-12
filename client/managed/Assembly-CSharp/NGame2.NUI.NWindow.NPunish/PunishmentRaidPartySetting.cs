using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPunish;

public class PunishmentRaidPartySetting : RaidSinglePartySetting, ShardSelectWidget.Listener
{
	public class ShardPlaskInfo
	{
		public int shardItemIndex;

		public int shardItemCount;
	}

	public new interface IDataProvider : BattlePartySetting.IDataProvider
	{
		RaidData GetRaidData();

		PunishmentGroupData GetPunishmentGroupData();

		int GetShardItemIndex();

		void SetShardItemIndex(int itemIndex);

		int GetShardItemCount();

		void SetShardItemCount(long itemCount);

		void SaveLastShardSetting(int itemIndex, int itemCount);
	}

	public new interface Listener
	{
		void OnClickShardSelect(PunishmentGroupData groupData, IDataProvider dataProvider);

		void OnClickStartPunishmentBattle(PunishmentRaidPartySetting caller);

		void StartPunishmentBattle(EnterDungeonData enterDungeon, BattlePartyInfo battlePartyInfo, ShardPlaskInfo extraOption, PunishmentGroupData punishmentGroupData);

		void OnClickRaidInfo(PunishmentRaidPartySetting caller);

		void OnClickOpenHeroInfo();

		void OnClickOpenInventoryInfo();
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Raid/PunishmentRaidPartySetting";

	[Header("[OnlyMain PartySlotSpacing]")]
	public PartySlotSpacing[] OnlyMainPartySlotSpacings;

	[Header("[Shard Select]")]
	public ShardSelectWidget ShardSelectWidget;

	private new IDataProvider _dataProvider;

	private new Listener _listener;

	public override StaminaType StaminaType => StaminaType.Chicken;

	public override int StaminaCount => 0;

	public void Open(Listener listener, IDataProvider dataProvider)
	{
		Open(delegate
		{
			Init(listener, dataProvider);
		}, null);
	}

	private void Init(Listener listener, IDataProvider dataProvider)
	{
		if (dataProvider != null)
		{
			RaidPartySetting.IsMultiRaid = false;
			RepeatBattle = false;
			_listener = listener;
			_dataProvider = dataProvider;
			base._dataProvider = dataProvider;
			_raidData = dataProvider.GetRaidData();
			Init();
			PunishmentInit();
			PunishmentGroupData punishmentGroupData = dataProvider.GetPunishmentGroupData();
			if (punishmentGroupData != null && punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Karma)
			{
				OnKarmaDungeonEnter triggerEvent = new OnKarmaDungeonEnter();
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
			}
		}
	}

	private void Init()
	{
		BaseInit();
		SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyTopmostMenu(this);
		UpdateBanTypeData();
		SetSquardEmptySlot();
		SetSquardHeroList(out var _, out var _);
		SetUserHeroList();
		_autoRepeatable = false;
		RefreshStaminaCount();
		InitBanRuleText();
		InitDeco();
		UpdateStaminaCount();
		RefreshSelectionMark();
		UpdateStartBattleState();
		SetTitle();
		SetBanDec();
		_mainPartyMaxCount = _raidData.MainPartyCount;
		_subPartyMaxCount = _raidData.SubPartyCount;
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		if (enterDungeon != null && enterDungeon.LinkedDungeonData != null && DispatchHelper.IsDispatchable(enterDungeon.LinkedDungeonData))
		{
			UserHeroListPanel.SetDispatch();
		}
		UpdateBanHeroPortrait();
		RestoredPortal.RestorePunishmentPartyFrames(this);
	}

	private void PunishmentInit()
	{
		ShardSelectWidget.Init(this, _dataProvider);
	}

	protected override void SetBanDec()
	{
		if (base.RaidData != null)
		{
			int banIndex = base.RaidData.BanIndex;
			BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(banIndex);
			if (data != null && UserHeroListPanel != null)
			{
				UserHeroListPanel.SetBanUserHeroList(data);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		ShardSelectWidget.Reset();
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	protected override void ProcessExitButton()
	{
		Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
	}

	public override void UpdateStartBattleState()
	{
		StartBattleUi.SetButtonLabel(_dataProvider.GetStartButtonLabelText());
		StartBattleUiData.ButtonGroupType buttonGroupType = StartBattleUiData.ButtonGroupType.NoResource;
		StartBattleUi.SetStamina(buttonGroupType, _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType, StaminaType, StaminaCount);
		PunishmentGroupData punishmentGroupData = _dataProvider.GetPunishmentGroupData();
		if (punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Karma)
		{
			int shardItemIndex = _dataProvider.GetShardItemIndex();
			if (shardItemIndex == 0)
			{
				UIHelper.EnableButton(StartButton, enable: false);
				return;
			}
			if (GameRoot.Instance.ItemDataContainer.GetData(shardItemIndex) == null)
			{
				UIHelper.EnableButton(StartButton, enable: false);
				return;
			}
			int shardItemCount = _dataProvider.GetShardItemCount();
			if (punishmentGroupData.PunishmentDungeonType == EPunishmentDungeonType.Boss && shardItemCount < 1)
			{
				UIHelper.EnableButton(StartButton, enable: false);
				return;
			}
		}
		if (GetSquardTotalHeroIndexCount() == 0)
		{
			UIHelper.EnableButton(StartButton, enable: false);
		}
		else
		{
			UIHelper.EnableButton(StartButton, enable: true);
		}
	}

	public void RefreshShardSelectWidget()
	{
		ShardSelectWidget.SetShardUI();
		UpdateStartBattleState();
	}

	protected override void SetSquardEmptySlot()
	{
		int mainPartyCount = _raidData.MainPartyCount;
		int subPartyCount = _raidData.SubPartyCount;
		SquardUi.ESquardType eSquardType = SquardUi.ESquardType.MainSubShowAll;
		switch (base.RaidData.Type)
		{
		case RaidType.KarmaRaid:
			eSquardType = SquardUi.ESquardType.OnlyMain;
			break;
		case RaidType.PunishmentRaid:
			if (subPartyCount <= 0)
			{
				eSquardType = SquardUi.ESquardType.OnlyMain;
			}
			break;
		}
		SetSquardUI(eSquardType);
		_currentSquard = GetSquardUi(eSquardType).Squard;
		_currentSquard.InitSquard(mainPartyCount, subPartyCount, this);
		int mainSquardSlotCount = _currentSquard.GetMainSquardSlotCount();
		for (int i = 0; i < mainSquardSlotCount; i++)
		{
			PartySlotComponent mainEmptySlotByIndex = _currentSquard.GetMainEmptySlotByIndex(i);
			if (!(mainEmptySlotByIndex == null))
			{
				mainEmptySlotByIndex.Reset();
			}
		}
		PartySlotSpacing partySlotSpacing = null;
		if (eSquardType == SquardUi.ESquardType.OnlyMain)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(OnlyMainPartySlotSpacings); j++)
			{
				PartySlotSpacing partySlotSpacing2 = OnlyMainPartySlotSpacings[j];
				if (mainSquardSlotCount <= partySlotSpacing2.MaxSlotCount && mainSquardSlotCount >= partySlotSpacing2.MinSlotCount)
				{
					partySlotSpacing = partySlotSpacing2;
					break;
				}
			}
		}
		else
		{
			for (int k = 0; k < __Extension_Method_ICollection__.COUNT(PartySlotSpacings); k++)
			{
				PartySlotSpacing partySlotSpacing3 = PartySlotSpacings[k];
				if (mainSquardSlotCount <= partySlotSpacing3.MaxSlotCount && mainSquardSlotCount >= partySlotSpacing3.MinSlotCount)
				{
					partySlotSpacing = partySlotSpacing3;
					break;
				}
			}
		}
		UIGrid mainEmptySlotGrid = _currentSquard.GetMainEmptySlotGrid();
		if ((bool)mainEmptySlotGrid && partySlotSpacing != null)
		{
			mainEmptySlotGrid.cellWidth = partySlotSpacing.Spacing;
			mainEmptySlotGrid.Reposition();
		}
		mainSquardSlotCount = _currentSquard.GetSubSquardSlotCount();
		for (int l = 0; l < mainSquardSlotCount; l++)
		{
			PartySlotComponent subEmptySlotByIndex = _currentSquard.GetSubEmptySlotByIndex(l);
			if (!(subEmptySlotByIndex == null))
			{
				subEmptySlotByIndex.Reset();
			}
		}
		partySlotSpacing = null;
		if (eSquardType == SquardUi.ESquardType.OnlyMain)
		{
			for (int m = 0; m < __Extension_Method_ICollection__.COUNT(OnlyMainPartySlotSpacings); m++)
			{
				PartySlotSpacing partySlotSpacing4 = OnlyMainPartySlotSpacings[m];
				if (mainSquardSlotCount <= partySlotSpacing4.MaxSlotCount && mainSquardSlotCount >= partySlotSpacing4.MinSlotCount)
				{
					partySlotSpacing = partySlotSpacing4;
					break;
				}
			}
		}
		else
		{
			for (int n = 0; n < __Extension_Method_ICollection__.COUNT(PartySlotSpacings); n++)
			{
				PartySlotSpacing partySlotSpacing5 = PartySlotSpacings[n];
				if (mainSquardSlotCount <= partySlotSpacing5.MaxSlotCount && mainSquardSlotCount >= partySlotSpacing5.MinSlotCount)
				{
					partySlotSpacing = partySlotSpacing5;
					break;
				}
			}
		}
		mainEmptySlotGrid = _currentSquard.GetSubEmptySlotGrid();
		if ((bool)mainEmptySlotGrid && partySlotSpacing != null)
		{
			mainEmptySlotGrid.cellWidth = partySlotSpacing.Spacing;
			mainEmptySlotGrid.Reposition();
		}
		_currentSquard.SwichingSquard(isOnMainSquard: true);
		_currentSquard.InitAnimationSquardPostionAction(AnimateSquardPositionAction);
	}

	private void SetSquardUI(SquardUi.ESquardType squardType)
	{
		UIHelper.EnableGameObject(OnlyMainSquard, squardType == SquardUi.ESquardType.OnlyMain);
		UIHelper.EnableGameObject(MainSubSquard, squardType != SquardUi.ESquardType.OnlyMain);
		UIHelper.EnableGameObject(OnlyMainSquardFrame, squardType == SquardUi.ESquardType.OnlyMain);
		UIHelper.EnableGameObject(MainSubSquardFrame, squardType != SquardUi.ESquardType.OnlyMain);
	}

	public override void OnClickRaidInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickRaidInfo(this);
		}
	}

	public override void OnClickOpenHero()
	{
		if (_listener != null)
		{
			_listener.OnClickOpenHeroInfo();
		}
	}

	public override void OnClickOpenInventory()
	{
		if (_listener != null)
		{
			_listener.OnClickOpenInventoryInfo();
		}
	}

	public override void OnClickStartBattleButton()
	{
		if (_currentSquard != null && _listener != null)
		{
			_listener.OnClickStartPunishmentBattle(this);
		}
	}

	protected override bool CheckAutoNextBattle()
	{
		return true;
	}

	protected override void RequestStartBattle(ExtraOptionData extraOption)
	{
		if (!SprkDispatchSelection.CanUseParty(this) || _listener == null)
		{
			return;
		}
		EnterDungeonData enterDungeonData = ((_dataProvider != null) ? _dataProvider.GetEnterDungeon() : null);
		if (enterDungeonData == null)
		{
			return;
		}
		BattlePartyInfo battlePartyInfo = GetBattlePartyInfo();
		if (battlePartyInfo != null)
		{
			if (extraOption == null)
			{
				extraOption = new ExtraOptionData();
				extraOption.IsEndCampaign = false;
			}
			extraOption.AutoRepeat = false;
			PunishmentGroupData punishmentGroupData = _dataProvider.GetPunishmentGroupData();
			ShardPlaskInfo shardPlaskInfo = new ShardPlaskInfo();
			shardPlaskInfo.shardItemIndex = _dataProvider.GetShardItemIndex();
			shardPlaskInfo.shardItemCount = _dataProvider.GetShardItemCount();
			_dataProvider.SaveLastShardSetting(shardPlaskInfo.shardItemIndex, shardPlaskInfo.shardItemCount);
			_listener.StartPunishmentBattle(enterDungeonData, battlePartyInfo, shardPlaskInfo, punishmentGroupData);
		}
	}

	void ShardSelectWidget.Listener.OnClickShardSelect()
	{
		if (_listener != null)
		{
			PunishmentGroupData punishmentGroupData = _dataProvider.GetPunishmentGroupData();
			_listener.OnClickShardSelect(punishmentGroupData, _dataProvider);
		}
	}
}
