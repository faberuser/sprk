using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceCraftResultWindow : BaseCinemaWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceCraftResultWindow";

	public GameObject HideRoot;

	private bool _isNodeTreeEnd;

	public Transform IconRoot;

	public UILabel LabelItemName;

	public UILabel LabelMainStatType;

	public UILabel LabelMainStatValue;

	public UILabel LabelLevelLimit;

	public UIScrollView[] ScrollViews;

	public UIPanel[] ScrollViewPanels;

	public GameObject[] GoDrags;

	public GameObject SuccessRoot;

	private List<StatListComponent> _listStatComponent = new List<StatListComponent>();

	private ItemIconComponent _iconComponent;

	public void Open(EquipItemInfo result, bool isAdvanced)
	{
		if (isAdvanced)
		{
			OpenCustomCinema("ValanceCraftResultRestoreWindow", delegate
			{
				Init(result);
			}, null);
		}
		else
		{
			Open(delegate
			{
				Init(result);
			}, null);
		}
	}

	public void OnOffRoot(bool isOn)
	{
		UIHelper.EnableGameObject(HideRoot, isOn);
	}

	public void Init(EquipItemInfo equipItemInfo)
	{
		OnOffRoot(isOn: false);
		if (equipItemInfo == null)
		{
			Close();
			return;
		}
		EquipItemData equipItemData = equipItemInfo.EquipItemData;
		if (equipItemData == null)
		{
			Close();
			return;
		}
		SetItemIcon(equipItemInfo);
		SetItemName(equipItemInfo);
		SetMainStat(equipItemInfo, equipItemData);
		SetOptionStat(equipItemInfo, equipItemData);
		SetExtraOptionStat(equipItemInfo);
		SetSetBonustat(equipItemInfo, equipItemData);
	}

	public void SetItemIcon(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null && !(IconRoot == null))
		{
			_iconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconRoot);
			if (!(_iconComponent == null))
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_iconComponent.transform);
				_iconComponent.Open(equipItemInfo, null);
			}
		}
	}

	public void SetItemName(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			UIHelper.SetLabelText(LabelItemName, ItemNameBuilder.GetItemName(equipItemInfo.ItemData, equipItemInfo));
		}
	}

	public void SetMainStat(EquipItemInfo equipItemInfo, EquipItemData equipItemData)
	{
		long statValue = 0L;
		if (equipItemData.GetMainStat(out var statType, out statValue, equipItemInfo.Star, equipItemData.DetailIndex, equipItemInfo.Level))
		{
			UIHelper.EnableGameObject(LabelMainStatType, enable: true);
			UIHelper.EnableGameObject(LabelMainStatValue, enable: true);
			UIHelper.SetLabelText(LabelMainStatType, statType);
			UIHelper.SetLabelText(LabelMainStatValue, statValue);
		}
		else
		{
			UIHelper.EnableGameObject(LabelMainStatType, enable: false);
			UIHelper.EnableGameObject(LabelMainStatValue, enable: false);
		}
	}

	public void SetLeveLimit(EquipItemData equipItemData)
	{
		UIHelper.SetLabelText(LabelLevelLimit, (equipItemData.ReqLevel <= 0) ? 1 : equipItemData.ReqLevel);
	}

	public void AddRepositon(StatListComponent comp, int index)
	{
		if (comp == null || comp.RootTable == null)
		{
			return;
		}
		UIPanel panel = GetScrollViewPanel(index);
		GameObject goDrag = GetDragGo(index);
		if (panel == null || goDrag == null || comp.RootTable.onReposition != null)
		{
			return;
		}
		comp.RootTable.onReposition = delegate
		{
			int totalHeight = comp.GetTotalHeight();
			float num = panel.height;
			if (panel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				num -= panel.clipSoftness.y * 2f;
			}
			bool enable = num <= (float)totalHeight;
			UIHelper.EnableGameObject(goDrag, enable);
		};
	}

	public void SetOptionStat(EquipItemInfo equipItemInfo, EquipItemData equipItemData)
	{
		UIScrollView scrollView = GetScrollView(0);
		UIPanel scrollViewPanel = GetScrollViewPanel(0);
		if (!(scrollView == null) && !(scrollViewPanel == null))
		{
			StatListComponent statListComponent = StatListComponent.Create(scrollViewPanel, 0, isValance: true);
			if (statListComponent != null)
			{
				StatListComponent.AddOptionStat(statListComponent, equipItemData, equipItemInfo);
				_listStatComponent.Add(statListComponent);
				AddRepositon(statListComponent, 0);
			}
		}
	}

	public void SetExtraOptionStat(EquipItemInfo equipItemInfo)
	{
		UIScrollView scrollView = GetScrollView(1);
		UIPanel scrollViewPanel = GetScrollViewPanel(1);
		if (!(scrollView == null) && !(scrollViewPanel == null))
		{
			StatListComponent statListComponent = StatListComponent.Create(scrollViewPanel, 0, isValance: true);
			if (statListComponent != null)
			{
				StatListComponent.AddExtraOptionStat(statListComponent, equipItemInfo);
				_listStatComponent.Add(statListComponent);
				AddRepositon(statListComponent, 1);
			}
		}
	}

	public void SetSetBonustat(EquipItemInfo equipItemInfo, EquipItemData equipItemData)
	{
		UIScrollView scrollView = GetScrollView(2);
		UIPanel scrollViewPanel = GetScrollViewPanel(2);
		if (!(scrollView == null) && !(scrollViewPanel == null))
		{
			StatListComponent statListComponent = StatListComponent.Create(scrollViewPanel, 0, isValance: true);
			if (statListComponent != null)
			{
				StatListComponent.AddSetOptionStat(statListComponent, null, equipItemData, equipItemInfo);
				_listStatComponent.Add(statListComponent);
				AddRepositon(statListComponent, 2);
			}
		}
	}

	private UIScrollView GetScrollView(int index)
	{
		return GetObj(ScrollViews, index);
	}

	private UIPanel GetScrollViewPanel(int index)
	{
		return GetObj(ScrollViewPanels, index);
	}

	private GameObject GetDragGo(int index)
	{
		return GetObj(GoDrags, index);
	}

	private T GetObj<T>(T[] array, int index) where T : class
	{
		if (array == null || array.Length <= index)
		{
			return null;
		}
		return array[index];
	}

	protected override void NodeTreeOpenOnEnd()
	{
		_isNodeTreeEnd = true;
		OnOffRoot(isOn: true);
	}

	public override void OnSystemBackButton()
	{
		if (_isNodeTreeEnd)
		{
			base.OnSystemBackButton();
		}
	}

	private void Release()
	{
		FactoryBase.ReleaseAndClearList(_listStatComponent);
		FactoryBase.SafeReleaseComponent(ref _iconComponent);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Release();
		_isNodeTreeEnd = false;
	}
}
