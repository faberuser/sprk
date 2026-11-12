using System.Collections;
using System.Collections.Generic;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/UI/Scroll Grid (Vespa)")]
public class UIScrollGrid : UIBaseGrid, IInspectorGuiDrawer, UIScrollView.ICallback
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

	public enum EAutoMode
	{
		None,
		AutoResize,
		AutoAdjustMaxPerLine
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

	public EAutoMode AutoMode;

	private List<UIWidget> FrontDummys = new List<UIWidget>();

	private List<UIWidget> RearDummys = new List<UIWidget>();

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
		}
		InitDummyWidget(FrontDummyCount, FrontDummys, "frontDummy");
		InitDummyWidget(RearDummyCount, RearDummys, "rearDummy");
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

	public void RepositionGridWithFixableScroll(bool useFix = false)
	{
		if (ScrollView == null)
		{
			return;
		}
		UIProgressBar uIProgressBar = null;
		uIProgressBar = ((ScrollView.movement == UIScrollView.Movement.Horizontal) ? ScrollView.horizontalScrollBar : ((ScrollView.movement != UIScrollView.Movement.Vertical) ? null : ScrollView.verticalScrollBar));
		if (uIProgressBar == null)
		{
			RepositionGrid();
			return;
		}
		float value = uIProgressBar.value;
		RepositionGrid();
		if (useFix)
		{
			uIProgressBar.value = value;
		}
	}

	private void UpdateAutoMode()
	{
		if (AutoMode == EAutoMode.None || Grid == null || ClippingPanel == null)
		{
			return;
		}
		ClippingPanel.Update();
		ClippingPanel.UpdateAnchors();
		int maxPerLine = Grid.maxPerLine;
		Vector2 vector = new Vector2(Grid.cellWidth, Grid.cellHeight);
		Vector2 viewSize = ClippingPanel.GetViewSize();
		switch (AutoMode)
		{
		case EAutoMode.AutoResize:
		{
			int gridChildCount = Grid.GetGridChildCount();
			switch (Grid.arrangement)
			{
			case UIGrid.Arrangement.Vertical:
			{
				Grid.cellHeight = viewSize.y / (float)maxPerLine;
				for (int j = 0; j < gridChildCount; j++)
				{
					UIWidget component2 = Grid.GetChild(j).GetComponent<UIWidget>();
					if (component2 != null)
					{
						component2.height = (int)Grid.cellHeight;
					}
				}
				break;
			}
			case UIGrid.Arrangement.Horizontal:
			{
				Grid.cellWidth = viewSize.x / (float)maxPerLine;
				for (int i = 0; i < gridChildCount; i++)
				{
					UIWidget component = Grid.GetChild(i).GetComponent<UIWidget>();
					if (component != null)
					{
						component.width = (int)Grid.cellWidth;
					}
				}
				break;
			}
			}
			break;
		}
		case EAutoMode.AutoAdjustMaxPerLine:
		{
			Vector2 clipOffset = ClippingPanel.clipOffset;
			switch (Grid.arrangement)
			{
			case UIGrid.Arrangement.Vertical:
				Grid.maxPerLine = (int)viewSize.y / (int)vector.y;
				clipOffset.y += (viewSize.y - (float)Grid.maxPerLine * vector.y) * 0.5f;
				break;
			case UIGrid.Arrangement.Horizontal:
				Grid.maxPerLine = (int)viewSize.x / (int)vector.x;
				clipOffset.x -= (viewSize.x - (float)Grid.maxPerLine * vector.x) * 0.5f;
				break;
			}
			ClippingPanel.clipOffset = clipOffset;
			break;
		}
		}
	}

	public override void RepositionGrid()
	{
		if (!_isSyncronization || _syncronizationWidgetStartOn)
		{
			if (_aniRepositionCo != null)
			{
				VespaCoroutine.Stop(this, _aniRepositionCo);
				_aniRepositionCo = null;
			}
			UpdateAutoMode();
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
		return GetComponent<UIWidget>();
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
		UIWidget component = GetComponent<UIWidget>();
		if (component == null)
		{
			return false;
		}
		if (ClippingPanel == null)
		{
			return false;
		}
		component.Update();
		Vector4 baseClipRegion = ClippingPanel.baseClipRegion;
		bool result = false;
		if (SynchronizationWidgetHeight && baseClipRegion.w != (float)component.height)
		{
			baseClipRegion.w = component.height;
			ClippingPanel.baseClipRegion = baseClipRegion;
			if (DragScrollView != null)
			{
				BoxCollider component2 = DragScrollView.GetComponent<BoxCollider>();
				component2.size = new Vector3(component2.size.x, component.height, component2.size.z);
			}
			result = true;
		}
		if (SynchronizationWidgetWidth && baseClipRegion.z != (float)component.width)
		{
			baseClipRegion.z = component.width;
			ClippingPanel.baseClipRegion = baseClipRegion;
			if (DragScrollView != null)
			{
				BoxCollider component3 = DragScrollView.GetComponent<BoxCollider>();
				component3.size = new Vector3(component.width, component3.size.y, component3.size.z);
			}
			result = true;
		}
		return result;
	}
}
