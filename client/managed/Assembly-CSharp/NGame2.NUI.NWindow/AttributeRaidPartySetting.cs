using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NParty;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NLocalization;
using NShared.NParty;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class AttributeRaidPartySetting : RaidSinglePartySetting
{
	public struct AttributeRaidPartySettingInfo
	{
		public Listener IListener;

		public RaidData RaidData;

		public bool IsMulti;
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Raid/AttributeRaidPartySetting";

	public Transform ChatTransform;

	private bool CheckPossibleInsertHero
	{
		get
		{
			if (_currentSquard == null)
			{
				return false;
			}
			if (!RaidPartySetting.IsMultiRaid)
			{
				return true;
			}
			List<int> mainSquardMyHeroIndexList = _currentSquard.GetMainSquardMyHeroIndexList();
			List<int> subSquardMyHeroIndexList = _currentSquard.GetSubSquardMyHeroIndexList();
			if (_currentSquard.IsSelectedMainSquard())
			{
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(subSquardMyHeroIndexList) && subSquardMyHeroIndexList.Count > 0)
				{
					return false;
				}
				List<int> mainSquardTotalHeroIndexList = _currentSquard.GetMainSquardTotalHeroIndexList();
				if (__Extension_Method_ICollection__.IsNullOrEmpty(mainSquardMyHeroIndexList) && !__Extension_Method_ICollection__.IsNullOrEmpty(mainSquardTotalHeroIndexList) && mainSquardTotalHeroIndexList.Count > 0)
				{
					return false;
				}
			}
			else
			{
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(mainSquardMyHeroIndexList) && mainSquardMyHeroIndexList.Count > 0)
				{
					return false;
				}
				List<int> subSquardTotalHeroIndexList = _currentSquard.GetSubSquardTotalHeroIndexList();
				if (__Extension_Method_ICollection__.IsNullOrEmpty(subSquardMyHeroIndexList) && !__Extension_Method_ICollection__.IsNullOrEmpty(subSquardTotalHeroIndexList) && subSquardTotalHeroIndexList.Count > 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	private BanRuleData BanRuleData
	{
		get
		{
			if (_raidData == null)
			{
				return null;
			}
			return GameRoot.Instance.BanRuleDataContainer.GetData(_raidData.BanIndex);
		}
	}

	private int MainSquardHeroCount
	{
		get
		{
			if (_currentSquard == null)
			{
				return 0;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_currentSquard.GetMainSquardTotalHeroIndexList()))
			{
				return 0;
			}
			return _currentSquard.GetMainSquardTotalHeroIndexList().Count;
		}
	}

	private int SubSquardHeroCount
	{
		get
		{
			if (_currentSquard == null)
			{
				return 0;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_currentSquard.GetSubSquardTotalHeroIndexList()))
			{
				return 0;
			}
			return _currentSquard.GetSubSquardTotalHeroIndexList().Count;
		}
	}

	private bool CheckMinimumStartBattleCondition
	{
		get
		{
			if (MainSquardHeroCount <= 0 || SubSquardHeroCount <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public void Open(AttributeRaidPartySettingInfo info)
	{
		Open(delegate
		{
			Init(info);
		}, delegate
		{
		});
	}

	private void Init(AttributeRaidPartySettingInfo? info)
	{
		if (!info.HasValue || info.Value.RaidData == null)
		{
			return;
		}
		_raidData = info.Value.RaidData;
		_dataProvider = SceneAutoBehaviour<RaidManagement>.instance.GetDefaultBattlePartySettingDataProdiverByRaidData(info.Value.RaidData, isStorageKey: true);
		if (_dataProvider == null)
		{
			return;
		}
		if (info.Value.IsMulti)
		{
			InitForRaid(null, _dataProvider, _raidData);
		}
		else
		{
			Init(null, _dataProvider, _raidData);
		}
		_listener = info.Value.IListener;
		UIHelper.EnableGameObject(ChatTransform, !info.Value.IsMulti);
		if (info.Value.IsMulti && _listener != null)
		{
			(_listener as RaidPartySetting.Listener).OnEnterRaidPartySetting(_raidData);
		}
		UpdateBanHero();
		if (base.CheckPossibleHeroAttributeType)
		{
			_switchSquardAction = delegate
			{
				SetPossibleHeroAttribute();
			};
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
	}

	public override void Reset()
	{
		base.Reset();
	}

	protected override void SetSquardHeroList(out List<CachedHeroInfo> mainHeroList, out List<CachedHeroInfo> subHeroList)
	{
		if (RaidPartySetting.IsMultiRaid)
		{
			SetSquardHeroListToMulti(out mainHeroList, out subHeroList);
		}
		else
		{
			base.SetSquardHeroList(out mainHeroList, out subHeroList);
		}
	}

	private bool CheckAlreadyExistHero(HeroPortraitComponent deckPortrait, out string squardMessageKey)
	{
		squardMessageKey = string.Empty;
		if (deckPortrait == null || deckPortrait.HeroInfo == null)
		{
			return true;
		}
		bool result = _currentSquard.IsContainSquardHero(deckPortrait.HeroInfo.HeroIndex, 0L);
		if (_currentSquard.IsSelectedMainSquard())
		{
			squardMessageKey = "TECHNO_ENCHANT_RIGHTSLOT_TITLE";
			return result;
		}
		squardMessageKey = "TECHNO_ENCHANT_LEFTSLOT_TITLE";
		return result;
	}

	protected override bool EnableInsertSquard(HeroPortraitComponent deckPortrait)
	{
		if (_currentSquard == null)
		{
			return false;
		}
		if (!CheckPossibleInsertSquard(deckPortrait))
		{
			return false;
		}
		string squardMessageKey = string.Empty;
		if (CheckAlreadyExistHero(deckPortrait, out squardMessageKey))
		{
			string stringDirect = LocalizationManager.GetStringDirect(squardMessageKey);
			CenterMessage.Open(LocalizationManager.GetStringDirect("TECHNOENCHANTRAID_HERO_ALREADY_EXIST", stringDirect));
			return false;
		}
		if (RaidPartySetting.IsMultiRaid)
		{
			if (_currentSquard == null)
			{
				return false;
			}
			if (GetMaxPartyMemberPerPlayer() <= _currentSquard.GetMainSquardMyHeroIndexList().Count + _currentSquard.GetSubSquardMyHeroIndexList().Count)
			{
				CenterMessage.Open(LocalizationManager.GetString(Battle.CANNOT_INSERT_MORE));
				return false;
			}
		}
		return true;
	}

	public override bool InsertSquardHero(HeroPortraitComponent deckPortrait, bool isMainSquard)
	{
		if (_currentSquard == null)
		{
			return false;
		}
		bool flag = false;
		if (RaidPartySetting.IsMultiRaid)
		{
			if (!CheckPossibleInsertHero)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("TECHNO_ENCHANT_HEROSELECT_ERROR"));
				return false;
			}
			return InsertSquardHeroToMulti(deckPortrait, isMainSquard);
		}
		return InsertSquardHeroToSingle(deckPortrait, isMainSquard);
	}

	public override bool RemoveSquardHero(int heroIndex, long ownerId = 0L, CreatureTagType classType = CreatureTagType.None, bool ignoreSelection = false)
	{
		bool flag = false;
		if (RaidPartySetting.IsMultiRaid)
		{
			return RemoveSquardHeroToMulti(heroIndex, ownerId, classType, ignoreSelection: true);
		}
		return RemoveSquardHeroToSingle(heroIndex, ownerId, classType, ignoreSelection);
	}

	protected override int GetPartyMemberCount()
	{
		if (RaidPartySetting.IsMultiRaid)
		{
			return base.PartyMemberCountToMulti;
		}
		return base.PartyMemberCountToSingle;
	}

	protected override int GetMaxPartyMemberPerPlayer()
	{
		return base.MaxPartMemberPerPlayer;
	}

	protected override void AnimateSquardPositionAction(List<Common.HeroPortraitAnim> heroPortraitList, List<PartySlotComponent> emptySlotList, Common.HeroPortraitAnim heroPortraitAnim)
	{
		if (RaidPartySetting.IsMultiRaid)
		{
			AnimateSquardPositionActionToMulti(heroPortraitList, emptySlotList, heroPortraitAnim);
		}
		else
		{
			AnimateSquardPositionActionToSingle(heroPortraitList, emptySlotList, heroPortraitAnim);
		}
	}

	protected override void ShowEnterClassBanMessage(HeroPortraitComponent heroPortraitComponent = null)
	{
		if (base.CheckPossibleHeroAttributeType)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TECHNO_ENCHANT_RAID_HERO_ATRRIBUTE_FIX"));
		}
	}

	private void UpdateBanHero()
	{
		UpdateBanTypeData();
		UpdateBanHeroPortrait();
	}

	protected override void UpdateBanTypeData()
	{
		base.UpdateBanTypeData();
	}

	protected override void UpdateBanHeroPortrait()
	{
		base.UpdateBanHeroPortrait();
		SetPossibleHeroAttribute();
	}

	protected override void InitBanRuleText(bool isForceShow = false)
	{
		UIHelper.EnableGameObject(BanRuleRoot, enable: false);
		if (base.CheckPossibleHeroAttributeType && !(BanRuleRoot == null) && !(BanRuleLabel == null) && !(BanRuleTweenAlpha == null))
		{
			BanRuleTweenAlpha.value = BanRuleTweenAlpha.to;
			_isEnterHeroOnce = true;
			if (isForceShow || (MainSquardHeroCount <= 0 && SubSquardHeroCount <= 0))
			{
				UIHelper.EnableGameObject(BanRuleRoot, enable: true);
				UIHelper.SetLabelText(BanRuleLabel, LocalizationManager.GetStringDirect("TECHNOENCHANT_DECKATTRIBUTE_NOTICE"));
				BanRuleTweenAlpha.ResetToBeginning();
				BanRuleTweenAlpha.enabled = false;
				_isEnterHeroOnce = false;
			}
		}
	}

	private void SetPossibleHeroAttribute()
	{
		if (!base.CheckPossibleHeroAttributeType || _currentSquard == null)
		{
			return;
		}
		int num = ((!_currentSquard.IsSelectedMainSquard()) ? 1 : 0);
		if (_raidData != null)
		{
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_raidData.ChapterIndex, _raidData.DungeonIndex);
			if (data != null && !__Extension_Method_ICollection__.IsNullOrEmpty(data.DeckAttributeCondition) && UserHeroListPanel != null)
			{
				UserHeroListPanel.SetPossibleHeroAttribute(data.DeckAttributeCondition[num]);
			}
		}
	}

	protected override void RequestStartBattle(ExtraOptionData extraOption)
	{
		if (!SprkDispatchSelection.CanUseParty(this))
		{
			return;
		}
		if (RaidPartySetting.IsMultiRaid)
		{
			RequestStartBattleToMulti(extraOption);
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
			if (base.CheckPossibleHeroAttributeType && _currentSquard != null)
			{
				battlePartyInfo.DeltaHeroIndexList = _currentSquard.GetMainSquardTotalHeroIndexList();
				battlePartyInfo.ZetaHeroIndexList = _currentSquard.GetSubSquardTotalHeroIndexList();
			}
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

	public override void UpdateStartBattleState()
	{
		if (RaidPartySetting.IsMultiRaid)
		{
			UpdateStartBattleStateToMulti(IsAllReady() && CheckMinimumStartBattleCondition);
		}
		else
		{
			UpdateStartBattleStateToSingle();
		}
	}

	private void UpdateStartBattleStateToSingle()
	{
		if (StartBattleUi != null && _dataProvider != null)
		{
			StartBattleUi.SetButtonLabel(_dataProvider.GetStartButtonLabelText());
			StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.UseResource, _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType, StaminaType, StaminaCount);
			UIHelper.EnableButton(StartButton, CheckMinimumStartBattleCondition);
		}
	}

	public void ChangeRoomSettingSingleRaid(RaidData raidData)
	{
		_raidData = raidData;
		_dataProvider = SceneAutoBehaviour<RaidManagement>.instance.GetDefaultBattlePartySettingDataProdiverByRaidData(raidData, isStorageKey: true);
		SetTitle();
		UpdateStaminaCount();
		UpdateStartBattleState();
		UpdateBanHero();
		ChangedRoomInfoUpdateBanRule(IsViolateBanRule());
	}

	protected override bool CheckBattleReadyToMulti()
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
		{
			return false;
		}
		if (!myPartyMemberInfo.IsMaster)
		{
			return myPartyMemberInfo.IsBattleReady;
		}
		return false;
	}

	protected override bool CheckIsMasterToParty()
	{
		return GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo()?.IsMaster ?? false;
	}

	protected override bool CheckPossibleSwapToSubParty()
	{
		return !RaidPartySetting.IsMultiRaid;
	}

	public override void ToggleBattleRepeat()
	{
		if (RaidPartySetting.IsMultiRaid)
		{
			ToggleBattleRepeatToMulti();
		}
		else
		{
			base.ToggleBattleRepeat();
		}
	}

	protected override void UpdateBattleRepeatToggle()
	{
		if (RaidPartySetting.IsMultiRaid)
		{
			UpdateBattleRepeatToggleToMulti();
		}
		else
		{
			base.UpdateBattleRepeatToggle();
		}
	}

	public override void OnClickRoomSetting()
	{
		if (_listener is Listener listener && _raidData != null)
		{
			if (RaidPartySetting.IsMultiRaid)
			{
				listener.OnClickRoomSetting(_raidData.Index, _raidData.Level);
			}
			else
			{
				OnRoomSettingSingleRaid();
			}
		}
	}

	public void OnClickMainSquard()
	{
		if (_currentSquard != null)
		{
			_currentSquard.SwichingSquard(isOnMainSquard: true);
			SetPossibleHeroAttribute();
		}
	}

	public void OnClickSubSquard()
	{
		if (_currentSquard != null)
		{
			_currentSquard.SwichingSquard(isOnMainSquard: false);
			SetPossibleHeroAttribute();
		}
	}

	public override void OnClickStartBattleButton()
	{
		if (_currentSquard != null)
		{
			base.OnClickStartBattleButton();
		}
	}
}
