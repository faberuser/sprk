using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NLobby;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBooster;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NInventory;
using NGame2.NUtil.NHelper.NInfoHelper;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NDispatchBattle;

public class ContentUIDispatchBattle : BaseWindow, HeroPortraitComponent.Listener, ContentUIDispatchGroupComponent.Listener, IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>
{
	public enum ProgressUIState
	{
		Idle,
		Prepare,
		Battle,
		ReadyToComplete
	}

	public const string PATH_PREFAB = "Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattle";

	private const int MIN_VALUE_REPEAT_COUNT = 10;

	private const int MAX_VALUE_REPEAT_COUNT = 200;

	private const int VALUE_CHECK_SIMUL_COUNT = 1;

	public UIGrid GridPicked;

	public UIScrollGrid UIScrollGridUnpicked;

	public Transform TransformSelectedDispatchGroup;

	public UIGrid GridDispatchGroup;

	public UIWidget stateUIIdle;

	public UIWidget stateUIProgressing;

	public UIBundleLoader TxMissionImg;

	public UILabel LabelMissionName;

	public UIButton ButtonStart;

	public TweenAlpha BanRuleTweenAlpha;

	public Transform BanRuleRoot;

	public UILabel BanRuleLabel;

	[Header("Idle Control Panel")]
	public UILabel LabelMissionDifficult;

	public UIButton ButtonPrev;

	public UIButton ButtonNext;

	public UIButton ButtonDecrease10;

	public UIButton ButtonDecrease;

	public UIButton ButtonIncrease;

	public UIButton ButtonIncrease10;

	public UISprite SpriteDifficultyBg;

	public UIInput InputRepeatCount;

	[Header("Idle Display Panel")]
	public GameObject WidgetNeedStaminaNormal;

	public UILabel LabelNeedStaminaNormal;

	public GameObject WidgetNeedStaminaDiscount;

	public GameObject IconDiscountBuff;

	public UILabel LabelNeedStaminaOriginal;

	public UILabel LabelNeedStaminaDiscounted;

	public UIScrollView ScrollProbabilityWin;

	[Header("Progressing")]
	public UILabel LabelMissionLocation;

	public UILabel LabelRepeatCount;

	public UILabel LabelState;

	public UILabel LabelProgressingDesc;

	public UIButton ButtonCancel;

	public UIButton ButtonResult;

	public ContentUIDispatchState StateUI;

	public ContentUIDispatchWinLoseTable WinLoseTableUI;

	[Header("PrepareState")]
	public GameObject StateObjPrepare;

	public UILabel LabelRandomMsg;

	[Header("BattleState")]
	public GameObject StateObjBattle;

	private PrefabAuto<ContentUIDispatchGroupComponent> _dispatchGroupComponentPrefab = new PrefabAuto<ContentUIDispatchGroupComponent>("Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchGroupComponent");

	private DispatchHelper.ContentMode _mode = DispatchHelper.ContentMode.Chapter;

	private ProgressUIState _state;

	private int _repeatCount = 10;

	private int _maxCountHero;

	private int _minDifficulty;

	private int _maxDifficulty;

	private NShared.DungeonDifficultyType _chapterDifficulty;

	private EnterDungeonData _enterDungeonData;

	private RaidData _multiRaidData;

	private RaidData _singleRaidData;

	private DispatchDeckNumber _deckNumber = DispatchDeckNumber.Deck1;

	private DispatchState _curState;

	private ContentUIDispatchGroupComponent _selectedDispatchComponent;

	private List<WCL_DispatchBattleInfo> _dataListDispatchInfo = new List<WCL_DispatchBattleInfo>();

	private List<ContentUIDispatchGroupComponent> _compListDispatchGroup = new List<ContentUIDispatchGroupComponent>();

	private List<CachedHeroInfo> _dataListHeroPicked = new List<CachedHeroInfo>(8);

	private List<HeroPortraitComponent> _compListHeroPicked = new List<HeroPortraitComponent>();

	private List<EmptyHeroPortraitSmallComponent> _compListEmptyPicked = new List<EmptyHeroPortraitSmallComponent>();

	private List<CachedHeroInfo> _dataListHeroUnpicked = new List<CachedHeroInfo>();

	private List<HeroPortraitComponent> _compListHeroUnpicked = new List<HeroPortraitComponent>();

	private Dictionary<int, HeroPortraitComponent> _heroIndexToComp = new Dictionary<int, HeroPortraitComponent>();

	private RaidPartySetting.ClassEnterPermissionBanTypeData _classEnterPermissionData;

	private RaidBanType _currentRaidBanType;

	private bool _isEnterHeroOnce;

	private BoosterComponent _discountComp;

	private CampaignDungeonData _campaignDungeonData => _mode switch
	{
		DispatchHelper.ContentMode.Chapter => _enterDungeonData.OriginalDungeonData, 
		DispatchHelper.ContentMode.Raid => GameRoot.Instance.CampaignDungeonDataContainer.GetData(_chapterIndex, _dungeonIndex), 
		_ => null, 
	};

	private WCL_DispatchBattleInfo _curInfo
	{
		get
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager == null)
			{
				return null;
			}
			WCL_DispatchBattleInfo result = null;
			switch (_mode)
			{
			case DispatchHelper.ContentMode.Chapter:
				result = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Get(_chapterIndex, _dungeonIndex, (int)_deckNumber);
				break;
			case DispatchHelper.ContentMode.Raid:
			{
				CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_chapterIndex, _dungeonIndex);
				if (data == null)
				{
					return null;
				}
				DispatchType dispatchType_Easy = data.DispatchType_Easy;
				result = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.GetByRaid(dispatchType_Easy, (int)_deckNumber);
				break;
			}
			}
			return result;
		}
	}

	private int _chapterIndex
	{
		get
		{
			int result = 0;
			switch (_mode)
			{
			case DispatchHelper.ContentMode.Chapter:
				if (_enterDungeonData != null && _enterDungeonData.OriginalDungeonData != null)
				{
					result = _enterDungeonData.OriginalDungeonData.ChapterIndex;
				}
				break;
			case DispatchHelper.ContentMode.Raid:
				if (_singleRaidData != null)
				{
					result = _singleRaidData.ChapterIndex;
				}
				break;
			}
			return result;
		}
	}

	private int _dungeonIndex
	{
		get
		{
			int result = 0;
			switch (_mode)
			{
			case DispatchHelper.ContentMode.Chapter:
				if (_enterDungeonData != null && _enterDungeonData.OriginalDungeonData != null)
				{
					result = _enterDungeonData.OriginalDungeonData.DungeonIndex;
				}
				break;
			case DispatchHelper.ContentMode.Raid:
				if (_singleRaidData != null)
				{
					result = _singleRaidData.DungeonIndex;
				}
				break;
			}
			return result;
		}
	}

	private int _deckIndex => (int)(_deckNumber - 1);

	private int _difficulty
	{
		get
		{
			int result = 0;
			switch (_mode)
			{
			case DispatchHelper.ContentMode.Chapter:
				result = (int)_chapterDifficulty;
				break;
			case DispatchHelper.ContentMode.Raid:
				result = 0;
				break;
			}
			return result;
		}
	}

	public static void OpenGlobalChapter(EnterDungeonData enterDungeonData, int deckNumber = 1)
	{
		OpenGlobal(enterDungeonData, null, DispatchHelper.ContentMode.Chapter, deckNumber);
	}

	public static void OpenGlobalRaid(RaidData raidData, int deckNumber = 1)
	{
		OpenGlobal(null, raidData, DispatchHelper.ContentMode.Raid, deckNumber);
	}

	private static void OpenGlobal(EnterDungeonData enterDungeonData, RaidData raidData, DispatchHelper.ContentMode mode, int deckNumber)
	{
		if (SceneAutoBehaviour<LobbyManagement>.instance == null)
		{
			return;
		}
		ContentUIDispatchBattle instance = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<ContentUIDispatchBattle>();
		if (!(instance == null))
		{
			switch (mode)
			{
			case DispatchHelper.ContentMode.Chapter:
				instance.OpenChapter(enterDungeonData, deckNumber);
				break;
			case DispatchHelper.ContentMode.Raid:
				instance.OpenRaid(raidData, deckNumber);
				break;
			}
		}
	}

	public override void OnClosing()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.RemoveObserver(this);
		}
		DispatchInfoListUpdater.Inst.ClearTempTime();
		DispatchInfoListUpdater.Inst.SetLazyUpdate(useLazyUpdate: true);
		base.OnClosing();
	}

	public void OpenChapter(EnterDungeonData enterDungeonData, int deckNumber)
	{
		ClearData();
		_enterDungeonData = enterDungeonData;
		if (_enterDungeonData != null)
		{
			_chapterDifficulty = _enterDungeonData.Difficulty;
		}
		_deckNumber = (DispatchDeckNumber)deckNumber;
		ChangeMode(DispatchHelper.ContentMode.Chapter);
		Initialize();
		base.Open(delegate
		{
			Refresh(updateOnlyInfo: false, initBanRule: true);
		});
	}

	public void OpenRaid(RaidData raidData, int deckNumber)
	{
		ClearData();
		_multiRaidData = raidData;
		_singleRaidData = RaidHelper.GetRaidDataSingleVersion(_multiRaidData);
		_deckNumber = (DispatchDeckNumber)deckNumber;
		ChangeMode(DispatchHelper.ContentMode.Raid);
		Initialize();
		base.Open(delegate
		{
			Refresh(updateOnlyInfo: false, initBanRule: true);
		});
	}

	public void OnClickPrev()
	{
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
			_chapterDifficulty = (NShared.DungeonDifficultyType)Mathf.Max(0, (int)(_chapterDifficulty - 1));
			UpdateDungeonInfoChapter();
			break;
		case DispatchHelper.ContentMode.Raid:
		{
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(_singleRaidData.Index, _singleRaidData.Level - 1);
			RaidData data2 = GameRoot.Instance.RaidDataContainer.GetData(_multiRaidData.Index, _multiRaidData.Level - 1);
			if (data != null && data2 != null)
			{
				_singleRaidData = data;
				_multiRaidData = data2;
				UpdateDungeonInfoRaid();
			}
			break;
		}
		}
		UpdateStaminaUI();
	}

	public void OnClickNext()
	{
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
			_chapterDifficulty = (NShared.DungeonDifficultyType)Mathf.Min(3, (int)(_chapterDifficulty + 1));
			UpdateDungeonInfoChapter();
			break;
		case DispatchHelper.ContentMode.Raid:
		{
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(_singleRaidData.Index, _singleRaidData.Level + 1);
			RaidData data2 = GameRoot.Instance.RaidDataContainer.GetData(_multiRaidData.Index, _multiRaidData.Level + 1);
			if (data != null && data2 != null)
			{
				_singleRaidData = data;
				_multiRaidData = data2;
				UpdateDungeonInfoRaid();
			}
			break;
		}
		}
		UpdateStaminaUI();
	}

	public void OnClickDecreseCount()
	{
		ChangeRepeatCount(-1);
		UpdateRepeatCountInputUI();
	}

	public void OnClickIncreaseCount()
	{
		ChangeRepeatCount(1);
		UpdateRepeatCountInputUI();
	}

	public void OnClickDecreaseCount10()
	{
		ChangeRepeatCount(-10);
		UpdateRepeatCountInputUI();
	}

	public void OnClickIncreaseCount10()
	{
		ChangeRepeatCount(10);
		UpdateRepeatCountInputUI();
	}

	private void ChangeRepeatCount(int diff)
	{
		int num = _repeatCount + diff;
		if (num < 10)
		{
			num = 10;
		}
		if (num > 200)
		{
			num = 200;
		}
		_repeatCount = num;
	}

	public void OnClickRewardList()
	{
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
			if (_enterDungeonData != null)
			{
				RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_enterDungeonData.DungeonRewardIndex);
				if (data != null)
				{
					DungeonRewardInfo.OpenGlobal(data, _enterDungeonData.OriginalDungeonData.BattleType);
				}
			}
			break;
		case DispatchHelper.ContentMode.Raid:
			if (_campaignDungeonData != null)
			{
				RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_campaignDungeonData.GetRewardIndexOfDifficulty(NShared.DungeonDifficultyType.Easy));
				if (data != null)
				{
					RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(_singleRaidData.IndividualReward);
					DungeonRewardInfo.OpenGlobal(data, _campaignDungeonData.BattleType, new List<RewardData> { data2 });
				}
			}
			break;
		}
	}

	public void OnClickStartDispatch()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			OpenPopupMoveToInventory();
			return;
		}
		int reqStamina = GetReqStamina();
		int legendCostumeBoosterValue = GlobalAutoBehaviour<BoosterManager>.instance.GetLegendCostumeBoosterValue(EventBonusType.SaveDispatchStamina);
		int needStamina = reqStamina * (100 - legendCostumeBoosterValue) / 100 * _repeatCount;
		UpdateNeedStaminaLabel(needStamina);
		UpdateNeedStaminaBeforeDisCountLabel(reqStamina * _repeatCount);
		if (!IsEnoughStamina(needStamina))
		{
			UIHelper.ResolveRecoverStamina(isNotEnough: false, null, StaminaType.Chicken, 0, delegate
			{
				UpdateRepeatCountInputUI();
			});
			return;
		}
		List<int> list = new List<int>();
		int num = 0;
		for (int count = _dataListHeroPicked.Count; num < count; num++)
		{
			CachedHeroInfo cachedHeroInfo = _dataListHeroPicked[num];
			if (cachedHeroInfo != null)
			{
				list.Add(cachedHeroInfo.HeroIndex);
			}
		}
		if (list.Count != _maxCountHero)
		{
			MessagePopup.OpenInform(LocalizationManager.GetString(DispatchBattle.WARNNING_NOT_ENOUGH_PARTY));
			return;
		}
		if (IsViolateBanRule())
		{
			ShowEnterClassBanMessage();
			return;
		}
		DispatchHelper.SendRQStartDispatch(_chapterIndex, _dungeonIndex, _difficulty, (int)_deckNumber, list.ToArray(), _repeatCount, delegate(WCL_DispatchBattleInfo info)
		{
			if (info != null)
			{
				PlayStartFx();
				ChangeState(GetCurrentState());
				UpdateUI();
			}
		});
	}

	public void OnClickCancel()
	{
		if (_curInfo == null)
		{
			return;
		}
		MessagePopup3Button.Open(LocalizationManager.GetString(DispatchBattle.DISPATCH_CANCEL), LocalizationManager.GetString(DispatchBattle.DISPATCH_CANCEL_POPUP_DESC), delegate
		{
			if (_curInfo != null)
			{
				DispatchHelper.SendRQCancelDispatch(_curInfo.SlotIndex, delegate(WCL_DispatchBattleInfo info)
				{
					OpenPopupResult(info);
					ChangeState(ProgressUIState.Idle);
					UpdateUI();
				});
			}
		}, delegate
		{
		});
	}

	public void OnClickResult()
	{
		if (_curInfo == null)
		{
			return;
		}
		DispatchHelper.SendRQRequestCompleteDispatch(_curInfo.SlotIndex, delegate(WCL_DispatchBattleInfo info)
		{
			if (info != null)
			{
				OpenPopupResult(info);
				ChangeState(ProgressUIState.Idle);
				UpdateUI();
			}
		});
	}

	public void OnChangeCount(string value)
	{
		if (InputRepeatCount != null)
		{
			InputRepeatCount.value = value;
		}
	}

	public void OnFocusInCount(string value)
	{
		InputRepeatCount.validation = UIInput.Validation.Integer;
		InputRepeatCount.value = InputRepeatCount.Validate(value);
		InputRepeatCount.validation = UIInput.Validation.None;
	}

	public void OnFocusOutCount(string value)
	{
		int result = 0;
		if (int.TryParse(value, out result))
		{
			_repeatCount = result;
		}
		_repeatCount = Mathf.Clamp(_repeatCount, 10, 200);
		UpdateRepeatCountInputUI();
	}

	public void OnFocusOutDowndownList()
	{
		UIHelper.EnableGameObject(GridDispatchGroup, enable: false);
		_selectedDispatchComponent.SetOpenedState(isOpen: false);
		int i = 0;
		for (int count = _compListDispatchGroup.Count; i < count; i++)
		{
			ContentUIDispatchGroupComponent contentUIDispatchGroupComponent = _compListDispatchGroup[i];
			if (!(contentUIDispatchGroupComponent == null))
			{
				contentUIDispatchGroupComponent.SetOpenedState(isOpen: false);
			}
		}
	}

	public void OnClickInven()
	{
		SceneAutoBehaviour<ActivityManager>.instance.Run(ClientActivityType.OpenInventory.ToString(), new string[1] { InvenList.EToggle.Equip.ToString() });
	}

	public void OnClickHeroList()
	{
		string text = "1";
		CachedHeroInfo firstHeroInfo = CachedHeroInfoHelper.GetFirstHeroInfo();
		if (firstHeroInfo != null)
		{
			text = firstHeroInfo.HeroIndex.ToString();
		}
		SceneAutoBehaviour<ActivityManager>.instance.Run(ClientActivityType.OpenHero.ToString(), new string[2]
		{
			text,
			Hero3DViewPanel.EToggle.Info.ToString()
		});
	}

	public void ChangeMode(DispatchHelper.ContentMode mode)
	{
		_mode = mode;
		OnChangeMode();
	}

	public void ChangeState(ProgressUIState state)
	{
		_state = state;
		OnChangeState();
	}

	private void OnEnable()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.AddObserver(this);
		}
		VespaCoroutine.Start(this, CoProcessingUpdateUIPrepareDesctription());
	}

	private void Initialize()
	{
		_repeatCount = 10;
		ChangeState(GetCurrentState());
		_dataListHeroPicked.Clear();
		_dataListHeroUnpicked.Clear();
		FactoryBase.ReleaseAndClearList(_compListHeroPicked);
		FactoryBase.ReleaseAndClearList(_compListHeroUnpicked);
		UpdateBanTypeData();
	}

	private void UpdateBanTypeData()
	{
		_classEnterPermissionData = null;
		_currentRaidBanType = RaidBanType.None;
		if (_mode != DispatchHelper.ContentMode.Raid)
		{
			return;
		}
		BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(_singleRaidData.BanIndex);
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
					_classEnterPermissionData = new RaidPartySetting.ClassEnterPermissionBanTypeData();
					_classEnterPermissionData.LimitEnterClassList = classEnterBanRuleData.Item1;
					_classEnterPermissionData.PossibleEnterCount = classEnterBanRuleData.Item2;
					_classEnterPermissionData.RuleTactic = data.RuleTactic;
					_currentRaidBanType = RaidBanType.ClassEnterPermission;
				}
			}
		}
	}

	private void InitBanRuleText()
	{
		if (BanRuleLabel == null || BanRuleRoot == null || BanRuleTweenAlpha == null)
		{
			return;
		}
		BanRuleTweenAlpha.value = BanRuleTweenAlpha.to;
		_isEnterHeroOnce = true;
		if (_mode != DispatchHelper.ContentMode.Raid)
		{
			return;
		}
		BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(_singleRaidData.BanIndex);
		if (data != null && __Extension_Method_ICollection__.COUNT(_dataListHeroPicked) <= 0)
		{
			bool flag = !String_Extension_Method.IsNullOrEmpty(data.RuleTactic);
			UIHelper.EnableGameObject(BanRuleRoot, flag);
			if (flag)
			{
				string stringDirect = LocalizationManager.GetStringDirect(data.RuleTactic, _classEnterPermissionData.PossibleEnterCount);
				UIHelper.SetLabelText(BanRuleLabel, stringDirect);
				BanRuleTweenAlpha.ResetToBeginning();
				BanRuleTweenAlpha.enabled = false;
				_isEnterHeroOnce = false;
			}
		}
	}

	private void UpdateBanRuleLabel()
	{
		if (_mode == DispatchHelper.ContentMode.Raid && !(BanRuleRoot == null) && !(BanRuleLabel == null) && !(BanRuleTweenAlpha == null) && UIHelper.IsEnabledGameObjet(BanRuleRoot) && !_isEnterHeroOnce && __Extension_Method_ICollection__.COUNT(_dataListHeroPicked) > 0)
		{
			_isEnterHeroOnce = true;
			BanRuleTweenAlpha.ResetToBeginning();
			BanRuleTweenAlpha.PlayForward();
		}
	}

	private bool CanBanClassEnter(HeroPortraitComponent heroPortrait)
	{
		if (_mode != DispatchHelper.ContentMode.Raid)
		{
			return true;
		}
		if (_currentRaidBanType != RaidBanType.ClassEnterPermission)
		{
			return true;
		}
		if (_classEnterPermissionData == null)
		{
			return true;
		}
		CreatureData creatureData = heroPortrait.CreatureData;
		if (!_classEnterPermissionData.LimitEnterClassList.Contains(creatureData.TagType))
		{
			return true;
		}
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_compListHeroPicked); i++)
		{
			if (creatureData.Index != _compListHeroPicked[i].CreatureData.Index && creatureData.TagType == _compListHeroPicked[i].CreatureData.TagType)
			{
				num++;
			}
		}
		if (num >= _classEnterPermissionData.PossibleEnterCount)
		{
			return false;
		}
		return true;
	}

	private void UpdateClassEnterBanHeroPortrait()
	{
		if (_mode != DispatchHelper.ContentMode.Raid || _currentRaidBanType != RaidBanType.ClassEnterPermission || _classEnterPermissionData == null)
		{
			return;
		}
		Dictionary<CreatureTagType, int> dictionary = new Dictionary<CreatureTagType, int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_compListHeroPicked); i++)
		{
			CreatureTagType tagType = _compListHeroPicked[i].CreatureData.TagType;
			int vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, tagType, 0);
			if (dictionary.ContainsKey(tagType))
			{
				dictionary[tagType] = vALUE + 1;
			}
			else
			{
				dictionary.Add(tagType, vALUE + 1);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_compListHeroUnpicked); j++)
		{
			if (!_compListHeroUnpicked[j].IsSelected && !_compListHeroUnpicked[j].IsDispatched)
			{
				if (Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, _compListHeroUnpicked[j].CreatureData.TagType, 0) >= _classEnterPermissionData.PossibleEnterCount)
				{
					_compListHeroUnpicked[j].SetBan(ban: true);
				}
				else
				{
					_compListHeroUnpicked[j].SetBan(ban: false);
				}
			}
		}
	}

	private void ShowEnterClassBanMessage()
	{
		if (_mode == DispatchHelper.ContentMode.Raid)
		{
			BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(_singleRaidData.BanIndex);
			if (data != null)
			{
				NGame2.NUI.Common.ShowEnterClassBanMessage(_currentRaidBanType, data.RuleTactic, _classEnterPermissionData.PossibleEnterCount);
			}
		}
	}

	private void ChangedRoomInfoUpdateBanRule()
	{
		if (_mode != DispatchHelper.ContentMode.Raid || _currentRaidBanType != RaidBanType.ClassEnterPermission)
		{
			return;
		}
		UpdateBanTypeData();
		if (IsViolateBanRule())
		{
			for (int i = 0; i < _dataListHeroPicked.Count; i++)
			{
				int heroIndex = _dataListHeroPicked[i].HeroIndex;
				if (_heroIndexToComp.ContainsKey(heroIndex) && _heroIndexToComp[heroIndex] != null)
				{
					_heroIndexToComp[heroIndex].SetSelection(select: false);
				}
			}
			_dataListHeroPicked.Clear();
			UpdateUIListPicked();
		}
		InitBanRuleText();
		UpdateClassEnterBanHeroPortrait();
	}

	private bool IsViolateBanRule()
	{
		if (_mode != DispatchHelper.ContentMode.Raid)
		{
			return false;
		}
		if (_currentRaidBanType != RaidBanType.ClassEnterPermission)
		{
			return false;
		}
		if (_classEnterPermissionData == null)
		{
			return false;
		}
		Dictionary<CreatureTagType, int> dictionary = new Dictionary<CreatureTagType, int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_compListHeroPicked); i++)
		{
			CreatureTagType tagType = _compListHeroPicked[i].CreatureData.TagType;
			int vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, tagType, 0);
			if (dictionary.ContainsKey(tagType))
			{
				dictionary[tagType] = vALUE + 1;
			}
			else
			{
				dictionary.Add(tagType, vALUE + 1);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_classEnterPermissionData.LimitEnterClassList); j++)
		{
			CreatureTagType i_key = _classEnterPermissionData.LimitEnterClassList[j];
			if (Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, i_key, 0) > _classEnterPermissionData.PossibleEnterCount)
			{
				return true;
			}
		}
		return false;
	}

	private void Refresh(bool updateOnlyInfo = false, bool initBanRule = false)
	{
		ChangeState(GetCurrentState());
		UpdateData();
		if (initBanRule)
		{
			InitBanRuleText();
		}
		UpdateUI(updateOnlyInfo);
	}

	public override void Reset()
	{
		base.Reset();
		UIHelper.EnableGameObject(GridDispatchGroup, enable: false);
	}

	private void ClearData()
	{
		_enterDungeonData = null;
		_singleRaidData = null;
	}

	private void OnChangeMode()
	{
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
			_maxCountHero = _enterDungeonData.OriginalDungeonData.DispatchDeckCount;
			_minDifficulty = Helper.GetMinDifficulty(_chapterIndex, _dungeonIndex);
			_maxDifficulty = Helper.GetMaxDifficulty(_chapterIndex, _dungeonIndex);
			_chapterDifficulty = (NShared.DungeonDifficultyType)_minDifficulty;
			UpdateDungeonInfoChapter();
			UIHelper.SetWidgetGrayScale(SpriteDifficultyBg, grayScale: true, setColliders: true);
			break;
		case DispatchHelper.ContentMode.Raid:
			if (_singleRaidData != null)
			{
				CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_singleRaidData.ChapterIndex, _singleRaidData.DungeonIndex);
				if (data != null)
				{
					_maxCountHero = data.DispatchDeckCount;
				}
				if (_multiRaidData != null)
				{
					_maxDifficulty = DispatchHelper.GetRaidMaxCompleteLv(_multiRaidData);
				}
				UIHelper.SetWidgetGrayScale(SpriteDifficultyBg, grayScale: false);
			}
			break;
		}
	}

	private void OnChangeState()
	{
		if (_curInfo != null)
		{
			_curState = _curInfo.State;
		}
		else
		{
			_curState = DispatchState.None;
		}
		switch (_state)
		{
		case ProgressUIState.Idle:
			UIHelper.EnableGameObject(stateUIIdle, enable: true);
			UIHelper.EnableGameObject(stateUIProgressing, enable: false);
			DispatchInfoListUpdater.Inst.ClearTempTime();
			DispatchInfoListUpdater.Inst.ClearInfo();
			DispatchInfoListUpdater.Inst.SetLazyUpdate(useLazyUpdate: true);
			break;
		case ProgressUIState.Prepare:
			UIHelper.EnableGameObject(StateObjPrepare, enable: true);
			UIHelper.EnableGameObject(StateObjBattle, enable: false);
			UIHelper.EnableButton(ButtonCancel, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(stateUIIdle, enable: false);
			UIHelper.EnableGameObject(stateUIProgressing, enable: true);
			DispatchInfoListUpdater.Inst.ClearInfo();
			DispatchInfoListUpdater.Inst.SetTempTime(10f);
			DispatchInfoListUpdater.Inst.SetLazyUpdate(useLazyUpdate: false);
			break;
		case ProgressUIState.Battle:
			UIHelper.EnableGameObject(StateObjPrepare, enable: false);
			UIHelper.EnableGameObject(StateObjBattle, enable: true);
			UIHelper.EnableButton(ButtonCancel, enable: true, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(stateUIIdle, enable: false);
			UIHelper.EnableGameObject(stateUIProgressing, enable: true);
			DispatchInfoListUpdater.Inst.ClearTempTime();
			DispatchInfoListUpdater.Inst.SetInfo(_curInfo);
			DispatchInfoListUpdater.Inst.SetLazyUpdate(useLazyUpdate: false);
			break;
		case ProgressUIState.ReadyToComplete:
			UIHelper.EnableGameObject(StateObjPrepare, enable: false);
			UIHelper.EnableGameObject(StateObjBattle, enable: true);
			UIHelper.EnableButton(ButtonCancel, enable: true, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(stateUIIdle, enable: false);
			UIHelper.EnableGameObject(stateUIProgressing, enable: true);
			DispatchInfoListUpdater.Inst.ClearTempTime();
			DispatchInfoListUpdater.Inst.ClearInfo();
			DispatchInfoListUpdater.Inst.SetLazyUpdate(useLazyUpdate: true);
			break;
		}
	}

	private void UpdateData()
	{
		UpdatePickableHeroMaxNumber();
		UpdateDataListDispatchDeck();
		UpdateDataListHeroes();
	}

	private void UpdateRaidData(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return;
		}
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(info.ChapterIndex, info.DungeonIndex);
		if (raidDataByDungeon != null)
		{
			RaidData raidDataMultiVersionBySingleVersion = RaidHelper.GetRaidDataMultiVersionBySingleVersion(raidDataByDungeon.Index, raidDataByDungeon.Level);
			if (raidDataMultiVersionBySingleVersion != null)
			{
				_singleRaidData = raidDataByDungeon;
				_multiRaidData = raidDataMultiVersionBySingleVersion;
			}
		}
	}

	private void UpdateUI(bool updateOnlyInfo = false)
	{
		if (!updateOnlyInfo)
		{
			UpdateUIListDispatchDeck();
			UpdateSelectedDispatchDeckUI();
			UpdateListUIHeroes();
		}
		switch (_state)
		{
		case ProgressUIState.Idle:
			UpdateDungeonInfo();
			InitProbabilityWinInfoDesc();
			if (!updateOnlyInfo)
			{
				UpdateRepeatCountInputUI();
			}
			UIHelper.EnableButton(ButtonStart, _dataListHeroPicked.Count == _maxCountHero, grayscaleIfDisable: true);
			break;
		case ProgressUIState.Prepare:
			UpdateDungeonImg(TxMissionImg);
			UpdateDungeonName(LabelMissionName);
			UpdateMissionLocation();
			UpdateRepeatCountLabelUI();
			UpdateStaminaUI();
			StateUI.SetData(_curInfo);
			if (!updateOnlyInfo)
			{
				DispatchUIHelper.UpdateUIPrepareRandomDescription(_curInfo, LabelProgressingDesc);
			}
			UIHelper.EnableGameObject(ButtonCancel, enable: true);
			UIHelper.EnableGameObject(ButtonResult, enable: false);
			break;
		case ProgressUIState.Battle:
			UpdateDungeonImg(TxMissionImg);
			UpdateDungeonName(LabelMissionName);
			UpdateMissionLocation();
			UpdateRepeatCountLabelUI();
			UpdateStaminaUI();
			StateUI.SetData(_curInfo);
			WinLoseTableUI.SetData(_curInfo);
			if (!updateOnlyInfo)
			{
				WinLoseTableUI.UpdateUI();
			}
			if (!updateOnlyInfo)
			{
				DispatchUIHelper.UpdateUIPrepareRandomDescription(_curInfo, LabelProgressingDesc);
			}
			UIHelper.EnableGameObject(ButtonCancel, enable: true);
			UIHelper.EnableGameObject(ButtonResult, enable: false);
			break;
		case ProgressUIState.ReadyToComplete:
			UpdateDungeonImg(TxMissionImg);
			UpdateDungeonName(LabelMissionName);
			UpdateMissionLocation();
			UpdateRepeatCountLabelUI();
			UpdateStaminaUI();
			StateUI.SetData(_curInfo);
			WinLoseTableUI.SetData(_curInfo);
			if (!updateOnlyInfo)
			{
				DispatchUIHelper.UpdateUIPrepareRandomDescription(_curInfo, LabelProgressingDesc);
			}
			UIHelper.EnableGameObject(ButtonCancel, enable: false);
			UIHelper.EnableGameObject(ButtonResult, enable: true);
			break;
		}
	}

	private void UpdateSelectedDispatchDeckUI()
	{
		if (_selectedDispatchComponent == null)
		{
			_selectedDispatchComponent = _dispatchGroupComponentPrefab.Factory.Create(TransformSelectedDispatchGroup);
			if (_selectedDispatchComponent == null)
			{
				return;
			}
		}
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_compListDispatchGroup, _deckIndex))
		{
			ContentUIDispatchGroupComponent contentUIDispatchGroupComponent = _compListDispatchGroup[_deckIndex];
			if (contentUIDispatchGroupComponent != null)
			{
				contentUIDispatchGroupComponent.CopyTo(ref _selectedDispatchComponent);
				_selectedDispatchComponent.Open(this);
				_selectedDispatchComponent.SetSelected(isSelected: true);
			}
		}
	}

	private void UpdateDataListDispatchDeck()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager == null)
		{
			return;
		}
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
		{
			_dataListDispatchInfo.Clear();
			int j = 1;
			for (int num2 = 4; j < num2; j++)
			{
				WCL_DispatchBattleInfo item = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Get(_chapterIndex, _dungeonIndex, j);
				_dataListDispatchInfo.Add(item);
			}
			break;
		}
		case DispatchHelper.ContentMode.Raid:
		{
			_dataListDispatchInfo.Clear();
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_chapterIndex, _dungeonIndex);
			if (data != null)
			{
				int i = 1;
				for (int num = 4; i < num; i++)
				{
					WCL_DispatchBattleInfo byRaid = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.GetByRaid(data.DispatchType_Easy, i);
					_dataListDispatchInfo.Add(byRaid);
				}
			}
			break;
		}
		}
	}

	private void UpdateUIListDispatchDeck()
	{
		FactoryBase.ReleaseAndClearList(_compListDispatchGroup);
		ContentUIDispatchGroupComponent.IsExistReadyToCompleteInGroupList = false;
		int i = 0;
		for (int count = _dataListDispatchInfo.Count; i < count; i++)
		{
			int deckNumber = i + 1;
			WCL_DispatchBattleInfo info = _dataListDispatchInfo[i];
			ContentUIDispatchGroupComponent contentUIDispatchGroupComponent = _dispatchGroupComponentPrefab.Factory.Create(GridDispatchGroup.transform);
			if (!(contentUIDispatchGroupComponent == null))
			{
				contentUIDispatchGroupComponent.Open(deckNumber, info, this);
				contentUIDispatchGroupComponent.SetSelected(isSelected: false);
				_compListDispatchGroup.Add(contentUIDispatchGroupComponent);
				if (!ContentUIDispatchGroupComponent.IsExistReadyToCompleteInGroupList)
				{
					ContentUIDispatchGroupComponent.IsExistReadyToCompleteInGroupList = contentUIDispatchGroupComponent.IsReadyToComplete();
				}
			}
		}
	}

	private void UpdateDataListHeroes()
	{
		UpdateDataListPicked();
		UpdateDataListUnpicked();
	}

	private void UpdateDataListPicked()
	{
		if (_state == ProgressUIState.Idle)
		{
			return;
		}
		_dataListHeroPicked.Clear();
		if (_curInfo != null)
		{
			int i = 0;
			for (int count = _curInfo.HeroIndexList.Count; i < count; i++)
			{
				int key = _curInfo.HeroIndexList[i];
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key);
				AddDataPicked(heroInfo);
			}
		}
	}

	private void UpdateDataListUnpicked()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			_dataListHeroUnpicked = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		}
	}

	private void UpdateListUIHeroes(bool isFixScrollPos = false)
	{
		UpdateUIListPicked();
		UpdateUIListUnpicked(isFixScrollPos);
		UpdateBanRuleLabel();
	}

	private void UpdateUIListPicked()
	{
		if (GridPicked == null)
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_compListHeroPicked);
		FactoryBase.ReleaseAndClearList(_compListEmptyPicked);
		int i = 0;
		for (int capacity = _dataListHeroPicked.Capacity; i < capacity; i++)
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_dataListHeroPicked, i))
			{
				EmptyHeroPortraitSmallComponent emptyHeroPortraitSmallComponent = EmptyHeroPortraitSmallComponent.PrefabAuto.Factory.Create(GridPicked.transform);
				if (!(emptyHeroPortraitSmallComponent == null))
				{
					_compListEmptyPicked.Add(emptyHeroPortraitSmallComponent);
				}
				continue;
			}
			CachedHeroInfo cachedHeroInfo = _dataListHeroPicked[i];
			if (cachedHeroInfo != null)
			{
				HeroPortraitComponent heroPortraitComponent = HeroPortraitComponent.SmallPrefabAuto.Factory.Create(GridPicked.transform);
				if (!(heroPortraitComponent == null))
				{
					heroPortraitComponent.transform.localScale = Vector3.one;
					heroPortraitComponent.Open(cachedHeroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
					heroPortraitComponent.SetDispatched(isDispatched: false);
					_compListHeroPicked.Add(heroPortraitComponent);
				}
			}
		}
		GridPicked.Reposition();
	}

	private void UpdateUIListUnpicked(bool isFixScrollPos = false)
	{
		if (UIScrollGridUnpicked == null)
		{
			return;
		}
		_heroIndexToComp.Clear();
		FactoryBase.ReleaseAndClearList(_compListHeroUnpicked);
		int i = 0;
		for (int count = _dataListHeroUnpicked.Count; i < count; i++)
		{
			CachedHeroInfo cachedHeroInfo = _dataListHeroUnpicked[i];
			if (cachedHeroInfo == null)
			{
				continue;
			}
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(UIScrollGridUnpicked, HeroPortraitComponent.SmallPrefabAuto);
			if (!(heroPortraitComponent == null))
			{
				heroPortraitComponent.Open(cachedHeroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
				if (DispatchHelper.IsDispatchedHero(cachedHeroInfo))
				{
					heroPortraitComponent.SetDispatched(isDispatched: true, isSmall: true);
				}
				else
				{
					heroPortraitComponent.SetDispatched(isDispatched: false);
				}
				if (_dataListHeroPicked.Contains(cachedHeroInfo))
				{
					heroPortraitComponent.SetSelection(select: true);
				}
				else
				{
					heroPortraitComponent.SetSelection(select: false);
				}
				_compListHeroUnpicked.Add(heroPortraitComponent);
				_heroIndexToComp[cachedHeroInfo.HeroIndex] = heroPortraitComponent;
			}
		}
		UIScrollGridUnpicked.RepositionGridWithFixableScroll(isFixScrollPos);
	}

	private void AddDataPicked(CachedHeroInfo heroInfo)
	{
		if (heroInfo != null)
		{
			_dataListHeroPicked.Add(heroInfo);
		}
	}

	private void AddDataUnpicked(CachedHeroInfo heroInfo)
	{
		if (heroInfo != null)
		{
			_dataListHeroUnpicked.Add(heroInfo);
		}
	}

	private void UpdateDungeonImg(UIBundleLoader txImg)
	{
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
			UIHelper.SetDungeonPortrait(this, txImg, _chapterIndex, _dungeonIndex);
			break;
		case DispatchHelper.ContentMode.Raid:
			RaidHelper.InitPortrait(this, txImg, _singleRaidData);
			break;
		}
	}

	private void UpdateDungeonName(UILabel labelName, bool postfixDifficulty = false)
	{
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
			DispatchUIHelper.UpdateDungeonNameOnChapter(_enterDungeonData, _chapterDifficulty, labelName, postfixDifficulty);
			break;
		case DispatchHelper.ContentMode.Raid:
			DispatchUIHelper.UpdateDungeonNameOnRaid(_singleRaidData, labelName, postfixDifficulty);
			break;
		}
	}

	private void UpdateDungeonInfo()
	{
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
			UpdateDungeonInfoChapter();
			break;
		case DispatchHelper.ContentMode.Raid:
			UpdateDungeonInfoRaid();
			break;
		}
	}

	private void UpdateDungeonInfoChapter()
	{
		UpdateDungeonImg(TxMissionImg);
		switch (_state)
		{
		case ProgressUIState.Idle:
			UpdateDungeonName(LabelMissionName);
			UIHelper.SetLabelText(LabelMissionDifficult, "");
			UpdateButtonForDifficultyChange();
			break;
		case ProgressUIState.Prepare:
		case ProgressUIState.Battle:
		case ProgressUIState.ReadyToComplete:
			UpdateDungeonName(LabelMissionName);
			break;
		}
	}

	private void UpdateDungeonInfoRaid()
	{
		if (_singleRaidData == null)
		{
			return;
		}
		UpdateDungeonImg(TxMissionImg);
		switch (_state)
		{
		case ProgressUIState.Idle:
			UpdateDungeonName(LabelMissionName);
			if (LabelMissionDifficult != null)
			{
				LabelMissionDifficult.text = LocalizationManager.GetStringDirect(_singleRaidData.Step_Name);
				UpdateButtonForDifficultyChange();
			}
			ChangedRoomInfoUpdateBanRule();
			break;
		case ProgressUIState.Prepare:
		case ProgressUIState.Battle:
		case ProgressUIState.ReadyToComplete:
			UpdateDungeonName(LabelMissionName);
			break;
		}
	}

	private void UpdateRepeatCountInputUI()
	{
		if (InputRepeatCount != null)
		{
			InputRepeatCount.value = LocalizationManager.GetString(DispatchBattle.DISPATCH_COUNT, _repeatCount.ToString());
		}
		UpdateButtonForRepeatCount();
		UpdateStaminaUI();
	}

	private void UpdateButtonForDifficultyChange()
	{
		UIHelper.EnableButton(ButtonPrev, IsPossibleDifficultyPrev());
		UIHelper.EnableButton(ButtonNext, IsPossibleDifficultyNext());
	}

	private void UpdateButtonForRepeatCount()
	{
		bool enable = _repeatCount > 10;
		bool enable2 = _repeatCount < 200;
		UIHelper.EnableButton(ButtonDecrease, enable);
		UIHelper.EnableButton(ButtonDecrease10, enable);
		UIHelper.EnableButton(ButtonIncrease, enable2);
		UIHelper.EnableButton(ButtonIncrease10, enable2);
	}

	private void UpdateStaminaUI()
	{
		bool flag = HasAnyDiscount();
		if (flag)
		{
			UpdateStaminaDiscountUI();
		}
		else
		{
			UpdateStaminaNormalUI();
		}
		UIHelper.EnableGameObject(WidgetNeedStaminaNormal, !flag);
		UIHelper.EnableGameObject(WidgetNeedStaminaDiscount, flag);
	}

	private void UpdateStaminaNormalUI()
	{
		int needStamina = GetReqStamina() * _repeatCount;
		if (IsEnoughStamina(needStamina))
		{
			UIHelper.SetLabelColor(LabelNeedStaminaNormal, Color.white);
		}
		else
		{
			UIHelper.SetLabelColor(LabelNeedStaminaNormal, Color.red);
		}
		UIHelper.SetLabelText(LabelNeedStaminaNormal, needStamina.ToString());
	}

	private void UpdateStaminaDiscountUI()
	{
		int reqStamina = GetReqStamina();
		BoosterInfo legendCostumeBooster = GlobalAutoBehaviour<BoosterManager>.instance.GetLegendCostumeBooster(EventBonusType.SaveDispatchStamina);
		int num = 0;
		if (legendCostumeBooster != null)
		{
			num = legendCostumeBooster.Value;
			if (_discountComp == null)
			{
				_discountComp = SetDiscountIcon(legendCostumeBooster);
			}
		}
		float num2 = (100f - (float)num) / 100f;
		int needStamina = Convert.ToInt32(Mathf.Floor((float)reqStamina * num2)) * _repeatCount;
		if (IsEnoughStamina(needStamina))
		{
			UIHelper.SetLabelColor(LabelNeedStaminaDiscounted, Color.yellow);
		}
		else
		{
			UIHelper.SetLabelColor(LabelNeedStaminaDiscounted, Color.red);
		}
		UIHelper.SetLabelText(LabelNeedStaminaOriginal, (reqStamina * _repeatCount).ToString());
		UIHelper.SetLabelText(LabelNeedStaminaDiscounted, needStamina.ToString());
	}

	private void InitProbabilityWinInfoDesc()
	{
		if (ScrollProbabilityWin != null)
		{
			ScrollProbabilityWin.ResetPosition();
		}
	}

	private void UpdateMissionLocation()
	{
		DispatchUIHelper.UpdateDungeonName(_curInfo, LabelMissionLocation, postfixDifficulty: true);
	}

	private void UpdateRepeatCountLabelUI()
	{
		if (_curInfo != null)
		{
			string text = LocalizationManager.GetString(DispatchBattle.DISPATCH_COUNT, _curInfo.RepeatCount.ToString());
			UIHelper.SetLabelText(LabelRepeatCount, text);
		}
	}

	private void UpdatePickableHeroMaxNumber()
	{
		_dataListHeroPicked.Capacity = _maxCountHero;
	}

	private void OpenPopupResult(WCL_DispatchBattleInfo info)
	{
		if (info != null)
		{
			PopupUIDispatchBattleResult.OpenGlobal(info, DispatchHelper.GetPopupOpenData(info), null);
		}
	}

	private void OpenPopupMoveToInventory()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(ResultType.EquipItemFull);
		MessagePopup.OpenEnabled(text, message, delegate
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
		}, null, "MOVE_BAG", bYesEnabled: true);
	}

	private void PlayStartFx()
	{
		PopupUIDispatchBattleFx.OpenGlobal(_curInfo, PopupUIDispatchBattleFx.AniState.Start, null);
	}

	private void PlayEndFx(Action onComplete)
	{
		if (_curInfo != null)
		{
			PopupUIDispatchBattleFx.AniState state = PopupUIDispatchBattleFx.AniState.EndFail;
			if (_curInfo.LastWinCount > 0)
			{
				state = PopupUIDispatchBattleFx.AniState.EndSuccess;
			}
			PopupUIDispatchBattleFx.OpenGlobal(_curInfo, state, null);
		}
	}

	private bool IsPossibleDifficultyPrev()
	{
		try
		{
			switch (_mode)
			{
			case DispatchHelper.ContentMode.Chapter:
			{
				int num = (int)(_chapterDifficulty - 1);
				if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_chapterIndex, _dungeonIndex, (NShared.DungeonDifficultyType)num))
				{
					return false;
				}
				if (_chapterDifficulty <= NShared.DungeonDifficultyType.Easy)
				{
					return false;
				}
				if (num < _minDifficulty)
				{
					return false;
				}
				break;
			}
			case DispatchHelper.ContentMode.Raid:
			{
				RaidData data = GameRoot.Instance.RaidDataContainer.GetData(_multiRaidData.Index, _multiRaidData.Level - 1);
				if (data == null)
				{
					return false;
				}
				if (data.Level < 1)
				{
					return false;
				}
				break;
			}
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool IsPossibleDifficultyNext()
	{
		try
		{
			switch (_mode)
			{
			case DispatchHelper.ContentMode.Chapter:
			{
				int num = (int)(_chapterDifficulty + 1);
				if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_chapterIndex, _dungeonIndex, (NShared.DungeonDifficultyType)num))
				{
					return false;
				}
				if (num >= 3)
				{
					return false;
				}
				if (num > _maxDifficulty)
				{
					return false;
				}
				break;
			}
			case DispatchHelper.ContentMode.Raid:
			{
				RaidData data = GameRoot.Instance.RaidDataContainer.GetData(_multiRaidData.Index, _multiRaidData.Level + 1);
				if (data == null)
				{
					return false;
				}
				if (data.Level > _maxDifficulty)
				{
					return false;
				}
				break;
			}
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool IsEnoughStamina(int needStamina)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Stamina >= needStamina;
	}

	private bool HasAnyDiscount()
	{
		if (GlobalAutoBehaviour<BoosterManager>.instance.GetLegendCostumeBoosterValue(EventBonusType.SaveDispatchStamina) > 0)
		{
			return true;
		}
		return false;
	}

	private void UpdateNeedStaminaLabel(int needStamina)
	{
		UIHelper.SetLabelText(LabelNeedStaminaNormal, needStamina.ToString());
		UIHelper.SetLabelText(LabelNeedStaminaDiscounted, needStamina.ToString());
	}

	private void UpdateNeedStaminaBeforeDisCountLabel(int needStamina)
	{
		UIHelper.SetLabelText(LabelNeedStaminaOriginal, needStamina.ToString());
	}

	private int GetReqStamina()
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(_chapterIndex, _dungeonIndex)?.ReqStamina ?? 0;
	}

	private BoosterComponent SetDiscountIcon(BoosterInfo boosterInfo)
	{
		BoosterComponent boosterComponent = BoosterComponent.PrefabAuto.Factory.Create(IconDiscountBuff.transform);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(boosterComponent.transform);
		boosterComponent.Open(boosterInfo);
		return boosterComponent;
	}

	private IEnumerator CoProcessingUpdateUIPrepareDesctription()
	{
		WaitUntil waitUntilUpdatingState = new WaitUntil(() => _curState == DispatchState.Prepare || _curState == DispatchState.Simulation);
		WaitForSeconds waitSec = new WaitForSeconds(3f);
		while (true)
		{
			yield return waitUntilUpdatingState;
			DispatchUIHelper.UpdateUIPrepareRandomDescription(_curInfo, LabelProgressingDesc);
			yield return waitSec;
		}
	}

	private ProgressUIState GetCurrentState()
	{
		return GetProgressState(_deckNumber);
	}

	private ProgressUIState GetProgressState(DispatchDeckNumber deckNumber)
	{
		ProgressUIState result = ProgressUIState.Idle;
		if (_curInfo == null)
		{
			return result;
		}
		switch (_curInfo.State)
		{
		case DispatchState.Prepare:
		case DispatchState.Simulation:
			result = ProgressUIState.Prepare;
			break;
		case DispatchState.Battle:
			result = ProgressUIState.Battle;
			break;
		case DispatchState.ReadyToComplete:
			result = ProgressUIState.ReadyToComplete;
			break;
		}
		return result;
	}

	private HeroPortraitComponent GetHeroPortraitComponent(int heroIndex, List<HeroPortraitComponent> list)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			HeroPortraitComponent heroPortraitComponent = list[i];
			if (!(heroPortraitComponent == null) && heroPortraitComponent.HeroInfo.HeroIndex == heroIndex)
			{
				return heroPortraitComponent;
			}
		}
		return null;
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null || heroPortrait.IsDispatched || GetCurrentState() != ProgressUIState.Idle)
		{
			return;
		}
		if (_dataListHeroPicked.Contains(heroPortrait.HeroInfo))
		{
			_dataListHeroPicked.Remove(heroPortrait.HeroInfo);
			if (_compListHeroPicked.Contains(heroPortrait))
			{
				if (_heroIndexToComp.ContainsKey(heroPortrait.HeroInfo.HeroIndex) && _heroIndexToComp[heroPortrait.HeroInfo.HeroIndex] != null)
				{
					_heroIndexToComp[heroPortrait.HeroInfo.HeroIndex].SetSelection(select: false);
				}
				UpdateUIListPicked();
				UpdateBanRuleLabel();
			}
			else
			{
				heroPortrait.SetSelection(select: false);
				UpdateUIListPicked();
				UpdateBanRuleLabel();
			}
		}
		else if (!_dataListHeroPicked.Contains(heroPortrait.HeroInfo))
		{
			if (_dataListHeroPicked.Capacity > _dataListHeroPicked.Count)
			{
				if (!CanBanClassEnter(heroPortrait))
				{
					ShowEnterClassBanMessage();
				}
				else
				{
					_dataListHeroPicked.Add(heroPortrait.HeroInfo);
					heroPortrait.SetSelection(select: true);
					UpdateUIListPicked();
					UpdateBanRuleLabel();
				}
			}
			else
			{
				MessagePopup.OpenInform(LocalizationManager.GetString(DispatchBattle.WARNNING_ALREADY_FULL_PARTY));
			}
		}
		UpdateClassEnterBanHeroPortrait();
		UIHelper.EnableButton(ButtonStart, _dataListHeroPicked.Count == _maxCountHero, grayscaleIfDisable: true);
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
		if (heroInfo != null && _compListHeroPicked.Contains(heroPortrait))
		{
			SceneAutoBehaviour<ActivityManager>.instance.Run(ClientActivityType.OpenHero.ToString(), new string[2]
			{
				heroInfo.HeroIndex.ToString(),
				Hero3DViewPanel.EToggle.Info.ToString()
			});
		}
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void ContentUIDispatchGroupComponent.Listener.OnClick(ContentUIDispatchGroupComponent comp)
	{
		if (comp.IsSelected)
		{
			UIHelper.EnableGameObject(GridDispatchGroup, !GridDispatchGroup.gameObject.activeSelf);
		}
		else
		{
			_deckNumber = (DispatchDeckNumber)comp.DeckNumber;
			_repeatCount = ((_curInfo != null) ? _curInfo.RepeatCount : 10);
			if (_mode == DispatchHelper.ContentMode.Raid)
			{
				UpdateRaidData(_curInfo);
			}
			UpdateSelectedDispatchDeckUI();
			ChangeState(GetCurrentState());
			_dataListHeroPicked.Clear();
			UpdateDataListHeroes();
			UpdateUI();
			UpdateBanTypeData();
			InitBanRuleText();
			UIHelper.EnableGameObject(GridDispatchGroup, enable: false);
		}
		_selectedDispatchComponent.SetOpenedState(GridDispatchGroup.gameObject.activeSelf);
		int i = 0;
		for (int count = _compListDispatchGroup.Count; i < count; i++)
		{
			ContentUIDispatchGroupComponent contentUIDispatchGroupComponent = _compListDispatchGroup[i];
			if (!(contentUIDispatchGroupComponent == null))
			{
				contentUIDispatchGroupComponent.SetOpenedState(GridDispatchGroup.gameObject.activeSelf);
			}
		}
	}

	void IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>.OnChanged(UserDispatchBattleInfoManager.Observer.BroadCastInfo noti)
	{
		UserDispatchBattleInfoManager.Observer.BroadCastReason reason = noti.Reason;
		if ((uint)(reason - 1) <= 2u)
		{
			Refresh(updateOnlyInfo: true);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
		{
			HeroPortraitComponent heroPortraitComponent = GetHeroPortraitComponent(changedInfo.HeroIndex, _compListHeroPicked);
			HeroPortraitComponent heroPortraitComponent2 = GetHeroPortraitComponent(changedInfo.HeroIndex, _compListHeroUnpicked);
			if (heroPortraitComponent != null)
			{
				heroPortraitComponent.SetHeroImage();
			}
			if (heroPortraitComponent2 != null)
			{
				heroPortraitComponent2.SetHeroImage();
			}
		}
	}
}
