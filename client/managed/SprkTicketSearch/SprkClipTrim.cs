using NVespa.NNGUIExtension;
using UnityEngine;

// Keeps a scroll grid's top edge lowered by `trim` (room for the filter row).
// Game code can rewrite the clip region or drag area later (panel anchors,
// UIScrollGrid.SynchronizationWidgetSize); each field it rewrites is trimmed again.
public sealed class SprkClipTrim : MonoBehaviour
{
	public UIScrollGrid grid;

	public float trim;

	private float clipY;

	private float clipH;

	private float dragY;

	private int dragH;

	private float colliderH;

	public void Record()
	{
		UIPanel clip = grid.ClippingPanel;
		clipY = clip.baseClipRegion.y;
		clipH = clip.baseClipRegion.w;
		UIDragScrollView drag = Drag();
		if (drag != null)
		{
			dragY = drag.transform.localPosition.y;
			UIWidget widget = drag.GetComponent<UIWidget>();
			dragH = (widget != null) ? widget.height : 0;
			BoxCollider collider = drag.GetComponent<BoxCollider>();
			colliderH = (collider != null) ? collider.size.y : 0f;
		}
	}

	private UIDragScrollView Drag()
	{
		UIDragScrollView drag = grid.DragScrollView;
		if (drag == null || drag.transform == grid.ClippingPanel.transform)
		{
			return null;
		}
		return drag;
	}

	// Returns true when something had been reset and was trimmed again.
	public bool Enforce()
	{
		if (grid == null || grid.ClippingPanel == null || trim <= 0f)
		{
			return false;
		}
		bool changed = false;
		UIPanel clip = grid.ClippingPanel;
		Vector4 region = clip.baseClipRegion;
		if (!Mathf.Approximately(region.w, clipH))
		{
			region.w -= trim;
			changed = true;
		}
		if (!Mathf.Approximately(region.y, clipY))
		{
			region.y -= trim * 0.5f;
			changed = true;
		}
		if (changed)
		{
			clip.baseClipRegion = region;
		}
		UIDragScrollView drag = Drag();
		if (drag != null)
		{
			Vector3 position = drag.transform.localPosition;
			if (!Mathf.Approximately(position.y, dragY))
			{
				drag.transform.localPosition = new Vector3(position.x, position.y - trim * 0.5f, position.z);
				changed = true;
			}
			UIWidget widget = drag.GetComponent<UIWidget>();
			if (widget != null && widget.height != dragH)
			{
				widget.height -= (int)trim;
				changed = true;
			}
			BoxCollider collider = drag.GetComponent<BoxCollider>();
			// The collider centre is local to the drag widget, which is moved above.
			if (collider != null && !Mathf.Approximately(collider.size.y, colliderH))
			{
				collider.size = new Vector3(collider.size.x, collider.size.y - trim, collider.size.z);
				changed = true;
			}
		}
		if (changed)
		{
			Record();
			if (grid.ScrollView != null)
			{
				grid.ScrollView.RestrictWithinBounds(instant: true);
			}
		}
		return changed;
	}

	private void LateUpdate()
	{
		Enforce();
	}
}
