using System.Collections.Generic;
using UnityEngine;

public class UILabelAligner : UIWidget
{
	private List<UIWidget> Widgets = new List<UIWidget>();

	public int SpaceWidth;

	public AnchorUpdate UpdateAligner = AnchorUpdate.OnStart;

	public bool IsDetail;

	public int TotalWidth { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		CollectWidget();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		CollectWidget();
		if (UpdateAligner == AnchorUpdate.OnEnable)
		{
			Align();
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		if (UpdateAligner == AnchorUpdate.OnStart)
		{
			Align();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (UpdateAligner == AnchorUpdate.OnUpdate)
		{
			Align();
		}
	}

	public void CollectWidget(bool isHideIgnore = true)
	{
		Widgets.Clear();
		int childCount = base.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (!(child == null))
			{
				UIWidget component = child.GetComponent<UIWidget>();
				if (!(component == null))
				{
					Widgets.Add(component);
				}
			}
		}
	}

	public void Align()
	{
		if (Widgets.Count == 0)
		{
			CollectWidget();
		}
		switch (base.pivot)
		{
		case Pivot.TopLeft:
		case Pivot.Left:
		case Pivot.BottomLeft:
			Align_Left();
			break;
		case Pivot.Top:
		case Pivot.Center:
		case Pivot.Bottom:
			Align_Center();
			break;
		case Pivot.TopRight:
		case Pivot.Right:
		case Pivot.BottomRight:
			Align_Right();
			break;
		}
	}

	private void SetTotalWidth(int width)
	{
		TotalWidth = width;
	}

	private void Align_Left()
	{
		int num = 0;
		foreach (UIWidget widget in Widgets)
		{
			if (widget.gameObject.activeSelf)
			{
				widget.pivot = base.pivot;
				Vector3 localPosition = widget.transform.localPosition;
				localPosition.x = num;
				widget.transform.localPosition = localPosition;
				int num2 = GetWidth(widget) + SpaceWidth;
				num += num2;
			}
		}
		SetTotalWidth(num + SpaceWidth);
	}

	private void Align_Right()
	{
		int num = 0;
		for (int num2 = Widgets.Count - 1; num2 >= 0; num2--)
		{
			UIWidget uIWidget = Widgets[num2];
			if (uIWidget.gameObject.activeSelf)
			{
				uIWidget.pivot = base.pivot;
				Vector3 localPosition = uIWidget.transform.localPosition;
				localPosition.x = num;
				uIWidget.transform.localPosition = localPosition;
				num -= GetWidth(uIWidget) + SpaceWidth;
			}
		}
		SetTotalWidth(Mathf.Abs(num) + SpaceWidth);
	}

	private void Align_Center()
	{
		int num = 0;
		List<UIWidget> list = new List<UIWidget>();
		foreach (UIWidget widget in Widgets)
		{
			if (widget.gameObject.activeSelf)
			{
				int num2 = GetWidth(widget);
				num += num2;
				list.Add(widget);
			}
		}
		num += SpaceWidth * (list.Count - 1);
		float num3 = 0f - (float)num / 2f;
		foreach (UIWidget item in list)
		{
			if (item.gameObject.activeSelf)
			{
				item.pivot = base.pivot;
				int num4 = GetWidth(item);
				float x = num3 + (float)num4 / 2f;
				Vector3 localPosition = item.transform.localPosition;
				localPosition.x = x;
				item.transform.localPosition = localPosition;
				num3 += (float)(num4 + SpaceWidth);
			}
		}
		SetTotalWidth(num + SpaceWidth * 2);
	}

	private int GetWidth(UIWidget widget)
	{
		if (IsDetail)
		{
			return (int)NGUIMath.CalculateRelativeWidgetBounds(widget.transform).size.x;
		}
		return widget.width;
	}
}
