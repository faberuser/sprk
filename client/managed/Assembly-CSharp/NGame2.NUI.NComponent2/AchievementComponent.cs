using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NShared.NWorldMap.NEventSubData;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class AchievementComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickRewardButton(int AchievementIndex);
	}

	public interface IDataProvider
	{
		EAchievementStatus GetAchievementStatus(int AchievementIndex);

		AchievementData GetAchievementData(int AchievementIndex);

		AchievementData GetLastAchievementData(int AchievementIndex);

		AchievementInfo GetAchievementInfo(int AchievementIndex);

		void GetAchievementAndRequirement(int AchievementIndex, out long Achievement, out long Requirement, out long PrevRequirement);

		ulong GetLastStepRequirement(int AchievementIndex);
	}

	public interface ITextProvider
	{
		string GetAchievementName(AchievementData achievementData);

		string GetAchievementDesc(AchievementData achievementData);

		string GetAchievementRequirement(AchievementData achievementData, AchievementInfo achievementInfo);

		string GetAchievementGoalRequirement(AchievementData achievementData, AchievementInfo achievementInfo);
	}

	public enum EAchievementStatus
	{
		InProgress,
		Cleared,
		Completed
	}

	public const string PrefabPath = "Prefab2/Component/Achievement/AchievementComponent";

	public const string HeroPrefabPath = "Prefab2/Component/Achievement/HeroAchievementComponent";

	public static PrefabAuto<AchievementComponent> PrefabAuto = new PrefabAuto<AchievementComponent>("Prefab2/Component/Achievement/AchievementComponent");

	public static PrefabAuto<AchievementComponent> HeroPrefabAuto = new PrefabAuto<AchievementComponent>("Prefab2/Component/Achievement/HeroAchievementComponent");

	public UILabel Label_AchievementName;

	public UILabel Label_AchievementDesc;

	public UISprite Sprite_Gold;

	public UISprite Sprite_Gem;

	public UISprite Sprite_Stamina;

	public UISprite Sprite_PvpCoin;

	public UILabel Label_Reward;

	public UISimpleGrid RewardGrid;

	public UIButton Button_GetReward;

	public UISprite Sprite_Progress;

	public UIProgressBar Sprite_ProgressBar;

	public UILabel Label_Progress;

	public UILabel Label_Complete;

	public UIWidget Widget_InProgress;

	public UILabel Label_GetReward;

	public UILabel Label_RewardCompleted;

	public UILabel Label_InProgress2;

	public UITexture Texture_AchievementType;

	public UILabel Label_Place;

	public UILabel Label_RemainTime;

	public UISprite Sprite_CompletedFront;

	public UISprite Sprite_AchievementFull;

	private Listener _listener;

	private IDataProvider _dataProvider;

	private ITextProvider _textProvider;

	private int _AchievementIndex;

	private EAchievementStatus _achievementStatus;

	private QuestDungeon _questDungeon;

	private List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _rewardCurrencyIconComponentList = new List<CurrencyIconComponent>();

	public void Open(Listener listener, IDataProvider dataProvider, ITextProvider textProvider, int AchievementIndex, bool Completed)
	{
		Reset();
		_listener = listener;
		_dataProvider = dataProvider;
		_textProvider = textProvider;
		_questDungeon = null;
		_AchievementIndex = AchievementIndex;
		_achievementStatus = (Completed ? EAchievementStatus.Completed : _dataProvider.GetAchievementStatus(_AchievementIndex));
		AchievementData achievementData = (Completed ? _dataProvider.GetLastAchievementData(_AchievementIndex) : _dataProvider.GetAchievementData(_AchievementIndex));
		SetupAchievementInfo(achievementData);
		long Achievement = 1L;
		long Requirement = 1L;
		long PrevRequirement = 0L;
		if (_achievementStatus == EAchievementStatus.InProgress)
		{
			_dataProvider.GetAchievementAndRequirement(_AchievementIndex, out Achievement, out Requirement, out PrevRequirement);
		}
		SetAchievementBarTextWidget(Achievement, Requirement, PrevRequirement);
	}

	public void Open(Listener listener, QuestDungeon questDungeon)
	{
		Reset();
		_listener = listener;
		_dataProvider = null;
		_questDungeon = questDungeon;
		SetupQuestDungeonInfo();
		SetAchievementBarTextWidget(_questDungeon.ProgressCount, _questDungeon.RequireCount, 0L);
	}

	private void Reset()
	{
		_listener = null;
		_textProvider = null;
		_dataProvider = null;
		FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
	}

	private void SetupQuestDungeonInfo()
	{
		if (_questDungeon != null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_questDungeon.MonsterIndex);
			if (data != null)
			{
				string text = "[FFFF00][" + LocalizationManager.GetString(Achievement.MONSTER_SUPPRESS) + "][-]";
				UIHelper.SetLabelText(Label_AchievementName, text + data.Name + " " + LocalizationManager.GetString(Achievement.MONSTER_SUPPRESS));
				UIHelper.SetLabelText(Label_AchievementDesc, data.Name + " " + LocalizationManager.GetString(Achievement.MONSTER_KILL));
				AssetTexture heroIcon = UIHelper.GetHeroIcon2(_questDungeon.MonsterIndex, ignoreAccount: true);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_AchievementType, heroIcon);
			}
			UIHelper.EnableGameObject(Sprite_Gold, _questDungeon.RewardGold > 0);
			UIHelper.EnableGameObject(Sprite_Gem, enable: false);
			UIHelper.EnableGameObject(Sprite_Stamina, enable: false);
			UIHelper.EnableGameObject(Sprite_PvpCoin, enable: false);
			if (_questDungeon.RewardGold > 0)
			{
				UILabel label_Reward = Label_Reward;
				int rewardGold = _questDungeon.RewardGold;
				UIHelper.SetLabelText(label_Reward, Vespa_Int_Extension_Method.ToCommaString(rewardGold));
			}
			UIHelper.EnableButton(Button_GetReward, _questDungeon.ProgressCount >= _questDungeon.RequireCount);
			UIHelper.EnableGameObject(Label_GetReward, _questDungeon.ProgressCount >= _questDungeon.RequireCount);
			UIHelper.EnableGameObject(Label_InProgress2, _questDungeon.ProgressCount < _questDungeon.RequireCount);
			UIHelper.EnableGameObject(Label_RewardCompleted, enable: false);
			UIHelper.SetLabelText(Label_Place, "Chapter " + _questDungeon.ChapterIndex + "-" + _questDungeon.DungeonIndex);
			if (_questDungeon.Completed)
			{
				UIHelper.SetLabelText(Label_RemainTime, LocalizationManager.GetString(Achievement.MISSION_STATUS_REWARD_COMPLETED));
			}
			else
			{
				UpdateRemainTime();
			}
		}
	}

	private void UpdateRemainTime()
	{
		if (_questDungeon != null && !_questDungeon.Completed)
		{
			int remainTime = _questDungeon.GetRemainTime();
			int num = 0;
			int num2 = 0;
			if (remainTime >= 0)
			{
				num = remainTime / 60;
				num2 = remainTime % 60;
			}
			UIHelper.SetLabelText(Label_RemainTime, LocalizationManager.GetString(NShared.NLocalization.System.REMAIN_TIME_MIN_SEC, num, num2));
		}
	}

	private void Update()
	{
		UpdateRemainTime();
	}

	private void SetupAchievementInfo(AchievementData achievementData)
	{
		if (achievementData == null || _textProvider == null)
		{
			return;
		}
		UIHelper.SetLabelText(Label_AchievementName, _textProvider.GetAchievementName(achievementData));
		UIHelper.SetLabelText(Label_AchievementDesc, _textProvider.GetAchievementDesc(achievementData));
		AssetTexture achievementTypeIcon = UIHelper.GetAchievementTypeIcon(EnumTools.GetName(achievementData.SubType));
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_AchievementType, achievementTypeIcon);
		if (achievementData.RewardType == AchievementRewardType.Item)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardGrid, ItemIconComponent.PrefabAuto);
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(achievementData.RewardCode);
			itemIconComponent.Open(dataByCode);
			if (achievementData.RewardValue > 1)
			{
				int rewardValue = achievementData.RewardValue;
				Color? color = null;
				int num = rewardValue;
				itemIconComponent.SetItemCount(num, color);
			}
			_rewardItemIconComponentList.Add(itemIconComponent);
		}
		else
		{
			CurrencyType currencyType = CurrencyType.None;
			int rewardValue2 = achievementData.RewardValue;
			long currencyAmount = rewardValue2;
			AchievementRewardTypeAttribute achievementRewardTypeAttribute = EnumAttributeContainer<AchievementRewardType, AchievementRewardTypeAttribute>.Get(achievementData.RewardType);
			if (achievementRewardTypeAttribute != null)
			{
				currencyType = achievementRewardTypeAttribute.CurrencyType;
			}
			if (achievementData.RewardType == AchievementRewardType.GoldTeamLevel)
			{
				currencyType = CurrencyType.Gold;
				int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
				int num2 = GameRoot.Instance.TeamLevelDataContainer.GetData(teamLevel)?.DailyGold ?? 0;
				currencyAmount = num2;
			}
			if (achievementData.RewardType == AchievementRewardType.StaminaTeamLevel)
			{
				currencyType = CurrencyType.Stamina;
				int teamLevel2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
				int num3 = GameRoot.Instance.TeamLevelDataContainer.GetData(teamLevel2)?.DailyBonusStamina ?? 0;
				currencyAmount = num3;
			}
			CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(RewardGrid, CurrencyIconComponent.PrefabAuto);
			currencyIconComponent.Open(currencyType, currencyAmount, null);
			_rewardCurrencyIconComponentList.Add(currencyIconComponent);
		}
		UIHelper.EnableButton(Button_GetReward, _achievementStatus == EAchievementStatus.Cleared);
		UIHelper.EnableGameObject(Label_GetReward, _achievementStatus == EAchievementStatus.Cleared);
		UIHelper.EnableGameObject(Label_InProgress2, _achievementStatus == EAchievementStatus.InProgress);
		UIHelper.EnableGameObject(Label_RewardCompleted, _achievementStatus == EAchievementStatus.Completed);
		UIHelper.EnableGameObject(Sprite_CompletedFront, _achievementStatus == EAchievementStatus.Completed);
	}

	private void SetAchievementBarTextWidget(long Achievement, long Requirement, long PrevRequirement)
	{
		if (Achievement > Requirement)
		{
			Achievement = Requirement;
		}
		UIHelper.EnableGameObject(Label_Progress, _achievementStatus == EAchievementStatus.InProgress);
		UIHelper.EnableGameObject(Label_Complete, _achievementStatus != EAchievementStatus.InProgress);
		if (_achievementStatus == EAchievementStatus.InProgress)
		{
			string text = $"{Achievement}/{Requirement}";
			UIHelper.SetLabelText(Label_Progress, text);
		}
		if (Achievement == Requirement)
		{
			UIHelper.EnableGameObject(Sprite_Progress, enable: false);
			UIHelper.EnableGameObject(Sprite_AchievementFull, enable: true);
			return;
		}
		UIHelper.EnableGameObject(Sprite_AchievementFull, enable: false);
		if (Sprite_ProgressBar == null)
		{
			Sprite_ProgressBar = Sprite_Progress.gameObject.AddComponent<UIProgressBar>();
			Sprite_ProgressBar.foregroundWidget = Sprite_Progress;
		}
		long num = System.Math.Max(0L, Achievement - PrevRequirement);
		float num2 = System.Math.Max(1L, Requirement - PrevRequirement);
		Sprite_ProgressBar.value = (float)num / num2;
		UIHelper.EnableGameObject(Sprite_Progress, Sprite_ProgressBar.value > 0f);
	}

	public void OnClickRewardButton()
	{
		if (_listener != null)
		{
			_listener.OnClickRewardButton(_AchievementIndex);
		}
	}

	public void OnClickQuestDungeonRewardButton()
	{
	}
}
