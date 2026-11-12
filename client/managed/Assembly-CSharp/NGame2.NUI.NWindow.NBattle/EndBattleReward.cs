using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NCutScene;
using NGame2.NEventTrigger;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NSubStory;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.ClassBuff;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NBooster;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class EndBattleReward : FrameControlWindow, IReservedAction, IInspectorGuiDrawer, IUserObserver<NGame2.NAccount.UserItemTimeDurationManager.BoosterChangeInfo>, IUserObserver
{
	public class StockData
	{
		public int[] HeroIndices;

		public int[] AiHeroIndices;

		public int[] GroupHeroInices;

		public long[] AccountIds;

		public bool[] IsDeadHeroes;

		public int[] DeadHeroIndices;

		public Helper.LevelInfo OldLevelInfo;

		public EnterDungeonData EnterDungeonData;
	}

	[Serializable]
	public class ResourceUiData
	{
		public UIWidget Root;

		public UIWidget IconRoot;

		public IconProxy IconProxy;

		public UILabel LabelStamina;

		public UILabel LabelEventStamina;

		public UIWidget StaminaTooltip;

		public UILabel Label_FullStaminaRecoverTime;

		public UILabel Label_StaminaRecoverTime;

		public UIButton RechargeButton;

		public UIWidget StaminaRoot;

		public UIWidget EventStaminaRoot;
	}

	public interface Listener
	{
		void BattleRepeat();

		void ReturnToLobby();

		void ShowStatistics();

		void ReturnToLobbyAndOpenNextDungeon();

		void ReturnToLobbyAndOpenNextChapterConquestDungeon();

		void ReturnToLobbyAndOpenNextChapterHideOut();

		void ReturnToLobbyAndOpenHeroDeck();

		void ReturnToLobbyAndOpenPartySetting();

		void ReturnToLobbyAndOpenInventory();

		void MoveToPurifyRoom(CachedHeroInfo heroInfo);
	}

	private class TeamExpLevelActionInfo
	{
		public Helper.LevelInfo oldLevelInfo;

		public Helper.LevelInfo curLevelInfo;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/EndBattleReward";

	public LocalizeLinkData ItemCountLocalize = new LocalizeLinkData();

	public UILabel ItemCountLabel;

	public UIScrollGrid RewardItemGrid;

	public float RewardItemAddInterval = 0.3f;

	public float RewardItemRepositionTime = 0.1f;

	public float DisabledButtonAlpha;

	public AnimationCurve RewardItemRepositionCurve = new AnimationCurve();

	public UIWidget HeroPivot;

	public GameObject ButtonGroup;

	public float ButtonDelay;

	public UIWidget Widget_NextDungeon;

	public UIWidget Widget_Repeat;

	public UIWidget Widget_ToHeroDeck;

	public UIWidget Widget_ToPartySetting;

	public UIWidget Widget_Bag;

	public UIButton Button_NextDungeon;

	public UIButton Button_Repeat;

	public UIButton Button_ToHeroDeck;

	public UIButton Button_ToPartySetting;

	public UIButton Button_Bag;

	public UIButton Button_ToPurifyRoom;

	private List<BoosterComponent> _ListBoosterComponent = new List<BoosterComponent>();

	public UISimpleGrid BoosterGrid;

	public UISimpleGrid DisplayRewardedItemListGrid;

	public Transform TeamLevelGaugeRoot;

	public ResourceUiData ResourceUi;

	public UILabel Label_TowerClear;

	public UISimpleGrid StaminaGrid;

	public Transform OnetimeBoosterRoot;

	public UILabel LabelOnetimeBooster;

	public UIScrollGrid SubPartyGrid;

	public UISprite SpriteSubPartyLogo;

	public GameObject SubPartyBg2;

	public GameObject SubPartyBg4;

	private Listener _listener;

	private StockData _stock = new StockData();

	private EndCampaignProxy _endCampaignProxy;

	private float _openedTime;

	private bool _isButtonEnabled;

	private bool _touchedAnyButton;

	private float _autoCampaignElapsedTime;

	private List<HeroInRewardComponent> _heroList = new List<HeroInRewardComponent>();

	private List<HeroInRewardSmallComponent> _subPartyList = new List<HeroInRewardSmallComponent>();

	private ReservedActionQueue _reservedActionQueue = new ReservedActionQueue();

	private Helper.LevelInfo _curData;

	private float _time;

	private bool _isUpdateCompleted;

	private float _prev_team_exp;

	private float _cur_team_exp;

	private TeamLevelGauge _teamLevelGauge;

	private StaminaType _currentStaminaType;

	private float _lastUpdateStaminaTime;

	private float _lastUpdateStaminaTooltipTime;

	private bool _isEventDungeon;

	private bool _isDungeonHeroSet;

	private IClassBuffListener _iClassBuffListener;

	private bool _isLevelUp;

	private List<TeamExpLevelActionInfo> _ListTeamExpLevelActionInfo = new List<TeamExpLevelActionInfo>();

	private bool CheckForceRandomPlaySound
	{
		get
		{
			if (_stock == null)
			{
				return false;
			}
			if (_stock.EnterDungeonData == null)
			{
				return false;
			}
			if (_stock.EnterDungeonData.LinkedDungeonData == null)
			{
				return false;
			}
			if (_stock.EnterDungeonData.LinkedDungeonData.BattleType == BattleType.ArcdimRaid || _stock.EnterDungeonData.LinkedDungeonData.BattleType == BattleType.ArcdimHardRaid)
			{
				return true;
			}
			return false;
		}
	}

	public void Open(Listener listener, StockData stock, EndCampaignProxy endCampaignProxy)
	{
		Open(delegate
		{
			Init(listener, stock, endCampaignProxy);
		}, null);
	}

	public void Init(Listener listener, StockData stock, EndCampaignProxy endCampaignProxy)
	{
		if (endCampaignProxy == null)
		{
			VespaDebug.LogError("endCampaignProxy is null");
		}
		Reset();
		_stock = stock;
		_endCampaignProxy = endCampaignProxy;
		_teamLevelGauge = TeamLevelGauge.PrefabAuto.Factory.Create(TeamLevelGaugeRoot);
		if (stock.EnterDungeonData != null)
		{
			_isDungeonHeroSet = Helper.CheckFixedSetHero(stock.EnterDungeonData.Chapter, stock.EnterDungeonData.LinkedDungeon, stock.EnterDungeonData.Difficulty, stock.EnterDungeonData.IsScenario);
		}
		if (_endCampaignProxy.ExpResult != null)
		{
			_curData = Helper.GetTeamLevelInfo(_endCampaignProxy.ExpResult.NewLevel, _endCampaignProxy.ExpResult.NewValue);
		}
		else
		{
			_curData = Helper.GetAccountTeamLevelInfo();
		}
		_iClassBuffListener = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWindowManager;
		SetupTeamExpLevel();
		BuildHeroInReward();
		BuildSubPartyReward();
		_listener = listener;
		_openedTime = Time.time;
		_touchedAnyButton = false;
		_autoCampaignElapsedTime = 0f;
		if (ButtonDelay > 0f)
		{
			UIHelper.EnableGameObject(ButtonGroup, enable: false);
			UIHelper.EnableGameObject(DisplayRewardedItemListGrid, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(ButtonGroup, enable: true);
			UIHelper.EnableGameObject(DisplayRewardedItemListGrid, enable: true);
		}
		SetupBooster();
		SetupManageButton();
		SetupResourceUi();
		SetupOnetimeBooster();
		SetUpClearTowerFloor();
		if (_isLevelUp && _iClassBuffListener != null)
		{
			_iClassBuffListener.AddPoint();
		}
		StaminaGrid.RepositionGrid();
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_reservedActionQueue.Clear();
		_isButtonEnabled = false;
		_currentStaminaType = StaminaType.None;
		_lastUpdateStaminaTime = 0f;
		_lastUpdateStaminaTooltipTime = 0f;
		_isDungeonHeroSet = false;
		FactoryBase.ReleaseAndClearList(_ListBoosterComponent);
		if (ResourceUi != null)
		{
			if (ResourceUi.IconProxy != null)
			{
				FactoryBase.SafeReleaseComponent(ref ResourceUi.IconProxy);
			}
			ResourceUi.IconProxy = null;
		}
		ClearItem();
		ClearHero();
		CloseTeamLevelGauge();
		ClearSubParty();
	}

	public override void OnSystemBackButton()
	{
		if ((!SingletonUpdater<TutorialManager>.isValidInstance || !SingletonUpdater<TutorialManager>.instance.IsProgressing) && (!SceneAutoBehaviour<CutSceneManager>.isValidInstance || !SceneAutoBehaviour<CutSceneManager>.instance.IsLoading) && !SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying && (!GlobalAutoBehaviour<PopupManegement>.isValidInstance || !GlobalAutoBehaviour<PopupManegement>.instance.AnyPopupExist()))
		{
			OnClickToLobby();
		}
	}

	private bool IsMultiPlay()
	{
		if (_endCampaignProxy == null || __Extension_Method_ICollection__.IsNullOrEmpty(_endCampaignProxy.MultiplayMemberIds))
		{
			return false;
		}
		return _endCampaignProxy.MultiplayMemberIds.Length != 0;
	}

	public void SetButtonEnabledAction(IWindow window, Action onButtonEnabled)
	{
		if (_isButtonEnabled)
		{
			Vespa_System_Action_Extension_Method.fire(onButtonEnabled);
			return;
		}
		_reservedActionQueue.Add(new ReservedPopup
		{
			Window = window,
			OpenAction = onButtonEnabled
		});
	}

	public void SetButtonEnabledAction<T>(T window, Action<T> action) where T : BaseWindow
	{
		SetButtonEnabledAction((IWindow)window, (Action)delegate
		{
			Vespa_System_Action_Extension_Method.fire(action, window);
		});
	}

	public void SetTeamExpAction(Helper.LevelInfo oldLevelInfo, Helper.LevelInfo curLevelInfo)
	{
		TeamExpLevelActionInfo teamExpLevelActionInfo = new TeamExpLevelActionInfo();
		teamExpLevelActionInfo.oldLevelInfo = oldLevelInfo;
		teamExpLevelActionInfo.curLevelInfo = curLevelInfo;
		_ListTeamExpLevelActionInfo.Add(teamExpLevelActionInfo);
		_reservedActionQueue.Add(this);
	}

	public void ReservePurchaseMarketingPopup()
	{
		ReserveConditionalPopup action = new ReserveConditionalPopup
		{
			OpenCondition = () => true,
			OpenAction = delegate
			{
				GlobalAutoBehaviour<PurchaseMarketingManager>.instance.OpenPurchaseMarketingPopup(OpenConditionType.EndBattleReward);
			},
			EndCondition = () => !PurchaseMarketingPopup.IsValidInstance
		};
		_reservedActionQueue.Add(action);
	}

	protected void ClearItem()
	{
	}

	private void ClearHero()
	{
		_heroList.ForEach(delegate(HeroInRewardComponent e)
		{
			e.Close();
		});
		FactoryBase.ReleaseAndClearList(_heroList);
	}

	private void ClearSubParty()
	{
		_subPartyList.ForEach(delegate(HeroInRewardSmallComponent e)
		{
			e.Close();
		});
		FactoryBase.ReleaseAndClearList(_subPartyList);
	}

	private void SetupManageButton(UIButton button, UIWidget widget, bool enabled)
	{
		UIHelper.EnableButton(button, enabled);
		widget.alpha = (enabled ? 1f : DisabledButtonAlpha);
	}

	private void SetupManageButton()
	{
		bool canNextDungeon = CanNextDungeon();
		bool flag = BattleRepeatable();
		SetupWidgetNextDungeon(canNextDungeon);
		SetupManageButton(Button_Repeat, Widget_Repeat, flag);
		BattleDefineData battleDefine = _stock.EnterDungeonData.OriginalDungeonData.GetBattleDefine(_stock.EnterDungeonData.OriginalDungeonData.Scenario);
		SetupManageButton(Button_ToHeroDeck, Widget_ToHeroDeck, flag && !battleDefine.DisableHeroListInResult);
		SetupManageButton(Button_ToPartySetting, Widget_ToPartySetting, flag && !battleDefine.DisablePartySettingInResult);
		SetupManageButton(Button_Bag, Widget_Bag, flag && !battleDefine.DisableInventoryInResult);
	}

	private void UpdateButtonState()
	{
		if (_isButtonEnabled || !(Time.time - _openedTime >= ButtonDelay))
		{
			return;
		}
		_isButtonEnabled = true;
		UIHelper.EnableGameObject(ButtonGroup, enable: true);
		UIHelper.EnableGameObject(DisplayRewardedItemListGrid, enable: true);
		if (GameOptionData.EditorCanAutoPlay)
		{
			bool num = CanNextDungeon();
			bool flag = BattleRepeatable();
			if (num && GameOptionData.EditorToggleAutoPlay)
			{
				SingletonUpdater<Executer>.instance.DelayRun(1f, this, RunNextBattle);
			}
			else if (flag && GameOptionData.EditorToggleAutoRepeat)
			{
				SingletonUpdater<Executer>.instance.DelayRun(1f, this, OnClickToPartySetting);
			}
		}
	}

	private void UpdateReservedPopup()
	{
		if (_isButtonEnabled)
		{
			_reservedActionQueue.Update();
		}
	}

	private void UpdateAutoCampaign()
	{
		if (!AutoRepeatBattle.IsEnabled())
		{
			return;
		}
		PartyRewardPopup partyRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<PartyRewardPopup>();
		if (partyRewardPopup != null && partyRewardPopup.GetSecondAfterOpened() > AutoRepeatBattle.AutoCampaignDelayTime)
		{
			partyRewardPopup.Close();
		}
		CampaignRewardPopup campaignRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<CampaignRewardPopup>();
		if (campaignRewardPopup != null && campaignRewardPopup.GetSecondAfterOpened() > AutoRepeatBattle.AutoCampaignDelayTime)
		{
			campaignRewardPopup.Close();
		}
		BaseRewardPopup baseRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<BaseRewardPopup>();
		if (baseRewardPopup != null && baseRewardPopup.GetSecondAfterOpened() > AutoRepeatBattle.AutoCampaignDelayTime)
		{
			baseRewardPopup.Close();
		}
		if (_reservedActionQueue.IsEmpty() && !GlobalAutoBehaviour<PopupManegement>.instance.AnyPopupExist() && _isButtonEnabled && !_touchedAnyButton)
		{
			_autoCampaignElapsedTime += Time.deltaTime;
			if (_autoCampaignElapsedTime > AutoRepeatBattle.AutoCampaignDelayTime)
			{
				_touchedAnyButton = true;
				OnClickRepeat();
			}
		}
	}

	private void UpdateAutoNextCampaign()
	{
		if (!AutoNextBattle.IsEnabled())
		{
			return;
		}
		if (AutoNextBattle.instance.AutoNextBattleType != BattleType.Conquest && AutoNextBattle.instance.AutoNextBattleType != BattleType.Hideout && !CanNextDungeon())
		{
			AutoNextBattle.instance.OnClickStop();
			return;
		}
		CampaignRewardPopup campaignRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<CampaignRewardPopup>();
		if (campaignRewardPopup != null && campaignRewardPopup.GetSecondAfterOpened() > AutoNextBattle.AutoCampaignDelayTime)
		{
			campaignRewardPopup.Close();
		}
		if (_reservedActionQueue.IsEmpty() && !GlobalAutoBehaviour<PopupManegement>.instance.AnyPopupExist() && _isButtonEnabled && !_touchedAnyButton)
		{
			_autoCampaignElapsedTime += Time.deltaTime;
			if (_autoCampaignElapsedTime > AutoNextBattle.AutoCampaignDelayTime)
			{
				_touchedAnyButton = true;
				VespaCoroutine.Start(coStartAutoNextBattle());
			}
		}
	}

	private IEnumerator coStartAutoNextBattle()
	{
		string forwardHost = _endCampaignProxy.ForwardHost;
		if (string.IsNullOrEmpty(forwardHost))
		{
			StartAutoNextBattle();
			yield break;
		}
		ServerChanger.ChangeServer(forwardHost, delegate
		{
			StartAutoNextBattle();
		});
	}

	private void StartAutoNextBattle()
	{
		if (AutoNextBattle.IsEnabled())
		{
			if (AutoNextBattle.instance.AutoNextBattleType == BattleType.Conquest)
			{
				ReturnToLobbyAndOpenNextChapterConquestDungeon();
			}
			else if (AutoNextBattle.instance.AutoNextBattleType == BattleType.Hideout)
			{
				ReturnToLobbyAndOpenNextChapterHideOut();
			}
			else
			{
				RunNextBattle();
			}
		}
	}

	private bool CanNextDungeon()
	{
		if (SprkStoryAuto.Active())
		{
			return SprkStoryAuto.CanNext();
		}
		if (IsMultiPlay())
		{
			return false;
		}
		if (SceneAutoBehaviour<MainStoryManagement>.instance != null && MainStoryManagement.IsPlayMainStoryMode)
		{
			return false;
		}
		NGame2.NBattleField.NManager.NPlay.IDataProvider dataProvider = SingletonAuto<BattleContext>.instance.DataProvider;
		if (dataProvider != null && !dataProvider.CanNextDungeon())
		{
			return false;
		}
		if (SingletonAuto<EventTriggerManager>.instance.CanRaiseEvent(new EnterLobby()))
		{
			return false;
		}
		if (SubStoryUtillity.IsSubStoryBattleType(_stock.EnterDungeonData))
		{
			return false;
		}
		return true;
	}

	private bool BattleRepeatable()
	{
		if (!SingletonAuto<BattleContext>.instance.DataProvider.CanRetryBattle(BattleRetryConditionType.Win))
		{
			return false;
		}
		if (SingletonAuto<EventTriggerManager>.instance.CanRaiseEvent(new EnterLobby()))
		{
			return false;
		}
		return true;
	}

	private void BuildHeroInReward()
	{
		if (_endCampaignProxy == null)
		{
			return;
		}
		List<int> list = new List<int>();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_stock.AiHeroIndices) && __Extension_Method_ICollection__.IsNullOrEmpty(_stock.GroupHeroInices))
		{
			list.AddRange(_stock.HeroIndices);
		}
		else
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_stock.HeroIndices); i++)
			{
				int num = _stock.HeroIndices[i];
				bool num2 = _stock.AiHeroIndices != null && TypeIterator.Contains(_stock.AiHeroIndices, num);
				bool flag = _stock.GroupHeroInices != null && TypeIterator.Contains(_stock.GroupHeroInices, num);
				if (!num2 && !flag)
				{
					list.Add(num);
				}
			}
		}
		bool flag2 = IsMultiPlay();
		int randomPlaySoundHeroIndex = GetRandomPlaySoundHeroIndex(list);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list) && j < __Extension_Method_ICollection__.COUNT(_stock.AccountIds); j++)
		{
			int heroIndex = list[j];
			long num3 = _stock.AccountIds[j];
			bool flag3 = num3 == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
			string nick = string.Empty;
			CachedHeroInfo cachedHeroInfo = null;
			HeroExpResultInfo[] array = null;
			FlaskEndCampaignResultInfo[] array2 = null;
			List<ForcedHeroInfo> forcedHeroInfos = SubStoryUtillity.GetForcedHeroInfos(_stock.EnterDungeonData);
			if (SubStoryUtillity.IsSubStoryBattleType(_stock.EnterDungeonData) && __Extension_Method_ICollection__.COUNT(forcedHeroInfos) != 0)
			{
				nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick;
				ForcedHeroInfo forcedHeroInfo = forcedHeroInfos.Find((ForcedHeroInfo e) => e.CachedHeroInfo.HeroIndex == heroIndex);
				if (forcedHeroInfo != null)
				{
					cachedHeroInfo = forcedHeroInfo.CachedHeroInfo;
				}
			}
			else if (flag3)
			{
				nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick;
				if (_isDungeonHeroSet && _stock.EnterDungeonData != null)
				{
					HeroInfo dungeonHeroSetHeroInfo = Helper.GetDungeonHeroSetHeroInfo(_stock.EnterDungeonData.Chapter, _stock.EnterDungeonData.LinkedDungeon, _stock.EnterDungeonData.Difficulty, heroIndex, _stock.EnterDungeonData.IsScenario);
					if (dungeonHeroSetHeroInfo != null)
					{
						cachedHeroInfo = dungeonHeroSetHeroInfo as CachedHeroInfo;
					}
				}
				if (cachedHeroInfo == null)
				{
					cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
				}
				if (!_isDungeonHeroSet)
				{
					array = _endCampaignProxy.HeroExpResults;
					array2 = _endCampaignProxy.FlaskResults;
				}
			}
			else if (flag2)
			{
				PartyInfo obj = (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty() ? GlobalAutoBehaviour<PartyManager>.instance.PartyInfo : null);
				int chapterIndex = obj.ChapterIndex;
				int dungeonIndex = obj.DungeonIndex;
				PartyDungeonData data = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
				MemberInfo partyMemberInfo = obj.GetPartyMemberInfo(num3);
				if (obj != null && data != null)
				{
					if (data.DeckCount > 4)
					{
						if (!flag3)
						{
							continue;
						}
					}
					else if (partyMemberInfo != null)
					{
						nick = partyMemberInfo.Nick;
						cachedHeroInfo = partyMemberInfo.GetDeckHero(heroIndex);
						array = partyMemberInfo.HeroExpResults;
						array2 = partyMemberInfo.FlaskResults;
					}
				}
			}
			if (cachedHeroInfo == null)
			{
				continue;
			}
			HeroInRewardComponent heroInRewardComponent = HeroInRewardComponent.PrefabAuto.Factory.Create(HeroPivot.transform);
			if (!(heroInRewardComponent == null))
			{
				bool isDead = false;
				if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_stock.IsDeadHeroes, j))
				{
					isDead = _stock.IsDeadHeroes[j];
				}
				HeroExpResultInfo expResultInfo = array?.FirstOrDefault((HeroExpResultInfo e) => e != null && e.HeroIndex == heroIndex);
				FlaskEndCampaignResultInfo flaskResultInfo = array2?.FirstOrDefault((FlaskEndCampaignResultInfo e) => e != null && e.Info.HeroIndex == heroIndex);
				bool isPlaySound = randomPlaySoundHeroIndex == heroIndex && flag3;
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroInRewardComponent.transform);
				if (!heroInRewardComponent.Open(cachedHeroInfo, nick, num3, isMercenary: false, isDead, expResultInfo, flaskResultInfo, isPlaySound))
				{
					heroInRewardComponent.Close();
					FactoryBase.ReleaseComponent(heroInRewardComponent);
				}
				else
				{
					_heroList.Add(heroInRewardComponent);
				}
			}
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_heroList))
		{
			return;
		}
		float num4 = (float)(-(_heroList.Count - 1) * HeroPivot.width) * 0.5f;
		for (int num5 = 0; num5 < _heroList.Count; num5++)
		{
			_heroList[num5].transform.localPosition = Vector3.right * (num4 + (float)(num5 * HeroPivot.width));
			if (_heroList[num5] != null && !_isLevelUp && _heroList[num5].IsLevelUp)
			{
				_isLevelUp = _heroList[num5].IsLevelUp;
			}
		}
	}

	private int GetRandomPlaySoundHeroIndex(List<int> heroIndexList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList))
		{
			return 0;
		}
		if (_stock == null)
		{
			return 0;
		}
		List<int> list = new List<int>();
		list.Clear();
		for (int i = 0; i < heroIndexList.Count && i < __Extension_Method_ICollection__.COUNT(_stock.AccountIds); i++)
		{
			int item = heroIndexList[i];
			if (_stock.AccountIds[i] != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
			{
				continue;
			}
			if (!CheckForceRandomPlaySound)
			{
				bool flag = false;
				if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_stock.IsDeadHeroes, i))
				{
					flag = _stock.IsDeadHeroes[i];
				}
				if (flag)
				{
					continue;
				}
			}
			list.Add(item);
		}
		return Vespa_Generic_IList_Extension_Method.RandomGet(list);
	}

	private void BuildSubPartyReward()
	{
		if (_endCampaignProxy == null)
		{
			return;
		}
		List<int> list = new List<int>();
		if (_stock.AiHeroIndices != null)
		{
			list.AddRange(_stock.AiHeroIndices);
		}
		if (_stock.GroupHeroInices != null)
		{
			list.AddRange(_stock.GroupHeroInices);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list) && i < __Extension_Method_ICollection__.COUNT(_stock.AccountIds); i++)
		{
			int num = list[i];
			long num2 = _stock.AccountIds[i];
			bool isMercenary = false;
			CachedHeroInfo cachedHeroInfo = null;
			if (num2 == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
			{
				cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(num);
			}
			if (cachedHeroInfo == null)
			{
				continue;
			}
			HeroInRewardSmallComponent heroInRewardSmallComponent = UIHelper.AddGridItem(SubPartyGrid, HeroInRewardSmallComponent.PrefabAuto);
			if (heroInRewardSmallComponent == null)
			{
				continue;
			}
			int num3 = __Extension_Method_ICollection__.COUNT(_stock.HeroIndices) - __Extension_Method_ICollection__.COUNT(_stock.AiHeroIndices) - __Extension_Method_ICollection__.COUNT(_stock.GroupHeroInices) + i;
			bool isDead = false;
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_stock.IsDeadHeroes, num3))
			{
				isDead = _stock.IsDeadHeroes[num3];
			}
			HeroExpResultInfo expResultInfo = null;
			bool flag = num2 == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
			if (flag)
			{
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_endCampaignProxy.HeroExpResults); j++)
				{
					HeroExpResultInfo heroExpResultInfo = _endCampaignProxy.HeroExpResults[j];
					if (heroExpResultInfo != null && num == heroExpResultInfo.HeroIndex)
					{
						expResultInfo = heroExpResultInfo;
						break;
					}
				}
			}
			FlaskEndCampaignResultInfo flaskResultInfo = null;
			if (flag)
			{
				for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_endCampaignProxy.FlaskResults); k++)
				{
					FlaskEndCampaignResultInfo flaskEndCampaignResultInfo = _endCampaignProxy.FlaskResults[k];
					if (flaskEndCampaignResultInfo != null && num == flaskEndCampaignResultInfo.Info.HeroIndex)
					{
						flaskResultInfo = flaskEndCampaignResultInfo;
						break;
					}
				}
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroInRewardSmallComponent.transform);
			if (!heroInRewardSmallComponent.Open(cachedHeroInfo, string.Empty, num2, isMercenary, isDead, expResultInfo, flaskResultInfo))
			{
				heroInRewardSmallComponent.Close();
				FactoryBase.ReleaseComponent(heroInRewardSmallComponent);
				continue;
			}
			_subPartyList.Add(heroInRewardSmallComponent);
			if (heroInRewardSmallComponent != null && !_isLevelUp && heroInRewardSmallComponent.IsLevelUp)
			{
				_isLevelUp = heroInRewardSmallComponent.IsLevelUp;
			}
			UIHelper.RepositionGrid(SubPartyGrid);
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_subPartyList))
		{
			bool flag2 = __Extension_Method_ICollection__.COUNT(_subPartyList) <= 2;
			UIHelper.EnableGameObject(SubPartyBg2, flag2);
			UIHelper.EnableGameObject(SubPartyBg4, !flag2);
		}
		UIHelper.EnableGameObject(SubPartyGrid, _subPartyList.Count > 0);
	}

	private void SetupTeamExpLevel()
	{
		if (_curData.IsMaxLevel)
		{
			if (_teamLevelGauge != null)
			{
				_teamLevelGauge.OpenMax(_curData.Level);
			}
			_isUpdateCompleted = true;
		}
		else if (_teamLevelGauge == null)
		{
			_isUpdateCompleted = true;
		}
		else
		{
			SingletonAuto<SoundContainer>.instance.PlayUnique("UI.Campaign.GetExp", this);
			_teamLevelGauge.Open(_stock.OldLevelInfo.Level, _stock.OldLevelInfo.CurExp, _curData.Level, _curData.CurExp, delegate
			{
				SoundContainer.StopUnique("UI.Campaign.GetExp", this);
				_isUpdateCompleted = true;
			});
		}
	}

	private void CloseTeamLevelGauge()
	{
		FactoryBase.SafeReleaseComponent(ref _teamLevelGauge);
	}

	public void ReturnToLobbyAndOpenNextChapterHideOut()
	{
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenNextChapterHideOut();
			_touchedAnyButton = true;
		}
	}

	public void ReturnToLobbyAndOpenNextChapterConquestDungeon()
	{
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenNextChapterConquestDungeon();
			_touchedAnyButton = true;
		}
	}

	public void RunNextBattle()
	{
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenNextDungeon();
			_touchedAnyButton = true;
		}
	}

	public void OnClickNextDungeon()
	{
		AutoNextBattle.StopWithClickButton();
		RunNextBattle();
	}

	public void OnClickRepeat()
	{
		string forwardHost = _endCampaignProxy.ForwardHost;
		if (string.IsNullOrEmpty(forwardHost))
		{
			OnClickRepeatInternal();
			return;
		}
		ServerChanger.ChangeServer(forwardHost, delegate
		{
			OnClickRepeatInternal();
		});
	}

	private void OnClickRepeatInternal()
	{
		AutoNextBattle.StopWithClickButton();
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.BattleRepeat();
			_touchedAnyButton = true;
		}
	}

	public void OnClickToLobby()
	{
		VespaCoroutine.Stop(SceneAutoBehaviour<EndCampaign>.instance.coCheckPartyReadyAll);
		VespaCoroutine.Stop(SceneAutoBehaviour<EndCampaign>.instance.coWaitPartyMember);
		if (AutoRepeatBattle.IsEnabled() && AutoRepeatBattle.IsPartyPlay)
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
			LobbyScene2.ListReturnState.Clear();
			SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
			AutoRepeatBattle.ClearSelectedReward();
			return;
		}
		AutoNextBattle.StopWithClickButton();
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.ReturnToLobby();
			_touchedAnyButton = true;
			AutoRepeatBattle.ClearSelectedReward();
		}
	}

	public void OnClickStatistics()
	{
		if (AutoRepeatBattle.IsEnabled() && AutoRepeatBattle.IsPartyPlay)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("REPEATBATTLE_MULTI_DISABLE_CONTENTS"));
			return;
		}
		AutoNextBattle.StopWithClickButton();
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.ShowStatistics();
			_touchedAnyButton = true;
		}
	}

	public void OnClickToPurifyRoom()
	{
		if (_listener == null)
		{
			return;
		}
		int[] forcedHeroIndex = _stock.EnterDungeonData.LinkedDungeonData.GetForcedHeroIndex(isScenario: false);
		if (forcedHeroIndex.Length == 1)
		{
			HeroInRewardComponent heroInRewardComponent = _heroList.Find((HeroInRewardComponent item) => item.HeroInfo.HeroIndex == forcedHeroIndex[0]);
			if (heroInRewardComponent != null)
			{
				_listener.MoveToPurifyRoom(heroInRewardComponent.HeroInfo);
			}
		}
		_touchedAnyButton = true;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateButtonState();
		UpdateReservedPopup();
		UpdateAutoCampaign();
		UpdateAutoNextCampaign();
		UpdateStaminaValue();
		UpdateStaminaTimeInfo();
	}

	void IReservedAction.Execute()
	{
		if (_ListTeamExpLevelActionInfo.Count != 0)
		{
			_stock.OldLevelInfo = _ListTeamExpLevelActionInfo[0].oldLevelInfo;
			_curData = _ListTeamExpLevelActionInfo[0].curLevelInfo;
			SetupTeamExpLevel();
			_ListTeamExpLevelActionInfo.RemoveAt(0);
		}
	}

	bool IReservedAction.Ended()
	{
		return _isUpdateCompleted;
	}

	public void OnClickToHeroDeck()
	{
		AutoNextBattle.StopWithClickButton();
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenHeroDeck();
			_touchedAnyButton = true;
		}
	}

	public void OnClickToPartySetting()
	{
		AutoNextBattle.StopWithClickButton();
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenPartySetting();
			_touchedAnyButton = true;
		}
	}

	public void OnClickToInventory()
	{
		AutoNextBattle.StopWithClickButton();
		if (_reservedActionQueue.Count() <= 0 && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenInventory();
			_touchedAnyButton = true;
		}
	}

	public void DisplayRewardedItemList(ItemResultInfo[] itemResults)
	{
	}

	public void SetupBooster()
	{
		FactoryBase.ReleaseAndClearList(_ListBoosterComponent);
		BattleContext instance = SingletonAuto<BattleContext>.instance;
		if (instance == null)
		{
			return;
		}
		NGame2.NBattleField.NManager.NPlay.IDataProvider dataProvider = instance.DataProvider;
		if (dataProvider == null || (dataProvider.GetBattleDefine().BattleMenu & EBattleMenu.BoosterStatus) == EBattleMenu.None)
		{
			return;
		}
		dataProvider.GetBattleType();
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		foreach (BoosterInfo item in GlobalAutoBehaviour<BoosterManager>.instance.GetAllEffectiveBooster())
		{
			if (item.IsShowAtBattle())
			{
				BoosterComponent boosterComponent = UIHelper.AddGridItem(BoosterGrid, BoosterComponent.PrefabAuto);
				boosterComponent.Open(item);
				_ListBoosterComponent.Add(boosterComponent);
			}
		}
		UIHelper.SetGridMaxPerLine(BoosterGrid.Grid, GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxBoosterPerLine", 10));
		UIHelper.RepositionGrid(BoosterGrid);
	}

	private void SetupOnetimeBooster()
	{
		UIHelper.EnableGameObject(OnetimeBoosterRoot, enable: false);
		if (_endCampaignProxy != null && _endCampaignProxy.ItemBoosterResultInfo != null)
		{
			UIHelper.EnableGameObject(OnetimeBoosterRoot, enable: true);
			int maxCount = _endCampaignProxy.ItemBoosterResultInfo.MaxCount;
			int remainCount = _endCampaignProxy.ItemBoosterResultInfo.RemainCount;
			UIHelper.SetLabelText(LabelOnetimeBooster, $"{remainCount}/{maxCount}");
		}
	}

	private void SetUpClearTowerFloor()
	{
		bool flag = true;
		NGame2.NBattleField.NManager.NPlay.IDataProvider dataProvider = SingletonAuto<BattleContext>.instance.DataProvider;
		if (dataProvider == null || !dataProvider.IsTowerBattle() || dataProvider.GetBattleType() == BattleType.GatewaytoTrial)
		{
			flag = false;
		}
		UIHelper.EnableGameObject(Label_TowerClear, flag);
		if (flag)
		{
			int towerFloor = dataProvider.GetTowerFloor();
			UIHelper.SetLabelText(Label_TowerClear, LocalizationManager.GetString(Tower.TOWER_FLOOR_CLEAR, towerFloor));
		}
	}

	private void SetupWidgetNextDungeon(bool canNextDungeon)
	{
		NGame2.NBattleField.NManager.NPlay.IDataProvider dataProvider = SingletonAuto<BattleContext>.instance.DataProvider;
		if (dataProvider != null && dataProvider.GetBattleType() == BattleType.Trial)
		{
			UIHelper.EnableGameObject(Button_NextDungeon, enable: false);
			UIHelper.EnableGameObject(Button_ToPurifyRoom, enable: true);
			SetupManageButton(Button_ToPurifyRoom, Widget_NextDungeon, enabled: true);
		}
		else
		{
			UIHelper.EnableGameObject(Button_NextDungeon, enable: true);
			UIHelper.EnableGameObject(Button_ToPurifyRoom, enable: false);
			SetupManageButton(Button_NextDungeon, Widget_NextDungeon, canNextDungeon);
		}
	}

	public void SetupResourceUi()
	{
		UIHelper.EnableGameObject(ResourceUi.Root, enable: false);
		UIHelper.EnableGameObject(ResourceUi.StaminaTooltip, enable: false);
		UIHelper.EnableGameObject(ResourceUi.StaminaRoot, enable: true);
		UIHelper.EnableGameObject(ResourceUi.EventStaminaRoot, enable: false);
		if (SubStoryUtillity.IsSubStoryBattleType(_stock.EnterDungeonData))
		{
			return;
		}
		EnterDungeonData enterDungeonData = _stock.EnterDungeonData;
		if (enterDungeonData == null)
		{
			return;
		}
		CampaignDungeonData linkedDungeonData = enterDungeonData.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return;
		}
		StaminaType reqStaminaType = linkedDungeonData.ReqStaminaType;
		if (reqStaminaType != StaminaType.None && reqStaminaType != StaminaType.Empty)
		{
			if (ResourceUi.IconProxy == null)
			{
				ResourceUi.IconProxy = IconProxy.Create(ResourceUi.IconRoot);
			}
			if (ResourceUi.IconProxy != null)
			{
				ResourceUi.IconProxy.SetupStamina(reqStaminaType);
			}
			_currentStaminaType = reqStaminaType;
			_lastUpdateStaminaTime = 0f;
			UIHelper.EnableGameObject(ResourceUi.Root, enable: true);
			UpdateStaminaValue();
			bool enable = reqStaminaType == StaminaType.Chicken;
			UIHelper.EnableGameObject(ResourceUi.RechargeButton, enable);
		}
	}

	private void UpdateStaminaValue(bool force = false)
	{
		if (_currentStaminaType == StaminaType.None || (Time.time - _lastUpdateStaminaTime < 1f && !force))
		{
			return;
		}
		_lastUpdateStaminaTime = Time.time;
		int num = 0;
		long currentValue = num;
		int num2 = 0;
		long maxValue = num2;
		EnterDungeonData enterDungeonData = _stock.EnterDungeonData;
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(_currentStaminaType, out currentValue, out maxValue, enterDungeonData?.Chapter ?? 0))
		{
			if (_currentStaminaType == StaminaType.TreasureHouseKey || _currentStaminaType == StaminaType.EventDungeonGoldKey)
			{
				UIHelper.SetLabelText(ResourceUi.LabelStamina, $"{currentValue}");
			}
			else
			{
				UIHelper.SetLabelText(ResourceUi.LabelStamina, $"{currentValue}/{maxValue}");
			}
		}
	}

	private void UpdateStaminaTimeInfo()
	{
		if (_currentStaminaType != StaminaType.None && UIHelper.IsEnabledGameObjet(ResourceUi.StaminaTooltip) && !(Time.time - _lastUpdateStaminaTooltipTime < 1f))
		{
			_lastUpdateStaminaTooltipTime = Time.time;
			string FullStaminaRechargeRemainTime = string.Empty;
			string OneStaminaRechargeRemainTime = string.Empty;
			if (GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.IsKeyTypeDungeon(_currentStaminaType))
			{
				GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetKeyTooltipInfo(_currentStaminaType, out FullStaminaRechargeRemainTime, out OneStaminaRechargeRemainTime);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurStaminaTimeInfo(_currentStaminaType, out FullStaminaRechargeRemainTime, out OneStaminaRechargeRemainTime);
			}
			UIHelper.SetLabelText(ResourceUi.Label_FullStaminaRecoverTime, FullStaminaRechargeRemainTime);
			UIHelper.SetLabelText(ResourceUi.Label_StaminaRecoverTime, OneStaminaRechargeRemainTime);
		}
	}

	public void OnShowStaminaTooltip()
	{
		UIHelper.EnableGameObject(ResourceUi.StaminaTooltip, enable: true);
		UpdateStaminaTimeInfo();
	}

	public void OnHideStaminaTooltip()
	{
		UIHelper.EnableGameObject(ResourceUi.StaminaTooltip, enable: false);
	}

	public void OnClickRechargeStamina()
	{
		if (_currentStaminaType != StaminaType.None && _reservedActionQueue.IsEmpty())
		{
			UIHelper.ResolveRecoverStamina(isNotEnough: false, null, _currentStaminaType, 0, delegate
			{
				UpdateStaminaValue(force: true);
			});
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public void OnChanged(NGame2.NAccount.UserItemTimeDurationManager.BoosterChangeInfo info)
	{
		NGame2.NAccount.UserItemTimeDurationManager.BoosterChangeType changeType = info.changeType;
		if ((uint)changeType <= 1u)
		{
			SetupBooster();
		}
	}

	public void SetupTutorialLayout()
	{
		UIHelper.EnableGameObject(OnetimeBoosterRoot, enable: false);
		UIHelper.EnableGameObject(Label_TowerClear, enable: false);
	}
}
