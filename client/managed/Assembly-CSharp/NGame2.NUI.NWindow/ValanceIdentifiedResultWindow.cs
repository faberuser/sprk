using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NComponent2.Nvalance;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceIdentifiedResultWindow : BaseCinemaWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceIdentifiedResultWindow";

	public Transform HideComponentRoot;

	public UIScrollGrid ScrollGrid;

	public ValanceIdentifiedComponent[] ResultCompoents;

	public GameObject SuccessRoot;

	public Animator AnimatorSuccess;

	public GameObject SingResultRoot;

	public Transform SingleItemIconRoot;

	public UILabel SingleResultLabel;

	public UIPanel[] SingleStatPanels;

	public UIScrollView[] ScrollViewSingleStats;

	public GameObject[] GoSingleDrags;

	public GameObject MultiResultRoot;

	public GameObject CloseButtonRoot;

	private ItemIconComponent _itemIconComponent;

	private List<StatListComponent> _listStatComponent = new List<StatListComponent>();

	private bool _isNodeTreeEnd;

	private bool _isSingleResult;

	public void OnOffRoot(bool isOn)
	{
		UIHelper.EnableGameObject(SuccessRoot, isOn);
		UIHelper.EnableGameObject(CloseButtonRoot, isOn);
		UIHelper.EnableGameObject(SingResultRoot, isOn && _isSingleResult);
		UIHelper.EnableGameObject(MultiResultRoot, isOn && !_isSingleResult);
	}

	public void PanelSort()
	{
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>(includeInactive: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
		{
			componentsInChildren[i].depth = i;
		}
	}

	public void Open(EquipItemInfo[] result)
	{
		_isSingleResult = __Extension_Method_ICollection__.COUNT(result) == 1;
		OnOffRoot(isOn: false);
		PanelSort();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(result))
		{
			Close();
			return;
		}
		base.Open(delegate
		{
			if (_isSingleResult)
			{
				EquipItemInfo equipItemInfo = result[0];
				SetSingleItemIcon(equipItemInfo);
				SetSingleResultName(equipItemInfo);
				SetSingleOptionStat(equipItemInfo, equipItemInfo.EquipItemData);
				SetSingleExtraOptionStat(equipItemInfo);
			}
			else
			{
				SetMultiScrollViewOrigin(__Extension_Method_ICollection__.COUNT(result));
				SetMultiResultItems(result);
			}
		}, null);
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

	public void SetSingleItemIcon(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null && !(SingleItemIconRoot == null))
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(SingleItemIconRoot);
			if (!(_itemIconComponent == null))
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
				_itemIconComponent.Open(equipItemInfo, null);
			}
		}
	}

	public void SetSingleResultName(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			UIHelper.SetLabelText(SingleResultLabel, ItemNameBuilder.GetItemName(equipItemInfo.ItemData, equipItemInfo));
		}
	}

	public T GetObj<T>(T[] array, int index) where T : class
	{
		if (__Extension_Method_ICollection__.COUNT(array) <= index)
		{
			return null;
		}
		return array[index];
	}

	private UIPanel GetStatPanel(int index)
	{
		return GetObj(SingleStatPanels, index);
	}

	private UIScrollView GetStatScrollView(int index)
	{
		return GetObj(ScrollViewSingleStats, index);
	}

	private GameObject GetDragGo(int index)
	{
		return GetObj(GoSingleDrags, index);
	}

	public void AddRepositon(StatListComponent comp, int index)
	{
		if (comp == null || comp.RootTable == null)
		{
			return;
		}
		UIPanel panel = GetStatPanel(index);
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

	public void SetSingleOptionStat(EquipItemInfo equipItemInfo, EquipItemData equipItemData)
	{
		UIScrollView statScrollView = GetStatScrollView(0);
		UIPanel statPanel = GetStatPanel(0);
		if (!(statPanel == null) && !(statScrollView == null))
		{
			StatListComponent statListComponent = StatListComponent.Create(statPanel, 0, isValance: true);
			StatListComponent.AddOptionStat(statListComponent, equipItemData, equipItemInfo);
			_listStatComponent.Add(statListComponent);
			AddRepositon(statListComponent, 0);
		}
	}

	public void SetSingleExtraOptionStat(EquipItemInfo equipItemInfo)
	{
		UIScrollView statScrollView = GetStatScrollView(1);
		UIPanel statPanel = GetStatPanel(1);
		if (!(statPanel == null) && !(statScrollView == null))
		{
			StatListComponent statListComponent = StatListComponent.Create(statPanel, 0, isValance: true);
			StatListComponent.AddExtraOptionStat(statListComponent, equipItemInfo);
			_listStatComponent.Add(statListComponent);
			AddRepositon(statListComponent, 1);
		}
	}

	public void SetMultiScrollViewOrigin(int count)
	{
		UIWidget.Pivot contentPivot = UIWidget.Pivot.TopLeft;
		if (count < 4)
		{
			contentPivot = UIWidget.Pivot.Center;
		}
		if (ScrollGrid != null)
		{
			UIScrollView scrollView = ScrollGrid.ScrollView;
			if (scrollView != null)
			{
				scrollView.contentPivot = contentPivot;
			}
		}
	}

	public void SetMultiResultItems(EquipItemInfo[] results)
	{
		if (ScrollGrid == null || __Extension_Method_ICollection__.IsNullOrEmpty(ResultCompoents))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ResultCompoents); i++)
		{
			ValanceIdentifiedComponent valanceIdentifiedComponent = ResultCompoents[i];
			if (!(valanceIdentifiedComponent == null))
			{
				bool num = i < __Extension_Method_ICollection__.COUNT(results);
				Transform parent = (num ? ScrollGrid.Grid.transform : HideComponentRoot);
				if (num)
				{
					EquipItemInfo equipItemInfo = results[i];
					valanceIdentifiedComponent.Open(equipItemInfo);
				}
				valanceIdentifiedComponent.transform.SetParent(parent);
			}
		}
		UIHelper.RepositionGrid(ScrollGrid);
	}

	private void Release()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ResultCompoents); i++)
		{
			ValanceIdentifiedComponent valanceIdentifiedComponent = ResultCompoents[i];
			if (valanceIdentifiedComponent != null)
			{
				valanceIdentifiedComponent.Close();
			}
		}
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		FactoryBase.ReleaseAndClearList(_listStatComponent);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_isNodeTreeEnd = false;
		_isSingleResult = false;
		Release();
	}
}
