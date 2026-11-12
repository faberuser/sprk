using System;
using System.Collections.Generic;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.Mission;

public class MissionNodeWindowTopView2 : BaseWindow, MissionMultipleStepsRewardComponent.IListener
{
	public interface IListener
	{
		void OnClickGetReward();

		void OnClickGetAllReward();
	}

	[Serializable]
	public class TopViewLayout
	{
		public EMissionTopViewLayout LayoutType;

		public Transform Root;

		public Transform DescriptionRoot;

		public UITable DescSortTable;

		public UILabel LabelNotice;

		public UILabel LabelRemainTime;

		public void SetDescVisible(bool showNotice, bool showRemainTime)
		{
			bool enable = showNotice || showRemainTime;
			UIHelper.EnableGameObject(DescriptionRoot, enable);
			UIHelper.EnableGameObject(LabelNotice, showNotice);
			UIHelper.EnableGameObject(LabelRemainTime, showRemainTime);
			if (DescSortTable != null)
			{
				DescSortTable.Reposition();
			}
		}

		public void SetNoticeMessage(string noticeMessage)
		{
			UIHelper.SetLabelText(LabelNotice, noticeMessage);
		}

		public void SetRemainTime(string time)
		{
			UIHelper.SetLabelText(LabelRemainTime, time);
		}

		public void SetVisibleRoot(bool visible)
		{
			UIHelper.EnableGameObject(Root, visible);
		}
	}

	[Serializable]
	public class TopViewRewardButtonGroup
	{
		public EMissionTopViewButtonGroup ButtonGroupType;

		public Transform Root;

		[Header("Button")]
		public UIButton GetRewardButton;

		public UIButton GetAllRewardButton;

		[Header("Complete")]
		public Transform QuestComplete;

		public Transform AllRewardComplete;

		public void SetGetRewardButton(bool cleared, bool completed)
		{
			UIHelper.EnableButton(GetRewardButton, cleared);
			UIHelper.EnableGameObject(GetRewardButton, !completed);
			UIHelper.EnableGameObject(QuestComplete, completed);
		}

		public void SetGetAllRewardButton(bool allGet)
		{
			UIHelper.EnableGameObject(GetAllRewardButton, allGet);
			UIHelper.EnableGameObject(AllRewardComplete, !allGet);
		}

		public void SetVisibleRoot(bool visible)
		{
			UIHelper.EnableGameObject(Root, visible);
		}
	}

	[Serializable]
	public class TopViewProgressBarLayout
	{
		public EMissionTopViewRewardType RewardType;

		public Transform Root;

		public UIClipSprite SpriteQuestProgressBar;

		public UISprite SpriteCompleteProgressBar;

		public UISprite SpriteDisableCompleteProgress;

		public UILabel LabelQuestProgress;

		public UILabel LabelCompleteProgress;

		public void SetProgressBar(long achieved, long requirement, bool complete, bool cleared)
		{
			SetProgressBarVisible(complete, cleared);
			SetProgressLabel(achieved, requirement);
			float progressBarLength = Mathf.Clamp01((float)achieved / (float)requirement);
			SetProgressBarLength(progressBarLength);
		}

		public void SetProgressBarVisible(bool complete, bool cleared)
		{
			bool flag = complete || cleared;
			UIHelper.EnableGameObject(LabelQuestProgress, !flag);
			UIHelper.EnableGameObject(LabelCompleteProgress, flag);
			UIHelper.EnableGameObject(SpriteQuestProgressBar, !flag);
			UIHelper.EnableGameObject(SpriteCompleteProgressBar, flag);
			UIHelper.EnableGameObject(SpriteDisableCompleteProgress, complete);
		}

		public void SetProgressLabel(long achieved, long requirement)
		{
			string text = ((RewardType == EMissionTopViewRewardType.SingleStep) ? (achieved + "/" + requirement) : achieved.ToString());
			string text2 = ((RewardType == EMissionTopViewRewardType.SingleStep) ? (requirement + "/" + requirement) : requirement.ToString());
			UIHelper.SetLabelText(LabelQuestProgress, text);
			UIHelper.SetLabelText(LabelCompleteProgress, text2);
		}

		public void SetProgressBarLength(float ratio)
		{
			UIHelper.SetSpriteFillAmount(SpriteQuestProgressBar, ratio);
		}

		public void SetVisibleRoot(bool visible)
		{
			UIHelper.EnableGameObject(Root, visible);
		}
	}

	[Serializable]
	public class TopViewSingleStepRewardLayout
	{
		public EMissionTopViewRewardType RewardType;

		public Transform Root;

		public UISimpleGrid RewardListGrid;

		public void SetVisibleRoot(bool visible)
		{
			UIHelper.EnableGameObject(Root, visible);
		}
	}

	[Serializable]
	public class TopViewMultipleStepsRewardLayout
	{
		public EMissionTopViewRewardType RewardType;

		public Transform Root;

		public UIWidget RewardListRoot;

		public void SetVisibleRoot(bool visible)
		{
			UIHelper.EnableGameObject(Root, visible);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/QuestIntegration/MissionNodeWindowTopView2";

	[Header("[Layout Style List]")]
	public TopViewLayout[] LayoutList;

	[Header("[Button Group List]")]
	public TopViewRewardButtonGroup[] RewardButtonGroupList;

	[Header("[ProgressBar Layout List]")]
	public TopViewProgressBarLayout[] ProgressBarLayoutList;

	[Header("[Reward Layout]")]
	public TopViewSingleStepRewardLayout SingleStepRewardLayout;

	public TopViewMultipleStepsRewardLayout MultipleStepsRewardLayout;

	public Color EventRewardCountLabelColor = Color.green;

	public Transform LayoutRoot;

	public Transform ButtonRoot;

	public Transform ProgressBarRoot;

	public Transform RewardRoot;

	private IListener _iListener;

	private TopViewLayout _Layout;

	private TopViewRewardButtonGroup _buttonGroup;

	private TopViewProgressBarLayout _progressBarLayout;

	private List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _rewardCurrencyIconComponentList = new List<CurrencyIconComponent>();

	private List<MissionMultipleStepsRewardComponent> _rewardMultipleStepsComponentList = new List<MissionMultipleStepsRewardComponent>();

	public void Open(IListener listener, Transform parent, MissionCategoryData missionCategoryData)
	{
		SprkMissionClaimAll.Attach(this, listener);
		_iListener = listener;
		Open(delegate
		{
			InitLayout(missionCategoryData);
			InitButtonGroup(missionCategoryData.TopViewButtonGroupType);
			InitProgressBar(missionCategoryData.TopViewRewardType);
			InitRewardLayout(missionCategoryData.TopViewRewardType);
		}, delegate
		{
			base.transform.parent = parent;
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(base.transform);
		});
	}

	public override void Reset()
	{
		base.Reset();
		ClearSingleStepRewardIconComponent();
		ClearMultipleStepsRewardIconComponent();
	}

	public void SetProgressBar(long achieved, long requirement, bool complete, bool cleared)
	{
		if (_progressBarLayout != null)
		{
			_progressBarLayout.SetProgressBar(achieved, requirement, complete, cleared);
		}
	}

	public void SetGetRewardButton(bool cleared, bool completed)
	{
		if (_buttonGroup != null)
		{
			_buttonGroup.SetGetRewardButton(cleared, completed);
		}
	}

	public void SetGetAllRewardButton(bool allGet)
	{
		if (_buttonGroup != null)
		{
			_buttonGroup.SetGetAllRewardButton(allGet);
		}
	}

	public void SetNoticeMessage(string key)
	{
		if (_Layout != null)
		{
			string noticeMessage = ((!String_Extension_Method.IsNullOrEmpty(key)) ? LocalizationManager.GetStringDirect(key) : string.Empty);
			_Layout.SetNoticeMessage(noticeMessage);
		}
	}

	public void SetRemainTime(TimeSpan remainTimeSpan)
	{
		if (_Layout != null)
		{
			string remainTime = ((!(remainTimeSpan > TimeSpan.Zero)) ? LocalizationManager.GetStringDirect("EVENT_MISSION_NOTICE_NOTPERIOD") : SingletonAuto<MissionStatus>.instance.GetRemainTimeString_Short(remainTimeSpan));
			_Layout.SetRemainTime(remainTime);
		}
	}

	public void SetAchievementReward(AchievementData achievementData)
	{
		SetRewardGrid(delegate
		{
			CommonMission.SetupAchievementReward(achievementData, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, SingleStepRewardLayout.RewardListGrid, EventRewardCountLabelColor);
		});
	}

	public void SetSingleSubQuestReward(SubQuestData subQuestData)
	{
		SetRewardGrid(delegate
		{
			CommonMission.SetupSubQuestReward(subQuestData, null, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, SingleStepRewardLayout.RewardListGrid);
		});
	}

	public void SetMultipleStepsSubQuestReward(List<SubQuestData> subQuestDataList)
	{
		ClearMultipleStepsRewardIconComponent();
		if (__Extension_Method_ICollection__.COUNT(subQuestDataList) == 0 || MultipleStepsRewardLayout == null || MultipleStepsRewardLayout.RewardListRoot == null)
		{
			return;
		}
		int num = __Extension_Method_ICollection__.COUNT(subQuestDataList);
		float num2 = MultipleStepsRewardLayout.RewardListRoot.localSize.x / (float)num;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(subQuestDataList); i++)
		{
			SubQuestData subQuestData = subQuestDataList[i];
			if (subQuestData == null)
			{
				continue;
			}
			int questIndex = subQuestData.QuestIndex;
			int step = subQuestData.Step;
			int reqProgress = subQuestData.GetReqProgress();
			RewardData rewardData = CommonMission.GetRewardData(subQuestData);
			if (rewardData == null)
			{
				continue;
			}
			List<RewardElementData> rewardItemList = rewardData.GetRewardItemList();
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(rewardItemList); j++)
			{
				RewardElementData rewardElementData = rewardItemList[j];
				if (rewardElementData != null)
				{
					MissionMultipleStepsRewardComponent missionMultipleStepsRewardComponent = MissionMultipleStepsRewardComponent.PrefabAuto.Factory.Create(MultipleStepsRewardLayout.RewardListRoot.transform);
					if (!(missionMultipleStepsRewardComponent == null))
					{
						missionMultipleStepsRewardComponent.Open(this, rewardElementData, questIndex, step, reqProgress);
						missionMultipleStepsRewardComponent.name = "MultipleStepsReward_" + i;
						Vespa_Unity_Transform_Extension_Method.ParentResetTM(missionMultipleStepsRewardComponent.transform, MultipleStepsRewardLayout.RewardListRoot.transform);
						missionMultipleStepsRewardComponent.transform.localPosition = new Vector3((i + 1) * (int)num2, 0f, 0f);
						_rewardMultipleStepsComponentList.Add(missionMultipleStepsRewardComponent);
					}
				}
			}
		}
	}

	public void SetMultipleStepsProgressBar(long totalAchieved, int lastStep, int totalStep, long curStepProgress = 0L, long curStepReqProgress = 0L)
	{
		if (_progressBarLayout != null)
		{
			bool flag = lastStep == totalStep;
			_progressBarLayout.SetProgressBarVisible(flag, cleared: false);
			_progressBarLayout.SetProgressLabel(totalAchieved, totalAchieved);
			if (!flag)
			{
				float num = 1f / (float)totalStep;
				float num2 = num * (float)lastStep;
				float num3 = ((curStepReqProgress > 0) ? ((float)curStepProgress / (float)curStepReqProgress) : 0f);
				float progressBarLength = num2 + num * num3;
				_progressBarLayout.SetProgressBarLength(progressBarLength);
			}
		}
	}

	public void SetRewardIconCompleteStatus(bool isComplete)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rewardItemIconComponentList); i++)
		{
			ItemIconComponent itemIconComponent = _rewardItemIconComponentList[i];
			if (!(itemIconComponent == null))
			{
				itemIconComponent.SetIconGrayScale(isComplete);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_rewardCurrencyIconComponentList); j++)
		{
			CurrencyIconComponent currencyIconComponent = _rewardCurrencyIconComponentList[j];
			if (!(currencyIconComponent == null))
			{
				currencyIconComponent.SetIconGrayScale(isComplete);
			}
		}
	}

	public void RefreshMultipleStepsRewardIconStatus(long lastStep, long progress)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rewardMultipleStepsComponentList); i++)
		{
			MissionMultipleStepsRewardComponent missionMultipleStepsRewardComponent = _rewardMultipleStepsComponentList[i];
			if (!(missionMultipleStepsRewardComponent == null))
			{
				bool cleared = progress >= missionMultipleStepsRewardComponent.ReqProgress;
				bool complete = lastStep >= missionMultipleStepsRewardComponent.Step;
				missionMultipleStepsRewardComponent.Refresh(cleared, complete);
			}
		}
	}

	private void SetRewardGrid(Action onAction)
	{
		ClearSingleStepRewardIconComponent();
		Vespa_System_Action_Extension_Method.fire(onAction);
		bool visibleRoot = __Extension_Method_ICollection__.COUNT(_rewardItemIconComponentList) > 0 || __Extension_Method_ICollection__.COUNT(_rewardCurrencyIconComponentList) > 0;
		SingleStepRewardLayout.SetVisibleRoot(visibleRoot);
		UIHelper.RepositionGrid(SingleStepRewardLayout.RewardListGrid);
	}

	public void OnClickGetReward()
	{
		if (_iListener != null)
		{
			_iListener.OnClickGetReward();
		}
	}

	public void OnClickGetAllReward()
	{
		if (_iListener != null)
		{
			_iListener.OnClickGetAllReward();
		}
	}

	void MissionMultipleStepsRewardComponent.IListener.OnClickRewardItem()
	{
		if (_iListener != null)
		{
			_iListener.OnClickGetReward();
		}
	}

	private void InitLayout(MissionCategoryData missionCategoryData)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(LayoutList); i++)
		{
			TopViewLayout topViewLayout = LayoutList[i];
			if (topViewLayout != null)
			{
				if (missionCategoryData != null && topViewLayout.LayoutType == missionCategoryData.TopViewLayoutType)
				{
					_Layout = topViewLayout;
					_Layout.SetVisibleRoot(visible: true);
				}
				else
				{
					topViewLayout.SetVisibleRoot(visible: false);
				}
			}
		}
		if (_Layout != null)
		{
			bool flag = !String_Extension_Method.IsNullOrEmpty(missionCategoryData.SubCategoryDescLocalKey);
			if (flag)
			{
				SetNoticeMessage(missionCategoryData.SubCategoryDescLocalKey);
			}
			bool showRemainTime = false;
			if (missionCategoryData.MissionType == EMissionType.Special || missionCategoryData.MissionType == EMissionType.Special2)
			{
				showRemainTime = true;
			}
			UIHelper.EnableGameObject(LayoutRoot, enable: true);
			_Layout.SetDescVisible(flag, showRemainTime);
		}
	}

	private void InitButtonGroup(EMissionTopViewButtonGroup buttonGroupType)
	{
		buttonGroupType = SprkMissionClaimAll.ButtonGroup(buttonGroupType);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RewardButtonGroupList); i++)
		{
			TopViewRewardButtonGroup topViewRewardButtonGroup = RewardButtonGroupList[i];
			if (topViewRewardButtonGroup != null)
			{
				if (topViewRewardButtonGroup.ButtonGroupType == buttonGroupType)
				{
					_buttonGroup = topViewRewardButtonGroup;
					_buttonGroup.SetVisibleRoot(visible: true);
				}
				else
				{
					topViewRewardButtonGroup.SetVisibleRoot(visible: false);
				}
			}
		}
		UIHelper.EnableGameObject(ButtonRoot, buttonGroupType != EMissionTopViewButtonGroup.None);
	}

	private void InitProgressBar(EMissionTopViewRewardType rewardType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ProgressBarLayoutList); i++)
		{
			TopViewProgressBarLayout topViewProgressBarLayout = ProgressBarLayoutList[i];
			if (topViewProgressBarLayout != null)
			{
				if (topViewProgressBarLayout.RewardType == rewardType)
				{
					_progressBarLayout = topViewProgressBarLayout;
					_progressBarLayout.SetVisibleRoot(visible: true);
				}
				else
				{
					topViewProgressBarLayout.SetVisibleRoot(visible: false);
				}
			}
		}
		UIHelper.EnableGameObject(ProgressBarRoot, rewardType != EMissionTopViewRewardType.None);
	}

	private void InitRewardLayout(EMissionTopViewRewardType rewardType)
	{
		SingleStepRewardLayout.SetVisibleRoot(rewardType == EMissionTopViewRewardType.SingleStep);
		MultipleStepsRewardLayout.SetVisibleRoot(rewardType == EMissionTopViewRewardType.MultipleSteps);
		UIHelper.EnableGameObject(RewardRoot, rewardType != EMissionTopViewRewardType.None);
	}

	private void ClearSingleStepRewardIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
		FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
	}

	private void ClearMultipleStepsRewardIconComponent()
	{
		FactoryBase.ReleaseAndClearList(_rewardMultipleStepsComponentList);
	}
}
