using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Popup List")]
public class UIPopupList : UIWidgetContainer, UICamera.IOnClickListener, UICamera.IOnDoubleClickListener
{
	public enum Position
	{
		Auto,
		Above,
		Below
	}

	public enum OpenOn
	{
		ClickOrTap,
		RightClick,
		DoubleClick,
		Manual
	}

	public delegate void LegacyEvent(string val);

	public static UIPopupList current;

	private const float animSpeed = 0.15f;

	public UIAtlas atlas;

	public UIFont bitmapFont;

	public Font trueTypeFont;

	public int fontSize = 16;

	public FontStyle fontStyle;

	public string backgroundSprite;

	public string highlightSprite;

	public Position position;

	public NGUIText.Alignment alignment = NGUIText.Alignment.Left;

	public List<string> items = new List<string>();

	public List<object> itemData = new List<object>();

	public Vector2 padding = new Vector3(4f, 4f);

	public Color textColor = Color.white;

	public Color backgroundColor = Color.white;

	public Color highlightColor = new Color(0.88235295f, 40f / 51f, 0.5882353f, 1f);

	public bool isAnimated = true;

	public bool isLocalized;

	public OpenOn openOn;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	[HideInInspector]
	[SerializeField]
	private string mSelectedItem;

	[HideInInspector]
	[SerializeField]
	private UIPanel mPanel;

	[HideInInspector]
	[SerializeField]
	private GameObject mChild;

	[HideInInspector]
	[SerializeField]
	private UISprite mBackground;

	[HideInInspector]
	[SerializeField]
	private UISprite mHighlight;

	[HideInInspector]
	[SerializeField]
	private UILabel mHighlightedLabel;

	[HideInInspector]
	[SerializeField]
	private List<UILabel> mLabelList = new List<UILabel>();

	[HideInInspector]
	[SerializeField]
	private float mBgBorder;

	[NonSerialized]
	private GameObject mSelection;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string functionName = "OnSelectionChange";

	[HideInInspector]
	[SerializeField]
	private float textScale;

	[HideInInspector]
	[SerializeField]
	private UIFont font;

	[HideInInspector]
	[SerializeField]
	private UILabel textLabel;

	private LegacyEvent mLegacyEvent;

	private bool mUseDynamicFont;

	private bool mTweening;

	public UnityEngine.Object ambigiousFont
	{
		get
		{
			if (trueTypeFont != null)
			{
				return trueTypeFont;
			}
			if (bitmapFont != null)
			{
				return bitmapFont;
			}
			return font;
		}
		set
		{
			if (value is Font)
			{
				trueTypeFont = value as Font;
				bitmapFont = null;
				font = null;
			}
			else if (value is UIFont)
			{
				bitmapFont = value as UIFont;
				trueTypeFont = null;
				font = null;
			}
		}
	}

	[Obsolete("Use EventDelegate.Add(popup.onChange, YourCallback) instead, and UIPopupList.current.value to determine the state")]
	public LegacyEvent onSelectionChange
	{
		get
		{
			return mLegacyEvent;
		}
		set
		{
			mLegacyEvent = value;
		}
	}

	public bool isOpen => mChild != null;

	public string value
	{
		get
		{
			return mSelectedItem;
		}
		set
		{
			mSelectedItem = value;
			if (mSelectedItem != null && mSelectedItem != null)
			{
				TriggerCallbacks();
			}
		}
	}

	public object data
	{
		get
		{
			int num = items.IndexOf(mSelectedItem);
			if (num <= -1 || num >= itemData.Count)
			{
				return null;
			}
			return itemData[num];
		}
	}

	[Obsolete("Use 'value' instead")]
	public string selection
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	private bool handleEvents
	{
		get
		{
			UIKeyNavigation component = GetComponent<UIKeyNavigation>();
			if (!(component == null))
			{
				return !component.enabled;
			}
			return true;
		}
		set
		{
			UIKeyNavigation component = GetComponent<UIKeyNavigation>();
			if (component != null)
			{
				component.enabled = !value;
			}
		}
	}

	private bool isValid
	{
		get
		{
			if (!(bitmapFont != null))
			{
				return trueTypeFont != null;
			}
			return true;
		}
	}

	private int activeFontSize
	{
		get
		{
			if (!(trueTypeFont != null) && !(bitmapFont == null))
			{
				return bitmapFont.defaultSize;
			}
			return fontSize;
		}
	}

	private float activeFontScale
	{
		get
		{
			if (!(trueTypeFont != null) && !(bitmapFont == null))
			{
				return (float)fontSize / (float)bitmapFont.defaultSize;
			}
			return 1f;
		}
	}

	public void Clear()
	{
		items.Clear();
		itemData.Clear();
	}

	public void AddItem(string text)
	{
		items.Add(text);
		itemData.Add(null);
	}

	public void AddItem(string text, object data)
	{
		items.Add(text);
		itemData.Add(data);
	}

	protected void TriggerCallbacks()
	{
		if (current != this)
		{
			UIPopupList uIPopupList = current;
			current = this;
			if (mLegacyEvent != null)
			{
				mLegacyEvent(mSelectedItem);
			}
			if (EventDelegate.IsValid(onChange))
			{
				EventDelegate.Execute(onChange);
			}
			else if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
			{
				Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(eventReceiver, functionName, mSelectedItem, SendMessageOptions.DontRequireReceiver);
			}
			current = uIPopupList;
		}
	}

	private void OnEnable()
	{
		if (EventDelegate.IsValid(onChange))
		{
			eventReceiver = null;
			functionName = null;
		}
		if (font != null)
		{
			if (font.isDynamic)
			{
				trueTypeFont = font.dynamicFont;
				fontStyle = font.dynamicFontStyle;
				mUseDynamicFont = true;
			}
			else if (bitmapFont == null)
			{
				bitmapFont = font;
				mUseDynamicFont = false;
			}
			font = null;
		}
		if (textScale != 0f)
		{
			fontSize = ((bitmapFont != null) ? Mathf.RoundToInt((float)bitmapFont.defaultSize * textScale) : 16);
			textScale = 0f;
		}
		if (trueTypeFont == null && bitmapFont != null && bitmapFont.isDynamic)
		{
			trueTypeFont = bitmapFont.dynamicFont;
			bitmapFont = null;
		}
	}

	private void OnValidate()
	{
		Font font = trueTypeFont;
		UIFont uIFont = bitmapFont;
		bitmapFont = null;
		trueTypeFont = null;
		if (font != null && (uIFont == null || !mUseDynamicFont))
		{
			bitmapFont = null;
			trueTypeFont = font;
			mUseDynamicFont = true;
		}
		else if (uIFont != null)
		{
			if (uIFont.isDynamic)
			{
				trueTypeFont = uIFont.dynamicFont;
				fontStyle = uIFont.dynamicFontStyle;
				fontSize = uIFont.defaultSize;
				mUseDynamicFont = true;
			}
			else
			{
				bitmapFont = uIFont;
				mUseDynamicFont = false;
			}
		}
		else
		{
			trueTypeFont = font;
			mUseDynamicFont = true;
		}
	}

	private void Start()
	{
		if (textLabel != null)
		{
			EventDelegate.Add(onChange, textLabel.SetCurrentSelection);
			textLabel = null;
		}
		if (!Application.isPlaying)
		{
			return;
		}
		if (string.IsNullOrEmpty(mSelectedItem))
		{
			if (items.Count > 0)
			{
				value = items[0];
			}
		}
		else
		{
			string text = mSelectedItem;
			mSelectedItem = null;
			value = text;
		}
	}

	private void OnLocalize()
	{
		if (isLocalized)
		{
			TriggerCallbacks();
		}
	}

	private void Highlight(UILabel lbl, bool instant)
	{
		if (!(mHighlight != null))
		{
			return;
		}
		mHighlightedLabel = lbl;
		if (mHighlight.GetAtlasSprite() == null)
		{
			return;
		}
		Vector3 highlightPosition = GetHighlightPosition();
		if (instant || !isAnimated)
		{
			mHighlight.cachedTransform.localPosition = highlightPosition;
			return;
		}
		TweenPosition.Begin(mHighlight.gameObject, 0.1f, highlightPosition).method = UITweener.Method.EaseOut;
		if (!mTweening)
		{
			mTweening = true;
			VespaCoroutine.Start(this, UpdateTweenPosition());
		}
	}

	private Vector3 GetHighlightPosition()
	{
		if (mHighlightedLabel == null || mHighlight == null)
		{
			return Vector3.zero;
		}
		UISpriteData atlasSprite = mHighlight.GetAtlasSprite();
		if (atlasSprite == null)
		{
			return Vector3.zero;
		}
		float num = atlas.pixelSize / atlas.finalAtlasScale;
		float num2 = (float)atlasSprite.BorderLeft * num;
		float y = (float)atlasSprite.BorderTop * num;
		return mHighlightedLabel.cachedTransform.localPosition + new Vector3(0f - num2, y, 1f);
	}

	private IEnumerator UpdateTweenPosition()
	{
		if (mHighlight != null && mHighlightedLabel != null)
		{
			TweenPosition tp = mHighlight.GetComponent<TweenPosition>();
			while (tp != null && tp.enabled)
			{
				tp.to = GetHighlightPosition();
				yield return null;
			}
		}
		mTweening = false;
	}

	private void OnItemHover(GameObject go, bool isOver)
	{
		if (isOver)
		{
			UILabel component = go.GetComponent<UILabel>();
			Highlight(component, instant: false);
		}
	}

	private void Select(UILabel lbl, bool instant)
	{
		Highlight(lbl, instant);
		UIEventListener component = lbl.gameObject.GetComponent<UIEventListener>();
		value = component.parameter as string;
		UIPlaySound[] components = GetComponents<UIPlaySound>();
		int i = 0;
		for (int num = components.Length; i < num; i++)
		{
			UIPlaySound uIPlaySound = components[i];
			if (uIPlaySound.trigger == UIPlaySound.Trigger.OnClick)
			{
				NGUITools.PlaySound(uIPlaySound.audioClip, uIPlaySound.volume, 1f);
			}
		}
	}

	private void OnItemPress(GameObject go, bool isPressed)
	{
		if (isPressed)
		{
			Select(go.GetComponent<UILabel>(), instant: true);
		}
	}

	private void OnItemClick(GameObject go)
	{
		Close();
	}

	private void OnKey(KeyCode key)
	{
		if (!base.enabled || !NGUITools.GetActive(base.gameObject) || !handleEvents)
		{
			return;
		}
		int num = mLabelList.IndexOf(mHighlightedLabel);
		if (num == -1)
		{
			num = 0;
		}
		switch (key)
		{
		case KeyCode.UpArrow:
			if (num > 0)
			{
				Select(mLabelList[--num], instant: false);
			}
			break;
		case KeyCode.DownArrow:
			if (num + 1 < mLabelList.Count)
			{
				Select(mLabelList[++num], instant: false);
			}
			break;
		case KeyCode.Escape:
			OnSelect(isSelected: false);
			break;
		}
	}

	private void OnDisable()
	{
		Close();
	}

	private void OnSelect(bool isSelected)
	{
		if (!isSelected)
		{
			Close();
		}
	}

	public void Close()
	{
		StopCoroutine("CloseIfUnselected");
		mSelection = null;
		if (!(mChild != null))
		{
			return;
		}
		mLabelList.Clear();
		handleEvents = false;
		if (isAnimated)
		{
			UIWidget[] componentsInChildren = mChild.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				UIWidget obj = componentsInChildren[i];
				Color color = obj.color;
				color.a = 0f;
				TweenColor.Begin(obj.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
			}
			Collider[] componentsInChildren2 = mChild.GetComponentsInChildren<Collider>();
			int j = 0;
			for (int num2 = componentsInChildren2.Length; j < num2; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
			VespaObject.DestroyGo(mChild, 0.15f);
		}
		else
		{
			VespaObject.DestroyGo(mChild);
		}
		mBackground = null;
		mHighlight = null;
		mChild = null;
	}

	private void AnimateColor(UIWidget widget)
	{
		Color color = widget.color;
		widget.color = new Color(color.r, color.g, color.b, 0f);
		TweenColor.Begin(widget.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
	}

	private void AnimatePosition(UIWidget widget, bool placeAbove, float bottom)
	{
		Vector3 localPosition = widget.cachedTransform.localPosition;
		Vector3 localPosition2 = (placeAbove ? new Vector3(localPosition.x, bottom, localPosition.z) : new Vector3(localPosition.x, 0f, localPosition.z));
		widget.cachedTransform.localPosition = localPosition2;
		TweenPosition.Begin(widget.gameObject, 0.15f, localPosition).method = UITweener.Method.EaseOut;
	}

	private void AnimateScale(UIWidget widget, bool placeAbove, float bottom)
	{
		GameObject go = widget.gameObject;
		Transform cachedTransform = widget.cachedTransform;
		float num = (float)activeFontSize * activeFontScale + mBgBorder * 2f;
		cachedTransform.localScale = new Vector3(1f, num / (float)widget.height, 1f);
		TweenScale.Begin(go, 0.15f, Vector3.one).method = UITweener.Method.EaseOut;
		if (placeAbove)
		{
			Vector3 localPosition = cachedTransform.localPosition;
			cachedTransform.localPosition = new Vector3(localPosition.x, localPosition.y - (float)widget.height + num, localPosition.z);
			TweenPosition.Begin(go, 0.15f, localPosition).method = UITweener.Method.EaseOut;
		}
	}

	private void Animate(UIWidget widget, bool placeAbove, float bottom)
	{
		AnimateColor(widget);
		AnimatePosition(widget, placeAbove, bottom);
	}

	public void OnClick()
	{
		if (openOn != OpenOn.DoubleClick && openOn != OpenOn.Manual && (openOn != OpenOn.RightClick || UICamera.currentTouchID == -2))
		{
			Show();
		}
	}

	public void OnDoubleClick()
	{
		if (openOn == OpenOn.DoubleClick)
		{
			Show();
		}
	}

	private IEnumerator CloseIfUnselected()
	{
		do
		{
			yield return null;
		}
		while (!(UICamera.selectedObject != mSelection));
		Close();
	}

	public void Show()
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && mChild == null && atlas != null && isValid && items.Count > 0)
		{
			mLabelList.Clear();
			if (mPanel == null)
			{
				mPanel = UIPanel.Find(base.transform);
				if (mPanel == null)
				{
					return;
				}
			}
			handleEvents = true;
			Transform transform = base.transform;
			mChild = new GameObject("Drop-down List");
			mChild.layer = base.gameObject.layer;
			Transform transform2 = mChild.transform;
			transform2.parent = transform.parent;
			StopCoroutine("CloseIfUnselected");
			if (UICamera.selectedObject == null)
			{
				mSelection = base.gameObject;
				UICamera.selectedObject = mSelection;
			}
			else
			{
				mSelection = UICamera.selectedObject;
			}
			Vector3 vector2;
			Vector3 vector3;
			Vector3 vector;
			if (openOn == OpenOn.Manual && mSelection != base.gameObject)
			{
				vector = transform2.parent.InverseTransformPoint(Vespa_Unity_Camera_Extension_Method.VespaScreenToWorldPoint(mPanel.anchorCamera, UICamera.lastTouchPosition));
				vector2 = vector;
				transform2.localPosition = vector;
				vector3 = transform2.position;
			}
			else
			{
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(transform.parent, transform, considerInactive: false, considerParents: false);
				vector = bounds.min;
				vector2 = bounds.max;
				transform2.localPosition = vector;
				vector3 = transform.position;
			}
			StartCoroutine("CloseIfUnselected");
			transform2.localRotation = Quaternion.identity;
			transform2.localScale = Vector3.one;
			mBackground = NGUITools.AddSprite(mChild, atlas, backgroundSprite);
			mBackground.pivot = UIWidget.Pivot.TopLeft;
			mBackground.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
			mBackground.color = backgroundColor;
			Vector4 border = mBackground.border;
			mBgBorder = border.y;
			mBackground.cachedTransform.localPosition = new Vector3(0f, border.y, 0f);
			mHighlight = NGUITools.AddSprite(mChild, atlas, highlightSprite);
			mHighlight.pivot = UIWidget.Pivot.TopLeft;
			mHighlight.color = highlightColor;
			UISpriteData atlasSprite = mHighlight.GetAtlasSprite();
			if (atlasSprite == null)
			{
				return;
			}
			float num = atlasSprite.BorderTop;
			float num2 = activeFontSize;
			float num3 = activeFontScale;
			float num4 = num2 * num3;
			float a = 0f;
			float num5 = 0f - padding.y;
			List<UILabel> list = new List<UILabel>();
			if (!items.Contains(mSelectedItem))
			{
				mSelectedItem = null;
			}
			int i = 0;
			for (int count = items.Count; i < count; i++)
			{
				string text = items[i];
				UILabel uILabel = NGUITools.AddWidget<UILabel>(mChild);
				uILabel.name = i.ToString();
				uILabel.pivot = UIWidget.Pivot.TopLeft;
				uILabel.bitmapFont = bitmapFont;
				uILabel.trueTypeFont = trueTypeFont;
				uILabel.fontSize = fontSize;
				uILabel.fontStyle = fontStyle;
				uILabel.text = (isLocalized ? Localization.Get(text) : text);
				uILabel.color = textColor;
				uILabel.cachedTransform.localPosition = new Vector3(border.x + padding.x - uILabel.pivotOffset.x, num5, -1f);
				uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
				uILabel.alignment = alignment;
				list.Add(uILabel);
				num5 -= num4;
				num5 -= padding.y;
				a = Mathf.Max(a, uILabel.printedSize.x);
				UIEventListener uIEventListener = UIEventListener.Get(uILabel.gameObject);
				uIEventListener.onHover = OnItemHover;
				uIEventListener.onPress = OnItemPress;
				uIEventListener.onClick = OnItemClick;
				uIEventListener.parameter = text;
				if (mSelectedItem == text || (i == 0 && string.IsNullOrEmpty(mSelectedItem)))
				{
					Highlight(uILabel, instant: true);
				}
				mLabelList.Add(uILabel);
			}
			a = Mathf.Max(a, (vector2.x - vector.x) * num3 - (border.x + padding.x) * 2f);
			float num6 = a;
			Vector3 vector4 = new Vector3(num6 * 0.5f, (0f - num4) * 0.5f, 0f);
			Vector3 vector5 = new Vector3(num6, num4 + padding.y, 1f);
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				UILabel uILabel2 = list[j];
				NGUITools.AddWidgetCollider(uILabel2.gameObject);
				uILabel2.autoResizeBoxCollider = false;
				BoxCollider component = uILabel2.GetComponent<BoxCollider>();
				if (component != null)
				{
					vector4.z = component.center.z;
					component.center = vector4;
					component.size = vector5;
				}
				else
				{
					BoxCollider2D component2 = uILabel2.GetComponent<BoxCollider2D>();
					component2.offset = vector4;
					component2.size = vector5;
				}
			}
			int width = Mathf.RoundToInt(a);
			a += (border.x + padding.x) * 2f;
			num5 -= border.y;
			mBackground.width = Mathf.RoundToInt(a);
			mBackground.height = Mathf.RoundToInt(0f - num5 + border.y);
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				UILabel uILabel3 = list[k];
				uILabel3.overflowMethod = UILabel.Overflow.ShrinkContent;
				uILabel3.width = width;
			}
			float num7 = 2f * atlas.pixelSize / atlas.finalAtlasScale;
			float f = a - (border.x + padding.x) * 2f + (float)atlasSprite.BorderLeft * num7;
			float f2 = num4 + num * num7;
			mHighlight.width = Mathf.RoundToInt(f);
			mHighlight.height = Mathf.RoundToInt(f2);
			bool flag = position == Position.Above;
			if (position == Position.Auto)
			{
				UICamera uICamera = UICamera.FindCameraForLayer(mSelection.layer);
				if (uICamera != null)
				{
					flag = uICamera.cachedCamera.WorldToViewportPoint(vector3).y < 0.5f;
				}
			}
			if (isAnimated)
			{
				float bottom = num5 + num4;
				Animate(mHighlight, flag, bottom);
				int l = 0;
				for (int count4 = list.Count; l < count4; l++)
				{
					Animate(list[l], flag, bottom);
				}
				AnimateColor(mBackground);
				AnimateScale(mBackground, flag, bottom);
			}
			if (flag)
			{
				transform2.localPosition = new Vector3(vector.x, vector2.y - num5 - border.y, vector.z);
			}
			vector = transform2.localPosition;
			vector2.x = vector.x + (float)mBackground.width;
			vector2.y = vector.y - (float)mBackground.height;
			vector2.z = vector.z;
			Vector3 vector6 = mPanel.CalculateConstrainOffset(vector, vector2);
			transform2.localPosition += vector6;
		}
		else
		{
			OnSelect(isSelected: false);
		}
	}
}
