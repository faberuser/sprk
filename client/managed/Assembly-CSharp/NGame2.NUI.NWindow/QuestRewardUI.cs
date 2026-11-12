using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class QuestRewardUI : MonoBehaviour
{
	public UISprite Sprite_EventImage;

	public UISprite Sprite_BoosterImage;

	public UIButton Button_Reward;

	public Transform GetAllDisableTransform;

	public UIButton GetAllRewardButton;

	public UISprite Sprite_QuestComplete;

	public UILabel Label_QuestProgress;

	public UIClipSprite Sprite_QuestProgressBar;

	public UILabel Label_CompleteProgress;

	public UISprite Sprite_CompleteProgressBar;

	public UISimpleGrid RewardListGrid;

	public Color EventRewardCountLabelColor = Color.green;

	private List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _rewardCurrencyIconComponentList = new List<CurrencyIconComponent>();

	public MissionComponentData ElementData { get; private set; }

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
	}

	public void Refresh(bool Reward)
	{
		Clear();
		SetupUI(ElementData, Reward);
	}

	public void SetupUI(MissionComponentData elementData, bool Reward)
	{
		if (elementData == null)
		{
			return;
		}
		Clear();
		ElementData = elementData;
		SetupAllReward(Reward);
		if (ElementData.GetData<AchievementData>() != null)
		{
			AchievementData data = ElementData.GetData<AchievementData>();
			bool flag = AchievementChecker.Completed(ElementData.Index);
			bool flag2 = AchievementChecker.Cleared(ElementData.Index);
			UIHelper.EnableButton(Button_Reward, !flag && flag2, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(Sprite_QuestComplete, flag);
			SingletonAuto<AchievementChecker>.instance.GetAchievementAndRequirement(ElementData.Index, out var curProgress, out var requirement, out var _);
			if (flag)
			{
				SetupProgressBar(curProgress, curProgress, Complete: true);
			}
			else
			{
				SetupProgressBar(curProgress, requirement, flag2);
			}
			CommonMission.SetupAchievementReward(data, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid, EventRewardCountLabelColor);
		}
		else if (ElementData.GetData<SubQuestData>() != null)
		{
			SubQuestData data2 = ElementData.GetData<SubQuestData>();
			QuesetStatusType subQuestStatus = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(data2);
			UIHelper.EnableButton(Button_Reward, subQuestStatus == QuesetStatusType.Reward);
			UIHelper.EnableGameObject(Sprite_QuestComplete, subQuestStatus == QuesetStatusType.Complete);
			long progress = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestInfoByQuestIndex(ElementData.Index).Progress;
			long requirement2 = ElementData.GetData<SubQuestData>().GetReqProgress();
			SetupProgressBar(progress, requirement2, subQuestStatus == QuesetStatusType.Complete);
			CommonMission.SetupSubQuestReward(data2, null, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid);
		}
	}

	public void SetupAllReward(bool enable)
	{
		UIHelper.EnableGameObject(GetAllRewardButton, enable);
		UIHelper.EnableGameObject(GetAllDisableTransform, !enable);
	}

	public void SetupProgressBar(long Achieved, long Requirement, bool Complete)
	{
		UIHelper.EnableGameObject(Label_QuestProgress, !Complete);
		UIHelper.EnableGameObject(Label_CompleteProgress, Complete);
		UIHelper.EnableGameObject(Sprite_QuestProgressBar, !Complete);
		UIHelper.EnableGameObject(Sprite_CompleteProgressBar, Complete);
		UIHelper.SetLabelText(Label_QuestProgress, Achieved + "/" + Requirement);
		UIHelper.SetLabelText(Label_CompleteProgress, Requirement + "/" + Requirement);
		float fillAmount = Mathf.Clamp01((float)Achieved / (float)Requirement);
		UIHelper.SetSpriteFillAmount(Sprite_QuestProgressBar, fillAmount);
	}
}
