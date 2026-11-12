using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Anchor")]
public class UIAnchor : MonoBehaviour
{
	public enum Side
	{
		BottomLeft,
		Left,
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		Center
	}

	public Camera uiCamera;

	public GameObject container;

	public Side side = Side.Center;

	public bool runOnlyOnce = true;

	public Vector2 relativeOffset = Vector2.zero;

	public Vector2 pixelOffset = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private UIWidget widgetContainer;

	private Transform mTrans;

	private Animation mAnim;

	private Rect mRect;

	private UIRoot mRoot;

	private bool mStarted;

	private void Awake()
	{
		mTrans = base.transform;
		mAnim = GetComponent<Animation>();
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
	}

	private void OnDestroy()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
	}

	private void ScreenSizeChanged()
	{
		if (mStarted && runOnlyOnce)
		{
			Update();
		}
	}

	private void Start()
	{
		if (container == null && widgetContainer != null)
		{
			container = widgetContainer.gameObject;
			widgetContainer = null;
		}
		mRoot = NGUITools.FindInParents<UIRoot>(base.gameObject);
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		Update();
		mStarted = true;
	}

	private void Update()
	{
		if (mAnim != null && mAnim.enabled && mAnim.isPlaying)
		{
			return;
		}
		bool flag = false;
		UIWidget uIWidget = ((container == null) ? null : container.GetComponent<UIWidget>());
		UIPanel uIPanel = ((container == null && uIWidget == null) ? null : container.GetComponent<UIPanel>());
		Rect rect = default(Rect);
		if (uIWidget != null)
		{
			Bounds bounds = uIWidget.CalculateBounds(container.transform.parent);
			rect.x = bounds.min.x;
			rect.y = bounds.min.y;
			rect.width = bounds.size.x;
			rect.height = bounds.size.y;
		}
		else if (uIPanel != null)
		{
			if (uIPanel.clipping == UIDrawCall.Clipping.None)
			{
				float num = ((mRoot != null) ? ((float)mRoot.activeHeight / (float)Screen.height * 0.5f) : 0.5f);
				rect.xMin = (float)(-Screen.width) * num;
				rect.yMin = (float)(-Screen.height) * num;
				rect.xMax = 0f - rect.xMin;
				rect.yMax = 0f - rect.yMin;
			}
			else
			{
				Vector4 finalClipRegion = uIPanel.finalClipRegion;
				rect.x = finalClipRegion.x - finalClipRegion.z * 0.5f;
				rect.y = finalClipRegion.y - finalClipRegion.w * 0.5f;
				rect.width = finalClipRegion.z;
				rect.height = finalClipRegion.w;
			}
		}
		else if (container != null)
		{
			Transform parent = container.transform.parent;
			Bounds bounds2 = ((parent != null) ? NGUIMath.CalculateRelativeWidgetBounds(parent, container.transform) : NGUIMath.CalculateRelativeWidgetBounds(container.transform));
			rect.x = bounds2.min.x;
			rect.y = bounds2.min.y;
			rect.width = bounds2.size.x;
			rect.height = bounds2.size.y;
		}
		else
		{
			if (!(uiCamera != null))
			{
				return;
			}
			flag = true;
			rect = uiCamera.pixelRect;
		}
		if (Mathf.Approximately(mRect.x, rect.x) && Mathf.Approximately(mRect.y, rect.y) && Mathf.Approximately(mRect.width, rect.width))
		{
			Mathf.Approximately(mRect.height, rect.height);
		}
		float x = (mRect.xMin + mRect.xMax) * 0.5f;
		float y = (mRect.yMin + mRect.yMax) * 0.5f;
		Vector3 vector = new Vector3(x, y, 0f);
		if (side != Side.Center)
		{
			if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
			{
				vector.x = mRect.xMax;
			}
			else if (side == Side.Top || side == Side.Center || side == Side.Bottom)
			{
				vector.x = x;
			}
			else
			{
				vector.x = mRect.xMin;
			}
			if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
			{
				vector.y = mRect.yMax;
			}
			else if (side == Side.Left || side == Side.Center || side == Side.Right)
			{
				vector.y = y;
			}
			else
			{
				vector.y = mRect.yMin;
			}
		}
		float width = mRect.width;
		float height = mRect.height;
		vector.x += pixelOffset.x + relativeOffset.x * width;
		vector.y += pixelOffset.y + relativeOffset.y * height;
		if (flag)
		{
			if (uiCamera.orthographic)
			{
				vector.x = Mathf.Round(vector.x);
				vector.y = Mathf.Round(vector.y);
			}
			vector.z = Vespa_Unity_Camera_Extension_Method.VespaWorldToScreenPoint(uiCamera, mTrans.position).z;
			vector = Vespa_Unity_Camera_Extension_Method.VespaScreenToWorldPoint(uiCamera, vector);
		}
		else
		{
			vector.x = Mathf.Round(vector.x);
			vector.y = Mathf.Round(vector.y);
			if (uIPanel != null)
			{
				vector = uIPanel.cachedTransform.TransformPoint(vector);
			}
			else if (container != null)
			{
				Transform parent2 = container.transform.parent;
				if (parent2 != null)
				{
					vector = parent2.TransformPoint(vector);
				}
			}
			vector.z = mTrans.position.z;
		}
		if (mTrans.position != vector)
		{
			mTrans.position = vector;
		}
		if (runOnlyOnce && Application.isPlaying)
		{
			base.enabled = false;
		}
	}
}
