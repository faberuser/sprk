using System.Collections.Generic;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/UI/Reuse Scroll Grid (Vespa)")]
public class UIReuseScrollGrid : UIBaseGrid, IInspectorGuiDrawer, UIScrollView.ICallback
{
	public enum EFitToPivot
	{
		GridItemScale,
		PivotWidth,
		PivotHeight,
		PivotSize
	}

	public enum EResetPositionType
	{
		ResetToZero,
		Fit,
		DelayedFit
	}

	public enum EScrollMoveDirection
	{
		Left,
		Right,
		Up,
		Down
	}

	public static LocalStorageBool ForceEnableOutside = new LocalStorageBool(i_default: false, typeof(UIScrollGrid).FullName + "ForceEnableOutside");

	[HideInInspector]
	public UIPanel ClippingPanel;

	[HideInInspector]
	public UIGrid Grid;

	[HideInInspector]
	public UIScrollView ScrollView;

	[HideInInspector]
	public UIDragScrollView DragScrollView;

	[HideInInspector]
	public UIWidget PivotWidget;

	[HideInInspector]
	public Vector3 GridItemScale = Vector2.one;

	[HideInInspector]
	public EFitToPivot FitToPivot;

	[HideInInspector]
	public bool AutoFitScale = true;

	[HideInInspector]
	public bool DisableOutside = true;

	[HideInInspector]
	public EResetPositionType ResetPositionType;

	[HideInInspector]
	public int TriggerLine;

	[HideInInspector]
	public int ReuseCount;

	private List<UIReuseScrollGridInfo> _listReuseScrollGridInfo = new List<UIReuseScrollGridInfo>();

	private List<ReuseItemInfo> _listReuseItemInfo = new List<ReuseItemInfo>();

	private ReuseCalculator _calculator = new ReuseCalculator();

	private EScrollMoveDirection _scrollMoveDirection;

	private int _totalItemCount = 2000;

	private bool _init;

	private bool _disableOutsideValue
	{
		get
		{
			if (ForceEnableOutside.Value)
			{
				return false;
			}
			return DisableOutside;
		}
	}

	private void OnDestroy()
	{
	}

	private void Awake()
	{
		if (ScrollView != null)
		{
			ScrollView.Callback = this;
			ScrollView.onMoveScroll = ReuseItemRePosition;
		}
		if (Grid != null)
		{
			Grid.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (_disableOutsideValue && Grid != null)
		{
			Grid.DisableChildIfNotVisible();
		}
	}

	void UIScrollView.ICallback.OnUpdate(UIScrollView scrollView)
	{
		if (_disableOutsideValue && Grid != null)
		{
			Grid.DisableChildIfNotVisible();
		}
	}

	public UIReuseScrollGrid CreateReuseItem<T>(PrefabAuto<T> prefabAuto, List<T> list, int totalCount, UIScrollItemBehaviour.IListener listener) where T : Component, IReuseItemListenerSetter
	{
		_listReuseItemInfo.Clear();
		if (!_init)
		{
			_calculator.Setup(ClippingPanel, ScrollView, Grid);
			_init = true;
		}
		if (list == null)
		{
			list = new List<T>();
		}
		else
		{
			list.Clear();
		}
		_totalItemCount = totalCount;
		int num = 0;
		num = ((totalCount >= ReuseCount) ? ReuseCount : totalCount);
		for (int i = 0; i < num; i++)
		{
			T val = AddGridItem(prefabAuto);
			if (!(val == null))
			{
				val.SetupReuseItemListener(listener);
				list.Add(val);
				ReuseItemInfo reuseItemInfo = new ReuseItemInfo();
				reuseItemInfo.Setup(val.transform, _calculator);
				_listReuseItemInfo.Add(reuseItemInfo);
			}
		}
		_listReuseScrollGridInfo.Clear();
		if (num != 0)
		{
			AddUIReuseScrollGridInfo(0, totalCount - 1, num);
		}
		return this;
	}

	public UIReuseScrollGrid CreateNextReuseItem<T>(PrefabAuto<T> prefabAuto, List<T> list, int totalCount, UIScrollItemBehaviour.IListener listener) where T : Component, IReuseItemListenerSetter
	{
		if (list == null)
		{
			list = new List<T>();
		}
		int num = 0;
		num = ((totalCount >= ReuseCount) ? ReuseCount : totalCount);
		for (int i = 0; i < num; i++)
		{
			T val = AddGridItem(prefabAuto);
			if (!(val == null))
			{
				val.SetupReuseItemListener(listener);
				list.Add(val);
				ReuseItemInfo reuseItemInfo = new ReuseItemInfo();
				reuseItemInfo.Setup(val.transform, _calculator);
				_listReuseItemInfo.Add(reuseItemInfo);
			}
		}
		_totalItemCount += totalCount;
		if (num != 0)
		{
			int firstItemIndex = _totalItemCount - totalCount;
			int lastItemIndex = _totalItemCount - 1;
			AddUIReuseScrollGridInfo(firstItemIndex, lastItemIndex, num);
		}
		return this;
	}

	private void AddUIReuseScrollGridInfo(int firstItemIndex, int lastItemIndex, int createItemCount)
	{
		UIReuseScrollGridInfo uIReuseScrollGridInfo = new UIReuseScrollGridInfo();
		int listCount = __Extension_Method_ICollection__.COUNT(_listReuseItemInfo);
		uIReuseScrollGridInfo.Setup(firstItemIndex, lastItemIndex, createItemCount, listCount);
		_listReuseScrollGridInfo.Add(uIReuseScrollGridInfo);
	}

	private void ScrollGridFitReposition()
	{
		if (_calculator.IsValid())
		{
			float firstItemIndex = 0f;
			float lastItemIndex = 0f;
			_calculator.GetClippingPanelShowIndex(ClippingPanel, ScrollView, out firstItemIndex, out lastItemIndex, out _scrollMoveDirection);
			float fixedFirstItemIndex = 0f;
			float fixedLastItemIndex = 0f;
			_calculator.GetClippingPanelFixedIndex(3, _totalItemCount, ref firstItemIndex, ref lastItemIndex, ref fixedFirstItemIndex, ref fixedLastItemIndex);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listReuseScrollGridInfo); i++)
			{
				_listReuseScrollGridInfo[i].FitRepostion(_listReuseItemInfo, (int)fixedFirstItemIndex, (int)fixedLastItemIndex);
			}
		}
	}

	public void ReuseItemRePosition()
	{
		if (!_calculator.IsValid() || (ScrollView.currentMomentum.x == 0f && ScrollView.currentMomentum.y == 0f))
		{
			return;
		}
		float firstItemIndex = 0f;
		float lastItemIndex = 0f;
		if (!_calculator.GetClippingPanelShowIndex(ClippingPanel, ScrollView, out firstItemIndex, out lastItemIndex, out _scrollMoveDirection))
		{
			return;
		}
		float fixedFirstItemIndex = 0f;
		float fixedLastItemIndex = 0f;
		_calculator.GetClippingPanelFixedIndex(TriggerLine, _totalItemCount, ref firstItemIndex, ref lastItemIndex, ref fixedFirstItemIndex, ref fixedLastItemIndex);
		int maxPerLine = Grid.maxPerLine;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listReuseScrollGridInfo); i++)
		{
			UIReuseScrollGridInfo uIReuseScrollGridInfo = _listReuseScrollGridInfo[i];
			if (_scrollMoveDirection == EScrollMoveDirection.Left || _scrollMoveDirection == EScrollMoveDirection.Up)
			{
				if (uIReuseScrollGridInfo.CheckChangeFirstToLast(_listReuseItemInfo, (int)fixedLastItemIndex))
				{
					if (maxPerLine <= 1)
					{
						uIReuseScrollGridInfo.ChangeFirstToLast(_listReuseItemInfo, 1);
					}
					else
					{
						uIReuseScrollGridInfo.ChangeFirstToLast(_listReuseItemInfo, maxPerLine);
					}
				}
			}
			else if (uIReuseScrollGridInfo.CheckChangeLastToFirst(_listReuseItemInfo, (int)fixedFirstItemIndex))
			{
				if (maxPerLine <= 1)
				{
					uIReuseScrollGridInfo.ChangeLastToFirst(_listReuseItemInfo, 1);
				}
				else
				{
					uIReuseScrollGridInfo.ChangeLastToFirst(_listReuseItemInfo, maxPerLine);
				}
			}
		}
	}

	public override void RepositionGrid()
	{
		ResetPosition();
		if ((bool)Grid.GridAnimation)
		{
			Grid.PlayForwardAnimation();
		}
	}

	private void ReuseGridRepostion()
	{
		if (Grid != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listReuseScrollGridInfo); i++)
			{
				_listReuseScrollGridInfo[i].Repostion(_listReuseItemInfo);
			}
			if (Grid.keepWithinPanel)
			{
				Grid.ConstrainWithinPanel();
			}
		}
	}

	public void ResetScroll(bool playAnimation = false)
	{
		if (ScrollView != null)
		{
			if (!playAnimation)
			{
				ReuseGridRepostion();
				ScrollView.ResetPosition();
			}
			else
			{
				SpringPanel.Begin(ClippingPanel.cachedGameObject, Vector3.zero, 15f);
			}
		}
	}

	public override T AddGridItem<T>(PrefabAuto<T> prefab)
	{
		return AddGridItemInternal(prefab.Value.Factory, addFirst: false);
	}

	public T AddGridItem<T>(PrefabAuto<T> prefab, bool addFirst) where T : Component
	{
		return AddGridItemInternal(prefab.Value.Factory, addFirst);
	}

	public T AddGridItem<T>(Factory<T> prefabFactory, bool addFirst = false) where T : Component
	{
		return AddGridItemInternal(prefabFactory, addFirst);
	}

	public override void ClearGridItem<T>()
	{
		List<T> list = new List<T>();
		foreach (Transform item in Grid.transform)
		{
			T component = item.GetComponent<T>();
			if ((bool)component)
			{
				list.Add(component);
			}
			else
			{
				VespaDebug.LogError("Failed to find component of type=" + typeof(T).Name);
			}
		}
		foreach (T item2 in list)
		{
			FactoryBase.ReleaseComponent(item2);
		}
	}

	public override void RemoveGridItem<T>(T comp)
	{
		foreach (Transform item in Grid.transform)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(item.GetComponent<T>(), comp))
			{
				FactoryBase.SafeReleaseComponent(ref comp);
			}
		}
	}

	public override void ScrollTo(Component comp, float amount, float strength)
	{
		if (comp == null)
		{
			return;
		}
		UIPanel panel = ScrollView.panel;
		UIScrollView scrollView = ScrollView;
		if (!(panel == null) && !(scrollView == null))
		{
			Vector3 vector = -panel.cachedTransform.InverseTransformPoint(comp.transform.position);
			if (!scrollView.canMoveHorizontally)
			{
				vector.x = panel.cachedTransform.localPosition.x;
			}
			if (!scrollView.canMoveVertically)
			{
				vector.y = panel.cachedTransform.localPosition.y;
			}
			if (!Math.Approximately(amount, 0f))
			{
				Vector3 vector2 = ((ScrollView.movement == UIScrollView.Movement.Horizontal) ? Vector3.right : ((ScrollView.movement == UIScrollView.Movement.Vertical) ? Vector3.up : new Vector3(ScrollView.customMovement.x, ScrollView.customMovement.y)));
				vector += vector2 * amount;
			}
			if (Math.Approximately(strength, 0f))
			{
				Vector3 localPosition = panel.cachedGameObject.transform.localPosition;
				Vector3 relative = vector - localPosition;
				ScrollView.MoveRelative(relative);
				ScrollView.RestrictWithinBounds(instant: true);
			}
			else
			{
				SpringPanel.Begin(panel.cachedGameObject, vector, strength);
			}
		}
	}

	private void ResetPosition()
	{
		bool considerInactive = false;
		if (ScrollView != null)
		{
			considerInactive = ScrollView.considerInactive;
		}
		if (ResetPositionType == EResetPositionType.ResetToZero)
		{
			ReuseGridRepostion();
			if (ScrollView != null)
			{
				ScrollView.ResetPosition();
			}
		}
		else if (ResetPositionType == EResetPositionType.Fit)
		{
			ScrollGridFitReposition();
			if (Grid != null)
			{
				Grid.ConstrainWithinPanel(considerInactive);
			}
		}
		else
		{
			if (ResetPositionType != EResetPositionType.DelayedFit)
			{
				return;
			}
			UIGrid grid = Grid;
			SingletonUpdater<Executer>.instance.NextRun(delegate
			{
				ScrollGridFitReposition();
				if (grid != null)
				{
					grid.ConstrainWithinPanel(considerInactive);
				}
			});
		}
	}

	private T AddGridItemInternal<T>(Factory<T> prefabFactory, bool addFirst) where T : Component
	{
		using (new RecommendPanel(ClippingPanel))
		{
			if (prefabFactory == null)
			{
				return null;
			}
			if (Grid == null)
			{
				return null;
			}
			T val = prefabFactory.Create(Grid.transform, !_disableOutsideValue);
			UIWidget component = val.GetComponent<UIWidget>();
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(val.transform);
			if (FitToPivot == EFitToPivot.GridItemScale)
			{
				val.transform.localScale = GridItemScale;
			}
			else
			{
				UIWidget pivotWidget = GetPivotWidget();
				if (component == null || pivotWidget == null)
				{
					val.transform.localScale = GridItemScale;
				}
				else if (FitToPivot == EFitToPivot.PivotWidth)
				{
					val.transform.localScale = Vector3.one * ((float)pivotWidget.width / (float)Mathf.Max(1, component.width));
				}
				else if (FitToPivot == EFitToPivot.PivotHeight)
				{
					val.transform.localScale = Vector3.one * ((float)pivotWidget.height / (float)Mathf.Max(1, component.height));
				}
				else if (FitToPivot == EFitToPivot.PivotSize)
				{
					float num = (float)pivotWidget.width / (float)Mathf.Max(1, component.width);
					float num2 = (float)pivotWidget.height / (float)Mathf.Max(1, component.height);
					val.transform.localScale = new Vector3(num, num2, Mathf.Max(num, num2));
				}
				else
				{
					val.transform.localScale = GridItemScale;
				}
			}
			GetDragScrollView(val).scrollView = ScrollView;
			if (addFirst)
			{
				val.transform.SetAsFirstSibling();
			}
			return val;
		}
	}

	public T GetChild<T>(int index) where T : Component
	{
		if (Grid == null || Grid.transform.childCount <= index)
		{
			return null;
		}
		return Grid.transform.GetChild(index) as T;
	}

	private UIDragScrollView GetDragScrollView<T>(T item) where T : Component
	{
		if (item is IGridItem gridItem)
		{
			UIDragScrollView dragScrollView = gridItem.GetDragScrollView();
			if (dragScrollView != null)
			{
				return dragScrollView;
			}
		}
		return SingletonAuto<LinkedTransform<UIDragScrollView>>.instance.GetAuto(item.transform);
	}

	private UIWidget GetPivotWidget()
	{
		if (PivotWidget != null)
		{
			return PivotWidget;
		}
		return GetComponent<UIWidget>();
	}
}
