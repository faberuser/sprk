using System;
using System.Collections.Generic;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NShared.NContentsDefine;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.Mission;

public class MissionNodeWindowV3 : CommonNodeWindow, MissionNodeMap.IListner
{
	[Serializable]
	public class ExtraWidgetGroup
	{
		public UIButton Button_ReceiveAll;

		public UIButton Button_Purchase_PayShopProduct;

		public UIPanel Panel_LockBeforePurchase;

		public UILabel Label_LockBeforePurchase;

		public UITable Table_ExtraButtons;

		public void DisableAll()
		{
			UIHelper.EnableGameObject(Button_ReceiveAll, enable: false);
			UIHelper.EnableGameObject(Button_Purchase_PayShopProduct, enable: false);
			UIHelper.EnableGameObject(Panel_LockBeforePurchase, enable: false);
		}

		public void Setup_Purchase(ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 extraWidgetDefine)
		{
			if (extraWidgetDefine.PayShop_Product_Index == 0)
			{
				UIHelper.EnableGameObject(Button_Purchase_PayShopProduct, enable: false);
				UIHelper.EnableGameObject(Panel_LockBeforePurchase, enable: false);
				return;
			}
			if (MissionManagement.IsOpenSubCategory(extraWidgetDefine.MissionCategory.Main, extraWidgetDefine.MissionCategory.Sub))
			{
				UIHelper.EnableGameObject(Button_Purchase_PayShopProduct, enable: false);
				UIHelper.EnableGameObject(Panel_LockBeforePurchase, enable: false);
				return;
			}
			UIHelper.EnableGameObject(Button_Purchase_PayShopProduct, enable: true);
			UIHelper.EnableGameObject(Panel_LockBeforePurchase, enable: true);
			bool flag = true;
			if (!extraWidgetDefine.Prerequisite_MissionCategory.IsEmpty())
			{
				flag = MissionManagement.IsOpenSubCategory(extraWidgetDefine.Prerequisite_MissionCategory.Main, extraWidgetDefine.Prerequisite_MissionCategory.Sub);
			}
			if (flag)
			{
				string localize_Need_Purchase_PayShop_Product = ContentsDefine.ReturnUserJumping.Localize_Need_Purchase_PayShop_Product;
				UIHelper.SetLabelTextLocalizeDirect(Label_LockBeforePurchase, localize_Need_Purchase_PayShop_Product);
			}
			else
			{
				string localize_Prerequisite_MissionCategory_Not_Open = ContentsDefine.ReturnUserJumping.Localize_Prerequisite_MissionCategory_Not_Open;
				UIHelper.SetLabelTextLocalizeDirect(Label_LockBeforePurchase, localize_Prerequisite_MissionCategory_Not_Open);
			}
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/QuestIntegration/MissionNodeWindowV3";

	[Header("[MissionNodeWindowV3]")]
	public GameObject NodeListGridTop;

	public MissionNodeMap NodeMapTop;

	public UIScrollView UIScrollViewNodeMapTop;

	public GameObject NodeListGridTop2;

	public MissionNodeMap NodeMapTop2;

	public UIScrollView UIScrollViewNodeMapTop2;

	public UILabel Label_RemainTime;

	public ExtraWidgetGroup ExtraWidgets;

	private MissionNodeMap _currentNodeMap;

	private UIScrollView _currentUIScrollViewNodeMap;

	private float _lastUpdateTick_of_RemainTime = -9999f;

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
			SetupExtraWidgets();
		}, null);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _lastUpdateTick_of_RemainTime > 1f)
		{
			_lastUpdateTick_of_RemainTime = Time.time;
			string remainTimeString_Long = SingletonAuto<MissionStatus>.instance.GetRemainTimeString_Long(base.SelectedMissionCategoryData);
			if (String_Extension_Method.IsNullOrEmpty(remainTimeString_Long))
			{
				UIHelper.EnableGameObject(Label_RemainTime, enable: false);
				return;
			}
			UIHelper.EnableGameObject(Label_RemainTime, enable: true);
			UIHelper.SetLabelText(Label_RemainTime, remainTimeString_Long);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		_currentNodeMap.Refresh();
		SetupSelectMarkSubCategory();
		SetupExtraWidgets();
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
		if (_isTimeOut)
		{
			SingletonAuto<MissionStatus>.instance.OpenTimeoutPopup(delegate
			{
				Close();
			});
		}
		else if (_listener != null)
		{
			_listener.OnClickNode(data);
		}
	}

	public override void SelectSubCategory(int mainCategory, int subCategory)
	{
		if (_isTimeOut)
		{
			SingletonAuto<MissionStatus>.instance.OpenTimeoutPopup(delegate
			{
				Close();
			});
			return;
		}
		base.SelectSubCategory(mainCategory, subCategory);
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(mainCategory, subCategory);
		base.SelectedMissionCategoryData = null;
		SelectSubCategory(data);
	}

	protected override void SelectSubCategory(MissionCategoryData categoryData)
	{
		if (_isTimeOut)
		{
			SingletonAuto<MissionStatus>.instance.OpenTimeoutPopup(delegate
			{
				Close();
			});
		}
		else if (categoryData != null)
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
			SetupExtraWidgets();
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

	private void SetupExtraWidgets()
	{
		if (base.SelectedMissionCategoryData == null)
		{
			ExtraWidgets.DisableAll();
			return;
		}
		ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 extraWidget_in_MissionNodeWindowV = ContentsDefine.ReturnUserJumping.GetExtraWidget_in_MissionNodeWindowV3(base.SelectedMissionCategoryData.MainCategoryIndex, base.SelectedMissionCategoryData.SubCategoryIndex);
		if (extraWidget_in_MissionNodeWindowV == null)
		{
			ExtraWidgets.DisableAll();
			return;
		}
		ExtraWidgets.Setup_Purchase(extraWidget_in_MissionNodeWindowV);
		GetList_AllAvailableRewards(out var achievementIndices, out var subQuestIndices);
		bool enable = !__Extension_Method_ICollection__.IsNullOrEmpty(achievementIndices) || !__Extension_Method_ICollection__.IsNullOrEmpty(subQuestIndices);
		UIHelper.EnableGameObject(ExtraWidgets.Button_ReceiveAll, extraWidget_in_MissionNodeWindowV.Show_Button_ReceiveAll);
		UIHelper.EnableButton(ExtraWidgets.Button_ReceiveAll, enable);
		if (ExtraWidgets.Table_ExtraButtons != null)
		{
			ExtraWidgets.Table_ExtraButtons.Reposition();
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

	private void GetList_AllAvailableRewards(out List<int> achievementIndices, out List<int> subQuestIndices)
	{
		if (_currentNodeMap != null)
		{
			achievementIndices = _currentNodeMap.GetAchievementRewardList();
			subQuestIndices = _currentNodeMap.GetSubQuestRewardList();
		}
		else
		{
			achievementIndices = new List<int>();
			subQuestIndices = new List<int>();
		}
		if (HasAvailableAchievementCategoryReward())
		{
			achievementIndices.Add(base.RewardElementData.Index);
		}
		if (HasAvailableSubQuestCategoryReward())
		{
			subQuestIndices.Add(base.RewardElementData.Index);
		}
	}

	public void OnClickAllReward()
	{
		RequestAllReward();
	}

	public override void RequestAllReward()
	{
		if (!_isTimeOut && _listener != null)
		{
			GetList_AllAvailableRewards(out var achievementIndices, out var subQuestIndices);
			_listener.RequestAllGetReward(achievementIndices, subQuestIndices);
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

	public void OnClick_Button_ReceiveAll()
	{
		if (_isTimeOut)
		{
			SingletonAuto<MissionStatus>.instance.OpenTimeoutPopup(delegate
			{
				Close();
			});
		}
		else
		{
			RequestAllReward();
		}
	}

	public void OnClick_Button_Purchase_PayShopProduct()
	{
		if (_isTimeOut)
		{
			SingletonAuto<MissionStatus>.instance.OpenTimeoutPopup(delegate
			{
				Close();
			});
		}
		else
		{
			if (base.SelectedMissionCategoryData == null)
			{
				return;
			}
			ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 extraWidgetDefine = ContentsDefine.ReturnUserJumping.GetExtraWidget_in_MissionNodeWindowV3(base.SelectedMissionCategoryData.MainCategoryIndex, base.SelectedMissionCategoryData.SubCategoryIndex);
			if (extraWidgetDefine == null || extraWidgetDefine.PayShop_Product_Index == 0 || MissionManagement.IsOpenSubCategory(extraWidgetDefine.MissionCategory.Main, extraWidgetDefine.MissionCategory.Sub))
			{
				return;
			}
			bool flag = true;
			if (!extraWidgetDefine.Prerequisite_MissionCategory.IsEmpty())
			{
				flag = MissionManagement.IsOpenSubCategory(extraWidgetDefine.Prerequisite_MissionCategory.Main, extraWidgetDefine.Prerequisite_MissionCategory.Sub);
			}
			if (flag)
			{
				SceneAutoBehaviour<PayShopManagement>.instance.Open_ReturnUserJumping_PayShopBuyPopup(extraWidgetDefine.PayShop_Product_Index);
				return;
			}
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect(ContentsDefine.ReturnUserJumping.Localize_Prerequisite_MissionCategory_Not_Open_Popup);
			PayShopMessagePopup.OpenYesOnly(text, stringDirect, delegate
			{
				SelectSubCategory(extraWidgetDefine.Prerequisite_MissionCategory.Main, extraWidgetDefine.Prerequisite_MissionCategory.Sub);
			});
		}
	}
}
