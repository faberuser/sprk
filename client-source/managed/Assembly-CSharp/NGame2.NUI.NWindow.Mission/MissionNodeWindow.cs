using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.Mission;

public class MissionNodeWindow : CommonNodeWindow, MissionNodeMap.IListner
{
	public const string PrefabPath = "Resources/Prefab2/Window/QuestIntegration/MissionNodeWindow";

	[Header("[MissionNodeWindow]")]
	public GameObject NodeListGridTop;

	public MissionNodeMap NodeMapTop;

	public UIScrollView UIScrollViewNodeMapTop;

	public GameObject NodeListGridTop2;

	public MissionNodeMap NodeMapTop2;

	public UIScrollView UIScrollViewNodeMapTop2;

	private MissionNodeMap _currentNodeMap;

	private UIScrollView _currentUIScrollViewNodeMap;

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
			MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(mainCategory, (subCategory == 0) ? 1 : subCategory);
			EUIPropType uIPropType = EUIPropType.Node;
			if (data != null)
			{
				uIPropType = EnumTools.Parse(data.UIPropName, EUIPropType.Node);
			}
			SetupSubCategoryButtons(mainCategory, subCategory, uIPropType);
			if (subCategory <= 0)
			{
				CurrentFocusingCalculation();
			}
			else
			{
				SelectSubCategory(mainCategory, subCategory);
			}
			SetupSelectMarkSubCategory();
			SetupTopView();
			SetupWindowBG();
		}, null);
	}

	public override void Refresh()
	{
		base.Refresh();
		_currentNodeMap.Refresh();
		SetupSelectMarkSubCategory();
	}

	private void ScrollReposition(Vector3 target)
	{
		Vector3 vector = new Vector3(target.x, target.y);
		if (_currentUIScrollViewNodeMap.customMovement.y == 0f)
		{
			vector.y = _currentUIScrollViewNodeMap.panel.clipOffset.y;
		}
		Vector3 pos = _currentUIScrollViewNodeMap.panel.cachedTransform.localPosition - vector;
		SpringPanel.Begin(_currentUIScrollViewNodeMap.panel.cachedGameObject, pos, 1000f).onFinished = delegate
		{
			_currentUIScrollViewNodeMap.RestrictWithinBounds(instant: true);
		};
	}

	void MissionNodeMap.IListner.ScrollViewCustomMovement(int x, int y)
	{
		_currentUIScrollViewNodeMap.customMovement = new Vector2(1f, 1f);
		_currentUIScrollViewNodeMap.ResetPosition();
		_currentUIScrollViewNodeMap.customMovement = new Vector2(x, y);
	}

	void MissionNodeMap.IListner.OnClick(MissionComponentData data)
	{
		if (!_isTimeOut && _listener != null)
		{
			_listener.OnClickNode(data);
		}
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
			else if (base.SelectedMissionCategoryData.MainCategoryIndex == categoryData.MainCategoryIndex && base.SelectedMissionCategoryData.SubCategoryIndex == categoryData.SubCategoryIndex)
			{
				return;
			}
			base.SelectedMissionCategoryData = categoryData;
			SetupSelectMarkSubCategory();
			title.Key = base.SelectedMissionCategoryData.MainCategoryLocalKey;
			title.GetLocalize();
			NodeMapTop.Clear();
			NodeMapTop2.Clear();
			SetupCurrentNodeMap();
			_currentNodeMap.CreateMap(this, base.SelectedMissionCategoryData.MainCategoryIndex, base.SelectedMissionCategoryData.SubCategoryIndex);
			if (_currentNodeMap.GetAwakenNodePosition() != Vector2.zero)
			{
				ScrollReposition(_currentNodeMap.GetAwakenNodePosition());
			}
			else
			{
				_currentUIScrollViewNodeMap.ResetPosition();
			}
			SetupTopView();
			SetupWindowBG();
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

	private void SetupCurrentNodeMap()
	{
		if (base.SelectedMissionCategoryData != null && (base.SelectedMissionCategoryData.IsSpecial2Mission() || base.SelectedMissionCategoryData.IsSpecial3Mission()))
		{
			_currentNodeMap = NodeMapTop2;
			_currentUIScrollViewNodeMap = UIScrollViewNodeMapTop2;
			UIHelper.EnableGameObject(NodeListGridTop, enable: false);
			UIHelper.EnableGameObject(NodeListGridTop2, enable: true);
		}
		else
		{
			_currentNodeMap = NodeMapTop;
			_currentUIScrollViewNodeMap = UIScrollViewNodeMapTop;
			UIHelper.EnableGameObject(NodeListGridTop, enable: true);
			UIHelper.EnableGameObject(NodeListGridTop2, enable: false);
		}
	}

	public void OnClickAllReward()
	{
		if (_isTimeOut || _listener == null)
		{
			return;
		}
		List<int> achievementRewardList = _currentNodeMap.GetAchievementRewardList();
		List<int> subQuestRewardList = _currentNodeMap.GetSubQuestRewardList();
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
			List<int> achievementRewardList = _currentNodeMap.GetAchievementRewardList();
			List<int> subQuestRewardList = _currentNodeMap.GetSubQuestRewardList();
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
		if (UIScrollGridSubCategory == null || base.SelectedMissionCategoryData == null)
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
