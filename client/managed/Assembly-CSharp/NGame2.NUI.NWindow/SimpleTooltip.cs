using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NManager;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SimpleTooltip<T> : BaseWindow where T : TooltipTextInfo
{
	public UIPanel RootPanel;

	public UIWidget RootWidget;

	public UILabel DescLabel;

	public UILabel NameLabel;

	public UILabel TypeLabel;

	public UITable Table;

	public int DefaultOffsetX;

	public int DefaultOffsetY;

	private WeakRef<UIWidget> _ownerWidget = new WeakRef<UIWidget>();

	protected static bool IsVisible<TTooltip>(WindowPrefab<TTooltip> prefab, UIWidget ownerWidget) where TTooltip : SimpleTooltip<T>
	{
		if (prefab != null && prefab.IsValidInstance && prefab.instance._ownerWidget.Value == ownerWidget)
		{
			return prefab.instance.IsVisible();
		}
		return false;
	}

	protected static void Open<TTooltip>(WindowPrefab<TTooltip> prefab, UIWidget ownerWidget, T textInfo, bool repositionWithCoroutine = false) where TTooltip : SimpleTooltip<T>
	{
		if (!(ownerWidget == null) && textInfo != null)
		{
			TooltipPrintInfo componentInParent = ownerWidget.GetComponentInParent<TooltipPrintInfo>();
			if (!(componentInParent == null) && componentInParent.Mode != UITooltipTrigger.ETooltipTriggerMode.ClickOnly)
			{
				prefab.instance.Open(ownerWidget, componentInParent, textInfo, repositionWithCoroutine);
			}
		}
	}

	protected static void Hide<TTooltip>(WindowPrefab<TTooltip> prefab, UIWidget ownerWidget) where TTooltip : SimpleTooltip<T>
	{
		if (prefab != null && prefab.IsValidInstance && !(prefab.instance._ownerWidget.Value != ownerWidget))
		{
			prefab.instance.Close(ownerWidget);
		}
	}

	protected static void Refresh<TTooltip>(WindowPrefab<TTooltip> prefab, UIWidget ownerWidget, T tooltipText) where TTooltip : SimpleTooltip<T>
	{
		if (prefab != null && prefab.IsValidInstance && !(prefab.instance._ownerWidget.Value != ownerWidget))
		{
			prefab.instance.SetupText(tooltipText);
		}
	}

	protected virtual void Open(UIWidget ownerWidget, TooltipPrintInfo info, T textInfo, bool repositionWithCoroutine)
	{
		UIWidget anchorWidget = ((info.CustomAnchor != null) ? info.CustomAnchor : ownerWidget);
		Open(ownerWidget, anchorWidget, info.OwnerPivot, info.TooltipPivot, info.Offset, textInfo, repositionWithCoroutine);
	}

	protected void Open(UIWidget ownerWidget, UIWidget anchorWidget, UIWidget.Pivot anchorPivot, UIWidget.Pivot tooltipPivot, Vector2 offset, T textInfo, bool repositionWithCoroutine)
	{
		if (textInfo != null)
		{
			Reset();
			_ownerWidget.Value = ownerWidget;
			base.gameObject.SetActive(value: true);
			SetupText(textInfo);
			if (repositionWithCoroutine)
			{
				VespaCoroutine.Start(RepositionUITableWithCoroutine(anchorWidget, anchorPivot, tooltipPivot, offset));
				return;
			}
			SetupDimension();
			UpdatePosition(anchorWidget, anchorPivot, tooltipPivot, offset);
			OnComplete();
		}
	}

	private IEnumerator RepositionUITableWithCoroutine(UIWidget anchorWidget, UIWidget.Pivot anchorPivot, UIWidget.Pivot tooltipPivot, Vector2 offset)
	{
		SetupDimension();
		yield return new WaitForEndOfFrame();
		UpdatePosition(anchorWidget, anchorPivot, tooltipPivot, offset);
		OnComplete();
	}

	protected bool IsVisible()
	{
		return base.gameObject.activeInHierarchy;
	}

	protected virtual void SetupText(T textInfo)
	{
		UIHelper.SetLabelText(NameLabel, textInfo.Name.Value);
		UIHelper.SetLabelText(DescLabel, textInfo.Desc.Value);
		UIHelper.SetLabelText(TypeLabel, textInfo.TypeText.Value);
	}

	protected virtual void OnComplete()
	{
	}

	protected void SetupDimension()
	{
		if (Table != null)
		{
			Table.Reposition();
		}
		CalcBounds();
		CalcBounds();
		if (Table != null)
		{
			Table.Reposition();
		}
	}

	protected virtual void CalcBounds()
	{
		if (!(Table == null))
		{
			Bounds bounds = default(Bounds);
			List<Transform> childList = Table.GetChildList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
			{
				Transform content = childList[i];
				Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(RootWidget.transform, content, considerInactive: true);
				bounds.Encapsulate(bounds2);
			}
			RootWidget.width = (int)bounds.size.x;
			RootWidget.height = (int)bounds.size.y;
			RootWidget.ForceUpdateAnchors(includeChildren: true);
		}
	}

	protected void UpdatePosition(UIWidget anchorWidget, UIWidget.Pivot anchorPivot, UIWidget.Pivot tooltipPivot, Vector2 offset)
	{
		offset.x += DefaultOffsetX;
		offset.y += DefaultOffsetY;
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(anchorWidget.gameObject.layer);
		if (layerInfo == null)
		{
			layerInfo = GlobalUIRoot.instance.GetLayerInfo(anchorWidget.gameObject.layer);
		}
		if (layerInfo == null)
		{
			Close();
			return;
		}
		if ((bool)RootPanel)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(RootPanel.gameObject, anchorWidget.gameObject.layer, forceRecursive: true);
		}
		Vector3[] worldCorners = NGUITools.GetWorldCorners(layerInfo.Camera);
		Vector3 position = worldCorners[0];
		Vector3 position2 = worldCorners[2];
		Vector3 lhs = RootWidget.transform.parent.InverseTransformPoint(position);
		Vector3 rhs = RootWidget.transform.parent.InverseTransformPoint(position2);
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(Vector3.Min(lhs, rhs), Vector3.Max(lhs, rhs));
		bounds.size -= new Vector3(RootWidget.width, RootWidget.height, 0f);
		Vector2 pivotOffset = NGUIMath.GetPivotOffset(anchorPivot);
		Vector2 pivotOffset2 = NGUIMath.GetPivotOffset(tooltipPivot);
		Vector3 tooltipPosition = GetTooltipPosition(RootWidget, anchorWidget, pivotOffset, pivotOffset2, offset);
		bool flag = false;
		if (tooltipPosition.x < bounds.min.x || tooltipPosition.x > bounds.max.x)
		{
			pivotOffset.x = 1f - pivotOffset.x;
			pivotOffset2.x = 1f - pivotOffset2.x;
			offset.x *= -1f;
			flag = true;
		}
		if (tooltipPosition.y < bounds.min.y || tooltipPosition.y > bounds.max.y)
		{
			pivotOffset.y = 1f - pivotOffset.y;
			pivotOffset2.y = 1f - pivotOffset2.y;
			offset.y *= -1f;
			flag = true;
		}
		if (flag)
		{
			tooltipPosition = GetTooltipPosition(RootWidget, anchorWidget, pivotOffset, pivotOffset2, offset);
			if (tooltipPosition.y < bounds.min.y)
			{
				tooltipPosition.y = bounds.min.y;
			}
			else if (tooltipPosition.y > bounds.max.y)
			{
				tooltipPosition.y = bounds.max.y;
			}
		}
		RootWidget.transform.localPosition = tooltipPosition;
	}

	private void AssignRootPanelDepth()
	{
		if (RootPanel == null || !GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
		{
			return;
		}
		BaseWindow topOpenedWindow = GlobalAutoBehaviour<GlobalManagement>.instance.GetTopOpenedWindow();
		if (!(topOpenedWindow == null))
		{
			UIPanel[] componentsInChildren = topOpenedWindow.GetComponentsInChildren<UIPanel>();
			int num = -1;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
			{
				num = Mathf.Max(num, componentsInChildren[i].depth);
			}
			RootPanel.depth = ((num == -1) ? RootPanel.depth : (num + 1));
		}
	}

	private static Vector3 GetTooltipPosition(UIWidget rootWidget, UIWidget ownerWidget, Vector3 ownerPivotOffset, Vector3 tooltipPivotOffset, Vector3 offset)
	{
		Bounds bounds = ownerWidget.CalculateBounds(rootWidget.transform.parent);
		Vector3 vector = new Vector3((float)rootWidget.width * (0.5f - tooltipPivotOffset.x), (float)rootWidget.height * (0.5f - tooltipPivotOffset.y), 0f);
		return bounds.min + new Vector3(bounds.size.x * ownerPivotOffset.x, bounds.size.y * ownerPivotOffset.y, 0f) + vector + offset;
	}

	public override void Close()
	{
		Close(_ownerWidget.Value);
	}

	public void Close(UIWidget i_ownerWidget)
	{
		if (Vespa_Common_Class_Extension_Method.EQUALS(_ownerWidget.Value, i_ownerWidget))
		{
			OnClose();
			base.gameObject.SetActive(value: false);
			_ownerWidget.Value = null;
		}
	}

	protected virtual void OnClose()
	{
	}
}
