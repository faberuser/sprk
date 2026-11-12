using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NParty;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NLocalization;
using NShared.NParty;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RaidSinglePartySetting : RaidPartySetting
{
	public new interface Listener : RaidPartySetting.Listener, BattlePartySetting.Listener
	{
	}

	[Serializable]
	public class TeamEffect
	{
		public UITweener Scale;

		public UITweener Alpha;

		public void Stop()
		{
			if (Scale != null)
			{
				Scale.ResetToBeginning();
				UIHelper.EnableGameObject(Scale, enable: false);
			}
			if (Alpha != null)
			{
				Alpha.ResetToBeginning();
				UIHelper.EnableGameObject(Alpha, enable: false);
			}
		}

		public void Play()
		{
			if (Scale != null)
			{
				UIHelper.EnableGameObject(Scale, enable: true);
				Scale.PlayForwardReset();
			}
			if (Alpha != null)
			{
				UIHelper.EnableGameObject(Alpha, enable: true);
				Alpha.PlayForwardReset();
			}
		}
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Raid/RaidSinglePartySetting";

	public UILabel LabelRaidInfo;

	public UILabel LabelBossInfo;

	protected int _mainPartyMaxCount = 4;

	protected int _subPartyMaxCount = 4;

	public GameObject OnlyMainSquard;

	public GameObject MainSubSquard;

	public GameObject OnlyMainSquardFrame;

	public GameObject MainSubSquardFrame;

	private GuildSuppressDungeonData _guildSuppressDungeonData;

	protected bool RepeatBattle;

	public override int StaminaCount => _staminaCount;

	public GuildSuppressDungeonData GuildSuppressDungeonData => _guildSuppressDungeonData;

	protected int PartyMemberCountToSingle
	{
		get
		{
			if (_raidData == null)
			{
				return base.GetPartyMemberCount();
			}
			if (_raidData.MainPartyCount <= 0 || _raidData.SubPartyCount <= 0)
			{
				return base.GetPartyMemberCount();
			}
			return _raidData.MainPartyCount + _raidData.SubPartyCount;
		}
	}

	public void Init(Listener listener, IDataProvider dataProvider, RaidData raidData, GuildSuppressDungeonData guildSuppressDungeonData)
	{
		_guildSuppressDungeonData = guildSuppressDungeonData;
		Init(listener, dataProvider, raidData);
	}

	public void Init(Listener listener, IDataProvider dataProvider, RaidData raidData)
	{
		if (raidData != null && dataProvider != null)
		{
			RaidPartySetting.IsMultiRaid = false;
			RepeatBattle = false;
			_raidData = raidData;
			_listener = listener;
			_dataProvider = dataProvider;
			Init();
		}
	}

	private void Init()
	{
		BaseInit();
		SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyTopmostMenu(this);
		UpdateBanTypeData();
		SetSquardEmptySlot();
		SetSquardHeroList(out var mainHeroList, out var subHeroList);
		SetUserHeroList();
		if (_raidData.IsOnlineSingle)
		{
			RaidPartySetting.IsMultiRaid = true;
			InitPartyMemeberInfo(mainHeroList, subHeroList);
		}
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		_autoRepeatable = BattlePartySetting.AutoRepeatable(enterDungeon);
		RefreshStaminaCount();
		InitBanRuleText();
		InitDeco();
		UpdateStaminaCount();
		InitSingRaidPartyMember();
		RefreshSelectionMark();
		InitSingleRaidFriendSlot();
		UpdateStartBattleState();
		UpdateInviteButton();
		UpdateRoomSettingButton();
		UpdateBattleRepeatToggle();
		SetTitle();
		SetBanDec();
		SetRaidInfoButtonLabel();
		if ((bool)PartyChat)
		{
			PartyChat.Open(noParty: false, this);
			PartyChat.ClearChat();
		}
		_mainPartyMaxCount = _raidData.MainPartyCount;
		_subPartyMaxCount = _raidData.SubPartyCount;
		if (enterDungeon != null && enterDungeon.LinkedDungeonData != null && DispatchHelper.IsDispatchable(enterDungeon.LinkedDungeonData))
		{
			UserHeroListPanel.SetDispatch();
		}
		UpdateBanHeroPortrait();
	}

	public void ChangeSingleRoomSetting(RaidData raidData, int[] affixindices)
	{
		bool flag = false;
		if (base.RaidData.Type == RaidType.ChallengeSingle)
		{
			bool flag2 = false;
			if (_raidData.Level != raidData.Level)
			{
				flag2 = true;
			}
			if (!AffixHelper.IsEqualAffixIndices(_dataProvider.GetAffixIndices(), affixindices))
			{
				flag2 = true;
				List<BanRuleData> banRuleDataList = AffixHelper.GetBanRuleDataList(affixindices);
				List<int> totalHeroIndexList = _currentSquard.GetTotalHeroIndexList();
				flag = AffixHelper.CheckBanRule(banRuleDataList, totalHeroIndexList);
			}
			if (flag2)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("CHALLENGE_RAID_CHANGE_DESC"));
			}
		}
		_raidData = raidData;
		_dataProvider = SceneAutoBehaviour<RaidManagement>.instance.CreateDataProvider(raidData, affixindices, isStorageKey: true);
		SetTitle();
		UpdateStaminaCount();
		UpdateStartBattleState();
		UpdateBanTypeData();
		bool isViolateBanRule = flag || IsViolateBanRule();
		ChangedRoomInfoUpdateBanRule(isViolateBanRule);
	}

	public void ChangeSingleRoomSetting(RaidData raidData)
	{
		_raidData = raidData;
		_dataProvider = SceneAutoBehaviour<RaidManagement>.instance.GetDefaultBattlePartySettingDataProdiverByRaidData(raidData, isStorageKey: true);
		SetTitle();
		UpdateStaminaCount();
		UpdateStartBattleState();
		UpdateBanTypeData();
		ChangedRoomInfoUpdateBanRule(IsViolateBanRule());
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
		if ((bool)PartyChat)
		{
			PartyChat.Close();
		}
	}

	public override void UpdateDerived()
	{
		UpdatePartyChatInputState();
		base.UpdateDerived();
	}

	protected override void RequestStartBattle(ExtraOptionData extraOption)
	{
		if (!SprkDispatchSelection.CanUseParty(this) || !RestoredPortal.CheckDragonHardEntry(base.RaidData))
		{
			return;
		}
		if (_raidData.IsOnlineSingle)
		{
			base.RequestStartBattle(extraOption);
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
				extraOption.AutoRepeat = RepeatBattle;
				extraOption.IsEndCampaign = false;
			}
			else
			{
				extraOption.AutoRepeat = RepeatBattle;
			}
			if (_listener != null)
			{
				_listener.StartBattle(enterDungeonData, battlePartyInfo, extraOption);
			}
		}
	}

	protected BattlePartyInfo GetBattlePartyInfo()
	{
		BattlePartyInfo battlePartyInfo = _currentSquard.GetBattlePartyInfo();
		if (_dataProvider != null)
		{
			battlePartyInfo.StaminaType = _dataProvider.GetStaminaType();
			battlePartyInfo.StaminaCount = _dataProvider.GetStaminaConsumeCount();
			battlePartyInfo.AffixIndices = _dataProvider.GetAffixIndices();
		}
		return battlePartyInfo;
	}

	private void InitPartyMemeberInfo(List<CachedHeroInfo> mainHeroList, List<CachedHeroInfo> subHeroList)
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(mainHeroList); i++)
		{
			CachedHeroInfo cachedHeroInfo = mainHeroList[i];
			if (cachedHeroInfo != null)
			{
				GlobalAutoBehaviour<PartyManager>.instance.AddDeckHero(myPartyMemberInfo, cachedHeroInfo.HeroIndex, cachedHeroInfo, isMain: true);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(subHeroList); j++)
		{
			CachedHeroInfo cachedHeroInfo2 = subHeroList[j];
			if (cachedHeroInfo2 != null)
			{
				GlobalAutoBehaviour<PartyManager>.instance.AddDeckHero(myPartyMemberInfo, cachedHeroInfo2.HeroIndex, cachedHeroInfo2, isMain: false);
			}
		}
	}

	protected override void SetSquardEmptySlot()
	{
		int mainPartyCount = _raidData.MainPartyCount;
		int subPartyCount = _raidData.SubPartyCount;
		SquardUi.ESquardType eSquardType = SquardUi.ESquardType.MainSubShowAll;
		switch (base.RaidData.Type)
		{
		case RaidType.FieldRaidSingle:
			if (subPartyCount == 0)
			{
				eSquardType = SquardUi.ESquardType.OnlyMain;
			}
			break;
		case RaidType.KarmaRaid:
			eSquardType = SquardUi.ESquardType.OnlyMain;
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
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(PartySlotSpacings); j++)
		{
			PartySlotSpacing partySlotSpacing2 = PartySlotSpacings[j];
			if (mainSquardSlotCount <= partySlotSpacing2.MaxSlotCount && mainSquardSlotCount >= partySlotSpacing2.MinSlotCount)
			{
				partySlotSpacing = partySlotSpacing2;
				break;
			}
		}
		UIGrid mainEmptySlotGrid = _currentSquard.GetMainEmptySlotGrid();
		if ((bool)mainEmptySlotGrid && partySlotSpacing != null)
		{
			mainEmptySlotGrid.cellWidth = partySlotSpacing.Spacing;
			mainEmptySlotGrid.Reposition();
		}
		mainSquardSlotCount = _currentSquard.GetSubSquardSlotCount();
		for (int k = 0; k < mainSquardSlotCount; k++)
		{
			PartySlotComponent subEmptySlotByIndex = _currentSquard.GetSubEmptySlotByIndex(k);
			if (!(subEmptySlotByIndex == null))
			{
				subEmptySlotByIndex.Reset();
			}
		}
		partySlotSpacing = null;
		for (int l = 0; l < __Extension_Method_ICollection__.COUNT(PartySlotSpacings); l++)
		{
			PartySlotSpacing partySlotSpacing3 = PartySlotSpacings[l];
			if (mainSquardSlotCount <= partySlotSpacing3.MaxSlotCount && mainSquardSlotCount >= partySlotSpacing3.MinSlotCount)
			{
				partySlotSpacing = partySlotSpacing3;
				break;
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

	protected override void SetSquardHeroList(out List<CachedHeroInfo> mainHeroList, out List<CachedHeroInfo> subHeroList)
	{
		mainHeroList = new List<CachedHeroInfo>();
		subHeroList = new List<CachedHeroInfo>();
		if (!IsLocalSaveEnabled())
		{
			return;
		}
		GetMyHeroPortraitComponentOwnerInfo();
		List<CachedHeroInfo> heroInfoList = GetHeroInfoList(isMainSquard: true);
		List<CachedHeroInfo> heroInfoList2 = GetHeroInfoList(isMainSquard: false);
		if (BattleType == BattleType.ChallengeRaidSingle)
		{
			List<BanRuleData> banRuleDataList = _dataProvider.GetBanRuleDataList();
			List<int> list = new List<int>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfoList); i++)
			{
				CachedHeroInfo cachedHeroInfo = heroInfoList[i];
				if (cachedHeroInfo != null)
				{
					list.Add(cachedHeroInfo.HeroIndex);
				}
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(heroInfoList2); j++)
			{
				CachedHeroInfo cachedHeroInfo2 = heroInfoList2[j];
				if (cachedHeroInfo2 != null)
				{
					list.Add(cachedHeroInfo2.HeroIndex);
				}
			}
			if (AffixHelper.CheckBanRule(banRuleDataList, list))
			{
				SaveSquardHeroListToLocal();
				return;
			}
		}
		if (CurrentRaidBanType == RaidBanType.ClassEnterPermission && ClassEnterPermissionData != null)
		{
			List<CreatureTagType> list2 = new List<CreatureTagType>();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoList))
			{
				list2.AddRange(heroInfoList.Select((CachedHeroInfo cachedHeroInfo3) => cachedHeroInfo3.CreatureData.TagType));
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoList2))
			{
				list2.AddRange(heroInfoList2.Select((CachedHeroInfo cachedHeroInfo3) => cachedHeroInfo3.CreatureData.TagType));
			}
			Dictionary<CreatureTagType, int> dictionary = new Dictionary<CreatureTagType, int>();
			for (int num = 0; num < list2.Count; num++)
			{
				CreatureTagType creatureTagType = list2[num];
				int vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, creatureTagType, 0);
				if (dictionary.ContainsKey(creatureTagType))
				{
					dictionary[creatureTagType] = vALUE + 1;
				}
				else
				{
					dictionary.Add(creatureTagType, vALUE + 1);
				}
			}
			for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(ClassEnterPermissionData.LimitEnterClassList); num2++)
			{
				CreatureTagType i_key = ClassEnterPermissionData.LimitEnterClassList[num2];
				if (Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, i_key, 0) > ClassEnterPermissionData.PossibleEnterCount)
				{
					return;
				}
			}
		}
		mainHeroList = GetSquardHeroList(heroInfoList, isMain: true);
		subHeroList = GetSquardHeroList(heroInfoList2, isMain: false);
	}

	private List<CachedHeroInfo> GetSquardHeroList(List<CachedHeroInfo> heroInfoList, bool isMain)
	{
		HeroPortraitComponent.OwnerInfo myHeroPortraitComponentOwnerInfo = GetMyHeroPortraitComponentOwnerInfo();
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		list.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfoList); i++)
		{
			CachedHeroInfo cachedHeroInfo = heroInfoList[i];
			if (cachedHeroInfo != null && _currentSquard != null)
			{
				Vector3 position = (isMain ? _currentSquard.GetMainEmptySlotPositionByIndex(i) : _currentSquard.GetSubEmptySlotPositionByIndex(i));
				CreateSquardHeroPortraitAnim(isMain, cachedHeroInfo, position, myHeroPortraitComponentOwnerInfo);
				list.Add(cachedHeroInfo);
			}
		}
		return list;
	}

	private void SetSquardUI(SquardUi.ESquardType squardType)
	{
		UIHelper.EnableGameObject(OnlyMainSquard, squardType == SquardUi.ESquardType.OnlyMain);
		UIHelper.EnableGameObject(MainSubSquard, squardType != SquardUi.ESquardType.OnlyMain);
		UIHelper.EnableGameObject(OnlyMainSquardFrame, squardType == SquardUi.ESquardType.OnlyMain);
		UIHelper.EnableGameObject(MainSubSquardFrame, squardType != SquardUi.ESquardType.OnlyMain);
	}

	private void InitSingRaidPartyMember()
	{
		MemberInfo memberInfo = new MemberInfo();
		memberInfo.MemberIndex = 0;
		memberInfo.IsMaster = true;
		memberInfo.Nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick;
		memberInfo.AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		PartyMemberNameComponent partyMemberNameComponent = UIHelper.AddGridItem(PartyMemberNameGrid, PartyMemberNameComponent.PrefabAuto);
		partyMemberNameComponent.Open(this, memberInfo);
		_partyMemberNameComponentList.Add(partyMemberNameComponent);
		UpdateInviteButton();
		UIHelper.RepositionGrid(PartyMemberNameGrid);
	}

	private void InitSingleRaidFriendSlot()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(LockedLabels))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(LockedLabels); i++)
			{
				UIHelper.EnableGameObject(LockedLabels[i], enable: true);
			}
			UIHelper.EnableGameObject(LockedLabels.First(), enable: false);
		}
	}

	private new void UpdatePartyChatInputState()
	{
		if (_listener is Listener listener && !(PartyChat == null) && !(PartyChat.Input == null))
		{
			bool enable = listener.CanInputPartyChat();
			UIHelper.EnableGameObject(PartyChat.Input.gameObject, enable);
		}
	}

	public void OnClickMainPartyButton()
	{
		_currentSquard.SwichingSquard(isOnMainSquard: true);
	}

	public void OnClickSubPartyButton()
	{
		_currentSquard.SwichingSquard(isOnMainSquard: false);
	}

	public override void ToggleBattleRepeat()
	{
		if (base.RaidData == null || base.RaidData.Type == RaidType.Challenge || base.RaidData.Type == RaidType.ChallengeSingle || base.RaidData.Type == RaidType.GuildSuppressSingle || base.RaidData.Type == RaidType.PunishmentRaid)
		{
			return;
		}
		if (!RepeatBattle)
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string stringDirect2 = LocalizationManager.GetStringDirect("REPEATBATTLE_INFORM_MULTI");
			MessagePopup.Open(stringDirect, stringDirect2, delegate
			{
				RepeatBattle = !RepeatBattle;
				UpdateBattleRepeatToggle();
			}, null);
		}
		else
		{
			RepeatBattle = !RepeatBattle;
			UpdateBattleRepeatToggle();
		}
	}

	private void SetRaidInfoButtonLabel()
	{
		bool flag = base.RaidData.Type == RaidType.GuildSuppressSingle;
		UIHelper.EnableGameObject(LabelRaidInfo, !flag);
		UIHelper.EnableGameObject(LabelBossInfo, flag);
		if (!flag)
		{
			SetInfoButtonLabel(LabelRaidInfo);
		}
	}

	protected override int GetPartyMemberCount()
	{
		return PartyMemberCountToSingle;
	}

	protected override int GetMaxPartyMemberPerPlayer()
	{
		if (_raidData == null || _raidData.DeckCountPerPlayer <= 0)
		{
			return base.GetMaxPartyMemberPerPlayer();
		}
		return _raidData.DeckCountPerPlayer;
	}

	protected override bool IsLocalSaveEnabled()
	{
		return true;
	}

	protected override bool IsSubPartyEnabled()
	{
		return true;
	}

	public override void UpdateStartBattleState()
	{
		StartBattleUi.SetButtonLabel(_dataProvider.GetStartButtonLabelText());
		StartBattleUiData.ButtonGroupType buttonGroupType = ((StaminaType == StaminaType.Empty) ? StartBattleUiData.ButtonGroupType.NoResource : StartBattleUiData.ButtonGroupType.UseResource);
		StartBattleUi.SetStamina(buttonGroupType, _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType, StaminaType, StaminaCount);
		if (GetSquardTotalHeroIndexCount() == 0)
		{
			UIHelper.EnableButton(StartButton, enable: false);
		}
		else
		{
			UIHelper.EnableButton(StartButton, enable: true);
		}
	}

	protected override void UpdateBattleRepeatToggle()
	{
		if (base.RaidData == null || base.RaidData.Type == RaidType.PunishmentRaid)
		{
			return;
		}
		bool flag = base.RaidData.Type == RaidType.Challenge || base.RaidData.Type == RaidType.ChallengeSingle;
		UIHelper.EnableGameObject(Widget_AffixInfo, flag);
		UIHelper.EnableGameObject(Widget_Repeat, !flag);
		if (!flag)
		{
			if (base.RaidData.Type == RaidType.GuildSuppressSingle || base.RaidData.Type == RaidType.PunishmentRaid)
			{
				UIHelper.EnableGameObject(SpriteBattleRepeatUnusable, enable: true);
				UIHelper.EnableGameObject(SpriteBattleRepeatChecked, enable: false);
			}
			else
			{
				UIHelper.EnableGameObject(SpriteBattleRepeatUnusable, enable: false);
				UIHelper.EnableGameObject(SpriteBattleRepeatChecked, enable: false);
				UIHelper.EnableGameObject(SpriteBattleRepeatChecked, RepeatBattle);
			}
		}
	}

	protected bool CheckPossibleInsertSquard(HeroPortraitComponent deckPortrait)
	{
		if (deckPortrait == null || deckPortrait.HeroInfo == null)
		{
			return false;
		}
		int num = 0;
		List<int> list = null;
		List<int> list2 = null;
		if (_currentSquard.IsSelectedMainSquard())
		{
			num = _currentSquard.GetMainSquardSlotCount();
			list = _currentSquard.GetMainSquardTotalHeroIndexList();
			list2 = _currentSquard.GetMainSquardMyHeroIndexList();
		}
		else
		{
			num = _currentSquard.GetSubSquardSlotCount();
			list = _currentSquard.GetSubSquardTotalHeroIndexList();
			list2 = _currentSquard.GetSubSquardMyHeroIndexList();
		}
		if (num <= __Extension_Method_ICollection__.COUNT(list))
		{
			return false;
		}
		if (GetMaxPartyMemberPerPlayer() <= __Extension_Method_ICollection__.COUNT(list2))
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.CANNOT_INSERT_MORE));
			return false;
		}
		return true;
	}

	protected override bool EnableInsertSquard(HeroPortraitComponent deckPortrait)
	{
		if (!RestoredPortal.CheckRaidHeroLevel(this, deckPortrait))
		{
			return false;
		}
		if (!CheckPossibleInsertSquard(deckPortrait))
		{
			return false;
		}
		bool num = _currentSquard.IsSelectedMainSquard();
		bool flag = _currentSquard.IsContainSquardHero(deckPortrait.HeroInfo.HeroIndex, 0L);
		Enum key = (Enum)((!num) ? ((object)Battle.HERO_ALREADY_EXIST_MAIN) : ((object)Battle.HERO_ALREADY_EXIST_SUB));
		if (flag)
		{
			CenterMessage.Open(LocalizationManager.GetString(key));
			return false;
		}
		return true;
	}

	public override bool InsertSquardHero(HeroPortraitComponent deckPortrait, bool isMainSquard)
	{
		return InsertSquardHeroToSingle(deckPortrait, isMainSquard);
	}

	protected bool InsertSquardHeroToSingle(HeroPortraitComponent deckPortrait, bool isMainSquard)
	{
		if (!EnableInsertSquard(deckPortrait))
		{
			return false;
		}
		PlayHeroSelectSound(deckPortrait.HeroInfo);
		Common.HeroPortraitAnim heroPortraitAnim = CreateSquardHeroPortraitAnim(isMainSquard, deckPortrait.HeroInfo, deckPortrait.transform.position, deckPortrait.Owner);
		_currentSquard.AnimateSqaurdPosition(heroPortraitAnim);
		RefreshSelectionMark();
		SaveSquardHeroListToLocal();
		UpdateStartBattleState();
		UpdateBanHeroPortrait();
		UpdateBanRuleLabel();
		if (base.RaidData.IsOnlineSingle)
		{
			MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
			GlobalAutoBehaviour<PartyManager>.instance.AddDeckHero(myPartyMemberInfo, deckPortrait.HeroInfo.HeroIndex, deckPortrait.HeroInfo, isMainSquard);
		}
		if (!isMainSquard)
		{
			return true;
		}
		if (__Extension_Method_ICollection__.COUNT(_currentSquard.GetMainSquardMyHeroIndexList()) >= _mainPartyMaxCount && _subPartyMaxCount > 0)
		{
			VespaCoroutine.Start(Co_EnableSubParty());
		}
		return true;
	}

	protected override void AnimateSquardPositionAction(List<Common.HeroPortraitAnim> heroPortraitList, List<PartySlotComponent> emptySlotList, Common.HeroPortraitAnim heroPortraitAnim)
	{
		AnimateSquardPositionActionToSingle(heroPortraitList, emptySlotList, heroPortraitAnim);
	}

	protected void AnimateSquardPositionActionToSingle(List<Common.HeroPortraitAnim> heroPortraitList, List<PartySlotComponent> emptySlotList, Common.HeroPortraitAnim heroPortraitAnim)
	{
		base.AnimateSquardPositionAction(heroPortraitList, emptySlotList, heroPortraitAnim);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroPortraitList); i++)
		{
			if (i < __Extension_Method_ICollection__.COUNT(heroPortraitList))
			{
				PartySlotComponent partySlotComponent = emptySlotList[i];
				if (!(partySlotComponent == null))
				{
					partySlotComponent.SetMemberIndex(-1);
				}
			}
		}
	}

	public override bool RemoveSquardHero(int heroIndex, long ownerId = 0L, CreatureTagType classType = CreatureTagType.None, bool ignoreSelection = false)
	{
		return RemoveSquardHeroToSingle(heroIndex, ownerId, classType, ignoreSelection);
	}

	protected bool RemoveSquardHeroToSingle(int heroIndex, long ownerId = 0L, CreatureTagType classType = CreatureTagType.None, bool ignoreSelection = false)
	{
		if (base.RaidData.IsOnlineSingle)
		{
			bool isMain = true;
			if (_currentSquard.GetSubSquardMyHeroIndexList().Contains(heroIndex))
			{
				isMain = false;
			}
			MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
			GlobalAutoBehaviour<PartyManager>.instance.RemoveDeckHero(myPartyMemberInfo, heroIndex, isMain);
		}
		return base.RemoveSquardHero(heroIndex, ownerId, classType, ignoreSelection);
	}

	protected override void RemoveAllHeroPortraitList()
	{
		List<Common.HeroPortraitAnim> allHeroPortraitAnimList = _currentSquard.GetAllHeroPortraitAnimList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allHeroPortraitAnimList); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = allHeroPortraitAnimList[i];
			if (heroPortraitAnim != null)
			{
				RemoveSquardHero(heroPortraitAnim.HeroIndex, heroPortraitAnim.Component.OwnerId, CreatureTagType.None, ignoreSelection: true);
			}
		}
	}

	private IEnumerator Co_EnableSubParty()
	{
		yield return new WaitForSeconds(0.3f);
		OnClickSubPartyButton();
		if (_switchSquardAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(_switchSquardAction);
		}
	}

	protected override bool CheckStartMemberCount()
	{
		return true;
	}
}
