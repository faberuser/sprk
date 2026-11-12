using System;
using System.Collections;
using System.Collections.Generic;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/UI/Animation Scroll Grid (Vespa)")]
public class UIAnimationScrollGrid : UIBaseGrid, IInspectorGuiDrawer, UIScrollView.ICallback
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
	public bool AutoNumbering;

	[HideInInspector]
	public bool DisableOutside = true;

	[HideInInspector]
	public EResetPositionType ResetPositionType;

	[HideInInspector]
	public int FrontDummyCount;

	[HideInInspector]
	public int RearDummyCount;

	[HideInInspector]
	public bool SynchronizationWidgetHeight;

	[HideInInspector]
	public bool SynchronizationWidgetWidth;

	[HideInInspector]
	public bool SetMomentumMove = true;

	private List<UIWidget> FrontDummys = new List<UIWidget>();

	private List<UIWidget> RearDummys = new List<UIWidget>();

	public Action RepositionAfterCallback;

	private bool _syncronizationWidgetStartOn;

	private Coroutine _aniRepositionCo;

	private int _serialNumber;

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

	private bool _isSyncronization
	{
		get
		{
			if (!SynchronizationWidgetHeight)
			{
				return SynchronizationWidgetWidth;
			}
			return true;
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
			if (SetMomentumMove)
			{
				ScrollView.onMomentumMove = OnScrollMoveEnd;
			}
			else
			{
				ScrollView.onMomentumMove = null;
			}
		}
		InitDummyWidget(FrontDummyCount, FrontDummys, "frontDummy");
	}

	private void Start()
	{
		if (_isSyncronization && !_syncronizationWidgetStartOn)
		{
			_syncronizationWidgetStartOn = true;
			SynchronizationWidgetSize();
			if (Grid != null && Grid.GetGridChildCount() > 0)
			{
				RepositionGrid();
			}
		}
	}

	private void InitDummyWidget(int dummyCount, List<UIWidget> dummyList, string dummyName = "")
	{
		for (int i = 0; i < dummyCount; i++)
		{
			UIWidget item = CreateDummyWidget(dummyName);
			dummyList.Add(item);
		}
	}

	private void OnEnable()
	{
		if (_disableOutsideValue && Grid != null)
		{
			Grid.DisableChildIfNotVisible();
		}
	}

	private int GetFocusingIndex(int index)
	{
		if (index < FrontDummyCount)
		{
			index = FrontDummyCount;
		}
		int gridChildCount = Grid.GetGridChildCount();
		if (index >= gridChildCount - RearDummyCount)
		{
			index = gridChildCount - RearDummyCount - 1;
		}
		return index;
	}

	private void OnScrollMoveEnd()
	{
		if (!(ClippingPanel == null) && !(ScrollView == null) && !(Grid == null) && !(ScrollView.currentMomentum.magnitude > 0.005f))
		{
			ScrollView.currentMomentum = Vector3.zero;
			float x = ClippingPanel.clipOffset.x;
			float y = ClippingPanel.clipOffset.y;
			float cellWidth = Grid.cellWidth;
			float cellHeight = Grid.cellHeight;
			int num = 0;
			if (ScrollView.movement == UIScrollView.Movement.Horizontal)
			{
				num = Mathf.RoundToInt(x / Vespa_Float_Extension_Method.NoneZero(cellWidth));
				num = GetFocusingIndex(num);
				x = (float)(-num) * cellWidth;
				SpringPanel.Begin(ClippingPanel.cachedGameObject, new Vector3(x, y), 15f);
			}
			else if (ScrollView.movement == UIScrollView.Movement.Vertical)
			{
				num = Mathf.RoundToInt((0f - y) / Vespa_Float_Extension_Method.NoneZero(cellHeight));
				num = GetFocusingIndex(num);
				y = (float)num * cellHeight;
				SpringPanel.Begin(ClippingPanel.cachedGameObject, new Vector3(x, y), 15f);
			}
		}
	}

	protected virtual void ScrollViewOnUpdate(UIScrollView scrollView)
	{
		if (_disableOutsideValue && Grid != null)
		{
			Grid.DisableChildIfNotVisible();
		}
	}

	void UIScrollView.ICallback.OnUpdate(UIScrollView scrollView)
	{
		ScrollViewOnUpdate(scrollView);
	}

	private void SetRearDummy()
	{
		if (!(ScrollView == null) && !(ClippingPanel == null) && !(Grid == null) && Grid.UseGridAnimation && __Extension_Method_ICollection__.COUNT(RearDummys) == 0)
		{
			UIScrollView.Movement movement = ScrollView.movement;
			__Extension_Method_ICollection__.COUNT(RearDummys);
			int num = 0;
			switch (movement)
			{
			case UIScrollView.Movement.Horizontal:
				num = Mathf.CeilToInt(ClippingPanel.GetViewSize().x / Vespa_Float_Extension_Method.NoneZero(Grid.cellWidth));
				break;
			case UIScrollView.Movement.Vertical:
				num = Mathf.CeilToInt(ClippingPanel.GetViewSize().y / Vespa_Float_Extension_Method.NoneZero(Grid.cellHeight));
				break;
			default:
				return;
			}
			InitDummyWidget(RearDummyCount = num - (FrontDummyCount + 1), RearDummys, "RearDummy");
		}
	}

	private void SetGridPosition()
	{
		if (!(ScrollView == null) && !(ClippingPanel == null) && !(Grid == null) && Grid.UseGridAnimation)
		{
			UIScrollView.Movement movement = ScrollView.movement;
			Vector3 localPosition = Grid.transform.localPosition;
			_ = RearDummyCount;
			switch (movement)
			{
			case UIScrollView.Movement.Horizontal:
			{
				float num = (0f - ClippingPanel.GetViewSize().x) * 0.5f;
				num += (float)FrontDummyCount * Grid.cellWidth + Grid.cellWidth * 0.5f;
				localPosition.x = num;
				break;
			}
			case UIScrollView.Movement.Vertical:
				_ = ClippingPanel.GetViewSize().y * 5f;
				_ = FrontDummyCount;
				_ = Grid.cellHeight;
				_ = Grid.cellHeight;
				break;
			default:
				return;
			}
			if (Grid.transform.localPosition != localPosition)
			{
				Grid.transform.localPosition = localPosition;
			}
		}
	}

	public override void RepositionGrid()
	{
		if (!_isSyncronization || _syncronizationWidgetStartOn)
		{
			SetGridPosition();
			SetRearDummy();
			if (_aniRepositionCo != null)
			{
				VespaCoroutine.Stop(this, _aniRepositionCo);
				_aniRepositionCo = null;
			}
			RepositionDummyWidget();
			if (Grid != null)
			{
				Grid.Reposition();
			}
			ResetPosition();
			if (Grid != null && Grid.UseGridAnimation)
			{
				Grid.PlayForwardAnimation();
			}
			Vespa_System_Action_Extension_Method.fire(RepositionAfterCallback);
		}
	}

	public void ResetScroll(bool playAnimation = false)
	{
		if (ScrollView != null)
		{
			if (!playAnimation)
			{
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

	public void ScrollTo<T>(T comp, bool isRestrict = false) where T : Component
	{
		if (Grid == null || comp == null)
		{
			return;
		}
		float cellWidth = Grid.cellWidth;
		float cellHeight = Grid.cellHeight;
		List<Transform> childList = Grid.GetChildList();
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
		{
			Transform transform = childList[i];
			if (comp.gameObject == transform.gameObject)
			{
				num = i;
			}
		}
		Vector3 zero = Vector3.zero;
		_ = Grid.GetPanel().finalClipRegion;
		_ = Grid.GetPanel().finalClipRegion;
		if (ScrollView.movement == UIScrollView.Movement.Horizontal)
		{
			zero.x = (float)(-num) * cellWidth;
			BeginScroll(zero, isRestrict);
		}
		else if (ScrollView.movement == UIScrollView.Movement.Vertical)
		{
			zero.y = (float)num * cellHeight;
			BeginScroll(zero, isRestrict);
		}
	}

	private void BeginScroll(Vector3 offset, bool isRestrict)
	{
		if (isRestrict)
		{
			Vector3 localPosition = Grid.GetPanel().cachedGameObject.transform.localPosition;
			Vector3 relative = offset - localPosition;
			ScrollView.MoveRelative(relative);
			ScrollView.RestrictWithinBounds(instant: true);
		}
		else
		{
			SpringPanel.Begin(ClippingPanel.cachedGameObject, offset, 15f);
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
			if (ScrollView != null)
			{
				ScrollView.ResetPosition();
			}
		}
		else if (ResetPositionType == EResetPositionType.Fit)
		{
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
				if (grid != null)
				{
					grid.ConstrainWithinPanel(considerInactive);
				}
			});
		}
	}

	private string GetNextItemName(string oldName)
	{
		string result = ((oldName.IndexOf("-[No") < 0) ? (oldName += $"[No{_serialNumber}]") : $"{oldName}-[No{_serialNumber}]");
		_serialNumber++;
		return result;
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
			if (AutoNumbering)
			{
				val.name = GetNextItemName(val.name);
			}
			UIWidget uIWidget = SingletonAuto<LinkedTransform<UIWidget>>.instance.Get(val.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(val.transform);
			if (FitToPivot == EFitToPivot.GridItemScale)
			{
				val.transform.localScale = GridItemScale;
			}
			else
			{
				UIWidget pivotWidget = GetPivotWidget();
				if (uIWidget == null || pivotWidget == null)
				{
					val.transform.localScale = GridItemScale;
				}
				else if (FitToPivot == EFitToPivot.PivotWidth)
				{
					val.transform.localScale = Vector3.one * ((float)pivotWidget.width / (float)Mathf.Max(1, uIWidget.width));
				}
				else if (FitToPivot == EFitToPivot.PivotHeight)
				{
					val.transform.localScale = Vector3.one * ((float)pivotWidget.height / (float)Mathf.Max(1, uIWidget.height));
				}
				else if (FitToPivot == EFitToPivot.PivotSize)
				{
					float num = (float)pivotWidget.width / (float)Mathf.Max(1, uIWidget.width);
					float num2 = (float)pivotWidget.height / (float)Mathf.Max(1, uIWidget.height);
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
			RepositionDummyWidget();
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

	private IEnumerator coAnimateReposition(float timeLength, AnimationCurve posCurve)
	{
		List<Tuple<Transform, Vector3, Vector3>> animateList = Grid.CalcReposition();
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += UnitySystemTimer.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / Vespa_Float_Extension_Method.NoneZero(timeLength));
			num = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(posCurve, num, num);
			for (int i = 0; i < animateList.Count; i++)
			{
				Tuple<Transform, Vector3, Vector3> tuple = animateList[i];
				tuple.Item1.localPosition = Math.LerpOver(tuple.Item2, tuple.Item3, num);
			}
		}
		while (!(elapsedTime >= timeLength));
		Grid.Reposition();
	}

	private UIWidget GetPivotWidget()
	{
		if (PivotWidget != null)
		{
			return PivotWidget;
		}
		return SingletonAuto<LinkedTransform<UIWidget>>.instance.Get(base.transform);
	}

	public UIWidget CreateDummyWidget(string dummyName = "")
	{
		UIWidget uIWidget = NGUITools.AddWidget<UIWidget>(Grid.gameObject);
		uIWidget.name = dummyName;
		if (PivotWidget != null)
		{
			UIWidget pivotWidget = GetPivotWidget();
			uIWidget.SetRect(0f, 0f, pivotWidget.width, pivotWidget.height);
		}
		else if (Grid != null)
		{
			uIWidget.SetRect(0f, 0f, Grid.cellWidth, Grid.cellHeight);
		}
		return uIWidget;
	}

	public void RepositionDummyWidget()
	{
		foreach (UIWidget frontDummy in FrontDummys)
		{
			frontDummy.transform.SetAsFirstSibling();
		}
		foreach (UIWidget rearDummy in RearDummys)
		{
			rearDummy.transform.SetAsLastSibling();
		}
	}

	public bool SynchronizationWidgetSize()
	{
		UIWidget uIWidget = SingletonAuto<LinkedTransform<UIWidget>>.instance.Get(base.transform);
		if (uIWidget == null)
		{
			return false;
		}
		if (ClippingPanel == null)
		{
			return false;
		}
		uIWidget.Update();
		Vector4 baseClipRegion = ClippingPanel.baseClipRegion;
		bool result = false;
		if (SynchronizationWidgetHeight && baseClipRegion.w != (float)uIWidget.height)
		{
			baseClipRegion.w = uIWidget.height;
			ClippingPanel.baseClipRegion = baseClipRegion;
			if (DragScrollView != null)
			{
				BoxCollider component = DragScrollView.GetComponent<BoxCollider>();
				component.size = new Vector3(component.size.x, uIWidget.height, component.size.z);
			}
			result = true;
		}
		if (SynchronizationWidgetWidth && baseClipRegion.z != (float)uIWidget.width)
		{
			baseClipRegion.z = uIWidget.width;
			ClippingPanel.baseClipRegion = baseClipRegion;
			if (DragScrollView != null)
			{
				BoxCollider component2 = DragScrollView.GetComponent<BoxCollider>();
				component2.size = new Vector3(uIWidget.width, component2.size.y, component2.size.z);
			}
			result = true;
		}
		return result;
	}

	public void SetOnMomentumMove(UIScrollView.OnDragNotification dragNotification)
	{
		ScrollView.onMomentumMove = dragNotification;
	}

	public void SetOnDragFinished(UIScrollView.OnDragNotification dragNotification)
	{
		ScrollView.onDragFinished = dragNotification;
	}
}
