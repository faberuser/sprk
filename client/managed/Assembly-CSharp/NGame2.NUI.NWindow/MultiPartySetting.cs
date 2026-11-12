using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NParty;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MultiPartySetting : RaidPartySetting, PartyManager.IPartyListener, LobbyTopmostMenu.ICustomChapterObserver
{
	public new interface Listener : RaidPartySetting.Listener, BattlePartySetting.Listener
	{
		void OnClickPartyRoomSetting();
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Multi/MultiPartySetting";

	public UISprite SpriteConsumeResourceChecked;

	private bool repeatableDungeon;

	private PartyDungeonData _partyDungeonData;

	public int ChapterIndex
	{
		get
		{
			if (_partyDungeonData == null)
			{
				return 0;
			}
			return _partyDungeonData.ChapterIndex;
		}
	}

	public override StaminaType StaminaType => Stamina;

	public void Open(Listener listener, IDataProvider dataProvider)
	{
		EnterDungeonData enterDungeon = dataProvider.GetEnterDungeon();
		int chapter = enterDungeon.Chapter;
		int linkedDungeon = enterDungeon.LinkedDungeon;
		_partyDungeonData = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapter, linkedDungeon);
		if (_partyDungeonData != null)
		{
			Stamina = enterDungeon.LinkedDungeonData.ReqStaminaType;
			base.Open(delegate
			{
				Init(listener, dataProvider);
			});
		}
	}

	public void Init(Listener listener, IDataProvider dataProvider)
	{
		if (dataProvider == null)
		{
			return;
		}
		if (!GlobalAutoBehaviour<PartyManager>.isValidInstance || !GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			VespaDebug.LogError("Party must exist before RaidPartySetting.InitForRaid()");
			return;
		}
		EnterDungeonData enterDungeon = dataProvider.GetEnterDungeon();
		_ = enterDungeon.Chapter;
		_ = enterDungeon.LinkedDungeon;
		repeatableDungeon = BattlePartySetting.AutoRepeatable(enterDungeon);
		Init((BattlePartySetting.Listener)listener, dataProvider);
		GlobalAutoBehaviour<PartyManager>.instance.AddPartyListener(this);
		UIHelper.EnableGameObject(RaidDeco, enable: true);
		_staminaCount = enterDungeon.LinkedDungeonData.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), enterDungeon.Difficulty);
		InitFriendSlot();
		UpdateStartBattleState();
		UpdateInviteButton();
		UpdateBattleRepeatToggle();
		UpdateRoomSettingButton();
		SetTitle();
		if ((bool)PartyChat)
		{
			PartyChat.Open(noParty: false, this);
			PartyChat.ClearChat();
		}
		SetBanDec();
		SetConsumeResource();
	}

	protected override void SetSquardEmptySlot()
	{
		if (_partyDungeonData == null)
		{
			return;
		}
		int deckCount = _partyDungeonData.DeckCount;
		_currentSquard = GetSquardUi(SquardUi.ESquardType.OnlyMain).Squard;
		_currentSquard.InitSquard(deckCount, 0, this);
		SetOnetimeBoosterUIs(0);
		SetupUseOnetimeBoosterPanel();
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

	protected override void SetSquardHeroList(out List<CachedHeroInfo> mainHeroList, out List<CachedHeroInfo> subHeroList)
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
					((PartyManager.IPartyListener)this).OnPartyMemberDeckHeroChanged(memberInfo, value.HeroIndex, value, isMain: true);
					mainHeroList.Add(value);
				}
			}
		}
	}

	private void SetConsumeResource()
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo != null)
		{
			if (myPartyMemberInfo.IsMaster)
			{
				UpdateConsumeResourceButtonState(isChecked: true);
			}
			else
			{
				UpdateConsumeResourceButtonState(isChecked: false);
			}
		}
	}

	protected override void SetTitle()
	{
		if (_partyDungeonData == null || GameRoot.Instance.CampaignDungeonDataContainer.GetData(_partyDungeonData.ChapterIndex, _partyDungeonData.DungeonIndex) == null)
		{
			return;
		}
		CampaignNodeData linkedNodeData = _dataProvider.GetEnterDungeon().LinkedNodeData;
		if (linkedNodeData != null)
		{
			string nodeDungeonName = UIHelper.GetNodeDungeonName(linkedNodeData);
			UIHelper.SetLabelText(TitleLabel, nodeDungeonName);
			UIHelper.EnableGameObject(LevelLabel, enable: false);
			UIHelper.EnableGameObject(HardModeLabel, enable: false);
			UIHelper.EnableGameObject(ChallengeDiffLabel, enable: false);
			if (Aligner != null)
			{
				Aligner.Align();
			}
		}
	}

	protected override void InitFriendSlot()
	{
		if (_partyDungeonData != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(LockedLabels); i++)
			{
				bool flag = i < _partyDungeonData.PlayerCount;
				UIHelper.EnableGameObject(LockedLabels[i], !flag);
			}
		}
	}

	protected override void UpdateRoomSettingButton()
	{
		if (!(RoomSettingButton == null))
		{
			bool enable = GlobalAutoBehaviour<PartyManager>.instance.IsPartyRoomMaster();
			UIHelper.EnableGameObject(RoomSettingButton, enable);
		}
	}

	protected override void UpdateBattleRepeatToggle()
	{
		if (repeatableDungeon)
		{
			UIHelper.EnableGameObject(SpriteBattleRepeatUnusable, enable: false);
			UIHelper.EnableGameObject(SpriteBattleRepeatChecked, GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle);
		}
		else
		{
			UIHelper.EnableGameObject(SpriteBattleRepeatUnusable, enable: true);
			UIHelper.EnableGameObject(SpriteBattleRepeatChecked, enable: false);
		}
		DisableOnetimeBooster();
	}

	public override void ToggleBattleRepeat()
	{
		if (!GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo().IsMaster || !repeatableDungeon)
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
			GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.SetRepeatBattle(IsRepeat: false);
			GlobalAutoBehaviour<PartyManager>.instance.SendRoomRepeat(IsRepeat: false);
			UpdateBattleRepeatToggle();
		}
	}

	protected override void RequestStartBattle(ExtraOptionData extraOption)
	{
		if (!SprkDispatchSelection.CanUseParty(this))
		{
			return;
		}
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
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
		}
		else if (myPartyMemberInfo.IsMaster)
		{
			if (!IsAllReady())
			{
				return;
			}
			BattlePartyInfo battlePartyInfo = _currentSquard.GetBattlePartyInfo();
			int[] heroIndices = battlePartyInfo.heroIndices;
			long[] accountIds = battlePartyInfo.accountIds;
			int num = 0;
			long itemBoosterCount = num;
			if (_onetimeBoosterPanel.UseOnetimeBoosterItem)
			{
				int num2 = 1;
				itemBoosterCount = num2;
				if (UIHelper.IsEnabledGameObjet(SpriteBattleRepeatChecked))
				{
					itemBoosterCount = SingletonAuto<CampaignContext>.instance.OnetimeBoosterCount;
				}
			}
			GlobalAutoBehaviour<PartyManager>.instance.StartBattleParty(heroIndices, accountIds, itemBoosterCount, delegate(PartyManager.StartBattleResult result)
			{
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(accountIds) && !__Extension_Method_ICollection__.IsNullOrEmpty(heroIndices))
				{
					GlobalAutoBehaviour<PartyManager>.instance.SetPartyInfoForEndBattle(accountIds, heroIndices);
				}
				GlobalAutoBehaviour<PartyManager>.instance.ProcessStartBattleResult(result, isMaster: true);
			});
		}
		else
		{
			bool isReady = !myPartyMemberInfo.IsBattleReady;
			GlobalAutoBehaviour<PartyManager>.instance.SendRoomReady(isReady);
		}
	}

	public override void Close()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().OpenStaminaType(this);
		base.Close();
	}

	public void ChangeRoomInfo(PartyDungeonData partyDungeonData, PartyDungeonDifficultyType difficulty)
	{
		PartyDungeonData partyDungeonData2 = _partyDungeonData;
		_partyDungeonData = partyDungeonData;
		if (partyDungeonData2.DeckCount != _partyDungeonData.DeckCount)
		{
			SetSquardEmptySlot();
		}
		if (partyDungeonData2.PlayerCount != _partyDungeonData.PlayerCount)
		{
			InitFriendSlot();
		}
		int chapterIndex = _partyDungeonData.ChapterIndex;
		int dungeonIndex = _partyDungeonData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		int staminaOfDifficulty = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), UIHelper.GetDungeonDifficultyType(difficulty));
		if (staminaOfDifficulty < 0)
		{
			VespaDebug.LogError("Raid chicken is under 0 : dungeon=" + data?.ToString() + " chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
			return;
		}
		LobbyTopmostMenu instance = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>();
		if (instance != null)
		{
			Stamina = data.ReqStaminaType;
			instance.OpenStaminaType(this);
		}
		NShared.DungeonDifficultyType dungeonDifficultyType = UIHelper.GetDungeonDifficultyType(difficulty);
		EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, dungeonDifficultyType);
		_dataProvider = new DefaultBattlePartySettingDataProdiver(null, Stamina, staminaOfDifficulty, Battle.START_BATTLE, enterDungeonData);
		UpdateStaminaCount();
		UpdateStartBattleState();
		repeatableDungeon = BattlePartySetting.AutoRepeatable(enterDungeonData);
		if (GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.IsRepeatBattle && !repeatableDungeon)
		{
			GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.SetRepeatBattle(IsRepeat: false);
			GlobalAutoBehaviour<PartyManager>.instance.SendRoomRepeat(IsRepeat: false);
		}
		UpdateBattleRepeatToggle();
		bool activeSelf = SpriteConsumeResourceChecked.gameObject.activeSelf;
		UpdateConsumeResourceButtonState(activeSelf);
		SetTitle();
		bool flag = DispatchHelper.IsDispatchable(enterDungeonData.LinkedDungeonData);
		UserHeroListPanel.SetDispatch(flag);
		if (flag)
		{
			UpdateSpuardHeroList();
		}
	}

	private void UpdateSpuardHeroList()
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(SquardUis); i < num; i++)
		{
			SquardUi squardUi = SquardUis[i];
			if (squardUi == null || squardUi.Squard == null)
			{
				continue;
			}
			List<int> mainSquardMyHeroIndexList = squardUi.Squard.GetMainSquardMyHeroIndexList();
			int j = 0;
			for (int num2 = __Extension_Method_ICollection__.COUNT(mainSquardMyHeroIndexList); j < num2; j++)
			{
				int heroIndex = mainSquardMyHeroIndexList[j];
				if (DispatchHelper.IsDispatchedHero(heroIndex))
				{
					RemoveSquardHero(heroIndex, 0L);
				}
			}
		}
	}

	public override void UpdateStartBattleState()
	{
		base.UpdateStartBattleState();
		StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.UseResource, _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType, StaminaType, StaminaCount, showForce: true);
	}

	protected override void UpdateStaminaCount()
	{
		int chapterIndex = _partyDungeonData.ChapterIndex;
		int dungeonIndex = _partyDungeonData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
			if (partyInfo != null)
			{
				_staminaCount = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), UIHelper.GetDungeonDifficultyType(partyInfo.Difficulty));
			}
		}
	}

	public void OnClickConsoumeResource()
	{
		if (!GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo().IsMaster)
		{
			bool activeSelf = SpriteConsumeResourceChecked.gameObject.activeSelf;
			activeSelf = !activeSelf;
			UpdateConsumeResourceButtonState(activeSelf);
		}
	}

	private void UpdateConsumeResourceButtonState(bool isChecked)
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		myPartyMemberInfo.UseStamina = isChecked;
		myPartyMemberInfo.UseOnetimeBooster = _onetimeBoosterPanel.UseOnetimeBoosterItem;
		UpdateStaminaCount();
		UIHelper.EnableGameObject(SpriteConsumeResourceChecked, isChecked);
		_onetimeBoosterPanel.UsableOnetimeBoosterItem = isChecked;
		if (!isChecked)
		{
			bool useOnetimeBooster = (_onetimeBoosterPanel.UseOnetimeBoosterItem = false);
			myPartyMemberInfo.UseOnetimeBooster = useOnetimeBooster;
			DisableOnetimeBooster();
		}
		StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.UseResource, _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType, StaminaType, isChecked ? StaminaCount : 0, showForce: true);
	}

	protected override int GetStaminaCount()
	{
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
		{
			return 0;
		}
		if (!myPartyMemberInfo.UseStamina)
		{
			return 0;
		}
		return _staminaCount;
	}

	public void OnClickStartBattleMultiParty()
	{
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		if (!SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle(enterDungeon))
		{
			return;
		}
		int chapter = enterDungeon.Chapter;
		int linkedDungeon = enterDungeon.LinkedDungeon;
		NShared.DungeonDifficultyType dungeonDifficultyType = UIHelper.GetDungeonDifficultyType(GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.Difficulty);
		bool num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(chapter) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapter, linkedDungeon);
		NShared.DungeonDifficultyType maxAvailableDifficulty = (NShared.DungeonDifficultyType)Helper.GetMaxAvailableDifficulty(chapter, linkedDungeon);
		if (!num || dungeonDifficultyType > maxAvailableDifficulty)
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect(" PARTY_ENTER_IMPOSSIBLE_DES");
			MessagePopup.Open(text, stringDirect);
			return;
		}
		if (IsUseOnetimeBoosterItem())
		{
			_useOnetimeBoosterItemCount = 1;
		}
		MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		bool flag = true;
		if (myPartyMemberInfo.UseStamina)
		{
			flag = ((enterDungeon.LinkedDungeonData != null) ? enterDungeon.CheckBattleEnter(StaminaCount) : UIHelper.CheckStamina(StaminaType, StaminaCount, enterDungeon.Chapter, null));
		}
		if (flag)
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

	public override void OnClickRoomSetting()
	{
		if (_listener is Listener listener)
		{
			listener.OnClickPartyRoomSetting();
		}
	}

	void PartyManager.IPartyListener.OnPartyRoomInfoUpdated(RaidRoomInfo raidRoomInfo, bool isViolateBanRule)
	{
		PartyDungeonData data = GameRoot.Instance.PartyDungeonDataContainer.GetData(raidRoomInfo.ChapterIndex, raidRoomInfo.DungeonIndex);
		if (data != null)
		{
			ChangeRoomInfo(data, raidRoomInfo.DungeonDifficulty);
		}
	}

	void PartyManager.IPartyListener.OnPartyMemberResults(MemberInfo memberInfo)
	{
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
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId == memberInfo.AccountId;
		if (flag)
		{
			UpdateConsumeResourceButtonState(flag);
		}
	}

	protected override int GetPartyMemberCount()
	{
		if (_partyDungeonData == null || _partyDungeonData.DeckCount <= 0)
		{
			return base.GetPartyMemberCount();
		}
		return _partyDungeonData.DeckCount;
	}

	protected override int GetMaxPartyMemberPerPlayer()
	{
		if (_partyDungeonData == null || _partyDungeonData.DeckCountPerPlayer <= 0)
		{
			return base.GetMaxPartyMemberPerPlayer();
		}
		return _partyDungeonData.DeckCountPerPlayer;
	}

	protected override bool IsLocalSaveEnabled()
	{
		return false;
	}

	public override void OnToggleOnetimeBootyBooster()
	{
		if (!UIHelper.IsEnabledGameObjet(SpriteConsumeResourceChecked))
		{
			return;
		}
		if (UIHelper.IsEnabledGameObjet(SpriteBattleRepeatChecked))
		{
			if (!_onetimeBoosterPanel.UseOnetimeBoosterItem)
			{
				string value = GameRoot.Instance.ConstantDataContainer.GetValue("OneTimeItemBoosterCode");
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(value);
				if (dataByCode == null || !GetStamina(out var curStamina, out var maxStamina, out var reqStamina))
				{
					return;
				}
				long num = curStamina;
				int num2 = reqStamina;
				long val = num / num2;
				int num3 = 0;
				if (System.Math.Max(num3, val) == 0L)
				{
					return;
				}
				UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
				long i_valueA = 0L;
				long totalGem = userManager.TotalGem;
				int buyGem = dataByCode.BuyGem;
				long num4 = NVespa.Math.Max(i_valueA, totalGem / buyGem);
				if (num4 == 0L)
				{
					if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
					{
						NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
					}
					else
					{
						CenterMessage.Open(LocalizationManager.GetStringDirect("BOOSTER_ONETIME_REWARD_INSUFFICIENT_RUBY"));
					}
				}
				else
				{
					OpenSelectItemMessagePopupForOnetimeBooster(maxStamina, curStamina, reqStamina, num4, dataByCode);
				}
			}
			else
			{
				DisableOnetimeBooster();
			}
		}
		else
		{
			base.OnToggleOnetimeBootyBooster();
			GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo().UseOnetimeBooster = _onetimeBoosterPanel.UseOnetimeBoosterItem;
			if (_onetimeBoosterPanel.UseOnetimeBoosterItem)
			{
				CampaignContext instance = SingletonAuto<CampaignContext>.instance;
				int num5 = 1;
				instance.OnetimeBoosterCount = num5;
			}
			else
			{
				CampaignContext instance2 = SingletonAuto<CampaignContext>.instance;
				int num6 = 0;
				instance2.OnetimeBoosterCount = num6;
			}
		}
	}

	private void DisableOnetimeBooster()
	{
		EndCampaign.DisableOnetimeBoosterRepeatBattle();
		IList<BoosterItemData> allData = GameRoot.Instance.BoosterItemDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			UIHelper.EnableGameObject(_onetimeBoosterPanel.OnetimeBoosterRoot, enable: false);
			return;
		}
		BoosterItemData boosterItemData = allData.Where((BoosterItemData v) => v.IsOnetime).FirstOrDefault();
		if (boosterItemData == null)
		{
			UIHelper.EnableGameObject(_onetimeBoosterPanel.OnetimeBoosterRoot, enable: false);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect(boosterItemData.OnetimeBoosterTitle);
		UIHelper.SetLabelText(_onetimeBoosterPanel.LabelName, stringDirect);
		_onetimeBoosterPanel.UseOnetimeBoosterItem = false;
		GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo().UseOnetimeBooster = _onetimeBoosterPanel.UseOnetimeBoosterItem;
	}

	private void OpenSelectItemMessagePopupForOnetimeBooster(long maxStamina, long curStamina, int reqStamina, long onetimeBoosterPlayableCount, ItemData onetimeBoosterItemData)
	{
		maxStamina = ((maxStamina >= curStamina) ? maxStamina : curStamina);
		long val = maxStamina / reqStamina;
		int num = 0;
		long val2 = System.Math.Max(num, val);
		long val3 = curStamina / reqStamina;
		int num2 = 0;
		long val4 = System.Math.Max(num2, val3);
		onetimeBoosterPlayableCount = System.Math.Min(onetimeBoosterPlayableCount, val2);
		onetimeBoosterPlayableCount = System.Math.Min(onetimeBoosterPlayableCount, val4);
		List<Tuple<ItemData, int>> list = new List<Tuple<ItemData, int>>();
		long num3 = onetimeBoosterPlayableCount;
		list.Add(new Tuple<ItemData, int>(onetimeBoosterItemData, checked((int)num3)));
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_INFORM_BY_BOOSTER");
		SelectItemMessagePopup.OpenWithGemPrice(text, stringDirect, list, onetimeBoosterItemData.BuyGem, delegate(SelectItemMessagePopup selectItemPopup, int idx, int count)
		{
			_onetimeBoosterPanel.UseOnetimeBoosterItem = true;
			onetimeBoosterPlayableCount = count;
			SingletonAuto<CampaignContext>.instance.OnetimeBoosterCount = onetimeBoosterPlayableCount;
			string stringDirect2 = LocalizationManager.GetStringDirect("REPEATBATTLE_MULTI_BOOSTER_COUNT", count);
			UIHelper.SetLabelText(_onetimeBoosterPanel.LabelName, stringDirect2);
			selectItemPopup.Close();
			GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo().UseOnetimeBooster = _onetimeBoosterPanel.UseOnetimeBoosterItem;
		}, DisableOnetimeBooster, closeOnYes: false);
	}

	private bool GetStamina(out long curStamina, out long maxStamina, out int reqStamina)
	{
		int num = 0;
		curStamina = num;
		int num2 = 0;
		maxStamina = num2;
		reqStamina = 0;
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEventDungeon(enterDungeon.Chapter, enterDungeon.LinkedDungeon, out maxStamina, out curStamina))
		{
			reqStamina = 1;
			return true;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(linkedDungeonData.ReqStaminaType, out curStamina, out maxStamina, enterDungeon.Chapter))
		{
			reqStamina = linkedDungeonData.ReqStamina;
			return true;
		}
		return false;
	}
}
