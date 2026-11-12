using System;
using System.Collections;
using System.Collections.Generic;
using NNguiExtension;
using NNguiExtension.NInternal;
using NVespa;
using NVespa.NNGUIExtension;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Panel")]
public class UIPanel : UIRect
{
	public class WidgetConatiner
	{
		public class WidgetIterator : IEnumerable<UIWidget>, IEnumerable, IDisposable
		{
			private WidgetConatiner _container;

			public WidgetIterator(WidgetConatiner container)
			{
				_container = container;
			}

			internal void OnCreate()
			{
				_container._iterateDepth++;
			}

			void IDisposable.Dispose()
			{
				_container._iterateDepth--;
				_container._iterator = this;
				_container.FlushPending();
			}

			IEnumerator<UIWidget> IEnumerable<UIWidget>.GetEnumerator()
			{
				return _container._widgets.Values.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _container._widgets.Values.GetEnumerator();
			}
		}

		private SortedDictionary<UIWidget.WidgetKey, UIWidget> _widgets = new SortedDictionary<UIWidget.WidgetKey, UIWidget>(UIWidget.KeyComparer.Default);

		private List<Tuple<UIWidget, bool>> _pendingAddRemove = new List<Tuple<UIWidget, bool>>();

		private int _iterateDepth;

		private WidgetIterator _iterator;

		public int Count => _widgets.Count;

		public WidgetIterator CreateIterator()
		{
			WidgetIterator widgetIterator;
			if (_iterator == null)
			{
				widgetIterator = new WidgetIterator(this);
			}
			else
			{
				widgetIterator = _iterator;
				_iterator = null;
			}
			widgetIterator.OnCreate();
			return widgetIterator;
		}

		public void Add(UIWidget w)
		{
			if (_iterateDepth > 0)
			{
				_pendingAddRemove.Add(Tuple.Create(w, i_item2: true));
				return;
			}
			UIWidget.WidgetKey key = w.CreateWidgetKeyInPanel();
			_widgets.Add(key, w);
		}

		public bool Remove(UIWidget w)
		{
			if (_iterateDepth > 0)
			{
				_pendingAddRemove.Add(Tuple.Create(w, i_item2: false));
				return true;
			}
			bool result = RemoveInternal(w);
			w.ClearWidgetKeyInPanel();
			return result;
		}

		private void FlushPending()
		{
			for (int i = 0; i < _pendingAddRemove.Count; i++)
			{
				Tuple<UIWidget, bool> tuple = _pendingAddRemove[i];
				if (tuple.Item2)
				{
					Add(tuple.Item1);
				}
				else
				{
					Remove(tuple.Item1);
				}
			}
			_pendingAddRemove.Clear();
		}

		private bool RemoveInternal(UIWidget w)
		{
			if (RemoveByKey(w))
			{
				return true;
			}
			foreach (KeyValuePair<UIWidget.WidgetKey, UIWidget> widget in _widgets)
			{
				if (widget.Value == w)
				{
					VespaDebug.LogError("Remove Warning: Invalid Key: " + Vespa_Unity_Transform_Extension_Method.GetTransformPath(w.transform, null));
					_widgets.Remove(widget.Key);
					return true;
				}
			}
			VespaDebug.LogError("Remove Error: Cannot Find: " + Vespa_Unity_Transform_Extension_Method.GetTransformPath(w.transform, null));
			return false;
		}

		private bool RemoveByKey(UIWidget w)
		{
			UIWidget.WidgetKey widgetKeyInPanel = w.GetWidgetKeyInPanel();
			return _widgets.Remove(widgetKeyInPanel);
		}

		public void Sort()
		{
			List<Tuple<UIWidget.WidgetKey, UIWidget>> list = new List<Tuple<UIWidget.WidgetKey, UIWidget>>();
			foreach (KeyValuePair<UIWidget.WidgetKey, UIWidget> widget in _widgets)
			{
				if (widget.Key.IsChanged(widget.Value))
				{
					list.Add(Tuple.Create(widget.Key, widget.Value));
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				UIWidget.WidgetKey item = list[i].Item1;
				UIWidget item2 = list[i].Item2;
				_widgets.Remove(item);
				item2.ClearWidgetKeyInPanel();
			}
			for (int j = 0; j < list.Count; j++)
			{
				UIWidget item3 = list[j].Item2;
				Add(item3);
			}
		}
	}

	public class WidgetConatiner2 : IEnumerable<UIWidget>, IEnumerable
	{
		private List<UIWidget> _widgets = new List<UIWidget>();

		public int Count => _widgets.Count;

		public void Add(UIWidget w)
		{
			if (_widgets.Count == 0)
			{
				_widgets.Add(w);
				return;
			}
			if (UIWidget.PanelCompareFunc(w, _widgets[0]) == -1)
			{
				_widgets.Insert(0, w);
				return;
			}
			if (UIWidget.PanelCompareFunc(w, _widgets[_widgets.Count - 1]) != -1)
			{
				_widgets.Add(w);
				return;
			}
			int num = 0;
			int num2 = _widgets.Count - 1;
			int num3;
			while (true)
			{
				if (num2 - num <= 1)
				{
					_widgets.Insert(num + 1, w);
					return;
				}
				num3 = (num + num2) / 2;
				int num4 = UIWidget.PanelCompareFunc(w, _widgets[num3]);
				if (num4 == 0)
				{
					break;
				}
				if (num4 < 0)
				{
					num2 = num3;
				}
				else
				{
					num = num3;
				}
			}
			_widgets.Insert(num3 + 1, w);
		}

		public bool Remove(UIWidget w)
		{
			bool flag = FastRemoveWidgetInList(w);
			if (!flag)
			{
				flag = _widgets.Remove(w);
			}
			return flag;
		}

		private bool FastRemoveWidgetInList(UIWidget w)
		{
			int num = -1;
			int num2 = _widgets.Count;
			int num3 = -1;
			while (num2 - num > 1)
			{
				int num4 = (num + num2) / 2;
				UIWidget uIWidget = _widgets[num4];
				if (uIWidget.depth < w.depth)
				{
					num = num4;
					continue;
				}
				if (uIWidget.depth > w.depth)
				{
					num2 = num4;
					continue;
				}
				num3 = num4;
				break;
			}
			if (num3 == -1)
			{
				return false;
			}
			for (int i = num3; i < _widgets.Count; i++)
			{
				UIWidget uIWidget2 = _widgets[i];
				if (!(uIWidget2 == null))
				{
					if (uIWidget2 == w)
					{
						_widgets.RemoveAt(i);
						return true;
					}
					if (uIWidget2.depth != w.depth)
					{
						break;
					}
				}
			}
			for (int num5 = num3 - 1; num5 >= 0; num5--)
			{
				UIWidget uIWidget3 = _widgets[num5];
				if (!(uIWidget3 == null))
				{
					if (uIWidget3 == w)
					{
						_widgets.RemoveAt(num5);
						return true;
					}
					if (uIWidget3.depth != w.depth)
					{
						break;
					}
				}
			}
			return false;
		}

		public void Sort()
		{
			_widgets.Sort(UIWidget.PanelCompareFunc);
		}

		IEnumerator<UIWidget> IEnumerable<UIWidget>.GetEnumerator()
		{
			return _widgets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _widgets.GetEnumerator();
		}
	}

	public enum RenderQueue
	{
		Automatic,
		StartAt,
		Explicit
	}

	public delegate void OnGeometryUpdated();

	public delegate void OnClippingMoved(UIPanel panel);

	public static UIPanel RecommendedPanel;

	public static List<UIPanel> list = new List<UIPanel>();

	public static List<GameObject> lateUpdateBeforePanelList = new List<GameObject>();

	public static List<GameObject> lateUpdateAfterPanelList = new List<GameObject>();

	public UIWidget OcclusionWidget;

	public bool OcclusionFullScreen;

	public OnGeometryUpdated onGeometryUpdated;

	public bool showInPanelTool = true;

	public bool generateNormals;

	public bool widgetsAreStatic;

	public bool cullWhileDragging = true;

	public bool alwaysOnScreen;

	public bool anchorOffset;

	public bool softBorderPadding = true;

	public RenderQueue renderQueue;

	public int startingRenderQueue = 3000;

	[NonSerialized]
	public WidgetConatiner widgets = new WidgetConatiner();

	private DrawCallContainer _drawCallContainer = new DrawCallContainer();

	[NonSerialized]
	public Matrix4x4Fast worldToLocal2 = new Matrix4x4Fast(Matrix4x4.identity);

	[NonSerialized]
	public Vector4 drawCallClipRange = new Vector4(0f, 0f, 1f, 1f);

	public OnClippingMoved onClipMove;

	[HideInInspector]
	[SerializeField]
	private Texture2D mClipTexture;

	[HideInInspector]
	[SerializeField]
	private float mAlpha = 1f;

	[HideInInspector]
	[SerializeField]
	private UIDrawCall.Clipping mClipping;

	[HideInInspector]
	[SerializeField]
	private Vector4 mClipRange = new Vector4(0f, 0f, 300f, 200f);

	[HideInInspector]
	[SerializeField]
	private Vector2 mClipSoftness = new Vector2(4f, 4f);

	[HideInInspector]
	[SerializeField]
	private int mDepth;

	[HideInInspector]
	[SerializeField]
	private int mSortingOrder;

	private bool mResized;

	[SerializeField]
	private Vector2 mClipOffset = Vector2.zero;

	private float mCullTime;

	private float mUpdateTime;

	private int mMatrixFrame = -1;

	private int mAlphaFrameID;

	private int mLayer = -1;

	private static float[] mTemp = new float[4];

	private Vector2 mMin = Vector2.zero;

	private Vector2 mMax = Vector2.zero;

	private bool mHalfPixelOffset;

	private bool mUpdateScroll;

	private bool mIsVisibleByOcclusion = true;

	private UIPanel mParentPanel;

	private static Vector3[] mCorners = new Vector3[4];

	private static int mUpdateFrame = -1;

	private bool mForced;

	public int DrawCallCount => _drawCallContainer.Count;

	public static int nextUnusedDepth
	{
		get
		{
			int num = int.MinValue;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				num = Mathf.Max(num, list[i].depth);
			}
			if (num != int.MinValue)
			{
				return num + 1;
			}
			return 0;
		}
	}

	public override bool canBeAnchored => mClipping != UIDrawCall.Clipping.None;

	public override float alpha
	{
		get
		{
			return mAlpha;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mAlpha != num)
			{
				mAlphaFrameID = -1;
				mResized = true;
				mAlpha = num;
				SetDirty();
			}
		}
	}

	public override bool isGrayscale
	{
		get
		{
			return mIsGrayscale;
		}
		set
		{
			if (mIsGrayscale != value)
			{
				mIsGrayscale = value;
				Invalidate(includeChildren: true);
				RebuildAllDrawCalls();
			}
		}
	}

	public int raycastDepth => depth * 1000;

	public int depth
	{
		get
		{
			return mDepth;
		}
		set
		{
			if (mDepth != value)
			{
				mDepth = value;
				list.Sort(CompareFunc);
			}
		}
	}

	public int sortingOrder
	{
		get
		{
			return mSortingOrder;
		}
		set
		{
			if (mSortingOrder != value)
			{
				mSortingOrder = value;
				UpdateDrawCalls();
			}
		}
	}

	public float width => GetViewSize().x;

	public float height => GetViewSize().y;

	public bool halfPixelOffset => mHalfPixelOffset;

	public bool usedForUI
	{
		get
		{
			if (base.anchorCamera != null)
			{
				return mCam.orthographic;
			}
			return false;
		}
	}

	public Vector3 drawCallOffset
	{
		get
		{
			if (mHalfPixelOffset && base.anchorCamera != null && mCam.orthographic)
			{
				Vector2 windowSize = GetWindowSize();
				float num = ((base.root != null) ? base.root.pixelSizeAdjustment : 1f) / windowSize.y / mCam.orthographicSize;
				return new Vector3(0f - num, num);
			}
			return Vector3.zero;
		}
	}

	public UIDrawCall.Clipping clipping
	{
		get
		{
			return mClipping;
		}
		set
		{
			if (mClipping != value)
			{
				mResized = true;
				mClipping = value;
				mMatrixFrame = -1;
			}
		}
	}

	public UIPanel parentPanel => mParentPanel;

	public int clipCount
	{
		get
		{
			int num = 0;
			UIPanel uIPanel = this;
			while (uIPanel != null)
			{
				if (uIPanel.mClipping == UIDrawCall.Clipping.SoftClip || uIPanel.mClipping == UIDrawCall.Clipping.TextureMask)
				{
					num++;
				}
				uIPanel = uIPanel.mParentPanel;
			}
			return num;
		}
	}

	public bool hasClipping
	{
		get
		{
			if (mClipping != UIDrawCall.Clipping.SoftClip)
			{
				return mClipping == UIDrawCall.Clipping.TextureMask;
			}
			return true;
		}
	}

	public bool hasCumulativeClipping => clipCount != 0;

	[Obsolete("Use 'hasClipping' or 'hasCumulativeClipping' instead")]
	public bool clipsChildren => hasCumulativeClipping;

	public Vector2 clipOffset
	{
		get
		{
			return mClipOffset;
		}
		set
		{
			if (Mathf.Abs(mClipOffset.x - value.x) > 0.001f || Mathf.Abs(mClipOffset.y - value.y) > 0.001f)
			{
				mClipOffset = value;
				InvalidateClipping();
				if (onClipMove != null)
				{
					onClipMove(this);
				}
			}
		}
	}

	public Vector2 UIAnimationScrollPosition
	{
		get
		{
			if (!hasClipping)
			{
				return base.cachedTransform.localPosition;
			}
			return mClipOffset;
		}
	}

	public Texture2D clipTexture
	{
		get
		{
			return mClipTexture;
		}
		set
		{
			if (mClipTexture != value)
			{
				mClipTexture = value;
			}
		}
	}

	[Obsolete("Use 'finalClipRegion' or 'baseClipRegion' instead")]
	public Vector4 clipRange
	{
		get
		{
			return baseClipRegion;
		}
		set
		{
			baseClipRegion = value;
		}
	}

	public Vector4 baseClipRegion
	{
		get
		{
			return mClipRange;
		}
		set
		{
			if (Mathf.Abs(mClipRange.x - value.x) > 0.001f || Mathf.Abs(mClipRange.y - value.y) > 0.001f || Mathf.Abs(mClipRange.z - value.z) > 0.001f || Mathf.Abs(mClipRange.w - value.w) > 0.001f)
			{
				mResized = true;
				mCullTime = ((mCullTime == 0f) ? 0.001f : (RealTime.time + 0.15f));
				mClipRange = value;
				mMatrixFrame = -1;
				UIScrollView component = GetComponent<UIScrollView>();
				if (component != null)
				{
					component.UpdatePosition();
				}
				if (onClipMove != null)
				{
					onClipMove(this);
				}
			}
		}
	}

	public Vector4 finalClipRegion
	{
		get
		{
			Vector2 viewSize = GetViewSize();
			if (mClipping != UIDrawCall.Clipping.None)
			{
				return new Vector4(mClipRange.x + mClipOffset.x, mClipRange.y + mClipOffset.y, viewSize.x, viewSize.y);
			}
			return new Vector4(0f, 0f, viewSize.x, viewSize.y);
		}
	}

	public Vector2 clipSoftness
	{
		get
		{
			return mClipSoftness;
		}
		set
		{
			if (mClipSoftness != value)
			{
				mClipSoftness = value;
			}
		}
	}

	public override Vector3[] localCorners
	{
		get
		{
			if (mClipping == UIDrawCall.Clipping.None)
			{
				Vector3[] array = worldCorners;
				Transform transform = base.cachedTransform;
				for (int i = 0; i < 4; i++)
				{
					array[i] = transform.InverseTransformPoint(array[i]);
				}
				return array;
			}
			float num = mClipOffset.x + mClipRange.x - 0.5f * mClipRange.z;
			float num2 = mClipOffset.y + mClipRange.y - 0.5f * mClipRange.w;
			float x = num + mClipRange.z;
			float y = num2 + mClipRange.w;
			mCorners[0] = new Vector3(num, num2);
			mCorners[1] = new Vector3(num, y);
			mCorners[2] = new Vector3(x, y);
			mCorners[3] = new Vector3(x, num2);
			return mCorners;
		}
	}

	public override Vector3[] worldCorners
	{
		get
		{
			if (mClipping != UIDrawCall.Clipping.None)
			{
				float num = mClipOffset.x + mClipRange.x - 0.5f * mClipRange.z;
				float num2 = mClipOffset.y + mClipRange.y - 0.5f * mClipRange.w;
				float x = num + mClipRange.z;
				float y = num2 + mClipRange.w;
				Transform transform = base.cachedTransform;
				mCorners[0] = transform.TransformPoint(num, num2, 0f);
				mCorners[1] = transform.TransformPoint(num, y, 0f);
				mCorners[2] = transform.TransformPoint(x, y, 0f);
				mCorners[3] = transform.TransformPoint(x, num2, 0f);
			}
			else
			{
				if (base.anchorCamera != null)
				{
					return NGUITools.GetWorldCorners(mCam, base.cameraRayDistance);
				}
				Vector2 viewSize = GetViewSize();
				float num3 = -0.5f * viewSize.x;
				float num4 = -0.5f * viewSize.y;
				float x2 = num3 + viewSize.x;
				float y2 = num4 + viewSize.y;
				mCorners[0] = new Vector3(num3, num4);
				mCorners[1] = new Vector3(num3, y2);
				mCorners[2] = new Vector3(x2, y2);
				mCorners[3] = new Vector3(x2, num4);
				if (anchorOffset && (mCam == null || mCam.transform.parent != base.cachedTransform))
				{
					Vector3 position = base.cachedTransform.position;
					for (int i = 0; i < 4; i++)
					{
						mCorners[i] += position;
					}
				}
			}
			return mCorners;
		}
	}

	public static int CompareFunc(UIPanel a, UIPanel b)
	{
		if (a != b && a != null && b != null)
		{
			if (a.mDepth < b.mDepth)
			{
				return -1;
			}
			if (a.mDepth > b.mDepth)
			{
				return 1;
			}
			if (a.GetInstanceID() >= b.GetInstanceID())
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	private void InvalidateClipping()
	{
		mResized = true;
		mMatrixFrame = -1;
		mCullTime = ((mCullTime == 0f) ? 0.001f : (RealTime.time + 0.15f));
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			UIPanel uIPanel = list[i];
			if (uIPanel != this && uIPanel.parentPanel == this)
			{
				uIPanel.InvalidateClipping();
			}
		}
	}

	public override Vector3[] GetSides(Transform relativeTo)
	{
		if (mClipping != UIDrawCall.Clipping.None)
		{
			float num = mClipOffset.x + mClipRange.x - 0.5f * mClipRange.z;
			float num2 = mClipOffset.y + mClipRange.y - 0.5f * mClipRange.w;
			float num3 = num + mClipRange.z;
			float num4 = num2 + mClipRange.w;
			float x = (num + num3) * 0.5f;
			float y = (num2 + num4) * 0.5f;
			Transform transform = base.cachedTransform;
			UIRect.mSides[0] = transform.TransformPoint(num, y, 0f);
			UIRect.mSides[1] = transform.TransformPoint(x, num4, 0f);
			UIRect.mSides[2] = transform.TransformPoint(num3, y, 0f);
			UIRect.mSides[3] = transform.TransformPoint(x, num2, 0f);
			if (relativeTo != null)
			{
				for (int i = 0; i < 4; i++)
				{
					UIRect.mSides[i] = relativeTo.InverseTransformPoint(UIRect.mSides[i]);
				}
			}
			return UIRect.mSides;
		}
		if (base.anchorCamera != null && anchorOffset)
		{
			Vector3[] sides = NGUITools.GetSides(mCam, base.cameraRayDistance);
			Vector3 position = base.cachedTransform.position;
			for (int j = 0; j < 4; j++)
			{
				sides[j] += position;
			}
			if (relativeTo != null)
			{
				for (int k = 0; k < 4; k++)
				{
					sides[k] = relativeTo.InverseTransformPoint(sides[k]);
				}
			}
			return sides;
		}
		return base.GetSides(relativeTo);
	}

	public override void Invalidate(bool includeChildren)
	{
		mAlphaFrameID = -1;
		mRenderKeywordsFrameID = -1;
		UpdateVisibleByOcclusion();
		base.Invalidate(includeChildren);
	}

	public override float CalculateFinalAlpha(int frameID)
	{
		if (mAlphaFrameID != frameID)
		{
			mAlphaFrameID = frameID;
			UIRect uIRect = base.parent;
			finalAlpha = ((base.parent != null) ? (uIRect.CalculateFinalAlpha(frameID) * mAlpha) : mAlpha);
		}
		return finalAlpha;
	}

	public override void SetRect(float x, float y, float width, float height)
	{
		int num = Mathf.FloorToInt(width + 0.5f);
		int num2 = Mathf.FloorToInt(height + 0.5f);
		num = num >> 1 << 1;
		num2 = num2 >> 1 << 1;
		Transform transform = base.cachedTransform;
		Vector3 localPosition = transform.localPosition;
		localPosition.x = Mathf.Floor(x + 0.5f);
		localPosition.y = Mathf.Floor(y + 0.5f);
		if (num < 2)
		{
			num = 2;
		}
		if (num2 < 2)
		{
			num2 = 2;
		}
		baseClipRegion = new Vector4(localPosition.x, localPosition.y, num, num2);
		if (base.isAnchored)
		{
			transform = transform.parent;
			if ((bool)leftAnchor.target)
			{
				leftAnchor.SetHorizontal(transform, x);
			}
			if ((bool)rightAnchor.target)
			{
				rightAnchor.SetHorizontal(transform, x + width);
			}
			if ((bool)bottomAnchor.target)
			{
				bottomAnchor.SetVertical(transform, y);
			}
			if ((bool)topAnchor.target)
			{
				topAnchor.SetVertical(transform, y + height);
			}
		}
	}

	public bool IsVisible(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		UpdateTransformMatrix();
		a = worldToLocal2.MultiplyPoint3x4(a);
		b = worldToLocal2.MultiplyPoint3x4(b);
		c = worldToLocal2.MultiplyPoint3x4(c);
		d = worldToLocal2.MultiplyPoint3x4(d);
		return IsVisibleReal(a, b, c, d);
	}

	private bool IsVisibleReal(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		mTemp[0] = a.x;
		mTemp[1] = b.x;
		mTemp[2] = c.x;
		mTemp[3] = d.x;
		float num = Mathf.Min(mTemp);
		float num2 = Mathf.Max(mTemp);
		mTemp[0] = a.y;
		mTemp[1] = b.y;
		mTemp[2] = c.y;
		mTemp[3] = d.y;
		float num3 = Mathf.Min(mTemp);
		float num4 = Mathf.Max(mTemp);
		if (num2 < mMin.x)
		{
			return false;
		}
		if (num4 < mMin.y)
		{
			return false;
		}
		if (num > mMax.x)
		{
			return false;
		}
		if (num3 > mMax.y)
		{
			return false;
		}
		return true;
	}

	public bool IsVisible(Vector3 worldPos)
	{
		if (mAlpha < 0.001f)
		{
			return false;
		}
		if (mClipping == UIDrawCall.Clipping.None || mClipping == UIDrawCall.Clipping.ConstrainButDontClip)
		{
			return true;
		}
		UpdateTransformMatrix();
		Vector3 vector = worldToLocal2.MultiplyPoint3x4(worldPos);
		if (vector.x < mMin.x)
		{
			return false;
		}
		if (vector.y < mMin.y)
		{
			return false;
		}
		if (vector.x > mMax.x)
		{
			return false;
		}
		if (vector.y > mMax.y)
		{
			return false;
		}
		return true;
	}

	public bool IsVisible(UIWidget w)
	{
		UIPanel uIPanel = this;
		Vector3[] array = null;
		if (!mIsVisibleByOcclusion)
		{
			return false;
		}
		while (uIPanel != null)
		{
			if ((uIPanel.mClipping == UIDrawCall.Clipping.None || uIPanel.mClipping == UIDrawCall.Clipping.ConstrainButDontClip) && !w.hideIfOffScreen)
			{
				uIPanel = uIPanel.mParentPanel;
				continue;
			}
			if (array == null)
			{
				array = w.worldCorners;
			}
			if (!uIPanel.IsVisible(array[0], array[1], array[2], array[3]))
			{
				return false;
			}
			uIPanel = uIPanel.mParentPanel;
		}
		return true;
	}

	public bool Affects(UIWidget w)
	{
		if (w == null)
		{
			return false;
		}
		UIPanel panel = w.panel;
		if (panel == null)
		{
			return false;
		}
		UIPanel uIPanel = this;
		while (uIPanel != null)
		{
			if (uIPanel == panel)
			{
				return true;
			}
			if (!uIPanel.hasCumulativeClipping)
			{
				return false;
			}
			uIPanel = uIPanel.mParentPanel;
		}
		return false;
	}

	[ContextMenu("Force Refresh")]
	public void RebuildAllDrawCalls()
	{
		_drawCallContainer.RebuildAll();
	}

	public void SetDirty()
	{
		_drawCallContainer.SetDirty();
		Invalidate(includeChildren: true);
	}

	private new void Awake()
	{
		SingletonAuto<LinkedTransform<UIPanel>>.instance.OnAwake(this);
		mGo = base.gameObject;
		mTrans = base.transform;
		mHalfPixelOffset = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.XBOX360 || Application.platform == RuntimePlatform.WindowsEditor;
		if (mHalfPixelOffset)
		{
			mHalfPixelOffset = SystemInfo.graphicsShaderLevel < 40 && SystemInfo.graphicsDeviceVersion.Contains("Direct3D");
		}
	}

	private void FindParent()
	{
		Transform transform = base.cachedTransform.parent;
		mParentPanel = ((transform != null) ? NGUITools.FindInParents<UIPanel>(transform.gameObject) : null);
	}

	public override void ParentHasChanged()
	{
		base.ParentHasChanged();
		FindParent();
	}

	protected override void OnStart()
	{
		mLayer = mGo.layer;
	}

	protected override void OnEnable()
	{
		_drawCallContainer.RebuildAll();
		mAlphaFrameID = -1;
		mRenderKeywordsFrameID = -1;
		mMatrixFrame = -1;
		OnStart();
		base.OnEnable();
		mMatrixFrame = -1;
	}

	protected override void OnInit()
	{
		if (list.Contains(this))
		{
			return;
		}
		base.OnInit();
		FindParent();
		if (GetComponent<Rigidbody>() == null && mParentPanel == null)
		{
			UICamera uICamera = ((base.anchorCamera != null) ? mCam.GetComponent<UICamera>() : null);
			if (uICamera != null && (uICamera.eventType == UICamera.EventType.UI_3D || uICamera.eventType == UICamera.EventType.World_3D))
			{
				Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
				rigidbody.isKinematic = true;
				rigidbody.useGravity = false;
			}
		}
		_drawCallContainer.RebuildAll();
		mAlphaFrameID = -1;
		mRenderKeywordsFrameID = -1;
		mMatrixFrame = -1;
		list.Add(this);
		list.Sort(CompareFunc);
	}

	protected override void OnDisable()
	{
		_drawCallContainer.DestroyAll();
		list.Remove(this);
		mAlphaFrameID = -1;
		mRenderKeywordsFrameID = -1;
		mMatrixFrame = -1;
		if (list.Count == 0)
		{
			UIDrawCall.ReleaseAll();
			mUpdateFrame = -1;
		}
		base.OnDisable();
	}

	private void OnDestroy()
	{
		_drawCallContainer.DestroyAll();
		list.Remove(this);
		SingletonAuto<LinkedTransform<UIPanel>>.instance.OnDestroy(this);
	}

	private void UpdateTransformMatrix()
	{
		int frameCount = Time.frameCount;
		if (mMatrixFrame != frameCount)
		{
			mMatrixFrame = frameCount;
			worldToLocal2.Reset(base.cachedTransform.worldToLocalMatrix);
			Vector2 vector = GetViewSize() * 0.5f;
			float num = mClipOffset.x + mClipRange.x;
			float num2 = mClipOffset.y + mClipRange.y;
			mMin.x = num - vector.x;
			mMin.y = num2 - vector.y;
			mMax.x = num + vector.x;
			mMax.y = num2 + vector.y;
			UpdateVisibleByOcclusion();
		}
	}

	private void UpdateVisibleByOcclusion()
	{
		if (mClipping == UIDrawCall.Clipping.None || mClipping == UIDrawCall.Clipping.ConstrainButDontClip || renderQueue != RenderQueue.Automatic)
		{
			mIsVisibleByOcclusion = true;
		}
		else
		{
			mIsVisibleByOcclusion = SingletonAuto<OcclusionManager>.instance.IsVisible(base.gameObject.layer, raycastDepth);
		}
	}

	protected override void OnAnchor()
	{
		if (mClipping == UIDrawCall.Clipping.None)
		{
			return;
		}
		Transform obj = base.cachedTransform;
		Transform transform = obj.parent;
		Vector2 viewSize = GetViewSize();
		Vector2 vector = obj.localPosition;
		float num;
		float num2;
		float num3;
		float num4;
		if (leftAnchor.target == bottomAnchor.target && leftAnchor.target == rightAnchor.target && leftAnchor.target == topAnchor.target)
		{
			Vector3[] sides = leftAnchor.GetSides(transform);
			if (sides != null)
			{
				num = NGUIMath.Lerp(sides[0].x, sides[2].x, leftAnchor.relative) + (float)leftAnchor.absolute;
				num2 = NGUIMath.Lerp(sides[0].x, sides[2].x, rightAnchor.relative) + (float)rightAnchor.absolute;
				num3 = NGUIMath.Lerp(sides[3].y, sides[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute;
				num4 = NGUIMath.Lerp(sides[3].y, sides[1].y, topAnchor.relative) + (float)topAnchor.absolute;
			}
			else
			{
				Vector2 vector2 = GetLocalPos(leftAnchor, transform);
				num = vector2.x + (float)leftAnchor.absolute;
				num3 = vector2.y + (float)bottomAnchor.absolute;
				num2 = vector2.x + (float)rightAnchor.absolute;
				num4 = vector2.y + (float)topAnchor.absolute;
			}
		}
		else
		{
			if ((bool)leftAnchor.target)
			{
				Vector3[] sides2 = leftAnchor.GetSides(transform);
				num = ((sides2 == null) ? (GetLocalPos(leftAnchor, transform).x + (float)leftAnchor.absolute) : (NGUIMath.Lerp(sides2[0].x, sides2[2].x, leftAnchor.relative) + (float)leftAnchor.absolute));
			}
			else
			{
				num = mClipRange.x - 0.5f * viewSize.x;
			}
			if ((bool)rightAnchor.target)
			{
				Vector3[] sides3 = rightAnchor.GetSides(transform);
				num2 = ((sides3 == null) ? (GetLocalPos(rightAnchor, transform).x + (float)rightAnchor.absolute) : (NGUIMath.Lerp(sides3[0].x, sides3[2].x, rightAnchor.relative) + (float)rightAnchor.absolute));
			}
			else
			{
				num2 = mClipRange.x + 0.5f * viewSize.x;
			}
			if ((bool)bottomAnchor.target)
			{
				Vector3[] sides4 = bottomAnchor.GetSides(transform);
				num3 = ((sides4 == null) ? (GetLocalPos(bottomAnchor, transform).y + (float)bottomAnchor.absolute) : (NGUIMath.Lerp(sides4[3].y, sides4[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute));
			}
			else
			{
				num3 = mClipRange.y - 0.5f * viewSize.y;
			}
			if ((bool)topAnchor.target)
			{
				Vector3[] sides5 = topAnchor.GetSides(transform);
				num4 = ((sides5 == null) ? (GetLocalPos(topAnchor, transform).y + (float)topAnchor.absolute) : (NGUIMath.Lerp(sides5[3].y, sides5[1].y, topAnchor.relative) + (float)topAnchor.absolute));
			}
			else
			{
				num4 = mClipRange.y + 0.5f * viewSize.y;
			}
		}
		num -= vector.x + mClipOffset.x;
		num2 -= vector.x + mClipOffset.x;
		num3 -= vector.y + mClipOffset.y;
		num4 -= vector.y + mClipOffset.y;
		float x = Mathf.Lerp(num, num2, 0.5f);
		float y = Mathf.Lerp(num3, num4, 0.5f);
		float num5 = num2 - num;
		float num6 = num4 - num3;
		float num7 = Mathf.Max(2f, mClipSoftness.x);
		float num8 = Mathf.Max(2f, mClipSoftness.y);
		if (num5 < num7)
		{
			num5 = num7;
		}
		if (num6 < num8)
		{
			num6 = num8;
		}
		baseClipRegion = new Vector4(x, y, num5, num6);
	}

	private void LateUpdate()
	{
		if (mUpdateFrame != Time.frameCount)
		{
			mUpdateFrame = Time.frameCount;
			UpdateAllPanels();
		}
	}

	public static void UpdateAllPanels()
	{
		LateUpdateBeforePanel();
		SingletonAuto<OcclusionManager>.instance.UpdateAll();
		int num = 0;
		for (int i = 0; i < 10; i++)
		{
			NguiFontRebuild.PanelState = NguiFontRebuild.EPanelState.PanelUpdating;
			UpdateAllPanelsInternal();
			num++;
			NguiFontRebuild.PanelState = NguiFontRebuild.EPanelState.EndOfUpdate;
			if (!NguiFontRebuild.IsDirty)
			{
				break;
			}
		}
		LateUpdateAfterPanel();
		_ = 1;
	}

	private static void LateUpdateBeforePanel()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(lateUpdateBeforePanelList); i++)
		{
			GameObject gameObject = lateUpdateBeforePanelList[i];
			if (gameObject == null)
			{
				lateUpdateBeforePanelList.RemoveAt(i);
				i--;
			}
			else if (gameObject.gameObject.activeInHierarchy)
			{
				LateUpdaterBeforePanel.IReceiver[] components = gameObject.gameObject.GetComponents<LateUpdaterBeforePanel.IReceiver>();
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(components); j++)
				{
					components[j].OnUpdate();
				}
			}
		}
	}

	private static void LateUpdateAfterPanel()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(lateUpdateAfterPanelList); i++)
		{
			GameObject gameObject = lateUpdateAfterPanelList[i];
			if (gameObject == null)
			{
				lateUpdateAfterPanelList.RemoveAt(i);
				i--;
			}
			else if (gameObject.gameObject.activeInHierarchy)
			{
				LateUpdaterAfterPanel.IReceiver[] components = gameObject.gameObject.GetComponents<LateUpdaterAfterPanel.IReceiver>();
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(components); j++)
				{
					components[j].OnUpdate();
				}
			}
		}
	}

	private static void UpdateAllPanelsInternal()
	{
		NguiFontRebuild.IsDirty = false;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].UpdateSelf();
		}
		int a = 3000;
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			UIPanel uIPanel = list[j];
			if (uIPanel.renderQueue == RenderQueue.Automatic)
			{
				uIPanel.startingRenderQueue = a;
				a = uIPanel.UpdateDrawCalls();
			}
			else if (uIPanel.renderQueue == RenderQueue.StartAt)
			{
				int b = uIPanel.UpdateDrawCalls();
				if (uIPanel._drawCallContainer.Count != 0)
				{
					a = Mathf.Max(a, b);
				}
			}
			else
			{
				uIPanel.UpdateDrawCalls();
				if (uIPanel._drawCallContainer.Count != 0)
				{
					a = Mathf.Max(a, uIPanel.startingRenderQueue + 1);
				}
			}
		}
	}

	private void UpdateSelf()
	{
		mUpdateTime = RealTime.time;
		UpdateTransformMatrix();
		UpdateLayers();
		UpdateWidgets();
		_drawCallContainer.UpdateDrawCalls(this, widgets, generateNormals);
		if (mUpdateScroll)
		{
			mUpdateScroll = false;
			UIScrollView component = GetComponent<UIScrollView>();
			if (component != null)
			{
				component.UpdateScrollbars();
			}
		}
	}

	public void SortWidgets()
	{
		widgets.Sort();
	}

	private int UpdateDrawCalls()
	{
		Transform transform = base.cachedTransform;
		bool num = usedForUI;
		if (clipping != UIDrawCall.Clipping.None)
		{
			drawCallClipRange = finalClipRegion;
			drawCallClipRange.z *= 0.5f;
			drawCallClipRange.w *= 0.5f;
		}
		else
		{
			drawCallClipRange = Vector4.zero;
		}
		if (drawCallClipRange.z == 0f)
		{
			drawCallClipRange.z = (float)Screen.width * 0.5f;
		}
		if (drawCallClipRange.w == 0f)
		{
			drawCallClipRange.w = (float)Screen.height * 0.5f;
		}
		if (halfPixelOffset)
		{
			drawCallClipRange.x -= 0.5f;
			drawCallClipRange.y += 0.5f;
		}
		Vector3 vector;
		if (num)
		{
			Transform transform2 = base.cachedTransform.parent;
			vector = base.cachedTransform.localPosition;
			if (clipping != UIDrawCall.Clipping.None)
			{
				vector.x = Mathf.RoundToInt(vector.x);
				vector.y = Mathf.RoundToInt(vector.y);
			}
			if (transform2 != null)
			{
				vector = transform2.TransformPoint(vector);
			}
			vector += drawCallOffset;
		}
		else
		{
			vector = transform.position;
		}
		Quaternion rotation = transform.rotation;
		Vector3 lossyScale = transform.lossyScale;
		return _drawCallContainer.UpdateRenderQueue(this, startingRenderQueue, vector, rotation, lossyScale);
	}

	private void UpdateLayers()
	{
		if (mLayer == base.cachedGameObject.layer)
		{
			return;
		}
		mLayer = mGo.layer;
		using (WidgetConatiner.WidgetIterator widgetIterator = widgets.CreateIterator())
		{
			foreach (UIWidget item in (IEnumerable<UIWidget>)widgetIterator)
			{
				if ((bool)item && item.parent == this)
				{
					item.gameObject.layer = mLayer;
				}
			}
		}
		ResetAnchors();
		_drawCallContainer.SetLayer(mLayer);
	}

	private void UpdateWidgets()
	{
		bool flag = !cullWhileDragging && mCullTime > mUpdateTime;
		bool flag2 = false;
		if (mForced != flag)
		{
			mForced = flag;
			mResized = true;
		}
		bool flag3 = hasCumulativeClipping;
		int frameCount = Time.frameCount;
		using (WidgetConatiner.WidgetIterator widgetIterator = widgets.CreateIterator())
		{
			foreach (UIWidget item in (IEnumerable<UIWidget>)widgetIterator)
			{
				if (!(item == null) && item.enabled && !(item.panel != this))
				{
					if (item.UpdateTransform(frameCount) || mResized)
					{
						bool visibleByAlpha = flag || item.CalculateCumulativeAlpha(frameCount) > 0.001f;
						bool visibleByPanel = flag || (!flag3 && !item.hideIfOffScreen) || IsVisible(item);
						bool visibleByOcclusion = SingletonAuto<OcclusionManager>.instance.IsVisible(item);
						item.UpdateVisibility(visibleByAlpha, visibleByPanel, visibleByOcclusion);
					}
					if (item.UpdateGeometry(frameCount))
					{
						flag2 = true;
						_drawCallContainer.OnGeometryChanged(item);
					}
				}
			}
		}
		if (flag2 && onGeometryUpdated != null)
		{
			onGeometryUpdated();
		}
		mResized = false;
	}

	public void AddWidget(UIWidget w)
	{
		mUpdateScroll = true;
		widgets.Add(w);
		_drawCallContainer.AssignDrawCall(w);
	}

	public void RemoveWidget(UIWidget w)
	{
		if (widgets.Remove(w))
		{
			_drawCallContainer.OnRemoveWidget(w);
		}
	}

	private IEnumerator coDelayedRebuildAll(float delay = 0.5f)
	{
		yield return new WaitForSeconds(delay);
		_drawCallContainer.RebuildAll();
	}

	public void Refresh()
	{
		_drawCallContainer.RebuildAll();
		mUpdateFrame = -1;
		if (list.Count > 0)
		{
			list[0].LateUpdate();
		}
	}

	public virtual Vector3 CalculateConstrainOffset(Vector2 min, Vector2 max)
	{
		Vector4 vector = finalClipRegion;
		float num = vector.z * 0.5f;
		float num2 = vector.w * 0.5f;
		Vector2 minRect = new Vector2(min.x, min.y);
		Vector2 maxRect = new Vector2(max.x, max.y);
		Vector2 minArea = new Vector2(vector.x - num, vector.y - num2);
		Vector2 maxArea = new Vector2(vector.x + num, vector.y + num2);
		if (softBorderPadding && clipping == UIDrawCall.Clipping.SoftClip)
		{
			minArea.x += mClipSoftness.x;
			minArea.y += mClipSoftness.y;
			maxArea.x -= mClipSoftness.x;
			maxArea.y -= mClipSoftness.y;
		}
		return NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
	}

	public bool ConstrainTargetToBounds(Transform target, ref Bounds targetBounds, bool immediate)
	{
		Vector3 min = targetBounds.min;
		Vector3 max = targetBounds.max;
		float num = 1f;
		if (mClipping == UIDrawCall.Clipping.None)
		{
			UIRoot uIRoot = base.root;
			if (uIRoot != null)
			{
				num = uIRoot.pixelSizeAdjustment;
			}
		}
		if (num != 1f)
		{
			min /= num;
			max /= num;
		}
		Vector3 vector = CalculateConstrainOffset(min, max) * num;
		if (vector.sqrMagnitude > 0f)
		{
			if (immediate)
			{
				target.localPosition += vector;
				targetBounds.center += vector;
				SpringPosition component = target.GetComponent<SpringPosition>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
			else
			{
				SpringPosition springPosition = SpringPosition.Begin(target.gameObject, target.localPosition + vector, 13f);
				springPosition.ignoreTimeScale = true;
				springPosition.worldSpace = false;
			}
			return true;
		}
		return false;
	}

	public bool ConstrainTargetToBounds(Transform target, bool immediate, bool considerInactive = false)
	{
		Bounds targetBounds = NGUIMath.CalculateRelativeWidgetBounds(base.cachedTransform, target, considerInactive);
		return ConstrainTargetToBounds(target, ref targetBounds, immediate);
	}

	public static UIPanel Find(Transform trans)
	{
		return Find(trans, createIfMissing: false, -1);
	}

	public static UIPanel Find(Transform trans, bool createIfMissing)
	{
		return Find(trans, createIfMissing, -1);
	}

	public static UIPanel Find(Transform trans, bool createIfMissing, int layer)
	{
		UIPanel recommendedPanel = GetRecommendedPanel(trans);
		if (recommendedPanel != null)
		{
			return recommendedPanel;
		}
		recommendedPanel = NGUITools.FindInParents<UIPanel>(trans);
		if (recommendedPanel != null)
		{
			return recommendedPanel;
		}
		if (createIfMissing)
		{
			return NGUITools.CreateUI(trans, advanced3D: false, layer);
		}
		return null;
	}

	public static UIPanel GetRecommendedPanel(Transform trans)
	{
		if (RecommendedPanel == null)
		{
			return null;
		}
		Transform transform = RecommendedPanel.cachedTransform;
		Transform transform2 = trans;
		while (transform2 != null)
		{
			if (transform2 == transform)
			{
				return RecommendedPanel;
			}
			transform2 = transform2.parent;
		}
		return null;
	}

	public Vector2 GetWindowSize()
	{
		UIRoot uIRoot = base.root;
		Vector2 screenSize = NGUITools.screenSize;
		if (uIRoot != null)
		{
			screenSize *= uIRoot.GetPixelSizeAdjustment(Mathf.RoundToInt(screenSize.y));
		}
		return screenSize;
	}

	public Vector2 GetViewSize()
	{
		if (mClipping != UIDrawCall.Clipping.None)
		{
			return new Vector2(mClipRange.z, mClipRange.w);
		}
		return NGUITools.screenSize;
	}
}
