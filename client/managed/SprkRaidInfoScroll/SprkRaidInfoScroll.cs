using System.Linq;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using UnityEngine;

public static class SprkRaidInfoScroll
{
	public static void Refresh(RaidInfoBase info)
	{
		if (info != null)
		{
			RefreshLabel(info.RaidTacticLabel);
		}
	}

	public static void SetText(UILabel label, object text, bool option)
	{
		SprkRaidInfoScrollState sprkRaidInfoScrollState = Bind(label);
		UIHelper.SetLabelText(label, text, option);
		if (sprkRaidInfoScrollState != null)
		{
			sprkRaidInfoScrollState.RefreshLayout();
		}
	}

	public static void RefreshLabel(UILabel label)
	{
		SprkRaidInfoScrollState sprkRaidInfoScrollState = Bind(label);
		if (sprkRaidInfoScrollState != null)
		{
			sprkRaidInfoScrollState.RefreshLayout();
		}
	}

	private static SprkRaidInfoScrollState Bind(UILabel label)
	{
		if (label == null)
		{
			return null;
		}
		SprkRaidInfoScrollState sprkRaidInfoScrollState = label.GetComponent<SprkRaidInfoScrollState>();
		if (sprkRaidInfoScrollState != null && sprkRaidInfoScrollState.View != null)
		{
			return sprkRaidInfoScrollState;
		}
		if (sprkRaidInfoScrollState == null)
		{
			sprkRaidInfoScrollState = label.gameObject.AddComponent<SprkRaidInfoScrollState>();
		}
		UIScrollView uIScrollView = label.GetComponentInParent<UIScrollView>(includeInactive: true);
		if (uIScrollView != null && uIScrollView.GetComponentsInChildren<UILabel>(includeInactive: true).Any((UILabel l) => l != label))
		{
			uIScrollView = null;
		}
		if (uIScrollView == null)
		{
			Transform parent = label.transform.parent;
			int width = label.width;
			int height = label.height;
			Vector3[] worldCorners = label.worldCorners;
			Vector3 localPosition = parent.InverseTransformPoint((worldCorners[0] + worldCorners[2]) * 0.5f);
			GameObject gameObject = new GameObject("SprkCharacteristicsScroll");
			gameObject.layer = label.gameObject.layer;
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			gameObject.transform.localPosition = localPosition;
			UIPanel uIPanel = gameObject.AddComponent<UIPanel>();
			UIPanel uIPanel2 = NGUITools.FindInParents<UIPanel>(parent.gameObject);
			uIPanel.depth = ((uIPanel2 == null) ? 1 : (uIPanel2.depth + 1));
			uIPanel.clipping = UIDrawCall.Clipping.SoftClip;
			uIPanel.clipSoftness = Vector2.zero;
			uIPanel.baseClipRegion = new Vector4(0f, 0f, width, height);
			uIScrollView = gameObject.AddComponent<UIScrollView>();
			label.transform.SetParent(gameObject.transform, worldPositionStays: false);
			label.pivot = UIWidget.Pivot.TopLeft;
			label.transform.localPosition = new Vector3((float)(-width) * 0.5f, (float)height * 0.5f, 0f);
		}
		uIScrollView.movement = UIScrollView.Movement.Vertical;
		uIScrollView.contentPivot = UIWidget.Pivot.TopLeft;
		uIScrollView.restrictWithinPanel = true;
		uIScrollView.disableDragIfFits = true;
		uIScrollView.dragEffect = UIScrollView.DragEffect.Momentum;
		label.leftAnchor.target = null;
		label.rightAnchor.target = null;
		label.topAnchor.target = null;
		label.bottomAnchor.target = null;
		label.ResetAnchors();
		label.overflowMethod = UILabel.Overflow.ResizeHeight;
		label.maxLineCount = 0;
		UIDragScrollView uIDragScrollView = label.GetComponent<UIDragScrollView>();
		if (uIDragScrollView == null)
		{
			uIDragScrollView = label.gameObject.AddComponent<UIDragScrollView>();
		}
		uIDragScrollView.scrollView = uIScrollView;
		sprkRaidInfoScrollState.Label = label;
		sprkRaidInfoScrollState.View = uIScrollView;
		return sprkRaidInfoScrollState;
	}
}
