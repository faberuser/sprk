using System.Collections.Generic;
using NNguiExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/UI/Simple Grid (Vespa)")]
public class UISimpleGrid : UIBaseGrid, IInspectorGuiDrawer
{
	[HideInInspector]
	public UIGrid Grid;

	[HideInInspector]
	public Vector2 GridItemScale = Vector2.one;

	[HideInInspector]
	public bool AutoFitScale = true;

	[HideInInspector]
	public bool ItemDepthRearrange;

	[HideInInspector]
	public bool ReverseOrder;

	[HideInInspector]
	public bool AssignPivotWidgetSize;

	private Vector3 _gridItemScaleV3 => new Vector3(GridItemScale.x, GridItemScale.y, GridItemScale.y);

	private void OnDestroy()
	{
	}

	public override void RepositionGrid()
	{
		if (Grid != null)
		{
			Grid.Reposition();
			if (Grid.UseGridAnimation)
			{
				Grid.PlayForwardAnimation();
			}
		}
	}

	public override T AddGridItem<T>(PrefabAuto<T> prefab)
	{
		return AddGridItem(prefab.Value.Factory);
	}

	public T AddGridItem<T>(Factory<T> prefabFactory) where T : Component
	{
		return AddGridItemInternal(prefabFactory);
	}

	private T AddGridItemInternal<T>(Factory<T> prefabFactory) where T : Component
	{
		if (prefabFactory == null)
		{
			return null;
		}
		UIPanel panel = null;
		if (Grid != null)
		{
			panel = Grid.GetPanel();
		}
		using (new RecommendPanel(panel))
		{
			T val = prefabFactory.Create(Grid.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(val.transform);
			AssignItemScale(val);
			if (ReverseOrder)
			{
				val.transform.SetAsFirstSibling();
			}
			if (ItemDepthRearrange)
			{
				UIWidget pivotWidget = GetPivotWidget();
				if (pivotWidget != null)
				{
					IDepthHelper depthHelper = val.gameObject.GetComponent<IDepthHelper>();
					if (depthHelper == null)
					{
						depthHelper = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<UIDepthHelper>(val.gameObject);
					}
					depthHelper?.RearrangeWidgetDepth(pivotWidget.depth + pivotWidget.reserveDepth + 1);
				}
			}
			return val;
		}
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

	private void AssignItemScale<T>(T itemT) where T : Component
	{
		if (!AssignPivotWidgetSize)
		{
			itemT.transform.localScale = _gridItemScaleV3;
			return;
		}
		UIWidget pivotWidget = GetPivotWidget();
		UIWidget component = itemT.GetComponent<UIWidget>();
		if (pivotWidget == null || component == null)
		{
			itemT.transform.localScale = _gridItemScaleV3;
			return;
		}
		component.width = pivotWidget.width;
		component.height = pivotWidget.height;
	}

	private UIWidget GetPivotWidget()
	{
		return GetComponent<UIWidget>();
	}

	public override void ScrollTo(Component comp, float amount, float strength)
	{
		VespaDebug.LogError("UISimpleGrid.ScrollTo : not implemented");
	}
}
