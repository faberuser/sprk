using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.Mission;

public class CommonNodeWindow : BaseWindow, MissionNodeWindowTopView2.IListener, MissionSubCategoryComponent.IListener
{
	public enum TopViewType
	{
		Reward,
		RemainTime,
		RewardRemainTimeBoth,
		RewardNoticeLocal
	}

	public interface IListner
	{
		void OnClickNode(MissionComponentData data);

		void RequestReward(MissionComponentData data);

		void RequestAllGetReward(List<int> achievementList, List<int> subQuestList);

		void CloseNodeWindow();

		void EventRemainTimeOut();
	}

	protected static int MAX_COUNT_VIEW_SUBCATEGORY = 5;

	[Header("[CommonNodeWindow]")]
	public UIScrollGrid UIScrollGridSubCategory;

	public Transform Transform_DirectionButton;

	public UIBundleLoader UIBundleLoader;

	public Transform TopViewParent;

	protected IListner _listener;

	protected bool _isTimeOut;

	protected MissionNodeWindowTopView2 _newTopView;

	protected List<MissionSubCategoryComponent> _listSubCategoryComp = new List<MissionSubCategoryComponent>();

	private float _remainTimeUpdate;

	private List<SubQuestData> _subQuestDataList = new List<SubQuestData>();

	public MissionCategoryData SelectedMissionCategoryData { get; set; }

	public MissionComponentData RewardElementData { get; set; }

	public virtual void Clear()
	{
		_subQuestDataList.Clear();
		FactoryBase.ReleaseAndClearList(_listSubCategoryComp);
	}

	public override void Close()
	{
		_isTimeOut = false;
		if (_newTopView != null && _newTopView.IsOpen())
		{
			_newTopView.Close();
		}
		if (_listener != null)
		{
			_listener.CloseNodeWindow();
		}
		base.Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_remainTimeUpdate += Time.deltaTime;
		if (_remainTimeUpdate > 1f)
		{
			_remainTimeUpdate = 0f;
			SetupRemainTime();
		}
	}

	public void SetupWindowBG()
	{
		if (SelectedMissionCategoryData != null)
		{
			UIHelper.SetBundleTexture(UIBundleLoader, SelectedMissionCategoryData.UIBackgroundPath);
		}
	}

	public virtual void Refresh()
	{
		RefreshRewardUi();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listSubCategoryComp); i++)
		{
			MissionSubCategoryComponent missionSubCategoryComponent = _listSubCategoryComp[i];
			if (!(missionSubCategoryComponent == null))
			{
				missionSubCategoryComponent.Refresh();
			}
		}
	}

	public void SetupTopView()
	{
		if (SelectedMissionCategoryData != null && !(_newTopView == null))
		{
			_newTopView.Open(this, TopViewParent, SelectedMissionCategoryData);
			SetupRewardUi();
			SetupRemainTime();
		}
	}

	private void SetupRemainTime()
	{
		if (!(_newTopView == null) && SelectedMissionCategoryData != null && SingletonAuto<MissionStatus>.instance.TryGetRemainTime(SelectedMissionCategoryData, out var remainTimeSpan))
		{
			bool flag = remainTimeSpan <= TimeSpan.Zero;
			if (!_isTimeOut && flag && _listener != null)
			{
				_listener.EventRemainTimeOut();
			}
			_isTimeOut = flag;
			_newTopView.SetRemainTime(remainTimeSpan);
		}
	}

	private void SetupRewardUi()
	{
		if (SelectedMissionCategoryData == null || _newTopView == null)
		{
			return;
		}
		int rewardQuestIndex = SelectedMissionCategoryData.RewardQuestIndex;
		string rewardQuestType = SelectedMissionCategoryData.RewardQuestType;
		RewardElementData = GetRewardElementData(rewardQuestIndex, rewardQuestType);
		if (RewardElementData == null)
		{
			return;
		}
		AchievementData data = RewardElementData.GetData<AchievementData>();
		if (data != null)
		{
			_newTopView.SetAchievementReward(data);
			RefreshAchievementRewardUi(data);
			return;
		}
		SubQuestData data2 = RewardElementData.GetData<SubQuestData>();
		if (data2 != null)
		{
			_subQuestDataList.Clear();
			for (SubQuestData subQuestData = data2; subQuestData != null; subQuestData = subQuestData.GetNextData())
			{
				_subQuestDataList.Add(subQuestData);
			}
			if (__Extension_Method_ICollection__.COUNT(_subQuestDataList) > 1)
			{
				_newTopView.SetMultipleStepsSubQuestReward(_subQuestDataList);
				RefreshMultipleStepsSubQuestRewardUi(_subQuestDataList);
			}
			else
			{
				_newTopView.SetSingleSubQuestReward(data2);
				RefreshSingleStepSubQuestRewardUi(data2);
			}
		}
	}

	private void RefreshRewardUi()
	{
		if (RewardElementData == null)
		{
			return;
		}
		AchievementData data = RewardElementData.GetData<AchievementData>();
		if (data != null)
		{
			RefreshAchievementRewardUi(data);
			return;
		}
		SubQuestData data2 = RewardElementData.GetData<SubQuestData>();
		if (data2 != null)
		{
			if (__Extension_Method_ICollection__.COUNT(_subQuestDataList) > 1)
			{
				RefreshMultipleStepsSubQuestRewardUi(_subQuestDataList);
			}
			else
			{
				RefreshSingleStepSubQuestRewardUi(data2);
			}
		}
	}

	private void RefreshAchievementRewardUi(AchievementData achievementData)
	{
		GetAchievementState(achievementData.Index, out var cleared, out var completed);
		SingletonAuto<AchievementChecker>.instance.GetAchievementAndRequirement(achievementData.Index, out var curProgress, out var requirement, out var _);
		_newTopView.SetGetRewardButton(cleared, completed);
		_newTopView.SetProgressBar(curProgress, requirement, completed, cleared);
		_newTopView.SetRewardIconCompleteStatus(completed);
	}

	private void RefreshSingleStepSubQuestRewardUi(SubQuestData subQuestData)
	{
		GetSubQuestState(subQuestData, out var cleared, out var completed);
		long achieved = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestInfoByQuestIndex(subQuestData.QuestIndex)?.Progress ?? 0;
		int reqProgress = subQuestData.GetReqProgress();
		_newTopView.SetGetRewardButton(cleared, completed);
		_newTopView.SetProgressBar(achieved, reqProgress, completed, cleared);
		_newTopView.SetRewardIconCompleteStatus(completed);
	}

	private void RefreshMultipleStepsSubQuestRewardUi(List<SubQuestData> subQuestDataList)
	{
		if (__Extension_Method_ICollection__.COUNT(subQuestDataList) != 0)
		{
			SubQuestData subQuestData = (Vespa_Generic_IList_Extension_Method.IsValidIndex(subQuestDataList, 0) ? subQuestDataList[0] : null);
			if (subQuestData != null)
			{
				SubQuestInfo subQuestInfoByQuestIndex = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestInfoByQuestIndex(subQuestData.QuestIndex);
				long num = subQuestInfoByQuestIndex?.Progress ?? 0;
				int num2 = subQuestInfoByQuestIndex?.LastStep ?? 0;
				RefreshMultipleStepsProgressBar(num2, num, subQuestDataList);
				_newTopView.RefreshMultipleStepsRewardIconStatus(num2, num);
			}
		}
	}

	private void RefreshMultipleStepsProgressBar(int lastStep, long totalAchieved, List<SubQuestData> subQuestDataList)
	{
		int num = __Extension_Method_ICollection__.COUNT(subQuestDataList);
		if (num == 0)
		{
			return;
		}
		if (lastStep >= num)
		{
			SubQuestData subQuestData = subQuestDataList.Find((SubQuestData questData) => questData.Step == lastStep);
			if (subQuestData != null)
			{
				int reqProgress = subQuestData.GetReqProgress();
				_newTopView.SetMultipleStepsProgressBar(reqProgress, lastStep, num, 0L, 0L);
			}
			return;
		}
		int lastStep2 = lastStep;
		int num2 = 0;
		int num3 = 0;
		for (int num4 = 0; num4 < __Extension_Method_ICollection__.COUNT(subQuestDataList); num4++)
		{
			SubQuestData subQuestData2 = subQuestDataList[num4];
			if (subQuestData2 != null)
			{
				int reqProgress2 = subQuestData2.GetReqProgress();
				if (totalAchieved < reqProgress2)
				{
					num3 = reqProgress2;
					break;
				}
				lastStep2 = subQuestData2.Step;
				num2 = reqProgress2;
			}
		}
		long curStepProgress = totalAchieved - num2;
		int num5 = num3 - num2;
		_newTopView.SetMultipleStepsProgressBar(totalAchieved, lastStep2, num, curStepProgress, num5);
	}

	public bool HasAvailableCategoryReward()
	{
		bool num = HasAvailableAchievementCategoryReward();
		bool result = HasAvailableSubQuestCategoryReward();
		if (!num)
		{
			return result;
		}
		return true;
	}

	public bool HasAvailableAchievementCategoryReward()
	{
		if (RewardElementData == null)
		{
			return false;
		}
		AchievementData data = RewardElementData.GetData<AchievementData>();
		if (data == null)
		{
			return false;
		}
		GetAchievementState(data.Index, out var cleared, out var completed);
		if (cleared)
		{
			return !completed;
		}
		return false;
	}

	public bool HasAvailableSubQuestCategoryReward()
	{
		if (RewardElementData == null)
		{
			return false;
		}
		SubQuestData data = RewardElementData.GetData<SubQuestData>();
		if (data == null)
		{
			return false;
		}
		GetSubQuestState(data, out var cleared, out var completed);
		if (cleared)
		{
			return !completed;
		}
		return false;
	}

	private void GetAchievementState(int achievementIndex, out bool cleared, out bool completed)
	{
		cleared = AchievementChecker.Cleared(achievementIndex);
		completed = AchievementChecker.Completed(achievementIndex);
	}

	private void GetSubQuestState(SubQuestData subQuestData, out bool cleared, out bool completed)
	{
		QuesetStatusType subQuestStatus = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(subQuestData);
		cleared = subQuestStatus == QuesetStatusType.Reward;
		completed = subQuestStatus == QuesetStatusType.Complete;
	}

	public void SetupSubCategoryButtons(int mainCategoryIndex, int subCategoryIndex, EUIPropType uIPropType)
	{
		MissionCategoryData missionCategoryData = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(mainCategoryIndex);
		List<MissionCategoryData> list = new List<MissionCategoryData>();
		while (missionCategoryData != null)
		{
			if (EnumTools.Parse(missionCategoryData.UIPropName, EUIPropType.Null) == uIPropType)
			{
				list.Add(missionCategoryData);
			}
			missionCategoryData = missionCategoryData.GetNextData();
		}
		int num = __Extension_Method_ICollection__.COUNT(list);
		if (num == 0)
		{
			return;
		}
		foreach (MissionCategoryData item in list)
		{
			CreateSubCategoryComponent(item);
		}
		SelectedMissionCategory(subCategoryIndex);
		bool flag = num > MAX_COUNT_VIEW_SUBCATEGORY;
		UIHelper.EnableGameObject(Transform_DirectionButton, flag);
		if (UIScrollGridSubCategory != null && flag)
		{
			UIScrollGridSubCategory.ScrollView.contentPivot = UIWidget.Pivot.TopLeft;
			UIScrollGridSubCategory.Grid.pivot = UIWidget.Pivot.TopLeft;
			UIHelper.GridOriginPositioning(UIScrollGridSubCategory.Grid);
		}
		else
		{
			UIScrollGridSubCategory.ScrollView.contentPivot = UIWidget.Pivot.Center;
			UIScrollGridSubCategory.Grid.pivot = UIWidget.Pivot.Center;
		}
		UIScrollGridSubCategory.RepositionGrid();
	}

	private void CreateSubCategoryComponent(MissionCategoryData data)
	{
		MissionSubCategoryComponent missionSubCategoryComponent = UIHelper.AddGridItem(UIScrollGridSubCategory, MissionSubCategoryComponent.PrefabAuto);
		missionSubCategoryComponent.OpenComponent(data, this);
		_listSubCategoryComp.Add(missionSubCategoryComponent);
	}

	private void SelectedMissionCategory(int subCategory = 0)
	{
		bool flag = false;
		if (subCategory > 0)
		{
			foreach (MissionSubCategoryComponent item in _listSubCategoryComp)
			{
				if (item.MissionCategoryData.SubCategoryIndex == subCategory && MissionManagement.IsOpenSubCategory(item.MissionCategoryData))
				{
					SelectedMissionCategoryData = item.MissionCategoryData;
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			foreach (MissionSubCategoryComponent item2 in _listSubCategoryComp)
			{
				if (MissionManagement.IsOpenSubCategory(item2.MissionCategoryData))
				{
					SelectedMissionCategoryData = item2.MissionCategoryData;
					break;
				}
			}
		}
		if (SelectedMissionCategoryData == null && __Extension_Method_ICollection__.COUNT(_listSubCategoryComp) > 0)
		{
			SelectedMissionCategoryData = _listSubCategoryComp[0].MissionCategoryData;
		}
	}

	protected bool SelectAny(Tuple<int, int> reward, Tuple<int, int> main, Tuple<int, int> sub)
	{
		if (!IsSelect(reward))
		{
			if (IsSelect(main))
			{
				return IsSelect(sub);
			}
			return false;
		}
		return true;
	}

	protected bool IsSelect(Tuple<int, int> element)
	{
		if (element.Item1 > 0)
		{
			return element.Item2 > 0;
		}
		return false;
	}

	protected void CurrentFocusingCalculation()
	{
		if (SelectedMissionCategoryData == null)
		{
			return;
		}
		Tuple<int, int> tuple = new Tuple<int, int>(0, 0);
		Tuple<int, int> tuple2 = new Tuple<int, int>(0, 0);
		Tuple<int, int> tuple3 = new Tuple<int, int>(0, 0);
		List<MissionCategoryData> missionCategoryList = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryList(SelectedMissionCategoryData.MainCategoryIndex);
		MissionCategoryData missionCategoryData = null;
		foreach (MissionCategoryData item in missionCategoryList)
		{
			if (!MissionManagement.IsOpenSubCategory(item))
			{
				continue;
			}
			List<MissionVisualData> missionVisualDataList = GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataList(item.MainCategoryIndex, item.SubCategoryIndex);
			if (missionVisualDataList == null)
			{
				continue;
			}
			missionCategoryData = item;
			foreach (MissionVisualData item2 in missionVisualDataList)
			{
				string questType = item2.QuestType;
				int questIndex = item2.QuestIndex;
				if (String_Extension_Method.IsNullOrEmpty(questType) || questIndex == 0)
				{
					continue;
				}
				if (questType.CompareTo("Achievement") == 0)
				{
					AchievementInfo achievementInfo = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(questIndex);
					if (achievementInfo != null && !AchievementChecker.Completed(achievementInfo))
					{
						if (AchievementChecker.Cleared(achievementInfo) && tuple3.Item1 == 0)
						{
							tuple3.Item1 = item2.MainCategoryIndex;
							tuple3.Item2 = item2.SubCategoryIndex;
							break;
						}
						if (tuple.Item1 == 0 && item2.SubStep == 0)
						{
							tuple.Item1 = item2.MainCategoryIndex;
							tuple.Item2 = item2.SubCategoryIndex;
						}
						else if (tuple2.Item1 == 0 && item2.SubStep > 0)
						{
							tuple2.Item1 = item2.MainCategoryIndex;
							tuple2.Item2 = item2.SubCategoryIndex;
						}
					}
				}
				else
				{
					if (questType.CompareTo("SubQuest") != 0)
					{
						continue;
					}
					SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(questIndex);
					switch (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(subQuestData))
					{
					case QuesetStatusType.Progress:
						if (tuple.Item1 == 0 && item2.SubStep == 0)
						{
							tuple.Item1 = item2.MainCategoryIndex;
							tuple.Item2 = item2.SubCategoryIndex;
						}
						else if (tuple2.Item1 == 0 && item2.SubStep > 0)
						{
							tuple2.Item1 = item2.MainCategoryIndex;
							tuple2.Item2 = item2.SubCategoryIndex;
						}
						break;
					case QuesetStatusType.Reward:
						if (tuple3.Item1 != 0)
						{
							break;
						}
						tuple3.Item1 = item2.MainCategoryIndex;
						tuple3.Item2 = item2.SubCategoryIndex;
						goto end_IL_0250;
					}
				}
				continue;
				end_IL_0250:
				break;
			}
			if (SelectAny(tuple3, tuple, tuple2))
			{
				break;
			}
		}
		if (IsSelect(tuple3))
		{
			SelectSubCategory(tuple3.Item1, tuple3.Item2);
		}
		else if (IsSelect(tuple))
		{
			SelectSubCategory(tuple.Item1, tuple.Item2);
		}
		else if (IsSelect(tuple2))
		{
			SelectSubCategory(tuple2.Item1, tuple2.Item2);
		}
		else if (missionCategoryData != null)
		{
			SelectSubCategory(missionCategoryData.MainCategoryIndex, missionCategoryData.SubCategoryIndex);
		}
	}

	private MissionComponentData GetRewardElementData(int rewardQuestIndex, string rewardQuestType)
	{
		if (String_Extension_Method.IsNullOrEmpty(rewardQuestType))
		{
			return null;
		}
		if (rewardQuestType.CompareTo("Achievement") == 0)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(rewardQuestIndex) == null)
			{
				return null;
			}
			return new MissionComponentData(rewardQuestIndex);
		}
		if (rewardQuestType.CompareTo("SubQuest") == 0)
		{
			SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(rewardQuestIndex);
			if (subQuestData == null)
			{
				return null;
			}
			return new MissionComponentData(subQuestData);
		}
		return null;
	}

	public virtual void SelectSubCategory(int mainCategory, int subCategory)
	{
	}

	protected virtual void SelectSubCategory(MissionCategoryData categoryData)
	{
	}

	public virtual void RequestAllReward()
	{
	}

	public void OnClickSubCategoryScrollLeft()
	{
		OnSubCategoryScrollMove(-1);
	}

	public void OnClickSubCategoryScrollRight()
	{
		OnSubCategoryScrollMove(1);
	}

	protected virtual void OnSubCategoryScrollMove(int direction)
	{
	}

	void MissionSubCategoryComponent.IListener.OnClick(MissionSubCategoryComponent comp)
	{
		SelectSubCategory(comp.MissionCategoryData);
	}

	void MissionNodeWindowTopView2.IListener.OnClickGetReward()
	{
		if (!_isTimeOut && RewardElementData != null && _listener != null)
		{
			_listener.RequestReward(RewardElementData);
		}
	}

	void MissionNodeWindowTopView2.IListener.OnClickGetAllReward()
	{
		RequestAllReward();
	}
}
