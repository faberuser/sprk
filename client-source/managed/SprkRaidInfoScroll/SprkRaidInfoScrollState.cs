using UnityEngine;

public sealed class SprkRaidInfoScrollState : MonoBehaviour
{
	public string LastText;

	public UILabel Label;

	public UIScrollView View;

	private int lastWidth;

	private int lastHeight;

	private void LateUpdate()
	{
		RefreshLayout();
	}

	private void OnEnable()
	{
		lastHeight = 0;
	}

	public void RefreshLayout()
	{
		if (Label == null || View == null || !Label.gameObject.activeInHierarchy)
		{
			return;
		}
		bool flag = LastText != Label.text;
		if (flag)
		{
			Label.MarkAsChanged();
			_ = Label.printedSize;
		}
		if (flag || lastWidth != Label.width || lastHeight != Label.height)
		{
			LastText = Label.text;
			lastWidth = Label.width;
			lastHeight = Label.height;
			NGUITools.AddWidgetCollider(Label.gameObject);
			View.InvalidateBounds();
			if (flag)
			{
				View.DisableSpring();
				View.currentMomentum = Vector3.zero;
				View.ResetPosition();
			}
			View.UpdateScrollbars(recalculateBounds: true);
		}
	}
}
