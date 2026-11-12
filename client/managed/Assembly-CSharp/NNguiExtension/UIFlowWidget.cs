using UnityEngine;

namespace NNguiExtension;

public class UIFlowWidget : MonoBehaviour
{
	public enum FlowDirection
	{
		Horizontal,
		Vertical
	}

	public FlowDirection FlowDir;

	public float velocity;

	public UIPanel ClipPanal;

	public UIWidget RootWidget;

	public bool FlowWhenWidgetOverFlow;

	private bool _flowWidget;

	private Vector4 _clipRegion;

	private Bounds _totalBounds;

	protected void Start()
	{
		Init();
	}

	private void Init()
	{
		SetClipRegion();
		CalcBounds();
		SetFlowWidget();
		RePostionChild();
		RePositionRoot();
	}

	private void SetClipRegion()
	{
		if (!(ClipPanal == null))
		{
			_clipRegion = ClipPanal.finalClipRegion;
		}
	}

	private void CalcBounds()
	{
		_totalBounds = default(Bounds);
		for (int i = 0; i < RootWidget.transform.childCount; i++)
		{
			Transform child = RootWidget.transform.GetChild(i);
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(RootWidget.transform, child, considerInactive: true);
			_totalBounds.Encapsulate(bounds);
		}
		RootWidget.width = (int)_totalBounds.size.x;
		RootWidget.height = (int)_totalBounds.size.y;
		RootWidget.ForceUpdateAnchors(includeChildren: true);
	}

	private void SetFlowWidget()
	{
		float num = ((FlowDir == FlowDirection.Horizontal) ? _clipRegion.z : _clipRegion.w);
		float num2 = ((FlowDir == FlowDirection.Horizontal) ? RootWidget.width : RootWidget.height);
		bool flag = num < num2;
		_flowWidget = flag || !FlowWhenWidgetOverFlow;
	}

	private void RePositionRoot()
	{
		if (_flowWidget)
		{
			Mathf.Sign(velocity);
			float x = ((FlowDir == FlowDirection.Horizontal) ? ((float)(RootWidget.width / 2) + _clipRegion.z / 2f) : 0f);
			float y = ((FlowDir == FlowDirection.Vertical) ? ((float)(RootWidget.height / 2) + _clipRegion.w / 2f) : 0f);
			RootWidget.transform.localPosition = new Vector3(x, y, 0f);
		}
	}

	private void RePostionChild()
	{
		for (int i = 0; i < RootWidget.transform.childCount; i++)
		{
			Transform child = RootWidget.transform.GetChild(i);
			child.transform.localPosition = new Vector3(child.transform.localPosition.x - _totalBounds.center.x, child.transform.localPosition.y - _totalBounds.center.y, 0f);
		}
	}

	protected void Update()
	{
		if (_flowWidget)
		{
			float moveAmount = Time.deltaTime * velocity;
			Vector3 localPosition = base.transform.localPosition;
			Vector3 localPosition2 = Flow(localPosition, moveAmount);
			base.transform.localPosition = localPosition2;
		}
	}

	private Vector3 Flow(Vector3 currentPosition, float moveAmount)
	{
		float curr = ((FlowDir == FlowDirection.Horizontal) ? (currentPosition.x + moveAmount) : (currentPosition.y + moveAmount));
		float num = ((FlowDir == FlowDirection.Horizontal) ? ((float)(RootWidget.width / 2) + _clipRegion.z / 2f) : ((float)(RootWidget.height / 2) + _clipRegion.w / 2f));
		curr = Loop(curr, 0f - num, num);
		return new Vector3((FlowDir == FlowDirection.Horizontal) ? curr : currentPosition.x, (FlowDir == FlowDirection.Horizontal) ? currentPosition.y : curr, currentPosition.z);
	}

	private float Loop(float curr, float min, float max)
	{
		if (curr > max)
		{
			curr = min;
		}
		else if (curr < min)
		{
			curr = max;
		}
		return curr;
	}
}
