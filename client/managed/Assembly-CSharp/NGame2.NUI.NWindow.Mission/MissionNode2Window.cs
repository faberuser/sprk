using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.Mission;

public class MissionNode2Window : CommonNodeWindow, MissionNode2Map.IListner
{
	public const string PrefabPath = "Resources/Prefab2/Window/QuestIntegration/MissionNode2Window";

	[Header("[MissionNode2Window]")]
	public MissionNode2Map Node2Map;

	public UIScrollView UIScrollViewNodeMap;

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void OpenNodeWindow(int mainCategory, int subCategory, IListner listener, MissionNodeWindowTopView2 topView)
	{
		Clear();
		Open(delegate
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
			ChangeLayerRecursive(layerInfo);
			_listener = listener;
			_newTopView = topView;
			SetupSubCategoryButtons(mainCategory, subCategory, EUIPropType.Main);
			if (subCategory <= 0)
			{
				CurrentFocusingCalculation();
			}
			else
			{
				SelectSubCategory(mainCategory, subCategory);
			}
			SetupSelectMarkSubCategory();
			MoveGridPositionToSubCategory();
			SetupTopView();
			SetupWindowBG();
		}, null);
	}

	private void SetupAllRewardButton()
	{
		if (!(_newTopView == null))
		{
			List<int> achievementRewardList = Node2Map.GetAchievementRewardList();
			List<int> subQuestRewardList = Node2Map.GetSubQuestRewardList();
			int num = __Extension_Method_ICollection__.COUNT(achievementRewardList) + __Extension_Method_ICollection__.COUNT(subQuestRewardList);
			bool getAllRewardButton = HasAvailableCategoryReward() || num > 0;
			_newTopView.SetGetAllRewardButton(getAllRewardButton);
		}
	}

	private void SetupSelectMarkSubCategory()
	{
		if (base.SelectedMissionCategoryData == null)
		{
			return;
		}
		foreach (MissionSubCategoryComponent item in _listSubCategoryComp)
		{
			if (item.MissionCategoryData.SubCategoryIndex == base.SelectedMissionCategoryData.SubCategoryIndex)
			{
				item.Select();
			}
			else
			{
				item.Refresh();
			}
		}
	}

	void MissionNode2Map.IListner.OnClick(MissionComponentData data)
	{
		if (!_isTimeOut && _listener != null)
		{
			_listener.OnClickNode(data);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		Node2Map.Refresh();
		SetupSelectMarkSubCategory();
		SetupAllRewardButton();
	}

	private void ScrollReposition(Vector3 target)
	{
		UIScrollViewNodeMap.ResetPosition();
		Vector2 clipOffset = UIScrollViewNodeMap.panel.clipOffset;
		Vector3 vector = new Vector3(target.x, clipOffset.y, 0f);
		UIScrollViewNodeMap.MoveRelative(-vector);
		UIScrollViewNodeMap.RestrictWithinBounds(instant: true);
	}

	public override void SelectSubCategory(int mainCategory, int subCategory)
	{
		base.SelectSubCategory(mainCategory, subCategory);
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(mainCategory, subCategory);
		base.SelectedMissionCategoryData = null;
		SelectSubCategory(data);
	}

	protected override void SelectSubCategory(MissionCategoryData categoryData)
	{
		if (categoryData != null)
		{
			if (base.SelectedMissionCategoryData == null)
			{
				base.SelectedMissionCategoryData = categoryData;
			}
			else if (base.SelectedMissionCategoryData.SubCategoryIndex == categoryData.SubCategoryIndex)
			{
				return;
			}
			base.SelectedMissionCategoryData = categoryData;
			SetupSelectMarkSubCategory();
			title.Key = base.SelectedMissionCategoryData.MainCategoryLocalKey;
			title.GetLocalize();
			Node2Map.CreateMap(this, base.SelectedMissionCategoryData.MainCategoryIndex, base.SelectedMissionCategoryData.SubCategoryIndex);
			if (Node2Map.GetAwakenNodePosition() != Vector2.zero)
			{
				ScrollReposition(Node2Map.GetAwakenNodePosition());
			}
			else
			{
				UIScrollViewNodeMap.ResetPosition();
			}
			SetupTopView();
			SetupAllRewardButton();
			SetupWindowBG();
		}
	}

	public void OnClickAllReward()
	{
		if (_isTimeOut || _listener == null)
		{
			return;
		}
		List<int> achievementRewardList = Node2Map.GetAchievementRewardList();
		List<int> subQuestRewardList = Node2Map.GetSubQuestRewardList();
		if (HasAvailableCategoryReward() && base.RewardElementData != null)
		{
			if (base.RewardElementData.GetData<AchievementData>() != null)
			{
				achievementRewardList.Add(base.RewardElementData.Index);
			}
			else if (base.RewardElementData.GetData<SubQuestData>() != null)
			{
				subQuestRewardList.Add(base.RewardElementData.Index);
			}
		}
		_listener.RequestAllGetReward(achievementRewardList, subQuestRewardList);
	}

	public override void RequestAllReward()
	{
		if (!_isTimeOut && _listener != null)
		{
			List<int> achievementRewardList = Node2Map.GetAchievementRewardList();
			List<int> subQuestRewardList = Node2Map.GetSubQuestRewardList();
			if (HasAvailableAchievementCategoryReward())
			{
				achievementRewardList.Add(base.RewardElementData.Index);
			}
			if (HasAvailableSubQuestCategoryReward())
			{
				subQuestRewardList.Add(base.RewardElementData.Index);
			}
			_listener.RequestAllGetReward(achievementRewardList, subQuestRewardList);
		}
	}

	protected void MoveGridPositionToSubCategory()
	{
		if (UIScrollGridSubCategory == null || base.SelectedMissionCategoryData == null || base.SelectedMissionCategoryData.IsSpecialMissionType())
		{
			return;
		}
		foreach (MissionSubCategoryComponent item in _listSubCategoryComp)
		{
			if (item.MissionCategoryData.SubCategoryIndex == base.SelectedMissionCategoryData.SubCategoryIndex)
			{
				UIHelper.MoveGridPositionToSelectedItem(UIScrollGridSubCategory.Grid, item.transform.localPosition);
				UIScrollGridSubCategory.ScrollView.RestrictWithinBounds(instant: true);
				break;
			}
		}
	}

	protected override void OnSubCategoryScrollMove(int direction)
	{
		base.OnSubCategoryScrollMove(direction);
		if (UIScrollGridSubCategory == null || base.SelectedMissionCategoryData == null)
		{
			return;
		}
		foreach (MissionSubCategoryComponent comp in _listSubCategoryComp)
		{
			if (comp.MissionCategoryData.SubCategoryIndex == base.SelectedMissionCategoryData.SubCategoryIndex + direction)
			{
				UIHelper.CheckOutOfGridSightItem(UIScrollGridSubCategory.Grid, comp.transform.localPosition, delegate
				{
					UIHelper.MoveGridPositionToSelectedItem(UIScrollGridSubCategory.Grid, comp.transform.localPosition);
					UIScrollGridSubCategory.ScrollView.RestrictWithinBounds(instant: true);
				}, delegate
				{
					UIHelper.MoveGridPositionToSelectedItem(UIScrollGridSubCategory.Grid, comp.transform.localPosition);
					UIScrollGridSubCategory.ScrollView.RestrictWithinBounds(instant: true);
				});
				SelectSubCategory(comp.MissionCategoryData);
				break;
			}
		}
	}
}
