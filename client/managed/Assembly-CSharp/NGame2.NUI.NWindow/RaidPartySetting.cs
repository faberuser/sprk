using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NParty;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NChatting;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RaidPartySetting : BattlePartySetting, PartyManager.IPartyListener, PartyMemberNameComponent.Listener, ChattingPartyComponent.Listener, ChattingListComponentBase.Listener, ChattingEmoticonSelectPopup.Listener, LobbyTopmostMenu.IUserStatObserver
{
	public new interface Listener : BattlePartySetting.Listener
	{
		void OnClickInvite(RaidPartySetting caller);

		void OnClickRaidInfo(RaidPartySetting caller);

		void OnClickPartyMemberName(MemberInfo memberInfo);

		void OnPartyMemberJoin(MemberInfo memberInfo);

		void OnPartyMemberLeave(long accountId);

		void OnAutoDestroyGuildRaidParty();

		void OnSubmitPartyChat(string chat);

		List<ChatInfo> GetPartyChat();

		bool CanInputPartyChat();

		void OnClickEmoticonSelect(RaidPartySetting caller);

		void OnClickRoomSetting(int raidIndex, int raidLevel);

		void OnClickSingleRoomSetting(int raidIndex, int raidLevel);

		BanRuleData GetRaidBanData(RaidData raidData);

		void OnEnterRaidPartySetting(RaidData raidData);

		void OnClickOpenHero();

		void OnClickOpenInventory();

		void OnClickStartBattle(RaidPartySetting caller);

		void OnClickClose(RaidPartySetting caller);

		void OnClickAffixSetting(int raidindex, int raidLevel, int[] affixIndices, bool canChange);
	}

	[Serializable]
	public class PartySlotSpacing
	{
		public int MinSlotCount;

		public int MaxSlotCount;

		public int Spacing;
	}

	public class HeroCostumeChangedInfoChecker
	{
		public bool ReserveSendPartyCostumeChanged;

		public NGame2.NAccount.UserHeroManager.ChangeHeroInfo ChangedInfo;
	}

	public class ClassEnterPermissionBanTypeData
	{
		public int PossibleEnterCount;

		public List<CreatureTagType> LimitEnterClassList;

		public string RuleTactic;
	}

	public class BanTypeInfo<T>
	{
		public RaidBanType BanType;

		public int PossibleHeroCount;

		public List<T> LimitBanTypeList;
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Raid/RaidPartySetting";

	public UIButton StartButton;

	public UISimpleGrid PartyMemberNameGrid;

	public Transform InviteButton;

	public bool EnableObserver;

	public Transform[] LockedLabels;

	public PartySlotSpacing[] PartySlotSpacings;

	public ChattingPartyComponent PartyChat;

	public UILabel TitleLabel;

	public UILabel LevelLabel;

	public UILabel HardModeLabel;

	public UILabel ChallengeDiffLabel;

	public UILabel ChallengeRaidPointLabel;

	public TweenAlpha BanRuleTweenAlpha;

	public Transform BanRuleRoot;

	public UILabel BanRuleLabel;

	public UILabelAligner Aligner;

	public UIButton RoomSettingButton;

	public Transform RaidDeco;

	public Transform ChallengeRaidDeco;

	public Transform TechnoMagicRaidDeco;

	public Transform EmoticonSelectPopupRoot;

	protected RaidData _raidData;

	protected List<PartyMemberNameComponent> _partyMemberNameComponentList = new List<PartyMemberNameComponent>();

	public static bool IsMultiRaid = true;

	public UISprite SpriteBattleRepeatChecked;

	public UISprite SpriteBattleRepeatUnusable;

	private HeroCostumeChangedInfoChecker _heroCostumeChangedInfoChecker = new HeroCostumeChangedInfoChecker();

	public UILabel InfoButtonLabel;

	public ClassEnterPermissionBanTypeData ClassEnterPermissionData;

	public RaidBanType CurrentRaidBanType;

	public UIWidget Widget_AffixInfo;

	public UIWidget Widget_Repeat;

	protected bool _isEnterHeroOnce;

	public override int StaminaCount => GetStaminaCount();

	public override StaminaType StaminaType
	{
		get
		{
			if (_raidData != null && _raidData.Type == RaidType.Guild)
			{
				return StaminaType.None;
			}
			return base.StaminaType;
		}
	}

	public UserManager.UserStatType StatType
	{
		get
		{
			if (RaidData == null)
			{
				return UserManager.UserStatType.None;
			}
			if (RaidData.Type == RaidType.Challenge || RaidData.Type == RaidType.ChallengeSingle || RaidData.Type == RaidType.GuildSuppress || RaidData.Type == RaidType.GuildSuppressSingle)
			{
				return UserManager.UserStatType.Gold;
			}
			if (RaidData.Type == RaidType.PunishmentRaid || RaidData.Type == RaidType.KarmaRaid)
			{
				return UserManager.UserStatType.None;
			}
			return UserManager.UserStatType.RaidPoint;
		}
	}

	public RaidData RaidData => _raidData;

	protected int PartyMemberCountToMulti
	{
		get
		{
			if (_raidData == null || _raidData.DeckCount <= 0)
			{
				return base.GetPartyMemberCount();
			}
			return _raidData.DeckCount;
		}
	}

	protected int MaxPartMemberPerPlayer
	{
		get
		{
			if (_raidData == null || _raidData.DeckCountPerPlayer <= 0)
			{
				return base.GetMaxPartyMemberPerPlayer();
			}
			return _raidData.DeckCountPerPlayer;
		}
	}

	private bool IgnoreSelection
	{
		get
		{
			if (IsMultiRaid)
			{
				if (BattleType != BattleType.ArcdimRaid)
				{
					return BattleType == BattleType.ArcdimHardRaid;
				}
				return true;
			}
			return false;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (!(PartyMemberNameGrid == null))
		{
			PartyMemberNameGrid.Grid.onCustomSort = SortMemberNameGrid;
		}
	}

	public void InitForRaid(Listener listener, IDataProvider dataProvider, RaidData raidData)
	{
		if (base.CreatureCreateInfoList != null && base.CreatureCreateInfoList.Count > 0)
		{
			base.CreatureCreateInfoList.Clear();
		}
		if (raidData == null || dataProvider == null)
		{
			return;
		}
		if (!GlobalAutoBehaviour<PartyManager>.isValidInstance || !GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			VespaDebug.LogError("Party must exist before RaidPartySetting.InitForRaid()");
			return;
		}
		IsMultiRaid = true;
		_raidData = raidData;
		_isEnterHeroOnce = false;
		Init(listener, dataProvider);
		GlobalAutoBehaviour<PartyManager>.instance.AddPartyListener(this);
		GlobalAutoBehaviour<PartyManager>.instance.SetRaid(raidData.Index, raidData.Level);
		UpdateBanTypeData();
		UpdateBanHeroPortrait();
		InitBanRuleText();
		InitDeco();
		UpdateStaminaCount();
		InitFriendSlot();
		UpdateStartBattleState();
		UpdateInviteButton();
		UpdateRoomSettingButton();
		UpdateBattleRepeatToggle();
		SetTitle();
		SetInfoButtonLabel(InfoButtonLabel);
		if ((bool)PartyChat)
		{
			PartyChat.Open(noParty: false, this);
			PartyChat.ClearChat();
		}
		SetBanDec();
		listener?.OnEnterRaidPartySetting(_raidData);
	}

	public override void Close()
	{
		base.Close();
	}

	protected override void SetSquardEmptySlot()
	{
		if (RaidData == null)
		{
			return;
		}
		_currentSquard = GetSquardUi(SquardUi.ESquardType.OnlyMain).Squard;
		if (_currentSquard != null)
		{
			_currentSquard.InitSquard(RaidData.DeckCount, 0, this);
		}
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
		_currentSquard.InitAnimationSquardPostionAction(AnimateSquardPositionAction);
	}

	protected virtual void UpdateBanTypeData()
	{
		ClassEnterPermissionData = null;
		CurrentRaidBanType = RaidBanType.None;
		BattleType battleType = BattleType;
		if (battleType == BattleType.ChallengeRaidSingle || battleType == BattleType.ChallengeRaid)
		{
			ClassEnterPermissionData = _dataProvider.GetClassEnterPermissionBanTypeData();
			if (ClassEnterPermissionData != null)
			{
				CurrentRaidBanType = RaidBanType.ClassEnterPermission;
			}
			return;
		}
		BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(_raidData.BanIndex);
		if (data == null)
		{
			return;
		}
		RaidBanRuleData[] banRuleDataAry = data.GetBanRuleDataAry();
		for (int i = 0; i < banRuleDataAry.Length; i++)
		{
			if (banRuleDataAry[i].BanType == RaidBanType.ClassEnterPermission)
			{
				Tuple<List<CreatureTagType>, int> classEnterBanRuleData = RaidHelper.GetClassEnterBanRuleData(banRuleDataAry[i]);
				if (classEnterBanRuleData != null)
				{
					ClassEnterPermissionData = new ClassEnterPermissionBanTypeData();
					ClassEnterPermissionData.LimitEnterClassList = classEnterBanRuleData.Item1;
					ClassEnterPermissionData.PossibleEnterCount = classEnterBanRuleData.Item2;
					ClassEnterPermissionData.RuleTactic = data.RuleTactic;
					CurrentRaidBanType = RaidBanType.ClassEnterPermission;
				}
			}
		}
	}

	protected override bool CanBanClassEnter(HeroPortraitComponent heroPortrait)
	{
		if (ClassEnterPermissionData == null)
		{
			return true;
		}
		if (CurrentRaidBanType != RaidBanType.ClassEnterPermission)
		{
			return true;
		}
		if (heroPortrait.IsSelected)
		{
			return true;
		}
		CreatureTagType tagType = heroPortrait.CreatureData.TagType;
		if (!ClassEnterPermissionData.LimitEnterClassList.Contains(tagType))
		{
			return true;
		}
		List<int> list = _currentSquard.GetMainSquardTotalHeroIndexList();
		if (list == null)
		{
			list = _currentSquard.GetSubSquardTotalHeroIndexList();
		}
		else
		{
			List<int> subSquardTotalHeroIndexList = _currentSquard.GetSubSquardTotalHeroIndexList();
			if (subSquardTotalHeroIndexList != null)
			{
				list.AddRange(subSquardTotalHeroIndexList);
			}
		}
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(list[i]);
			if (data != null && data.TagType == tagType)
			{
				num++;
			}
		}
		if (num >= ClassEnterPermissionData.PossibleEnterCount)
		{
			return false;
		}
		return true;
	}

	protected override void ShowEnterClassBanMessage(HeroPortraitComponent heroPortrait = null)
	{
		BattleType battleType = BattleType;
		if (battleType != BattleType.ChallengeRaidSingle && battleType != BattleType.ChallengeRaid && GameRoot.Instance.BanRuleDataContainer.GetData(_raidData.BanIndex) != null && CurrentRaidBanType == RaidBanType.ClassEnterPermission)
		{
			NGame2.NUI.Common.ShowEnterClassBanMessage(CurrentRaidBanType, ClassEnterPermissionData.RuleTactic, ClassEnterPermissionData.PossibleEnterCount);
		}
	}

	protected override void UpdateBanHeroPortrait()
	{
		BattleType battleType = BattleType;
		if (battleType == BattleType.ChallengeRaidSingle || battleType == BattleType.ChallengeRaid)
		{
			if (UserHeroListPanel != null)
			{
				UserHeroListPanel.AllUnBan();
			}
			UpdateClassEnterBanHeroPortrait();
			UpdateAffixBanDec();
		}
		else
		{
			UpdateClassEnterBanHeroPortrait();
		}
	}

	protected void UpdateAffixBanDec()
	{
		BattleType battleType = BattleType;
		if (battleType != BattleType.ChallengeRaidSingle && battleType != BattleType.ChallengeRaid)
		{
			return;
		}
		List<BanRuleData> banRuleDataList = _dataProvider.GetBanRuleDataList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(banRuleDataList); i++)
		{
			BanRuleData banRuleData = banRuleDataList[i];
			if (banRuleData != null && UserHeroListPanel != null)
			{
				UserHeroListPanel.SetBanUserHeroList(banRuleData);
			}
		}
	}

	protected virtual void UpdateClassEnterBanHeroPortrait()
	{
		if (ClassEnterPermissionData == null || CurrentRaidBanType != RaidBanType.ClassEnterPermission)
		{
			return;
		}
		List<int> list = _currentSquard.GetMainSquardTotalHeroIndexList();
		if (list == null)
		{
			list = _currentSquard.GetSubSquardTotalHeroIndexList();
		}
		else
		{
			List<int> subSquardTotalHeroIndexList = _currentSquard.GetSubSquardTotalHeroIndexList();
			if (subSquardTotalHeroIndexList != null)
			{
				list.AddRange(subSquardTotalHeroIndexList);
			}
		}
		Dictionary<CreatureTagType, int> dictionary = new Dictionary<CreatureTagType, int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(list[i]);
			if (data != null)
			{
				int vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, data.TagType, 0);
				if (dictionary.ContainsKey(data.TagType))
				{
					dictionary[data.TagType] = vALUE + 1;
				}
				else
				{
					dictionary.Add(data.TagType, vALUE + 1);
				}
			}
		}
		List<CreatureTagType> listOrderCreatureTagType = FilterHelper.GetListOrderCreatureTagType();
		for (int j = 0; j < listOrderCreatureTagType.Count; j++)
		{
			if (ClassEnterPermissionData.LimitEnterClassList.Contains(listOrderCreatureTagType[j]) && Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, listOrderCreatureTagType[j], 0) >= ClassEnterPermissionData.PossibleEnterCount)
			{
				UserHeroListPanel.BanByType(listOrderCreatureTagType[j], list);
			}
			else
			{
				UserHeroListPanel.UnBanByTag(listOrderCreatureTagType[j]);
			}
		}
	}

	protected virtual void InitBanRuleText(bool isForceShow = false)
	{
		if (BanRuleRoot == null || BanRuleLabel == null || BanRuleTweenAlpha == null)
		{
			return;
		}
		BanRuleTweenAlpha.value = BanRuleTweenAlpha.to;
		_isEnterHeroOnce = true;
		if (ClassEnterPermissionData != null && (isForceShow || (__Extension_Method_ICollection__.COUNT(_currentSquard.GetMainSquardTotalHeroIndexList()) <= 0 && __Extension_Method_ICollection__.COUNT(_currentSquard.GetSubSquardTotalHeroIndexList()) <= 0)))
		{
			bool flag = !String_Extension_Method.IsNullOrEmpty(ClassEnterPermissionData.RuleTactic);
			UIHelper.EnableGameObject(BanRuleRoot, flag);
			if (flag)
			{
				string stringDirect = LocalizationManager.GetStringDirect(ClassEnterPermissionData.RuleTactic, ClassEnterPermissionData.PossibleEnterCount);
				UIHelper.SetLabelText(BanRuleLabel, stringDirect);
				BanRuleTweenAlpha.ResetToBeginning();
				BanRuleTweenAlpha.enabled = false;
				_isEnterHeroOnce = false;
			}
		}
	}

	protected void UpdateBanRuleLabel()
	{
		if (!(BanRuleRoot == null) && !(BanRuleLabel == null) && !(BanRuleTweenAlpha == null) && UIHelper.IsEnabledGameObjet(BanRuleRoot) && !_isEnterHeroOnce)
		{
			_isEnterHeroOnce = true;
			BanRuleTweenAlpha.ResetToBeginning();
			BanRuleTweenAlpha.PlayForward();
		}
	}

	protected void ChangedRoomInfoUpdateBanRule(bool isViolateBanRule)
	{
		if (isViolateBanRule)
		{
			RemoveAllHeroPortraitList();
		}
		InitBanRuleText(isViolateBanRule);
		UpdateBanHeroPortrait();
	}

	protected virtual void RemoveAllHeroPortraitList()
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		List<Common.HeroPortraitAnim> squardHeroPortraitAnimListByAccountId = _currentSquard.GetSquardHeroPortraitAnimListByAccountId(myPartyMemberInfo.AccountId);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(squardHeroPortraitAnimListByAccountId); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = squardHeroPortraitAnimListByAccountId[i];
			if (heroPortraitAnim != null)
			{
				RemoveSquardHero(heroPortraitAnim.HeroIndex, heroPortraitAnim.Component.OwnerId);
			}
		}
	}

	public virtual bool IsViolateBanRule(ClassEnterPermissionBanTypeData classEnterBanData = null)
	{
		if (CurrentRaidBanType != RaidBanType.ClassEnterPermission)
		{
			return false;
		}
		ClassEnterPermissionBanTypeData classEnterPermissionBanTypeData = ((classEnterBanData == null) ? ClassEnterPermissionData : classEnterBanData);
		if (classEnterPermissionBanTypeData == null)
		{
			return false;
		}
		List<Common.HeroPortraitAnim> allHeroPortraitAnimList = _currentSquard.GetAllHeroPortraitAnimList();
		Dictionary<CreatureTagType, int> dictionary = new Dictionary<CreatureTagType, int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allHeroPortraitAnimList); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = allHeroPortraitAnimList[i];
			if (heroPortraitAnim == null)
			{
				continue;
			}
			int heroIndex = heroPortraitAnim.HeroIndex;
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			if (data != null)
			{
				int value = 0;
				CreatureTagType tagType = data.TagType;
				if (!dictionary.TryGetValue(tagType, out value))
				{
					dictionary.Add(tagType, 1);
				}
				else
				{
					dictionary[tagType] = value + 1;
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(classEnterPermissionBanTypeData.LimitEnterClassList); j++)
		{
			CreatureTagType i_key = classEnterPermissionBanTypeData.LimitEnterClassList[j];
			if (Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, i_key, 0) > classEnterPermissionBanTypeData.PossibleEnterCount)
			{
				return true;
			}
		}
		return false;
	}

	protected void InitDeco()
	{
		bool flag = _raidData.Type == RaidType.Challenge || _raidData.Type == RaidType.ChallengeSingle;
		bool flag2 = _raidData.Type == RaidType.TechnoMagic || _raidData.Type == RaidType.TechnoMagicSingle;
		UIHelper.EnableGameObject(ChallengeRaidDeco, flag || flag2);
		UIHelper.EnableGameObject(RaidDeco, !flag && !flag2);
	}

	protected virtual void SetBanDec()
	{
		if (RaidData == null)
		{
			return;
		}
		BattleType battleType = BattleType;
		if (battleType != BattleType.ChallengeRaidSingle && battleType != BattleType.ChallengeRaid && _listener is Listener listener)
		{
			BanRuleData raidBanData = listener.GetRaidBanData(RaidData);
			if (raidBanData != null && UserHeroListPanel != null)
			{
				UserHeroListPanel.SetBanUserHeroList(raidBanData);
			}
		}
	}

	public void SetBanByHeroIndices(List<int> banHeroIndices)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(banHeroIndices) && UserHeroListPanel != null)
		{
			UserHeroListPanel.SetBanByHeroIndices(banHeroIndices);
		}
	}

	protected virtual void InitFriendSlot()
	{
		if (_raidData != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(LockedLabels); i++)
			{
				bool flag = i < _raidData.PlayerCount;
				UIHelper.EnableGameObject(LockedLabels[i], !flag);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearPartyMemberName();
		GlobalAutoBehaviour<PartyManager>.instance.RemovePartyListener(this);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ClassEnterPermissionData = null;
		GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
		if ((bool)PartyChat)
		{
			PartyChat.Close();
		}
	}

	public override void UpdateDerived()
	{
		UpdatePartyChatInputState();
		CheckSendPartyCostumeChangedInfo();
		base.UpdateDerived();
	}

	protected override void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		base.OnChanged(changedInfo);
		if (CostumeChanged(changedInfo.ChangeType))
		{
			_heroCostumeChangedInfoChecker.ReserveSendPartyCostumeChanged = true;
			_heroCostumeChangedInfoChecker.ChangedInfo = changedInfo;
		}
	}

	private void CheckSendPartyCostumeChangedInfo()
	{
		if (_heroCostumeChangedInfoChecker.ReserveSendPartyCostumeChanged && _heroCostumeChangedInfoChecker.ChangedInfo != null)
		{
			int heroIndex = _heroCostumeChangedInfoChecker.ChangedInfo.HeroIndex;
			_heroCostumeChangedInfoChecker.ReserveSendPartyCostumeChanged = false;
			_heroCostumeChangedInfoChecker.ChangedInfo = null;
			if (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendPartyMemberCostumeInfo(heroIndex);
			}
		}
	}

	private bool CostumeChanged(NGame2.NAccount.UserHeroManager.HeroChangeType changeType)
	{
		return changeType switch
		{
			NGame2.NAccount.UserHeroManager.HeroChangeType.Costume => true, 
			NGame2.NAccount.UserHeroManager.HeroChangeType.HairCostume => true, 
			NGame2.NAccount.UserHeroManager.HeroChangeType.WeaponCostume => true, 
			NGame2.NAccount.UserHeroManager.HeroChangeType.AccessoryCostume => true, 
			_ => false, 
		};
	}

	protected void UpdatePartyChatInputState()
	{
		if (_listener is Listener listener && !(PartyChat == null) && !(PartyChat.Input == null))
		{
			bool enable = listener.CanInputPartyChat();
			UIHelper.EnableGameObject(PartyChat.Input.gameObject, enable);
		}
	}

	public virtual void OnClickStartBattleButton()
	{
		if (!RestoredPortal.CheckDragonHardEntry(RaidData) || !(_listener is Listener listener))
		{
			return;
		}
		if (_raidData != null)
		{
			bool flag = true;
			if (_raidData.Type == RaidType.FieldRaid)
			{
				flag = RaidHelper.IsOpenedFieldRaid(_raidData);
			}
			else if (_raidData.Type == RaidType.TechnoMagic)
			{
				flag = RaidHelper.IsOpenedTechnoMagicRaid(_raidData);
			}
			if (!flag)
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string stringDirect = LocalizationManager.GetStringDirect("PARTY_ENTER_IMPOSSIBLE_DES");
				MessagePopup.Open(text, stringDirect);
				return;
			}
		}
		BattleType battleType = BattleType;
		if (battleType == BattleType.ChallengeRaidSingle || battleType == BattleType.ChallengeRaid)
		{
			List<BanRuleData> banRuleDataList = _dataProvider.GetBanRuleDataList();
			List<int> totalHeroIndexList = _currentSquard.GetTotalHeroIndexList();
			if (AffixHelper.CheckBanRule(banRuleDataList, totalHeroIndexList))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("AFFIX_WARNING_CLASS_01"));
				return;
			}
		}
		else if (IsViolateBanRule())
		{
			ShowEnterClassBanMessage();
			return;
		}
		listener.OnClickStartBattle(this);
	}

	public void OnClickClose()
	{
		if (_listener is Listener listener)
		{
			listener.OnClickClose(this);
		}
	}

	public void OnClickChat()
	{
		SceneAutoBehaviour<ChattingManagement>.instance.OpenChatting(ChatType.None, 0L);
	}

	public void OnClickInviteFriend()
	{
		if (_listener is Listener listener)
		{
			listener.OnClickInvite(this);
		}
	}

	public virtual void OnClickRaidInfo()
	{
		if (_listener is Listener listener)
		{
			listener.OnClickRaidInfo(this);
		}
	}

	public virtual void OnClickOpenHero()
	{
		if (_listener is Listener listener)
		{
			listener.OnClickOpenHero();
		}
	}

	public virtual void OnClickOpenInventory()
	{
		if (_listener is Listener listener)
		{
			listener.OnClickOpenInventory();
		}
	}

	public virtual void OnClickAffixSelect()
	{
		if (_listener is Listener listener && _raidData != null)
		{
			int index = _raidData.Index;
			int level = _raidData.Level;
			int[] affixIndices = _dataProvider.GetAffixIndices();
			bool canChange = true;
			MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
			if (myPartyMemberInfo != null && !myPartyMemberInfo.IsMaster)
			{
				canChange = false;
			}
			listener.OnClickAffixSetting(index, level, affixIndices, canChange);
		}
	}

	protected virtual int GetStaminaCount()
	{
		if (GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo() == null)
		{
			return 0;
		}
		return _staminaCount;
	}

	protected virtual void UpdateStaminaCount()
	{
		int chapterIndex = _raidData.ChapterIndex;
		int dungeonIndex = _raidData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			_staminaCount = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina());
		}
	}

	protected override int GetPartyMemberCount()
	{
		return PartyMemberCountToMulti;
	}

	protected override int GetMaxPartyMemberPerPlayer()
	{
		return MaxPartMemberPerPlayer;
	}

	protected override bool IsLocalSaveEnabled()
	{
		return false;
	}

	public virtual void UpdateStartBattleState()
	{
		UpdateStartBattleStateToMulti(CanStartBattle());
	}

	protected void UpdateStartBattleStateToMulti(bool canStartBattle)
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
		{
			UIHelper.EnableButton(StartButton, enable: false);
			return;
		}
		if (myPartyMemberInfo.IsMaster)
		{
			StartBattleUi.SetButtonLabel(_dataProvider.GetStartButtonLabelText());
			StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.UseResource, _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType, StaminaType, StaminaCount);
			UIHelper.EnableButton(StartButton, canStartBattle);
			return;
		}
		if (myPartyMemberInfo.IsBattleReady)
		{
			string buttonLabel = LocalizationManager.GetString(PartyBattle.PARTY_BATTLE_CANCEL_READY);
			StartBattleUi.SetButtonLabel(buttonLabel);
			StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.UseResource, _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType, StaminaType, StaminaCount);
		}
		else
		{
			string buttonLabel2 = LocalizationManager.GetString(PartyBattle.PARTY_BATTLE_READY);
			StartBattleUi.SetButtonLabel(buttonLabel2);
			StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.UseResource, _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType, StaminaType, StaminaCount);
		}
		UIHelper.EnableButton(StartButton, enable: true);
	}

	protected override void UpdateStartBattleUI()
	{
		UpdateStartBattleState();
	}

	protected bool IsAllReady()
	{
		foreach (MemberInfo partyMember in GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.PartyMembers)
		{
			if (partyMember.Status == MemberStatus.Battle)
			{
				return false;
			}
			if (!partyMember.IsMaster && !partyMember.IsBattleReady)
			{
				return false;
			}
		}
		return true;
	}

	protected bool CanStartBattle()
	{
		if (!IsAllReady())
		{
			return false;
		}
		if (GetSquardTotalHeroIndexCount() == 0)
		{
			return false;
		}
		return true;
	}

	protected override bool CheckStartMemberCount()
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
		{
			return false;
		}
		if (!myPartyMemberInfo.IsMaster)
		{
			return false;
		}
		return true;
	}

	protected override void RequestStartBattle(ExtraOptionData extraOption)
	{
		if (SprkDispatchSelection.CanUseParty(this))
		{
			RequestStartBattleToMulti(extraOption);
		}
	}

	protected void RequestStartBattleToMulti(ExtraOptionData extraOption)
	{
		MemberInfo myMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myMemberInfo == null)
		{
			return;
		}
		if (GetSquardTotalMyHeroIndexCount() == 0 && !EnableObserver)
		{
			CenterMessage.Open(LocalizationManager.GetString(PartyBattle.NO_PARTY_HEROES));
			return;
		}
		if (GameRoot.Instance.PotionItemDataContainer.CanRecoverStaminaWithItem(StaminaType))
		{
			long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType);
			int staminaCount = GetStaminaCount();
			if (currentStamina < staminaCount)
			{
				UIHelper.ResolveRecoverStamina(isNotEnough: true, null, StaminaType);
				return;
			}
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			CenterMessage.Open(LocalizationManager.GetString(ResultType.EquipItemFull));
			return;
		}
		string text = string.Empty;
		if (_raidData.Type == RaidType.GuildSuppress || _raidData.Type == RaidType.GuildSuppressSingle)
		{
			text = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_ASK_BATTLE_START_MSG");
			if (!myMemberInfo.IsMaster && myMemberInfo.IsBattleReady)
			{
				text = string.Empty;
			}
		}
		Action action = delegate
		{
			RquestStartBattle(myMemberInfo);
		};
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else
		{
			MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), text, action, null);
		}
	}

	private void RquestStartBattle(MemberInfo myMemberInfo)
	{
		if (myMemberInfo.IsMaster)
		{
			if (IsAllReady())
			{
				GlobalAutoBehaviour<PartyManager>.instance.StartBattle(_raidData.Index, _raidData.Level, 0, delegate(PartyManager.StartBattleResult result)
				{
					GlobalAutoBehaviour<PartyManager>.instance.ProcessStartBattleResult(result, isMaster: true);
				});
			}
		}
		else
		{
			bool isReady = !myMemberInfo.IsBattleReady;
			GlobalAutoBehaviour<PartyManager>.instance.SendRoomReady(isReady);
		}
	}

	protected override void ProcessExitButton()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(PartyBattle.PARTY_BATTLE_LEAVE_ASK_MESSAGE);
		MessagePopup.Open(text, message, delegate
		{
			base.ProcessExitButton();
			GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
		}, null);
	}

	public override void ReOpen()
	{
	}

	protected void OnRoomSettingSingleRaid()
	{
		if (_listener is Listener listener)
		{
			int index = _raidData.Index;
			if (_raidData.ClearRaidIndex > 0)
			{
				index = _raidData.ClearRaidIndex;
				listener.OnClickSingleRoomSetting(index, _raidData.Level);
			}
		}
	}

	public virtual void OnClickRoomSetting()
	{
		if (_listener is Listener listener)
		{
			listener.OnClickRoomSetting(_raidData.Index, _raidData.Level);
		}
	}

	public virtual void OnClickSingleRoomSetting()
	{
		OnRoomSettingSingleRaid();
	}

	protected virtual void SetTitle()
	{
		UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect(_raidData.Name));
		string stringDirect = LocalizationManager.GetStringDirect(_raidData.Step_Name);
		UIHelper.SetLabelText(LevelLabel, stringDirect);
		UIHelper.EnableGameObject(LevelLabel, RaidData.Type == RaidType.DragonNormal || RaidData.Type == RaidType.DragonHard || RaidData.Type == RaidType.DragonNormalSingle || RaidData.Type == RaidType.DragonHardSingle || RaidData.Type == RaidType.FieldRaid || RaidData.Type == RaidType.FieldRaidSingle || RaidData.Type == RaidType.TechnoMagic || RaidData.Type == RaidType.TechnoMagicSingle || RaidData.Type == RaidType.TechnoEnchant || RaidData.Type == RaidType.TechnoEnchantSingle);
		UIHelper.EnableGameObject(HardModeLabel, enable: false);
		bool flag = RaidData.Type == RaidType.Challenge || RaidData.Type == RaidType.ChallengeSingle;
		UIHelper.EnableGameObject(ChallengeDiffLabel, flag);
		ChallengeRaidDiff challengeRaidDiff = RaidHelper.ChallengeRaidDiffClamp(RaidData.Level);
		UIHelper.SetLabelText(ChallengeDiffLabel, challengeRaidDiff.ToString());
		int affixPoint = GetAffixPoint();
		UIHelper.EnableGameObject(ChallengeRaidPointLabel, flag && affixPoint > 0);
		UIHelper.SetLabelText(ChallengeRaidPointLabel, $"{affixPoint}P");
		if (Aligner != null)
		{
			Aligner.Align();
		}
	}

	private int GetAffixPoint()
	{
		int[] affixIndices = _dataProvider.GetAffixIndices();
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(RaidData.ChapterIndex, RaidData.DungeonIndex);
		if (data == null)
		{
			return 0;
		}
		NShared.DungeonDifficultyType challengeDungeonDifficulty = RaidHelper.GetChallengeDungeonDifficulty();
		return AffixHelper.GetTotalAffixPoint(data, challengeDungeonDifficulty, affixIndices);
	}

	protected void SetInfoButtonLabel(UILabel label)
	{
		string key = ((RaidData.Type == RaidType.FieldRaid) ? "DUNGEON_REWARD_INFO" : "RAID_INFO_2");
		UIHelper.SetLabelText(label, LocalizationManager.GetStringDirect(key));
	}

	private PartyMemberNameComponent GetPartyMemberName(long accountId)
	{
		return _partyMemberNameComponentList.Find((PartyMemberNameComponent e) => e.PartyMemberInfo != null && e.PartyMemberInfo.AccountId == accountId);
	}

	protected void AddPartyMemberName(MemberInfo memberInfo)
	{
		if (!(GetPartyMemberName(memberInfo.AccountId) != null))
		{
			PartyMemberNameComponent partyMemberNameComponent = UIHelper.AddGridItem(PartyMemberNameGrid, PartyMemberNameComponent.PrefabAuto);
			partyMemberNameComponent.Open(this, memberInfo);
			_partyMemberNameComponentList.Add(partyMemberNameComponent);
			UpdateInviteButton();
			UIHelper.RepositionGrid(PartyMemberNameGrid);
		}
	}

	private void RemovePartyMemberName(long accountId)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_partyMemberNameComponentList); i++)
		{
			PartyMemberNameComponent partyMemberNameComponent = _partyMemberNameComponentList[i];
			if (partyMemberNameComponent.PartyMemberInfo != null && partyMemberNameComponent.PartyMemberInfo.AccountId == accountId)
			{
				_partyMemberNameComponentList.RemoveAt(i);
				FactoryBase.ReleaseComponent(partyMemberNameComponent);
				UpdateInviteButton();
				break;
			}
		}
		UIHelper.RepositionGrid(PartyMemberNameGrid);
	}

	public void ClearPartyMemberName()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_partyMemberNameComponentList); i++)
		{
			FactoryBase.ReleaseComponent(_partyMemberNameComponentList[i]);
		}
		_partyMemberNameComponentList.Clear();
		UpdateInviteButton();
	}

	void PartyMemberNameComponent.Listener.OnClickName(PartyMemberNameComponent component)
	{
		if (_listener is Listener listener)
		{
			listener.OnClickPartyMemberName(component.PartyMemberInfo);
		}
	}

	protected void UpdateInviteButton()
	{
		if (InviteButton == null)
		{
			return;
		}
		UIHelper.EnableGameObject(InviteButton, enable: false);
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance && __Extension_Method_ICollection__.COUNT(_partyMemberNameComponentList) != GlobalAutoBehaviour<PartyManager>.instance.GetMaxPartyMemberCount())
		{
			MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
			if (myPartyMemberInfo != null && myPartyMemberInfo.IsMaster)
			{
				UIHelper.EnableGameObject(InviteButton, enable: true);
				UIHelper.RepositionGrid(PartyMemberNameGrid);
			}
		}
	}

	public virtual void ToggleBattleRepeat()
	{
		ToggleBattleRepeatToMulti();
	}

	protected void ToggleBattleRepeatToMulti()
	{
		if (!GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			return;
		}
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null || !myPartyMemberInfo.IsMaster || _raidData == null || _raidData.Type == RaidType.Challenge || _raidData.Type == RaidType.ChallengeSingle)
		{
			return;
		}
		if (!GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle)
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string stringDirect2 = LocalizationManager.GetStringDirect("REPEATBATTLE_INFORM_MULTI");
			MessagePopup.Open(stringDirect, stringDirect2, delegate
			{
				GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.SetRepeatBattle(IsRepeat: true);
				GlobalAutoBehaviour<PartyManager>.instance.SendRoomRepeat(IsRepeat: true);
				UpdateBattleRepeatToggle();
			}, null);
		}
		else
		{
			GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.SetRepeatBattle(!GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle);
			GlobalAutoBehaviour<PartyManager>.instance.SendRoomRepeat(IsRepeat: false);
			UpdateBattleRepeatToggle();
		}
	}

	protected virtual void UpdateBattleRepeatToggle()
	{
		UpdateBattleRepeatToggleToMulti();
	}

	protected void UpdateBattleRepeatToggleToMulti()
	{
		if (_raidData != null)
		{
			bool flag = RaidData.Type == RaidType.Challenge || RaidData.Type == RaidType.ChallengeSingle;
			UIHelper.EnableGameObject(Widget_AffixInfo, flag);
			UIHelper.EnableGameObject(Widget_Repeat, !flag);
			if (!flag)
			{
				UIHelper.EnableGameObject(SpriteBattleRepeatUnusable, enable: false);
				UIHelper.EnableGameObject(SpriteBattleRepeatChecked, enable: false);
				UIHelper.EnableGameObject(SpriteBattleRepeatChecked, GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle);
			}
		}
	}

	protected virtual void UpdateRoomSettingButton()
	{
		if (!(RoomSettingButton == null))
		{
			bool flag = true;
			if (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
			{
				bool flag2 = GlobalAutoBehaviour<PartyManager>.instance.IsPartyRoomMaster();
				flag = _raidData.Type != RaidType.Challenge && _raidData.Type != RaidType.GuildSuppress && _raidData.Type != RaidType.GuildSuppressSingle && flag2;
			}
			else
			{
				flag = _raidData.Type == RaidType.DragonNormalSingle || _raidData.Type == RaidType.DragonHardSingle || _raidData.Type == RaidType.FieldRaidSingle || _raidData.Type == RaidType.TechnoMagicSingle || _raidData.Type == RaidType.TechnoEnchantSingle;
			}
			UIHelper.EnableGameObject(RoomSettingButton, flag);
		}
	}

	private int SortMemberNameGrid(Transform a, Transform b)
	{
		if (a == b)
		{
			return 0;
		}
		if (b == InviteButton)
		{
			return -1;
		}
		if (a == InviteButton)
		{
			return 1;
		}
		PartyMemberNameComponent component = a.GetComponent<PartyMemberNameComponent>();
		PartyMemberNameComponent component2 = b.GetComponent<PartyMemberNameComponent>();
		int num = _partyMemberNameComponentList.IndexOf(component);
		int value = _partyMemberNameComponentList.IndexOf(component2);
		return num.CompareTo(value);
	}

	public override bool InsertSquardHero(HeroPortraitComponent deckPortrait, bool isMainSquard)
	{
		return InsertSquardHeroToMulti(deckPortrait, isMainSquard);
	}

	protected bool InsertSquardHeroToMulti(HeroPortraitComponent deckPortrait, bool isMainSquard)
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo != null && myPartyMemberInfo.IsBattleReady)
		{
			CenterMessage.Open(LocalizationManager.GetString(PartyBattle.CANNOT_WHILE_READY));
			return false;
		}
		if (!base.InsertSquardHero(deckPortrait, isMainSquard))
		{
			return false;
		}
		int heroIndex = deckPortrait.HeroInfo.HeroIndex;
		GlobalAutoBehaviour<PartyManager>.instance.SendPartyDeckHeroInfo(heroIndex, deckPortrait.HeroInfo, isMainSquard);
		UpdateStartBattleState();
		UpdatePartyMemberDepth();
		UpdateBanRuleLabel();
		return true;
	}

	public override bool RemoveSquardHero(int heroIndex, long ownerId = 0L, CreatureTagType classType = CreatureTagType.None, bool ignoreSelection = false)
	{
		return RemoveSquardHeroToMulti(heroIndex, ownerId, classType, ignoreSelection);
	}

	protected bool RemoveSquardHeroToMulti(int heroIndex, long ownerId = 0L, CreatureTagType classType = CreatureTagType.None, bool ignoreSelection = false)
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo != null && myPartyMemberInfo.IsBattleReady)
		{
			CenterMessage.Open(LocalizationManager.GetString(PartyBattle.CANNOT_WHILE_READY));
			return false;
		}
		if (_currentSquard == null)
		{
			return false;
		}
		bool isMain = true;
		if (_currentSquard.GetSubSquardTotalHeroIndexList().Contains(heroIndex))
		{
			isMain = false;
		}
		if (!base.RemoveSquardHero(heroIndex, ownerId, classType, ignoreSelection))
		{
			return false;
		}
		GlobalAutoBehaviour<PartyManager>.instance.SendPartyDeckHeroInfo(heroIndex, null, isMain);
		UpdateStartBattleState();
		UpdatePartyMemberDepth();
		return true;
	}

	protected override void SetSquardHeroList(out List<CachedHeroInfo> mainHeroList, out List<CachedHeroInfo> subHeroList)
	{
		SetSquardHeroListToMulti(out mainHeroList, out subHeroList);
	}

	protected void SetSquardHeroListToMulti(out List<CachedHeroInfo> mainHeroList, out List<CachedHeroInfo> subHeroList)
	{
		mainHeroList = new List<CachedHeroInfo>();
		subHeroList = new List<CachedHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.PartyMembers); i++)
		{
			MemberInfo memberInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.PartyMembers[i];
			if (memberInfo == null)
			{
				continue;
			}
			AddPartyMemberName(memberInfo);
			foreach (CachedHeroInfo value in memberInfo.DeckHeros.Values)
			{
				if (value != null && value.HeroIndex != 0)
				{
					_isEnterHeroOnce = true;
					((PartyManager.IPartyListener)this).OnPartyMemberDeckHeroChanged(memberInfo, value.HeroIndex, value, isMain: true);
					mainHeroList.Add(value);
				}
			}
			foreach (CachedHeroInfo value2 in memberInfo.SubDeckHeros.Values)
			{
				if (value2 != null && value2.HeroIndex != 0)
				{
					((PartyManager.IPartyListener)this).OnPartyMemberDeckHeroChanged(memberInfo, value2.HeroIndex, value2, isMain: false);
					subHeroList.Add(value2);
				}
			}
		}
	}

	private void UpdatePartyMemberDepth()
	{
		_currentSquard.UpdateSquardPortraitAnimDepth();
	}

	protected override void AnimateSquardPositionAction(List<Common.HeroPortraitAnim> heroPortraitList, List<PartySlotComponent> emptySlotList, Common.HeroPortraitAnim heroPortraitAnim)
	{
		AnimateSquardPositionActionToMulti(heroPortraitList, emptySlotList, heroPortraitAnim);
	}

	protected void AnimateSquardPositionActionToMulti(List<Common.HeroPortraitAnim> heroPortraitList, List<PartySlotComponent> emptySlotList, Common.HeroPortraitAnim heroPortraitAnim)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(emptySlotList); i++)
		{
			PartySlotComponent partySlotComponent = emptySlotList[i];
			if (partySlotComponent == null)
			{
				continue;
			}
			if (__Extension_Method_ICollection__.COUNT(heroPortraitList) > i)
			{
				Common.HeroPortraitAnim heroPortraitAnim2 = heroPortraitList[i];
				long accountId = (heroPortraitAnim2.Component ? heroPortraitAnim2.Component.OwnerId : 0);
				MemberInfo partyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberInfo(accountId);
				string text = ((partyMemberInfo != null) ? partyMemberInfo.Nick : "Unknown");
				int memberIndex = partyMemberInfo?.MemberIndex ?? (-1);
				partySlotComponent.AccountId = accountId;
				partySlotComponent.SetName(text);
				partySlotComponent.SetState(PartySlotComponent.MemberStateType.Joined);
				partySlotComponent.SetMemberIndex(memberIndex);
				if (Vespa_Common_Class_Extension_Method.EQUALS(heroPortraitAnim2, heroPortraitAnim))
				{
					heroPortraitAnim2.Component.transform.position = partySlotComponent.transform.position;
					heroPortraitAnim2.StopAnimation();
				}
				else
				{
					Common.AnimateMemberPosition(heroPortraitAnim2, partySlotComponent.transform.position, isInsertMember: false);
				}
			}
			else
			{
				partySlotComponent.Reset();
			}
		}
	}

	void PartyManager.IPartyListener.OnPartyMemberChangeAvatar(MemberInfo memberInfo)
	{
	}

	void PartyManager.IPartyListener.OnPartyMemberJoin(MemberInfo memberInfo)
	{
		AddPartyMemberName(memberInfo);
		UpdateStartBattleState();
		if (_listener is Listener listener)
		{
			listener.OnPartyMemberJoin(memberInfo);
		}
	}

	void PartyManager.IPartyListener.OnPartyDestroy(bool isMasterLeave)
	{
		if (IsOpen() && !IsClosingWindowAnimating())
		{
			Close();
			if (_listener is Listener listener)
			{
				listener.OnAutoDestroyGuildRaidParty();
			}
		}
	}

	void PartyManager.IPartyListener.OnPartyMemberDeckHeroChanged(MemberInfo memberInfo, int heroIndex, CachedHeroInfo heroInfo, bool isMain)
	{
		if (heroInfo == null || heroInfo.HeroIndex == 0)
		{
			bool ignoreSelection = IgnoreSelection;
			if (_currentSquard.GetSquardHeroProtraitAnim(heroIndex, memberInfo.AccountId, ignoreSelection) != null)
			{
				RemoveSquardHeroByRaid(heroIndex, memberInfo.AccountId, ignoreSelection);
				UpdatePartyMemberDepth();
				UpdateBanHeroPortrait();
			}
		}
		else
		{
			Vector3 squardPanelPosition = _currentSquard.GetSquardPanelPosition();
			HeroPortraitComponent.OwnerInfo ownerInfo = new HeroPortraitComponent.OwnerInfo
			{
				AccountId = memberInfo.AccountId,
				Nick = memberInfo.Nick,
				Level = memberInfo.TeamLevel
			};
			InsertSquardHeroByRaid(isMain, heroInfo, squardPanelPosition, ownerInfo);
			UpdatePartyMemberDepth();
			UpdateBanRuleLabel();
			UpdateBanHeroPortrait();
		}
	}

	void PartyManager.IPartyListener.OnPartyMemberReady(MemberInfo memberInfo, bool isReady)
	{
		UpdateStartBattleState();
		PartyMemberNameComponent partyMemberName = GetPartyMemberName(memberInfo.AccountId);
		if ((bool)partyMemberName && memberInfo.Status == MemberStatus.PartyRoom)
		{
			if (memberInfo.IsMaster)
			{
				partyMemberName.SetState(PartyMemberNameComponent.StateType.Master);
			}
			else if (isReady)
			{
				partyMemberName.SetState(PartyMemberNameComponent.StateType.Ready);
			}
			else
			{
				partyMemberName.SetState(PartyMemberNameComponent.StateType.None);
			}
		}
	}

	void PartyManager.IPartyListener.OnPartyMemberDeckCostumeChanged(long accountId, int heroIndex, CachedHeroInfo heroInfo)
	{
		if (_currentSquard == null)
		{
			return;
		}
		List<Common.HeroPortraitAnim> squardHeroPortraitAnimListByAccountId = _currentSquard.GetSquardHeroPortraitAnimListByAccountId(accountId);
		if (squardHeroPortraitAnimListByAccountId != null)
		{
			Common.HeroPortraitAnim heroPortraitAnim = squardHeroPortraitAnimListByAccountId.Find((Common.HeroPortraitAnim item) => item != null && item.HeroIndex == heroIndex);
			if (heroPortraitAnim != null && !(heroPortraitAnim.Component == null))
			{
				heroPortraitAnim.Component.SetHeroImage();
			}
		}
	}

	void PartyManager.IPartyListener.OnPartyRoomSetRepeat(bool IsRepeat)
	{
		UpdateBattleRepeatToggle();
	}

	void PartyManager.IPartyListener.OnPartyMemberLeave(MemberInfo memberInfo)
	{
		long accountId = memberInfo.AccountId;
		List<Common.HeroPortraitAnim> squardHeroPortraitAnimListByAccountId = _currentSquard.GetSquardHeroPortraitAnimListByAccountId(accountId);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(squardHeroPortraitAnimListByAccountId); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = squardHeroPortraitAnimListByAccountId[i];
			if (heroPortraitAnim != null)
			{
				RemoveSquardHeroByRaid(heroPortraitAnim.HeroIndex, heroPortraitAnim.Component.OwnerId, IgnoreSelection);
			}
		}
		UpdateStartBattleState();
		RemovePartyMemberName(accountId);
		if (_listener is Listener listener)
		{
			listener.OnPartyMemberLeave(accountId);
		}
	}

	void PartyManager.IPartyListener.OnChangePartyMemberStatus(MemberInfo memberInfo)
	{
		GetPartyMemberName(memberInfo.AccountId).SetStatus(memberInfo.Status);
	}

	void PartyManager.IPartyListener.OnPartyRoomInfoUpdated(RaidRoomInfo raidRoomInfo, bool isViolateBanRule)
	{
		bool flag = false;
		if (RaidData.Type == RaidType.Challenge)
		{
			bool flag2 = false;
			if (_raidData.Level != raidRoomInfo.RaidLevel)
			{
				flag2 = true;
			}
			int[] affixIndices = _dataProvider.GetAffixIndices();
			int[] affixIndices2 = raidRoomInfo.AffixIndices;
			if (!AffixHelper.IsEqualAffixIndices(affixIndices, affixIndices2))
			{
				flag2 = true;
				List<BanRuleData> banRuleDataList = AffixHelper.GetBanRuleDataList(affixIndices2);
				List<int> totalHeroIndexList = _currentSquard.GetTotalHeroIndexList();
				flag = AffixHelper.CheckBanRule(banRuleDataList, totalHeroIndexList);
			}
			if (flag2)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("CHALLENGE_RAID_CHANGE_DESC"));
			}
		}
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(raidRoomInfo.RaidIndex, raidRoomInfo.RaidLevel);
		if (_dataProvider != null)
		{
			_dataProvider.SetAffixIndices(raidRoomInfo.AffixIndices);
		}
		UpdateStaminaCount();
		UpdateStartBattleState();
		UpdateBanTypeData();
		isViolateBanRule = flag || isViolateBanRule;
		ChangedRoomInfoUpdateBanRule(isViolateBanRule);
		SetTitle();
		SetDebugLabel();
	}

	void PartyManager.IPartyListener.OnPartyRoomMasterChanged(MemberInfo memberInfo)
	{
		foreach (PartyMemberNameComponent partyMemberNameComponent in _partyMemberNameComponentList)
		{
			if (partyMemberNameComponent.PartyMemberInfo != null)
			{
				PartyMemberNameComponent.StateType state = PartyMemberNameComponent.StateType.None;
				if (partyMemberNameComponent.PartyMemberInfo.IsBattleReady)
				{
					state = PartyMemberNameComponent.StateType.Ready;
				}
				if (partyMemberNameComponent.PartyMemberInfo.AccountId == memberInfo.AccountId)
				{
					state = PartyMemberNameComponent.StateType.Master;
				}
				partyMemberNameComponent.SetState(state);
			}
		}
		UpdateInviteButton();
		UpdateRoomSettingButton();
		UpdateStartBattleState();
	}

	void PartyManager.IPartyListener.OnPartyMemberResults(MemberInfo memberInfo)
	{
	}

	void ChattingListComponentBase.Listener.OnClickChatUser(ChatInfo chatInfo)
	{
	}

	void ChattingPartyComponent.Listener.OnSubmit(string chat)
	{
		if (_listener is Listener listener)
		{
			listener.OnSubmitPartyChat(chat);
		}
	}

	List<ChatInfo> ChattingPartyComponent.Listener.GetPartyChat()
	{
		if (_listener is Listener listener)
		{
			return listener.GetPartyChat();
		}
		return null;
	}

	void ChattingListComponentBase.Listener.OnClickEmoticonSelect()
	{
		if (_listener is Listener listener)
		{
			listener.OnClickEmoticonSelect(this);
		}
	}

	bool ChattingEmoticonSelectPopup.Listener.CheckEnableChatting()
	{
		return false;
	}

	Vector3 ChattingEmoticonSelectPopup.Listener.GetRootPosition()
	{
		if (EmoticonSelectPopupRoot == null)
		{
			return default(Vector3);
		}
		return EmoticonSelectPopupRoot.position;
	}

	ChatType ChattingEmoticonSelectPopup.Listener.GetChatType()
	{
		return ChatType.Party;
	}

	public void OnPartyMemberDeckHeroChangedForTutorial(MemberInfo memberInfo, int heroIndex, CachedHeroInfo heroInfo)
	{
		if (heroInfo == null || heroInfo.HeroIndex == 0)
		{
			if (_currentSquard.GetSquardHeroProtraitAnim(heroIndex, memberInfo.AccountId) != null)
			{
				RemoveSquardHeroByRaid(heroIndex, memberInfo.AccountId);
				UpdatePartyMemberDepth();
			}
		}
		else
		{
			Vector3 squardPanelPosition = _currentSquard.GetSquardPanelPosition();
			HeroPortraitComponent.OwnerInfo ownerInfo = new HeroPortraitComponent.OwnerInfo
			{
				AccountId = memberInfo.AccountId,
				Nick = memberInfo.Nick,
				Level = memberInfo.TeamLevel
			};
			InsertSquardHeroByRaid(_currentSquard.IsSelectedMainSquard(), heroInfo, squardPanelPosition, ownerInfo);
			UpdatePartyMemberDepth();
		}
	}

	public void SetStaminaCount(int stamina)
	{
		_staminaCount = stamina;
	}

	protected override void OpenClearParty()
	{
		if (_raidData != null)
		{
			int chapterIndex = _raidData.ChapterIndex;
			int dungeonIndex = _raidData.DungeonIndex;
			NShared.DungeonDifficultyType difficulty = ((_raidData.Type != RaidType.DragonHard) ? NShared.DungeonDifficultyType.Normal : NShared.DungeonDifficultyType.Hard);
			SceneAutoBehaviour<InspectUserManagement>.instance.OpenRecommendClearPartyPopup(chapterIndex, dungeonIndex, difficulty);
		}
	}

	protected override void SetDebugLabel()
	{
		if (!ClientValueManager.IsShowDungeonIndex)
		{
			UIHelper.EnableGameObject(Label_DebugText, enable: false);
		}
		else if (_raidData != null)
		{
			UIHelper.EnableGameObject(Label_DebugText, enable: true);
			UIHelper.SetLabelText(Label_DebugText, $"{_raidData.ChapterIndex},{_raidData.DungeonIndex}");
		}
	}
}
