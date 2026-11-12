using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NShared;
using NShared.BeginCampaign;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ShakmehPartySetting : BaseWindow, IBattlePartySetting, LobbyTopmostMenu.ICustomStaminaObserver, LobbyTopmostMenu.IMiniGuideButtonObserver, HeroPortraitComponent.Listener, UserHeroListPanel.IListener, UserHeroDPSListPanel.IListener, PreSelectRewardPopup.Listener
{
	public interface Listener
	{
		void StartBattle(ShakmehBossType shakmehBossType, EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption);

		void OpenHeroDetailView(CachedHeroInfo heroInfo);

		bool OpenAutoNextBattleSelect();
	}

	public enum ButtonGroupType
	{
		UseResource,
		NoResource
	}

	[Serializable]
	public class ButtonGroup
	{
		public ButtonGroupType ButtonGroupType;

		public UIWidget ButtonGroupRoot;

		public UIWidget IconRoot;

		public IconProxy IconProxy;

		public UILabel Label_Stamina;

		public void SetupStamina(StaminaType staminaType, int StaminaVal)
		{
			CreateIconProxy();
			if (IconProxy != null)
			{
				IconProxy.SetupStamina(staminaType);
			}
			UIHelper.SetLabelText(Label_Stamina, $"- {StaminaVal}");
		}

		public void CreateIconProxy()
		{
			if (IconProxy == null)
			{
				IconProxy = IconProxy.Create(IconRoot);
			}
		}

		public void ReleaseIconProxy()
		{
			if (IconProxy != null)
			{
				FactoryBase.SafeReleaseComponent(ref IconProxy);
			}
			IconProxy = null;
		}
	}

	[Serializable]
	public class SquardFrameUI
	{
		public ShakmehBossType ShakmehBossType;

		public Transform SquardFrameRoot;

		public Transform MainSquardFrame;

		public Transform SubSquardFrame;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/ShakmehDungeon/ShakmehPartySetting";

	public const int DefaultPartySlotCount = 4;

	public const int DefaultSubPartySlotCount = 4;

	public UILabel Label_DebugText;

	public UIWidget HeroPostionTab;

	public UserHeroListPanel UserHeroListPanel;

	public UIWidget HeroDPSTab;

	public UserHeroDPSListPanel UserHeroDPSListPanel;

	public Squard Squard;

	public Transform MainSquardFrameSelected;

	public Transform SubSquardFrameSelected;

	public SquardFrameUI[] SquardFrameUi;

	public Transform SquardMiddleDescRoot;

	public Transform SquardFinalDescRoot;

	public UILabel SquardDesc1;

	public UILabel SquardDesc2;

	public ButtonGroup[] ButtonGroups;

	public UIScrollGrid BuffListGrid;

	private List<CreatureData> _bannedHeroDataList;

	private HashSet<string> _voiceReservedHeroSet = new HashSet<string>();

	private List<SkillIconComponent> _buffIconList = new List<SkillIconComponent>();

	private BattlePartySetting.IDataProvider _dataProvider;

	private Listener _listener;

	private bool _isDungeonHeroSet;

	private ShakmehBossType _shakmehBossType;

	private EnterDungeonData _enterDungeonData;

	public virtual StaminaType StaminaType
	{
		get
		{
			if (_dataProvider == null)
			{
				return StaminaType.Chicken;
			}
			return _dataProvider.GetStaminaType();
		}
	}

	public virtual BattleType BattleType
	{
		get
		{
			if (_dataProvider == null)
			{
				return BattleType.None;
			}
			return _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType;
		}
	}

	public virtual int StaminaCount
	{
		get
		{
			if (_dataProvider == null)
			{
				return 0;
			}
			return _dataProvider.GetStaminaConsumeCount();
		}
	}

	public new virtual int WindowGuideIndex => 0;

	private bool IsLocalSaveEnabled()
	{
		return true;
	}

	private HeroPortraitComponent.OwnerInfo GetMyHeroPortraitComponentOwnerInfo()
	{
		return new HeroPortraitComponent.OwnerInfo
		{
			AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
			Nick = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick,
			Level = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel
		};
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		Squard.UpdateHeroPortraitAnim();
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_enterDungeonData = null;
		ClearHeroListPanel();
		ClearSquard();
		ClearBuffList();
	}

	private void ClearHeroListPanel()
	{
		if (_shakmehBossType == ShakmehBossType.Middle)
		{
			if (UserHeroListPanel != null)
			{
				UserHeroListPanel.Release();
			}
		}
		else if (UserHeroDPSListPanel != null)
		{
			UserHeroDPSListPanel.Release();
		}
	}

	private void ClearSquard()
	{
		if (Squard != null)
		{
			Squard.Release();
		}
	}

	private void ClearBuffList()
	{
		FactoryBase.ReleaseAndClearList(_buffIconList);
	}

	public void Open(ShakmehBossType shakmehBossType, Listener listener, BattlePartySetting.IDataProvider dataProvider)
	{
		Open(delegate
		{
			Init(shakmehBossType, listener, dataProvider);
		}, null);
	}

	public void Init(ShakmehBossType shakmehBossType, Listener listener, BattlePartySetting.IDataProvider dataProvider)
	{
		_listener = listener;
		_dataProvider = dataProvider;
		_enterDungeonData = _dataProvider.GetEnterDungeon();
		if (_enterDungeonData != null)
		{
			_isDungeonHeroSet = GameRoot.Instance.DungeonHeroSetDataContainer.IsDungeonHeroSet(_enterDungeonData.Chapter, _enterDungeonData.OriginalDungeon, _enterDungeonData.Difficulty);
		}
		_shakmehBossType = shakmehBossType;
		InitSquard(out var meleeHeroList, out var rangeHeroList);
		SetHeroListPanel();
		SetBuffList();
		SetButtonGroup();
		SetBannedHeroList();
		SetDebugLabel();
		SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyTopmostMenu(this);
		SetEventTriggerSquardInit(_enterDungeonData.Chapter, _enterDungeonData.LinkedDungeon, meleeHeroList.ToArray(), rangeHeroList.ToArray());
	}

	private void SetHeroListPanel()
	{
		bool flag = _shakmehBossType == ShakmehBossType.Middle;
		if (flag)
		{
			SetUserHeroList();
		}
		else
		{
			SetUserHeroDpsList();
		}
		UIHelper.EnableGameObject(HeroPostionTab, flag);
		UIHelper.EnableGameObject(HeroDPSTab, !flag);
	}

	private void InitSquard(out List<CachedHeroInfo> meleeHeroList, out List<CachedHeroInfo> rangeHeroList)
	{
		SetSquardFrame();
		SetSquardEmptySlot();
		SetSquardHeroList(out meleeHeroList, out rangeHeroList);
		SwitchToSquardParty(isMainSquard: true);
	}

	private void SetBuffList()
	{
		ClearBuffList();
		int[] projectileGroupIndexFromServer = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetProjectileGroupIndexFromServer(BattleType.Shakmeh);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(projectileGroupIndexFromServer); i++)
		{
			int key = projectileGroupIndexFromServer[i];
			SkillProjectileGroupData data = GameRoot.Instance.SkillProjectileGroupDataContainer.GetData(key);
			if (data == null)
			{
				return;
			}
			foreach (SkillProjectileGroupData.SkillProjectileInfo skillProjectileInfo in data.GetSkillProjectileInfos())
			{
				SkillProjectileData data2 = GameRoot.Instance.SkillProjectileDataContainer.GetData(skillProjectileInfo.Index);
				if (data2 != null)
				{
					SkillData data3 = GameRoot.Instance.SkillDataContainer.GetData(data2.SkillIndex);
					if (data3 != null)
					{
						SkillIconComponent skillIconComponent = BuffListGrid.AddGridItem(SkillIconComponent.PrefabAuto);
						skillIconComponent.Open(data3.Index);
						_buffIconList.Add(skillIconComponent);
					}
				}
			}
		}
		BuffListGrid.RepositionGrid();
	}

	private void SetButtonGroup()
	{
		_dataProvider.GetEnterDungeon();
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		StaminaType staminaType = StaminaType.None;
		int num = 0;
		if (linkedDungeonData != null)
		{
			staminaType = linkedDungeonData.ReqStaminaType;
			num = linkedDungeonData.ReqStamina;
		}
		ButtonGroupType buttonGroupType = ((staminaType != StaminaType.ShakmehMiddleBossKey || num == 0) ? ButtonGroupType.NoResource : ButtonGroupType.UseResource);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ButtonGroups); i++)
		{
			ButtonGroup buttonGroup = ButtonGroups[i];
			bool flag = buttonGroup.ButtonGroupType == buttonGroupType;
			if (flag && buttonGroupType == ButtonGroupType.UseResource)
			{
				buttonGroup.SetupStamina(StaminaType.ShakmehMiddleBossKey, num);
			}
			UIHelper.EnableGameObject(buttonGroup.ButtonGroupRoot, flag);
		}
	}

	public void SetBannedHeroList()
	{
		_bannedHeroDataList = _dataProvider.GetBannedHeroDataList();
	}

	private void SetDebugLabel()
	{
		if (!ClientValueManager.IsShowDungeonIndex)
		{
			UIHelper.EnableGameObject(Label_DebugText, enable: false);
			return;
		}
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		if (enterDungeon == null)
		{
			UIHelper.EnableGameObject(Label_DebugText, enable: false);
			return;
		}
		CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			UIHelper.EnableGameObject(Label_DebugText, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Label_DebugText, enable: true);
		UIHelper.SetLabelText(Label_DebugText, $"{linkedDungeonData.ChapterIndex},{linkedDungeonData.DungeonIndex}");
	}

	private void SetUserHeroList()
	{
		if (UserHeroListPanel != null)
		{
			UserHeroListPanel.InitHeroListPanel(_dataProvider.GetEnterDungeon(), this, SetUserHeroListSelectionMark);
		}
	}

	private void SetUserHeroDpsList()
	{
		if (UserHeroDPSListPanel != null)
		{
			UserHeroDPSListPanel.InitHeroListPanel(_dataProvider.GetEnterDungeon(), this, SetUserHeroListSelectionMark);
		}
	}

	private void SetUserHeroListSelectionMark(HeroPortraitComponent heroPortraitComponent)
	{
		if (!(heroPortraitComponent == null) && Squard != null)
		{
			bool num = heroPortraitComponent.HeroInfo != null && Squard.IsContainSquardHero(heroPortraitComponent.HeroInfo.HeroIndex, 0L);
			EHeroState portraitState = EHeroState.Free;
			if (num)
			{
				portraitState = _dataProvider.GetPartyHeroState();
			}
			heroPortraitComponent.SetPortraitState(portraitState);
		}
	}

	private void SetUserHeroDPSToggle(bool isMain)
	{
		if (_shakmehBossType == ShakmehBossType.Final && UserHeroDPSListPanel != null)
		{
			UserHeroDPSListPanel.SetDPSToggle(isMain);
		}
	}

	private void RefreshSelectionMark()
	{
		if (UserHeroListPanel != null)
		{
			UserHeroListPanel.SelectionMark();
		}
		if (UserHeroDPSListPanel != null)
		{
			UserHeroDPSListPanel.SelectionMark();
		}
	}

	private void SetSquardFrame()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SquardFrameUi); i++)
		{
			SquardFrameUI squardFrameUI = SquardFrameUi[i];
			UIHelper.EnableGameObject(squardFrameUI.SquardFrameRoot, squardFrameUI.ShakmehBossType == _shakmehBossType);
		}
		bool flag = _shakmehBossType == ShakmehBossType.Final;
		UIHelper.EnableGameObject(SquardMiddleDescRoot, !flag);
		UIHelper.EnableGameObject(SquardFinalDescRoot, flag);
	}

	private void SetSquardEmptySlot()
	{
		if (_dataProvider == null || _dataProvider.GetEnterDungeon() == null || _dataProvider.GetEnterDungeon().LinkedDungeonData == null)
		{
			Squard.InitSquard(4, 4, this);
			Squard.InitAnimationSquardPostionAction(AnimateSquardPositionAciton);
			return;
		}
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		_ = linkedDungeonData.BattleType;
		int mainSquardCount = ((linkedDungeonData.MainSquardCount == 0) ? 4 : linkedDungeonData.MainSquardCount);
		sbyte subSquardCount = linkedDungeonData.SubSquardCount;
		Squard.InitSquard(mainSquardCount, subSquardCount, this);
		Squard.InitAnimationSquardPostionAction(AnimateSquardPositionAciton);
	}

	private void SetSquardHeroList(out List<CachedHeroInfo> mainHeroList, out List<CachedHeroInfo> subHeroList)
	{
		mainHeroList = new List<CachedHeroInfo>();
		subHeroList = new List<CachedHeroInfo>();
		if (!IsLocalSaveEnabled())
		{
			return;
		}
		HeroPortraitComponent.OwnerInfo myHeroPortraitComponentOwnerInfo = GetMyHeroPortraitComponentOwnerInfo();
		int mainSquardSlotCount = Squard.GetMainSquardSlotCount();
		List<CachedHeroInfo> heroInfoList = GetHeroInfoList(isMainSquard: true, mainSquardSlotCount);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoList))
		{
			for (int i = 0; i < heroInfoList.Count; i++)
			{
				CachedHeroInfo cachedHeroInfo = heroInfoList[i];
				Vector3 mainEmptySlotPositionByIndex = Squard.GetMainEmptySlotPositionByIndex(i);
				CreateSquardHeroPortraitAnim(isMainSquard: true, cachedHeroInfo, mainEmptySlotPositionByIndex, myHeroPortraitComponentOwnerInfo);
				mainHeroList.Add(cachedHeroInfo);
			}
		}
		int subSquardSlotCount = Squard.GetSubSquardSlotCount();
		List<CachedHeroInfo> heroInfoList2 = GetHeroInfoList(isMainSquard: false, subSquardSlotCount);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoList2))
		{
			for (int j = 0; j < heroInfoList2.Count; j++)
			{
				CachedHeroInfo cachedHeroInfo2 = heroInfoList2[j];
				Vector3 subEmptySlotPositionByIndex = Squard.GetSubEmptySlotPositionByIndex(j);
				CreateSquardHeroPortraitAnim(isMainSquard: false, cachedHeroInfo2, subEmptySlotPositionByIndex, myHeroPortraitComponentOwnerInfo);
				subHeroList.Add(cachedHeroInfo2);
			}
		}
	}

	private void AnimateSquardPositionAciton(List<Common.HeroPortraitAnim> heroPortraitList, List<PartySlotComponent> emptySlotList, Common.HeroPortraitAnim heroPortraitAnim)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroPortraitList); i++)
		{
			Common.HeroPortraitAnim partyMember = heroPortraitList[i];
			PartySlotComponent partySlotComponent = emptySlotList[i];
			Common.AnimateMemberPosition(partyMember, partySlotComponent.transform.position, isInsertMember: true);
		}
	}

	public bool InsertSquardHero(HeroPortraitComponent deckPortrait, bool isMainSquard = true)
	{
		if (!IsPossibleInsertSquard(deckPortrait))
		{
			return false;
		}
		PlayHeroSelectSound(deckPortrait.HeroInfo);
		Common.HeroPortraitAnim heroPortraitAnim = CreateSquardHeroPortraitAnim(isMainSquard, deckPortrait.HeroInfo, deckPortrait.transform.position, deckPortrait.Owner);
		Squard.AnimateSqaurdPosition(heroPortraitAnim);
		RefreshSelectionMark();
		SaveSquardHeroListToLocal();
		SetEventTriggerSquardInsert(deckPortrait.HeroInfo, isMainSquard);
		if (CheckSwapToSubParty())
		{
			SwitchToSquardParty(isMainSquard: false);
		}
		return true;
	}

	public bool RemoveSquardHero(int heroIndex, long ownerId = 0L, CreatureTagType classType = CreatureTagType.None)
	{
		Common.HeroPortraitAnim squardHeroProtraitAnim = Squard.GetSquardHeroProtraitAnim(heroIndex, ownerId);
		if (squardHeroProtraitAnim == null)
		{
			return false;
		}
		long ownerId2 = squardHeroProtraitAnim.Component.OwnerId;
		HeroPortraitComponent deckHeroPortrait = GetDeckHeroPortrait(squardHeroProtraitAnim.HeroIndex, ownerId2);
		Squard.RemoveHeroPortraitAnim(deckHeroPortrait, heroIndex, ownerId);
		Squard.AnimateSqaurdPosition(squardHeroProtraitAnim);
		SaveSquardHeroListToLocal();
		SetEventTriggerSquardRemove(heroIndex);
		RefreshSelectionMark();
		return true;
	}

	private Common.HeroPortraitAnim CreateSquardHeroPortraitAnim(bool isMainSquard, CachedHeroInfo heroInfo, Vector3 position, HeroPortraitComponent.OwnerInfo ownerInfo)
	{
		bool isControllerale = ownerInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		return Squard.CreateHeroPortraitAnim(isMainSquard, heroInfo, position, ownerInfo, isControllerale, delegate(Common.HeroPortraitAnim portraitAnim)
		{
			if (isControllerale)
			{
				portraitAnim.Component.EnableViewMyInfo(enable: true);
			}
			else
			{
				portraitAnim.Component.EnableViewOtherInfo(enable: true);
			}
		});
	}

	private bool IsPossibleInsertSquard(HeroPortraitComponent deckPortrait)
	{
		if (deckPortrait == null || deckPortrait.HeroInfo == null)
		{
			return false;
		}
		int num = 0;
		List<int> list = null;
		List<int> list2 = null;
		if (Squard.IsSelectedMainSquard())
		{
			num = Squard.GetMainSquardSlotCount();
			list = Squard.GetMainSquardTotalHeroIndexList();
			list2 = Squard.GetMainSquardMyHeroIndexList();
		}
		else
		{
			num = Squard.GetSubSquardSlotCount();
			list = Squard.GetSubSquardTotalHeroIndexList();
			list2 = Squard.GetSubSquardMyHeroIndexList();
		}
		if (num <= __Extension_Method_ICollection__.COUNT(list))
		{
			return false;
		}
		if (4 <= __Extension_Method_ICollection__.COUNT(list2))
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.CANNOT_INSERT_MORE));
			return false;
		}
		if (Squard.IsContainSquardHero(deckPortrait.HeroInfo.HeroIndex, 0L))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("HERO_DUPLICATED_NOT_ALLOWED"));
			return false;
		}
		return true;
	}

	private bool IsMainSquardFull()
	{
		if (Squard.IsSelectedMainSquard())
		{
			int mainSquardSlotCount = Squard.GetMainSquardSlotCount();
			List<int> mainSquardTotalHeroIndexList = Squard.GetMainSquardTotalHeroIndexList();
			if (mainSquardSlotCount <= __Extension_Method_ICollection__.COUNT(mainSquardTotalHeroIndexList))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckSwapToSubParty()
	{
		if (IsMainSquardFull())
		{
			return Squard.GetSubSquardSlotCount() != 0;
		}
		return false;
	}

	private List<CachedHeroInfo> GetHeroInfoList(bool isMainSquard, int squardSlotCount)
	{
		List<Tuple<CachedHeroInfo, long>> list = new List<Tuple<CachedHeroInfo, long>>();
		int[] forcedPartyMemberHeroIndices = _dataProvider.GetForcedPartyMemberHeroIndices();
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		int chapterIndex = 0;
		int dungeonIndex = 0;
		NShared.DungeonDifficultyType difficulty = NShared.DungeonDifficultyType.Easy;
		if (enterDungeon != null)
		{
			chapterIndex = enterDungeon.Chapter;
			dungeonIndex = enterDungeon.LinkedDungeon;
			difficulty = enterDungeon.Difficulty;
		}
		int squardSlotCount2 = Squard.GetSquardSlotCount(isMainSquard);
		int[] partyMemberHeroIndices = _dataProvider.GetPartyMemberHeroIndices(isMainSquard, squardSlotCount2);
		List<int> list2 = new List<int>();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(forcedPartyMemberHeroIndices))
		{
			list2.AddRange(forcedPartyMemberHeroIndices);
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(partyMemberHeroIndices))
		{
			int num = System.Math.Min(squardSlotCount2 - list2.Count(), __Extension_Method_ICollection__.COUNT(partyMemberHeroIndices));
			for (int i = 0; i < num; i++)
			{
				list2.Add(partyMemberHeroIndices[i]);
			}
		}
		foreach (int index in list2)
		{
			if (list.Find((Tuple<CachedHeroInfo, long> item) => item.Item1.HeroIndex == index) != null)
			{
				continue;
			}
			CachedHeroInfo cachedHeroInfo = null;
			if (_isDungeonHeroSet)
			{
				HeroInfo dungeonHeroSetHeroInfo = Helper.GetDungeonHeroSetHeroInfo(chapterIndex, dungeonIndex, difficulty, index, _enterDungeonData.IsScenario);
				if (dungeonHeroSetHeroInfo != null)
				{
					cachedHeroInfo = dungeonHeroSetHeroInfo as CachedHeroInfo;
				}
			}
			else
			{
				cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(index);
			}
			if (cachedHeroInfo != null)
			{
				long creatureWavePositionScore = StatCalculator.GetCreatureWavePositionScore(cachedHeroInfo);
				list.Add(Tuple.Create(cachedHeroInfo, creatureWavePositionScore));
			}
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			list.Sort((Tuple<CachedHeroInfo, long> l, Tuple<CachedHeroInfo, long> r) => r.Item2.CompareTo(l.Item2));
		}
		List<CachedHeroInfo> list3 = new List<CachedHeroInfo>();
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			list3.Add(list[num2].Item1);
		}
		return list3;
	}

	private HeroPortraitComponent GetDeckHeroPortrait(int heroIndex, long accountId = 0L)
	{
		if (accountId == 0L)
		{
			accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		}
		if (_shakmehBossType == ShakmehBossType.Middle)
		{
			if (UserHeroListPanel != null)
			{
				return UserHeroListPanel.GetUserHeroPortraitComponent(accountId, heroIndex);
			}
		}
		else if (UserHeroDPSListPanel != null)
		{
			return UserHeroDPSListPanel.GetUserHeroPortraitComponent(accountId, heroIndex);
		}
		return null;
	}

	private void SwitchToSquardParty(bool isMainSquard, bool isChangeTab = true)
	{
		Squard.SwichingSquard(isMainSquard);
		SetSelectedSquardFrame(isMainSquard);
		SetSelectedSquardDesc(isMainSquard);
		if (isChangeTab)
		{
			SetUserHeroDPSToggle(isMainSquard);
		}
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		if (linkedDungeonData != null)
		{
			OpenSubPartySetting openSubPartySetting = new OpenSubPartySetting();
			openSubPartySetting.BattleType = linkedDungeonData.BattleType;
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(openSubPartySetting);
		}
	}

	private void SetSelectedSquardFrame(bool isMain)
	{
		UIHelper.EnableGameObject(MainSquardFrameSelected, isMain);
		UIHelper.EnableGameObject(SubSquardFrameSelected, !isMain);
	}

	private void SetSelectedSquardDesc(bool isMain)
	{
		string key = (isMain ? "BOSS_SHAKMEH_MELEE_STORY" : "BOSS_SHAKMEH_RANGE_STORY");
		string key2 = (isMain ? "BOSS_SHAKMEH_MELEE_RULE" : "BOSS_SHAKMEH_RANGE_RULE");
		UIHelper.SetLabelText(SquardDesc1, LocalizationManager.GetStringDirect(key));
		UIHelper.SetLabelText(SquardDesc2, LocalizationManager.GetStringDirect(key2));
	}

	private void SaveSquardHeroListToLocal()
	{
		if (IsLocalSaveEnabled())
		{
			List<int> mainSquardTotalHeroIndexList = Squard.GetMainSquardTotalHeroIndexList();
			int mainSquardSlotCount = Squard.GetMainSquardSlotCount();
			SaveSquardHeroListToLocal(mainSquardTotalHeroIndexList, mainSquardSlotCount);
			List<int> subSquardTotalHeroIndexList = Squard.GetSubSquardTotalHeroIndexList();
			int subSquardSlotCount = Squard.GetSubSquardSlotCount();
			SaveSquardHeroListToLocal(subSquardTotalHeroIndexList, subSquardSlotCount, isMainParty: false);
		}
	}

	private void SaveSquardHeroListToLocal(List<int> squardHeroIndexList, int squardSlotCount, bool isMainParty = true)
	{
		for (int i = 0; i < squardSlotCount; i++)
		{
			int heroIndex = 0;
			if (__Extension_Method_ICollection__.COUNT(squardHeroIndexList) > i)
			{
				heroIndex = squardHeroIndexList[i];
			}
			_dataProvider.SetPartyMemberHeroIndex(i, heroIndex, isMainParty);
		}
	}

	private void SetEventTriggerSquardInit(int chapterIndex, int dungeonIndex, CachedHeroInfo[] mainHeroInfos, CachedHeroInfo[] subHeroInfos)
	{
		IEvent triggerEvent = new BattleInitDeck
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			MainHeroInfos = mainHeroInfos,
			SubHeroInfos = subHeroInfos
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	private void SetEventTriggerSquardInsert(CachedHeroInfo insertHeroInfo, bool isMain)
	{
		IEvent triggerEvent = new BattleInsertDeck
		{
			IsMain = isMain,
			InsertHeroInfo = insertHeroInfo
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	private void SetEventTriggerSquardRemove(int removeHeroIndex)
	{
		IEvent triggerEvent = new BattleRemoveDeck
		{
			RemoveHeroIndex = removeHeroIndex
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	private string GetVoiceCode(int heroIndex, int heroStar, int costumeIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return null;
		}
		string text = DefineSound.HeroSelectSound(data.CodeName);
		CostumeData costumeData = null;
		costumeData = ((costumeIndex == 0) ? GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(heroIndex, heroStar) : GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex));
		if (costumeData != null)
		{
			text += costumeData.VoicePostfix;
		}
		return text;
	}

	private void PlayHeroSelectSound(HeroInfo heroInfo)
	{
		if (heroInfo != null)
		{
			SoundStock stock = new SoundStock
			{
				Code = GetVoiceCode(heroInfo.HeroIndex, heroInfo.Star, heroInfo.CostumeIndex),
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 99f);
		}
	}

	public void TryStartBattle(BattlePartySetting.ExtraOptionData extraOption)
	{
		if (__Extension_Method_ICollection__.COUNT(Squard.GetMainSquardMyHeroIndexList()) == 0)
		{
			CenterMessage.Open(LocalizationManager.GetString(PartyBattle.NO_PARTY_HEROES));
			return;
		}
		OnClickStartBattle triggerEvent = new OnClickStartBattle
		{
			ChapterIndex = _dataProvider.GetEnterDungeon().Chapter,
			DungeonIndex = _dataProvider.GetEnterDungeon().LinkedDungeon,
			DifficultyType = _dataProvider.GetEnterDungeon().Difficulty,
			IsScenario = _dataProvider.GetEnterDungeon().IsScenario
		};
		if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent))
		{
			if (AutoNextBattle.IsEnabled())
			{
				AutoNextBattle.instance.OnClickStop();
			}
			return;
		}
		if (!AutoNextBattle.IsEnabled())
		{
			int num = Squard.GetMainSquardTotalHeroIndexList().Count();
			int num2 = __Extension_Method_ICollection__.COUNT(Squard.GetSubSquardTotalHeroIndexList());
			int num3 = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Count();
			int totalSquardSlotCount = Squard.GetTotalSquardSlotCount();
			if (num + num2 < totalSquardSlotCount && num3 >= totalSquardSlotCount)
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(PartyBattle.INSUFFICIENT_PARTY_HEROES_CONFIRM);
				MessagePopup.Open(text, message, delegate
				{
					CheckPreSelectReward(extraOption);
				}, null);
				return;
			}
		}
		CheckPreSelectReward(extraOption);
	}

	private void CheckPreSelectReward(BattlePartySetting.ExtraOptionData extraOption)
	{
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
		if (CheckSelectRewardPopup(linkedDungeonData))
		{
			int chapterIndex = linkedDungeonData.ChapterIndex;
			int dungeonIndex = linkedDungeonData.DungeonIndex;
			SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(chapterIndex, dungeonIndex);
			bool isScenarioReplay = enterDungeon.IsScenarioReplay;
			if (data != null && !isScenarioReplay)
			{
				PreSelectRewardPopup.Open(linkedDungeonData.BattleType, data, this, delegate
				{
					RequestStartBattle(extraOption);
				});
			}
			else
			{
				RequestStartBattle(extraOption);
			}
		}
		else
		{
			RequestStartBattle(extraOption);
		}
	}

	protected virtual void RequestStartBattle(BattlePartySetting.ExtraOptionData extraOption)
	{
		EnterDungeonData enterDungeon = new EnterDungeonData();
		BattlePartySetting.BattlePartyInfo battlePartyInfo = Squard.GetBattlePartyInfo();
		if (_dataProvider != null)
		{
			enterDungeon = _dataProvider.GetEnterDungeon();
			battlePartyInfo.StaminaType = _dataProvider.GetStaminaType();
			battlePartyInfo.StaminaCount = _dataProvider.GetStaminaConsumeCount();
		}
		if (_listener != null)
		{
			_listener.StartBattle(_shakmehBossType, enterDungeon, battlePartyInfo, extraOption);
		}
	}

	public static void OpenRepeatBattleNoticePopup(PreSelectRewardPopup.Listener listener, bool isEndCampaign = false)
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_INFORM2");
		MessagePopup.OpenEnabled(text, stringDirect, delegate
		{
			BattlePartySetting.ExtraOptionData extraOption = new BattlePartySetting.ExtraOptionData
			{
				AutoRepeat = true,
				IsEndCampaign = isEndCampaign
			};
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
			listener.TryStartBattle(extraOption);
		}, delegate
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
		}, "COMMON_CONFIRM", bYesEnabled: true);
	}

	private void OpenHeroDetailView(HeroPortraitComponent heroPortrait, bool isMine)
	{
		if (heroPortrait == null || AutoNextBattle.IsEnabled())
		{
			return;
		}
		if (TopmostMenuType == ETopmostMenuType.Tutorial)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
		}
		else if (!_isDungeonHeroSet)
		{
			if (isMine)
			{
				SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(heroPortrait.HeroInfo);
			}
			else if (_listener != null)
			{
				_listener.OpenHeroDetailView(heroPortrait.HeroInfo);
			}
		}
	}

	private bool CheckSelectRewardPopup(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return false;
		}
		if (dungeonData.BattleType != BattleType.Shakmeh)
		{
			return false;
		}
		int chapterIndex = dungeonData.ChapterIndex;
		int dungeonIndex = dungeonData.DungeonIndex;
		SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		if (!data.IsSelectable())
		{
			return false;
		}
		return true;
	}

	public void OnHeroPositionToggleChange()
	{
		if (UserHeroListPanel != null)
		{
			UserHeroListPanel.OnHeroPositionToggleChange();
		}
	}

	public void OnHeroDPSToggleChange()
	{
		if (UserHeroDPSListPanel != null && UserHeroDPSListPanel.OnDPSToggleChange())
		{
			bool isMainSquard = UserHeroDPSListPanel.IsSelectedMelee();
			SwitchToSquardParty(isMainSquard, isChangeTab: false);
		}
	}

	public void OnClickMainSquardFrame()
	{
		if (!Squard.IsSelectedMainSquard())
		{
			SwitchToSquardParty(isMainSquard: true);
		}
	}

	public void OnClickSubSquardFrame()
	{
		if (Squard.IsSelectedMainSquard())
		{
			SwitchToSquardParty(isMainSquard: false);
		}
	}

	public void OnClickStartBattle()
	{
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		if (enterDungeon != null && enterDungeon.LinkedDungeonData != null && !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEnterCount(enterDungeon))
		{
			if (!enterDungeon.CheckIsNotInitDailyCount())
			{
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetString(ResultType.ExceededDailyEntryCount));
			}
			return;
		}
		if (BattleSelectPopup.IsMultipleMenu(_dataProvider) && !AutoNextBattle.IsEnabled() && !NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			BattleSelectPopup.Open(_dataProvider);
			return;
		}
		bool flag = false;
		if ((enterDungeon.LinkedDungeonData != null) ? enterDungeon.CheckBattleEnter(StaminaCount) : UIHelper.CheckStamina(StaminaType, StaminaCount, enterDungeon.Chapter, null))
		{
			if (AutoNextBattle.IsEnabled() || !_listener.OpenAutoNextBattleSelect())
			{
				TryStartBattle(null);
			}
		}
		else if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnClickStop();
		}
	}

	public void OnClickRepeatBattle()
	{
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		_ = enterDungeon.LinkedDungeonData;
		if (enterDungeon.CheckBattleEnter())
		{
			OpenRepeatBattleNoticePopup(this);
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	public void OnClickExitButton()
	{
		if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnClickStop();
		}
		Close();
	}

	public bool IsBannedHero(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null)
		{
			return false;
		}
		if (heroPortrait.CreatureData == null)
		{
			return false;
		}
		if (_bannedHeroDataList == null)
		{
			return false;
		}
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_bannedHeroDataList); i < num; i++)
		{
			CreatureData creatureData = _bannedHeroDataList[i];
			if (creatureData != null && heroPortrait.CreatureData.Index == creatureData.Index)
			{
				return true;
			}
		}
		return false;
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (!heroPortrait.IsControllerable)
		{
			return;
		}
		if (heroPortrait.HeroInfo == null)
		{
			VespaDebug.LogError("HeroInfo is null!!!");
			return;
		}
		int heroIndex = heroPortrait.HeroInfo.HeroIndex;
		long ownerId = heroPortrait.OwnerId;
		Common.HeroPortraitAnim squardHeroProtraitAnim = Squard.GetSquardHeroProtraitAnim(heroIndex, 0L);
		if (squardHeroProtraitAnim != null && Vespa_Common_Class_Extension_Method.EQUALS(squardHeroProtraitAnim.Component, heroPortrait))
		{
			RemoveSquardHero(heroPortrait.HeroInfo.HeroIndex, heroPortrait.OwnerId, heroPortrait.CreatureData.TagType);
			return;
		}
		squardHeroProtraitAnim = Squard.GetSquardHeroProtraitAnim(heroIndex, ownerId);
		if (squardHeroProtraitAnim != null)
		{
			RemoveSquardHero(heroPortrait.HeroInfo.HeroIndex, heroPortrait.OwnerId, heroPortrait.CreatureData.TagType);
			return;
		}
		squardHeroProtraitAnim = Squard.GetSquardHeroProtraitAnim(heroIndex, 0L);
		if (squardHeroProtraitAnim != null)
		{
			CenterMessage.Open(LocalizationManager.GetString(PartyBattle.HERO_DUPLICATED_NOT_ALLOWED));
			return;
		}
		if (IsBannedHero(heroPortrait))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("NOT_REQ_HEROES"));
			return;
		}
		bool num = Squard.IsContainSquardHero(heroIndex, 0L);
		bool flag = Squard.IsSelectedMainSquard();
		if (num && !heroPortrait.IsSelected)
		{
			SwitchToSquardParty(!flag);
		}
		else
		{
			InsertSquardHero(heroPortrait, flag);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
		OpenHeroDetailView(heroPortrait, isMine);
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
		OpenHeroDetailView(heroPortrait, isMine: true);
	}

	bool UserHeroListPanel.IListener.IsRestrictedHero(HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return false;
		}
		if (heroInfo.CreatureData == null)
		{
			return false;
		}
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_bannedHeroDataList); i < num; i++)
		{
			CreatureData creatureData = _bannedHeroDataList[i];
			if (creatureData != null && creatureData.Index == heroInfo.CreatureData.Index)
			{
				return true;
			}
		}
		return false;
	}

	bool UserHeroListPanel.IListener.hasAcceptedHeroes()
	{
		return false;
	}

	bool UserHeroListPanel.IListener.hasBannedHeroes()
	{
		return __Extension_Method_ICollection__.COUNT(_bannedHeroDataList) > 0;
	}

	bool UserHeroDPSListPanel.IListener.IsRestrictedHero(HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return false;
		}
		if (heroInfo.CreatureData == null)
		{
			return false;
		}
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_bannedHeroDataList); i < num; i++)
		{
			CreatureData creatureData = _bannedHeroDataList[i];
			if (creatureData != null && creatureData.Index == heroInfo.CreatureData.Index)
			{
				return true;
			}
		}
		return false;
	}

	bool UserHeroDPSListPanel.IListener.hasAcceptedHeroes()
	{
		return false;
	}

	bool UserHeroDPSListPanel.IListener.hasBannedHeroes()
	{
		return __Extension_Method_ICollection__.COUNT(_bannedHeroDataList) > 0;
	}
}
