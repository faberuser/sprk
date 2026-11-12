using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NDropBonusEvent;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUtil.NPayShop;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LobbyRightMenu : BaseWindow, IUserObserver<NGame2.NAccount.UserAchievementManager.ChangeAchievementInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserItemManager.ItemCountChangedInfo>, IUserObserver<ItemInfo>, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver<FriendChangeInfo>, IUserObserver<AttendanceInfo>, IUserObserver<NGame2.NAccount.UserEquipGachaManager.ChangedGachaInfo>, IUserObserver<CraftSlotInfo>
{
	public interface Listener
	{
		void OnClickHeroButton();

		void OnClickInventoryButton();

		void OnClickAchievementButton();

		void OnClickQuestButton();

		void OnClickFriendButton();

		void OnClickPayShopButton();

		void OnClickArchive();

		void OnClickHeroStatButton();

		void OnClickPortalRenewalButton();
	}

	public interface IDataProvider
	{
		int GetClearedAchievements();

		bool IsNewClearQuest();

		int GetNotClearedDailyQuests();
	}

	public enum EQuestPlayEffect
	{
		None,
		Begin,
		Normal,
		Updated
	}

	[Serializable]
	public class EffectInfo
	{
		public string Type;

		public UIWidget WidgetEffect;
	}

	[Serializable]
	public class ButtonInfo
	{
		public ERightMenuButton ButtonType;

		public UIButton Button;

		public UIWidget WidgetNew;

		public UIWidget WidgetCount;

		public EffectInfo[] Effects;

		public UILabel Label_Count;
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/RightMenu";

	public UIGrid ButtonGrid;

	public ButtonInfo[] ButtonInfos;

	private LocalAccountBool _OpenMenu = new LocalAccountBool(i_default: true, "MainLobby_Open_Menu");

	private Listener _listener;

	private IDataProvider _dataProvider;

	private bool _CannotPoll;

	private bool _HideMenus;

	private bool _ToggleNew;

	private EventTriggerData _lastMainScenarioData;

	private float _lastUpdatedTime;

	private ERightMenuButton _flyType;

	private float _targetScaleDiff;

	private float _targetScale;

	public AnimationCurve SwipeCurve;

	public UISprite SpriteButtonBackground;

	private Coroutine _flyCoroutine;

	public IEnumerator coPlayEffect(Action onStart, Action onEnd)
	{
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		ChattingWindow chattingWindow = SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<ChattingWindow>();
		if (chattingWindow != null && chattingWindow.IsOpen())
		{
			while (chattingWindow.IsOpen())
			{
				yield return null;
			}
		}
		GlobalAutoBehaviour<CampaignNodeNpcAvatarManager>.instance.UpdateNpcState(CampaignNodeNPCData.NPCAvatarState.Dead);
	}

	public void Open(Listener listener, IDataProvider dataProvider)
	{
		base.Open();
		_listener = listener;
		_dataProvider = dataProvider;
		_ToggleNew = false;
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.ObserverManager.AddObserver(this);
		SetupAchievementTag();
		SetupQuestTag();
		SetupInvenNewTag();
		SetupHasCompletedCraftTag();
		SetupFriendTag();
		SetupPayShopTag2();
		SetupButtons();
		SetupMissionEventEffect();
		SetInvenEventEffect();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			_listener = null;
			_dataProvider = null;
			_lastMainScenarioData = null;
			if (_flyCoroutine != null)
			{
				VespaCoroutine.StopScene(_flyCoroutine);
				_flyCoroutine = null;
			}
		}
	}

	public void OnEnterLobby()
	{
	}

	public void OnMoveMap()
	{
		_lastMainScenarioData = null;
	}

	public void OnEndMoveMap()
	{
	}

	public void SetFlyButton(ERightMenuButton type)
	{
		_flyType = type;
		SetupButtons(type);
		_targetScaleDiff = 0f;
		float x = SpriteButtonBackground.transform.localScale.x;
		BottomButtonData data = GameRoot.Instance.BottomButtonDataContainer.GetData(type);
		if (data != null)
		{
			_targetScale = data.BgScale;
			if (_targetScale > x)
			{
				_targetScaleDiff = _targetScale - x;
			}
		}
	}

	public void StartButtonFlyAnimation(float flyDuration, float effectDuration)
	{
		if (_flyCoroutine != null)
		{
			VespaCoroutine.StopScene(_flyCoroutine);
			_flyCoroutine = null;
		}
		_flyCoroutine = VespaCoroutine.StartScene(coFlyAnimation(flyDuration, effectDuration, delegate
		{
			EndButtonFlyAnimation();
		}));
	}

	public void EndButtonFlyAnimation()
	{
		SetupButtons(ERightMenuButton.None, _flyType);
	}

	private IEnumerator coFlyAnimation(float flyDuration, float effectDuration, Action onCompleteFly)
	{
		float elapsedTime = -0.05f;
		float cellWidth = ButtonGrid.cellWidth;
		Dictionary<ButtonInfo, float> animateStartPosXOfButtons = new Dictionary<ButtonInfo, float>();
		ButtonInfo[] buttonInfos = ButtonInfos;
		foreach (ButtonInfo buttonInfo in buttonInfos)
		{
			if (buttonInfo.ButtonType > _flyType && buttonInfo.ButtonType.GetEffectIndex() >= 0)
			{
				animateStartPosXOfButtons[buttonInfo] = buttonInfo.Button.transform.localPosition.x;
			}
		}
		bool bOnCompleteFlyFired = false;
		do
		{
			yield return null;
			elapsedTime += GlobalTimer.deltaTime;
			if (elapsedTime < flyDuration)
			{
				foreach (KeyValuePair<ButtonInfo, float> item in animateStartPosXOfButtons)
				{
					ButtonInfo key = item.Key;
					float value = item.Value;
					Vector3 localPosition = key.Button.transform.localPosition;
					float num = cellWidth * SwipeCurve.Evaluate(elapsedTime / flyDuration);
					localPosition.x = value + num;
					key.Button.transform.localPosition = localPosition;
				}
				if (_targetScaleDiff > 0f)
				{
					Vector3 localScale = SpriteButtonBackground.transform.localScale;
					localScale.x += _targetScaleDiff * Time.deltaTime / flyDuration;
					SpriteButtonBackground.transform.localScale = localScale;
				}
			}
			if (elapsedTime > flyDuration && !bOnCompleteFlyFired)
			{
				bOnCompleteFlyFired = true;
				Vespa_System_Action_Extension_Method.fire(onCompleteFly);
			}
		}
		while (!(elapsedTime > flyDuration));
	}

	private void SetupTag(ERightMenuButton type, Func<bool> condition)
	{
		ButtonInfo[] buttonInfos = ButtonInfos;
		foreach (ButtonInfo buttonInfo in buttonInfos)
		{
			if (buttonInfo.ButtonType == type)
			{
				UIHelper.EnableGameObject(buttonInfo.WidgetNew, Vespa_System_Action_Extension_Method.fire(condition));
				break;
			}
		}
	}

	private void SetupTag(ERightMenuButton type, Func<int> getter)
	{
		ButtonInfo[] buttonInfos = ButtonInfos;
		foreach (ButtonInfo buttonInfo in buttonInfos)
		{
			if (buttonInfo.ButtonType == type)
			{
				int num = Vespa_System_Action_Extension_Method.fire(getter);
				UIHelper.EnableGameObject(buttonInfo.WidgetCount, num > 0);
				if (num > 0)
				{
					UIHelper.SetLabelText(buttonInfo.Label_Count, num);
				}
			}
		}
	}

	private void SetupTag(ERightMenuButton type, Func<bool> condition, Func<int> getter)
	{
		ButtonInfo[] buttonInfos = ButtonInfos;
		foreach (ButtonInfo buttonInfo in buttonInfos)
		{
			if (buttonInfo.ButtonType != type)
			{
				continue;
			}
			bool flag = Vespa_System_Action_Extension_Method.fire(condition);
			UIHelper.EnableGameObject(buttonInfo.WidgetNew, flag);
			UIHelper.EnableGameObject(buttonInfo.WidgetCount, !flag);
			if (!flag)
			{
				int num = Vespa_System_Action_Extension_Method.fire(getter);
				UIHelper.EnableGameObject(buttonInfo.WidgetCount, num > 0);
				if (num > 0)
				{
					UIHelper.SetLabelText(buttonInfo.Label_Count, num);
				}
			}
		}
	}

	private void SetupEffect(ERightMenuButton type, string effectType, Func<bool> condition)
	{
		ButtonInfo buttonInfo = GetButtonInfo(type);
		if (buttonInfo != null && buttonInfo.Effects != null)
		{
			EffectInfo effectInfo = Array.Find(buttonInfo.Effects, (EffectInfo e) => e.Type == effectType);
			if (effectInfo != null)
			{
				UIHelper.EnableGameObject(effectInfo.WidgetEffect, Vespa_System_Action_Extension_Method.fire(condition));
			}
		}
	}

	private ButtonInfo GetButtonInfo(ERightMenuButton type)
	{
		return Array.Find(ButtonInfos, (ButtonInfo b) => b.ButtonType == type);
	}

	private void SetupAchievementTag()
	{
		SetupTag(ERightMenuButton.Achievement, () => _dataProvider.GetClearedAchievements() > 0);
	}

	private void SetupQuestTag()
	{
		SetupTag(ERightMenuButton.Mission, () => _dataProvider.IsNewClearQuest());
	}

	private void SetupInvenNewTag()
	{
		SetupTag(ERightMenuButton.Inven, delegate
		{
			bool num = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetTotalAddedItemCount() > 0;
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.AddedEquipItemExists();
			return num || flag;
		});
	}

	private void SetupHasCompletedCraftTag()
	{
		SetupEffect(ERightMenuButton.Inven, "Complete", () => GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager != null && GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.HasAnyCompletedCraftSlot());
	}

	private void SetInvenEventEffect()
	{
		SetupEffect(ERightMenuButton.Inven, "Event", delegate
		{
			if (GlobalAutoBehaviour<AccountManager>.instance != null && GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.HasAnyCompletedCraftSlot())
			{
				return false;
			}
			return GlobalAutoBehaviour<DropBonusEventManager>.instance.IsEffectiveEvent(EventBonusType.BonusEquipSellReward) || GlobalAutoBehaviour<DropBonusEventManager>.instance.IsEffectiveEvent(EventBonusType.BonusEquipBreakReward);
		});
	}

	private void SetupHeroTag()
	{
		if (SceneAutoBehaviour<HeroNotifyContainer>.isValidInstance)
		{
			SetupTag(ERightMenuButton.Hero, delegate
			{
				int addedHeroCount = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetAddedHeroCount();
				bool anyNotifyExist = SceneAutoBehaviour<HeroNotifyContainer>.instance.GetAnyNotifyExist();
				return addedHeroCount > 0 || anyNotifyExist;
			});
		}
	}

	private void SetupFriendTag()
	{
		SetupTag(ERightMenuButton.Friend, () => GlobalAutoBehaviour<AccountManager>.instance.FriendManager.GetFriendRequestCount() > 0);
	}

	private void SetupPayShopTag2()
	{
		bool hasFreePackage = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.HasAnyFreePackage();
		if (hasFreePackage)
		{
			SetupEffect(ERightMenuButton.PayShop2, "Sale", () => false);
			SetupEffect(ERightMenuButton.PayShop2, "Free", () => hasFreePackage);
			return;
		}
		bool hasAnySale = Checker.HasAnySaleEquipGacha2();
		SetupEffect(ERightMenuButton.PayShop2, "Sale", () => hasAnySale);
		SetupEffect(ERightMenuButton.PayShop2, "Free", () => !hasAnySale && GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.HasAnyFreeEquipGachaChance());
	}

	private void SetupButtons(ERightMenuButton excludeButton = ERightMenuButton.None, ERightMenuButton includeButton = ERightMenuButton.None)
	{
		float num = 0f;
		ButtonInfo[] buttonInfos = ButtonInfos;
		foreach (ButtonInfo buttonInfo in buttonInfos)
		{
			if (buttonInfo.ButtonType == excludeButton)
			{
				UIHelper.EnableGameObject(buttonInfo.Button, enable: false);
				continue;
			}
			BottomButtonData data = GameRoot.Instance.BottomButtonDataContainer.GetData(buttonInfo.ButtonType);
			if (data == null)
			{
				UIHelper.EnableGameObject(buttonInfo.Button, enable: false);
				continue;
			}
			bool flag = false;
			flag = data.Type == includeButton || CheckShowButtonByBottomButtonData(data);
			if (data.Type == ERightMenuButton.HeroStat || data.Type == ERightMenuButton.HeroStat)
			{
				UIHelper.EnableGameObject(buttonInfo.Button, enable: false);
				continue;
			}
			UIHelper.EnableGameObject(buttonInfo.Button, flag);
			if (flag)
			{
				num = Mathf.Max(num, data.BgScale);
			}
		}
		Vector3 localScale = SpriteButtonBackground.transform.localScale;
		localScale.x = num;
		SpriteButtonBackground.transform.localScale = localScale;
		ButtonGrid.Reposition();
	}

	public static bool CheckShowButtonByBottomButtonData(BottomButtonData buttonData)
	{
		bool flag = false;
		int tutorialIndex = buttonData.TutorialIndex;
		flag = tutorialIndex == 0 || !SingletonUpdater<TutorialManager>.instance.IsTutorialEnabled || GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(tutorialIndex);
		if (!flag && !__Extension_Method_ICollection__.IsNullOrEmpty(buttonData.ForceOpenDungeon))
		{
			List<Tuple<int, int, string>> forceOpenDungeon = buttonData.GetForceOpenDungeon();
			if (forceOpenDungeon != null)
			{
				foreach (Tuple<int, int, string> item in forceOpenDungeon)
				{
					if (item != null)
					{
						DungeonDifficultyType difficultyType = EnumTools.Parse(item.Item3, DungeonDifficultyType.Easy);
						if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(item.Item1, item.Item2, difficultyType))
						{
							flag = true;
							break;
						}
					}
				}
			}
		}
		return flag;
	}

	private void SetupMissionEventEffect()
	{
		bool isMissionComplete = true;
		foreach (int item in GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(AchievementType.Daily))
		{
			if (!AchievementChecker.Completed(item))
			{
				isMissionComplete = false;
				break;
			}
		}
		bool isOnBooster = false;
		PlayerItemTimeDurationInfo effectiveBooster = GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.GetEffectiveBooster(EventBonusType.BonusDailyMissionReward);
		isOnBooster = effectiveBooster != null;
		SetupEffect(ERightMenuButton.Mission, "Booster", delegate
		{
			if (isMissionComplete)
			{
				return false;
			}
			return isOnBooster ? true : false;
		});
		SetupEffect(ERightMenuButton.Mission, "Event", delegate
		{
			if (isOnBooster && !isMissionComplete)
			{
				return false;
			}
			return CheckLimitedEventMissionEffect() || CheckMissionEventEffect();
		});
	}

	private bool CheckMissionEventEffect()
	{
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.BonusDailyMissionReward, AchievementType.Daily.ToString());
		List<DropBonusEvent> effectiveEvents = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.BonusEventMissionReward);
		if (effectiveEvent == null && effectiveEvents == null)
		{
			return false;
		}
		if (effectiveEvent != null)
		{
			foreach (int item in GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(AchievementType.Daily))
			{
				if (!AchievementChecker.Completed(item))
				{
					return true;
				}
			}
		}
		if (effectiveEvents != null)
		{
			foreach (DropBonusEvent item2 in effectiveEvents)
			{
				string condition = item2.Condition;
				HashSet<int> achievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetAchievementIndexSet(condition);
				if (achievementIndexSet == null)
				{
					continue;
				}
				foreach (int item3 in achievementIndexSet)
				{
					if (!AchievementChecker.Completed(item3))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool CheckLimitedEventMissionEffect()
	{
		bool result = false;
		List<AchievementData> list = GameRoot.Instance.AchievementDataContainer.SelectAchievementOfType(AchievementType.Limited, (AchievementData data) => (data.LimitedEventMissionType != EventBonusType.None) ? true : false);
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
		{
			AchievementData achievementData = list[num];
			if (achievementData != null && GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(achievementData.LimitedEventMissionType, achievementData.EventName) != null)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	[UIEvent(AllowDuringPatchDownloading = true)]
	public void OnClickHeroButton()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickHeroButton();
		}
	}

	[UIEvent(AllowDuringPatchDownloading = true)]
	public void OnClickInventoryButton()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickInventoryButton();
		}
	}

	public void OnClickAchievementButton()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickAchievementButton();
		}
	}

	public void OnClickQuestButton()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickQuestButton();
		}
	}

	public void OnClickFriendButton()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickFriendButton();
		}
	}

	public void OnClickPortalButton()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickPortalRenewalButton();
		}
	}

	public void OnClickPayShopButton()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickPayShopButton();
		}
	}

	public void OnClickArchive()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickArchive();
		}
	}

	public void OnClickHeroStatButton()
	{
		if (_OpenMenu.Value && _listener != null)
		{
			_listener.OnClickHeroStatButton();
		}
	}

	private void Start()
	{
	}

	public override void Close()
	{
		if (_flyCoroutine != null)
		{
			VespaCoroutine.StopScene(_flyCoroutine);
			_flyCoroutine = null;
		}
		base.Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (AccountManager.IsLogined() && Time.time - _lastUpdatedTime > 1f)
		{
			_lastUpdatedTime = Time.time;
			SetupHeroTag();
			SetupPayShopTag2();
			SetupHasCompletedCraftTag();
			SetupMissionEventEffect();
			SetInvenEventEffect();
		}
	}

	public void OnClickToggleMenu()
	{
	}

	public void OnClickGPGS()
	{
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserAchievementManager.ChangeAchievementInfo>.OnChanged(NGame2.NAccount.UserAchievementManager.ChangeAchievementInfo info)
	{
		SetupAchievementTag();
		SetupQuestTag();
	}

	void IUserObserver<NGame2.NAccount.UserItemManager.ItemCountChangedInfo>.OnChanged(NGame2.NAccount.UserItemManager.ItemCountChangedInfo info)
	{
		SetupInvenNewTag();
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		SetupInvenNewTag();
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo statInfo)
	{
		SetupInvenNewTag();
	}

	void IUserObserver<FriendChangeInfo>.OnChanged(FriendChangeInfo changedInfo)
	{
		if (changedInfo.Type == FriendChangeInfo.ChangeType.AddInvitor)
		{
			SetupFriendTag();
		}
	}

	void IUserObserver<AttendanceInfo>.OnChanged(AttendanceInfo changedInfo)
	{
	}

	void IUserObserver<NGame2.NAccount.UserEquipGachaManager.ChangedGachaInfo>.OnChanged(NGame2.NAccount.UserEquipGachaManager.ChangedGachaInfo changedInfo)
	{
		SetupPayShopTag2();
	}

	void IUserObserver<CraftSlotInfo>.OnChanged(CraftSlotInfo craftSlotInfo)
	{
		SetupHasCompletedCraftTag();
	}

	public void OnClickOccurQuest()
	{
	}

	public void OnClickCompleteQuest()
	{
	}
}
