using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class QuestComponent : CommonQuest
{
	public const string PrefabPath = "Prefab2/Component/QuestIntegration/MissionListComponent";

	public static PrefabAuto<QuestComponent> PrefabAuto = new PrefabAuto<QuestComponent>("Prefab2/Component/QuestIntegration/MissionListComponent");

	[Header("[QuestComponent]")]
	public Color EventRewardCountLabelColor = Color.green;

	public int GridHeight = 200;

	private AchievementComponent.EAchievementStatus _achievementStatus;

	private bool _available;

	private bool _cleared;

	private bool _completed;

	private int _towerIndex;

	private IListener<QuestComponent> _iListener;

	private AchievementComponent.ITextProvider _textProvider;

	private AchievementComponent.IDataProvider _dataProvider;

	public AchievementData AchievementData { get; private set; }

	public void Open(IListener<QuestComponent> listener, AchievementComponent.IDataProvider dataProvider, AchievementComponent.ITextProvider textProvider, int AchievementIndex, bool Completed, bool Available)
	{
		Open();
		Clear();
		_iListener = listener;
		_textProvider = textProvider;
		_dataProvider = dataProvider;
		_available = Available;
		AchievementData = (Completed ? _dataProvider.GetLastAchievementData(AchievementIndex) : _dataProvider.GetAchievementData(AchievementIndex));
		_achievementStatus = (Completed ? AchievementComponent.EAchievementStatus.Completed : _dataProvider.GetAchievementStatus(AchievementIndex));
		if (AchievementData != null)
		{
			SetTowerIndex();
			SetupAchievement();
		}
	}

	private void Clear()
	{
		_iListener = null;
		_textProvider = null;
		_dataProvider = null;
		AchievementData = null;
	}

	private void SetTowerIndex()
	{
		switch (AchievementData.ReqType)
		{
		case AchievementReqType.LimitedClearOrdealTower:
			_towerIndex = GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Ordeal);
			break;
		case AchievementReqType.LimitedClearChallengeTower:
			_towerIndex = GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Challenge);
			break;
		case AchievementReqType.LimitedMazeClearRewardClearCheck:
			_towerIndex = GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Maze);
			break;
		default:
			_towerIndex = 0;
			break;
		}
	}

	protected virtual void SetupAchievement()
	{
		if (_textProvider != null)
		{
			EnableRemainTime(AchievementData.Type == AchievementType.Limited);
			SetTitleLabel(_textProvider.GetAchievementName(AchievementData));
			AchievementInfo achievementInfo = _dataProvider.GetAchievementInfo(AchievementData.Index);
			SetRequireLabel(_textProvider.GetAchievementRequirement(AchievementData, achievementInfo));
			SetupAchievementReward();
			bool flag = (_completed = _achievementStatus == AchievementComponent.EAchievementStatus.Completed);
			bool flag2 = (_cleared = _achievementStatus == AchievementComponent.EAchievementStatus.Cleared);
			String_Extension_Method.IsNullOrEmpty(AchievementData.MoveTaskType);
			if (false)
			{
				UIHelper.EnableGameObject(CompleteBlackSprite, enable: false);
				SetStateIcon(EStateIconType.ExclamationMark, isGrayScale: false);
				SetStateIcon(EStateIconType.CheckMark, isGrayScale: false);
				EnableStateIcon(isEnable: false);
			}
			else
			{
				UIHelper.EnableGameObject(CompleteBlackSprite, flag || !_available);
				SetStateIcon(EStateIconType.GiftBox, flag || !flag2);
				EnableStateIcon(isEnable: true);
				SetupRemainTime();
			}
		}
	}

	private void SetupAchievementReward()
	{
		SetupAchievementReward(AchievementData, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid, EventRewardCountLabelColor);
	}

	public static void SetupAchievementReward(AchievementData achievementData, ref List<ItemIconComponent> itemIconList, ref List<CurrencyIconComponent> currencyIconList, IGrid grid, Color eventRewardColor)
	{
		if (achievementData == null)
		{
			return;
		}
		foreach (AchievementReward achievementReward in achievementData.GetAchievementRewardList())
		{
			IIconComponent iconComponent = CreateAchievementReward(achievementReward, achievementData, grid, eventRewardColor);
			if (iconComponent is ItemIconComponent)
			{
				itemIconList.Add(iconComponent as ItemIconComponent);
			}
			else if (iconComponent is CurrencyIconComponent)
			{
				currencyIconList.Add(iconComponent as CurrencyIconComponent);
			}
		}
		UIHelper.RepositionGrid(grid);
	}

	public static IIconComponent CreateAchievementReward(AchievementReward reward, AchievementData achievementData, IGrid grid, Color eventRewardColor)
	{
		if (!AchievementData.ValidReward(reward))
		{
			return null;
		}
		Color color = Color.white;
		int rewardValue = reward.RewardValue;
		long num = rewardValue;
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.GetEffectiveBooster(EventBonusType.BonusDailyMissionReward) != null && achievementData.Type == AchievementType.Daily)
		{
			int effectiveBoosterAddPercent = GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.GetEffectiveBoosterAddPercent(EventBonusType.BonusDailyMissionReward);
			if (effectiveBoosterAddPercent > 0)
			{
				long num2 = num;
				int rewardValue2 = reward.RewardValue;
				long num3 = (long)((float)effectiveBoosterAddPercent / 100f);
				int num4 = rewardValue2;
				num = checked(num2 + num4 * num3);
				color = eventRewardColor;
			}
		}
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.BonusDailyMissionReward, achievementData.Type.ToString());
		if (effectiveEvent != null && effectiveEvent.BonusPercentage > 0)
		{
			long num5 = num;
			int rewardValue3 = reward.RewardValue;
			long num6 = (long)((float)effectiveEvent.BonusPercentage / 100f);
			int num7 = rewardValue3;
			num = checked(num5 + num7 * num6);
			color = eventRewardColor;
		}
		if (reward.RewardType == AchievementRewardType.Item || reward.RewardType == AchievementRewardType.Booster)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(grid, ItemIconComponent.PrefabAuto);
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(reward.RewardCode);
			itemIconComponent.Open(dataByCode);
			long num8 = num;
			int num9 = 1;
			if (num8 > num9)
			{
				itemIconComponent.SetItemCount(num, color);
			}
			return itemIconComponent;
		}
		CurrencyType currencyType = CurrencyType.None;
		long currencyAmount = num;
		AchievementRewardTypeAttribute achievementRewardTypeAttribute = EnumAttributeContainer<AchievementRewardType, AchievementRewardTypeAttribute>.Get(reward.RewardType);
		if (achievementRewardTypeAttribute != null)
		{
			currencyType = achievementRewardTypeAttribute.CurrencyType;
		}
		if (reward.RewardType == AchievementRewardType.GoldTeamLevel)
		{
			currencyType = CurrencyType.Gold;
			int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
			int num10 = GameRoot.Instance.TeamLevelDataContainer.GetData(teamLevel)?.DailyGold ?? 0;
			currencyAmount = num10;
		}
		else if (reward.RewardType == AchievementRewardType.StaminaTeamLevel)
		{
			currencyType = CurrencyType.Stamina;
			int teamLevel2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
			int num11 = GameRoot.Instance.TeamLevelDataContainer.GetData(teamLevel2)?.DailyBonusStamina ?? 0;
			currencyAmount = num11;
		}
		CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(grid, CurrencyIconComponent.PrefabAuto);
		currencyIconComponent.Open(currencyType, currencyAmount, null);
		currencyIconComponent.SetAmountColor(color);
		return currencyIconComponent;
	}

	public void OnClick()
	{
		if (_iListener != null)
		{
			_iListener.OnClick(this);
		}
	}

	public static void SetupEventOrBooster(AchievementData data, UIWidget eventImage, UIWidget boosterImage)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.GetEffectiveBooster(EventBonusType.BonusDailyMissionReward) != null && data.Type == AchievementType.Daily)
		{
			UIHelper.EnableGameObject(boosterImage, enable: true);
			UIHelper.EnableGameObject(eventImage, enable: false);
			return;
		}
		UIHelper.EnableGameObject(boosterImage, enable: false);
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.BonusDailyMissionReward, data.Type.ToString());
		DropBonusEvent effectiveEvent2 = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.BonusEventMissionReward, data.EventName);
		DropBonusEvent effectiveEvent3 = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(data.LimitedEventMissionType, data.EventName);
		bool flag = effectiveEvent != null;
		bool flag2 = effectiveEvent2 != null && !string.IsNullOrEmpty(data.EventName);
		bool flag3 = effectiveEvent3 != null && !string.IsNullOrEmpty(data.EventName);
		UIHelper.EnableGameObject(eventImage, flag || flag2 || flag3);
	}

	private void SetRemainTime()
	{
		if (AchievementData == null)
		{
			return;
		}
		UIHelper.SetLabelText(RemainTimeLabel, "");
		if (__Extension_Method_ICollection__.COUNT(AchievementData.MonthlyResetDate) > 0)
		{
			int remainMonthSecond = RequestEnterLobbyWrapper.GetRemainMonthSecond(AchievementData.MonthlyResetDate);
			UIHelper.SetRemainTimeLabelText(RemainTimeLabel, remainMonthSecond);
		}
		else if (AchievementData.WeeklyReset)
		{
			int remainWeekSecond = RequestEnterLobbyWrapper.GetRemainWeekSecond();
			UIHelper.SetRemainTimeLabelText(RemainTimeLabel, remainWeekSecond);
		}
		else if (AchievementData.DailyReset)
		{
			int remainDaySecond = RequestEnterLobbyWrapper.GetRemainDaySecond();
			UIHelper.SetRemainTimeLabelText(RemainTimeLabel, remainDaySecond);
		}
		else if (AchievementData.ReqType == AchievementReqType.LimitedClearChallengeRaid)
		{
			int remainTime = SceneAutoBehaviour<RaidManagement>.instance.GetRemainTime();
			long num = remainTime;
			int num2 = 0;
			if (num > num2)
			{
				UIHelper.SetLabelText(RemainTimeLabel, LocalizationManager.GetString(GuildRaid.GUILDRAID_STATE_OPENED, UIHelper.GetRemainTimeLabelText(num)));
			}
		}
		else if (AchievementData.LimitedEventMissionType != EventBonusType.None)
		{
			int remainEventTime = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetRemainEventTime(AchievementData.LimitedEventMissionType, AchievementData.EventName);
			if (remainEventTime > 0)
			{
				UIHelper.SetRemainTimeLabelText(RemainTimeLabel, remainEventTime);
				return;
			}
			string stringDirect = LocalizationManager.GetStringDirect("EVENT_MISSION_NOTICE_NOTPERIOD");
			UIHelper.SetLabelText(RemainTimeLabel, stringDirect);
		}
	}

	public void SetupRemainTime()
	{
		if (_towerIndex > 0)
		{
			TowerHelper.SetupRemainResetTimeLabel(RemainTimeLabel, _towerIndex);
		}
		else
		{
			SetRemainTime();
		}
	}
}
