using System;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Label")]
public class UILabel : UIWidget
{
	public enum Effect
	{
		None,
		Shadow,
		Outline,
		Outline8,
		OutlineShader,
		OutlineShader8
	}

	public enum Overflow
	{
		ShrinkContent,
		ClampContent,
		ResizeFreely,
		ResizeHeight
	}

	public enum Crispness
	{
		Never,
		OnDesktop,
		Always
	}

	public Crispness keepCrispWhenShrunk = Crispness.OnDesktop;

	[HideInInspector]
	[SerializeField]
	private Font mTrueTypeFont;

	[HideInInspector]
	[SerializeField]
	private UIFont mFont;

	[Multiline(6)]
	[HideInInspector]
	[SerializeField]
	private string mText = "";

	[HideInInspector]
	[SerializeField]
	private int mFontSize = 16;

	[HideInInspector]
	[SerializeField]
	private FontStyle mFontStyle;

	[HideInInspector]
	[SerializeField]
	private NGUIText.Alignment mAlignment;

	[HideInInspector]
	[SerializeField]
	private bool mEncoding = true;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineCount;

	[HideInInspector]
	[SerializeField]
	private Effect mEffectStyle;

	[HideInInspector]
	[SerializeField]
	private Color mEffectColor = Color.black;

	[HideInInspector]
	[SerializeField]
	private NGUIText.SymbolStyle mSymbols = NGUIText.SymbolStyle.Normal;

	[HideInInspector]
	[SerializeField]
	private Vector2 mEffectDistance = Vector2.one;

	[HideInInspector]
	[SerializeField]
	private Overflow mOverflow;

	[HideInInspector]
	[SerializeField]
	private Material mMaterial;

	[HideInInspector]
	[SerializeField]
	private bool mApplyGradient;

	[HideInInspector]
	[SerializeField]
	private Color mGradientTop = Color.white;

	[HideInInspector]
	[SerializeField]
	private Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);

	[HideInInspector]
	[SerializeField]
	private int mSpacingX;

	[HideInInspector]
	[SerializeField]
	private int mSpacingY;

	[HideInInspector]
	[SerializeField]
	private bool mUseFloatSpacing;

	[HideInInspector]
	[SerializeField]
	private float mFloatSpacingX;

	[HideInInspector]
	[SerializeField]
	private float mFloatSpacingY;

	[HideInInspector]
	[SerializeField]
	private bool mShrinkToFit;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineWidth;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineHeight;

	[HideInInspector]
	[SerializeField]
	private float mLineWidth;

	[HideInInspector]
	[SerializeField]
	private bool mMultiline = true;

	[NonSerialized]
	private float mDensity = 1f;

	private bool mShouldBeProcessed = true;

	private string mProcessedText;

	private bool mPremultiply;

	private Vector2 mCalculatedSize = Vector2.zero;

	private float mScale = 1f;

	private int mPrintedSize;

	private int mLastWidth;

	private int mLastHeight;

	private Material mOutlineShaderMaterial;

	public static Dictionary<int, WeakRef<UILabel>> GlobalDic = new Dictionary<int, WeakRef<UILabel>>();

	private static bool mTexRebuildAdded = false;

	private static RenderingList<Vector3> mTempVerts = new RenderingList<Vector3>();

	private static RenderingList<int> mTempIndices = new RenderingList<int>();

	private bool shouldBeProcessed
	{
		get
		{
			return mShouldBeProcessed;
		}
		set
		{
			if (value)
			{
				mChanged = true;
				mShouldBeProcessed = true;
			}
			else
			{
				mShouldBeProcessed = false;
			}
		}
	}

	public override bool isAnchoredHorizontally
	{
		get
		{
			if (!base.isAnchoredHorizontally)
			{
				return mOverflow == Overflow.ResizeFreely;
			}
			return true;
		}
	}

	public override bool isAnchoredVertically
	{
		get
		{
			if (!base.isAnchoredVertically && mOverflow != Overflow.ResizeFreely)
			{
				return mOverflow == Overflow.ResizeHeight;
			}
			return true;
		}
	}

	public override Material material
	{
		get
		{
			if (mMaterial != null)
			{
				return mMaterial;
			}
			if (mOutlineShaderMaterial != null)
			{
				return mOutlineShaderMaterial;
			}
			if (mFont != null)
			{
				return mFont.material;
			}
			if (mTrueTypeFont != null)
			{
				return mTrueTypeFont.material;
			}
			return null;
		}
		set
		{
			if (mMaterial != value)
			{
				RemoveFromPanel();
				mMaterial = value;
				MarkAsChanged();
			}
		}
	}

	[Obsolete("Use UILabel.bitmapFont instead")]
	public UIFont font
	{
		get
		{
			return bitmapFont;
		}
		set
		{
			bitmapFont = value;
		}
	}

	public UIFont bitmapFont
	{
		get
		{
			return mFont;
		}
		set
		{
			if (mFont != value)
			{
				RemoveFromPanel();
				DestroyOutlineShaderMaterial();
				mFont = value;
				mTrueTypeFont = null;
				MarkAsChanged();
			}
		}
	}

	public Font trueTypeFont
	{
		get
		{
			if (mTrueTypeFont != null)
			{
				return mTrueTypeFont;
			}
			if (!(mFont != null))
			{
				return null;
			}
			return mFont.dynamicFont;
		}
		set
		{
			if (mTrueTypeFont != value)
			{
				SetActiveFont(null);
				RemoveFromPanel();
				DestroyOutlineShaderMaterial();
				mTrueTypeFont = value;
				shouldBeProcessed = true;
				mFont = null;
				SetActiveFont(value);
				ProcessAndRequest();
				base.MarkAsChanged();
			}
		}
	}

	public UnityEngine.Object ambigiousFont
	{
		get
		{
			if (!(mFont != null))
			{
				return mTrueTypeFont;
			}
			return mFont;
		}
		set
		{
			UIFont uIFont = value as UIFont;
			if (uIFont != null)
			{
				bitmapFont = uIFont;
			}
			else
			{
				trueTypeFont = value as Font;
			}
		}
	}

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			if (mText == value)
			{
				return;
			}
			if (string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(mText))
				{
					mText = "";
					MarkAsChanged();
					ProcessAndRequest();
				}
			}
			else if (mText != value)
			{
				mText = value;
				MarkAsChanged();
				ProcessAndRequest();
			}
			if (autoResizeBoxCollider)
			{
				ResizeCollider();
			}
		}
	}

	public int defaultFontSize
	{
		get
		{
			if (!(trueTypeFont != null))
			{
				if (!(mFont != null))
				{
					return 16;
				}
				return mFont.defaultSize;
			}
			return mFontSize;
		}
	}

	public int fontSize
	{
		get
		{
			return mFontSize;
		}
		set
		{
			value = Mathf.Clamp(value, 0, 256);
			if (mFontSize != value)
			{
				mFontSize = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public FontStyle fontStyle
	{
		get
		{
			return mFontStyle;
		}
		set
		{
			if (mFontStyle != value)
			{
				mFontStyle = value;
				DestroyOutlineShaderMaterial();
				shouldBeProcessed = true;
				MarkAsChanged();
				ProcessAndRequest();
			}
		}
	}

	public NGUIText.Alignment alignment
	{
		get
		{
			return mAlignment;
		}
		set
		{
			if (mAlignment != value)
			{
				mAlignment = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public bool applyGradient
	{
		get
		{
			return mApplyGradient;
		}
		set
		{
			if (mApplyGradient != value)
			{
				mApplyGradient = value;
				MarkAsChanged();
			}
		}
	}

	public Color gradientTop
	{
		get
		{
			return mGradientTop;
		}
		set
		{
			if (mGradientTop != value)
			{
				mGradientTop = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public Color gradientBottom
	{
		get
		{
			return mGradientBottom;
		}
		set
		{
			if (mGradientBottom != value)
			{
				mGradientBottom = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public int spacingX
	{
		get
		{
			return mSpacingX;
		}
		set
		{
			if (mSpacingX != value)
			{
				mSpacingX = value;
				MarkAsChanged();
			}
		}
	}

	public int spacingY
	{
		get
		{
			return mSpacingY;
		}
		set
		{
			if (mSpacingY != value)
			{
				mSpacingY = value;
				MarkAsChanged();
			}
		}
	}

	public bool useFloatSpacing
	{
		get
		{
			return mUseFloatSpacing;
		}
		set
		{
			if (mUseFloatSpacing != value)
			{
				mUseFloatSpacing = value;
				shouldBeProcessed = true;
			}
		}
	}

	public float floatSpacingX
	{
		get
		{
			return mFloatSpacingX;
		}
		set
		{
			if (!Mathf.Approximately(mFloatSpacingX, value))
			{
				mFloatSpacingX = value;
				MarkAsChanged();
			}
		}
	}

	public float floatSpacingY
	{
		get
		{
			return mFloatSpacingY;
		}
		set
		{
			if (!Mathf.Approximately(mFloatSpacingY, value))
			{
				mFloatSpacingY = value;
				MarkAsChanged();
			}
		}
	}

	public float effectiveSpacingY
	{
		get
		{
			if (!mUseFloatSpacing)
			{
				return mSpacingY;
			}
			return mFloatSpacingY;
		}
	}

	public float effectiveSpacingX
	{
		get
		{
			if (!mUseFloatSpacing)
			{
				return mSpacingX;
			}
			return mFloatSpacingX;
		}
	}

	private bool keepCrisp
	{
		get
		{
			if (trueTypeFont != null && keepCrispWhenShrunk != Crispness.Never)
			{
				return true;
			}
			return false;
		}
	}

	public bool supportEncoding
	{
		get
		{
			return mEncoding;
		}
		set
		{
			if (mEncoding != value)
			{
				mEncoding = value;
				shouldBeProcessed = true;
			}
		}
	}

	public NGUIText.SymbolStyle symbolStyle
	{
		get
		{
			return mSymbols;
		}
		set
		{
			if (mSymbols != value)
			{
				mSymbols = value;
				shouldBeProcessed = true;
			}
		}
	}

	public Overflow overflowMethod
	{
		get
		{
			return mOverflow;
		}
		set
		{
			if (mOverflow != value)
			{
				mOverflow = value;
				shouldBeProcessed = true;
			}
		}
	}

	[Obsolete("Use 'width' instead")]
	public int lineWidth
	{
		get
		{
			return base.width;
		}
		set
		{
			base.width = value;
		}
	}

	[Obsolete("Use 'height' instead")]
	public int lineHeight
	{
		get
		{
			return base.height;
		}
		set
		{
			base.height = value;
		}
	}

	public bool multiLine
	{
		get
		{
			return mMaxLineCount != 1;
		}
		set
		{
			if (mMaxLineCount != 1 != value)
			{
				mMaxLineCount = ((!value) ? 1 : 0);
				shouldBeProcessed = true;
			}
		}
	}

	public override Vector3[] localCorners
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return base.localCorners;
		}
	}

	public override Vector3[] worldCorners
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return base.worldCorners;
		}
	}

	public override Vector4 drawingDimensions
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return base.drawingDimensions;
		}
	}

	public int maxLineCount
	{
		get
		{
			return mMaxLineCount;
		}
		set
		{
			if (mMaxLineCount != value)
			{
				mMaxLineCount = Mathf.Max(value, 0);
				shouldBeProcessed = true;
				if (overflowMethod == Overflow.ShrinkContent)
				{
					MakePixelPerfect();
				}
			}
		}
	}

	public Effect rawEffectStyle
	{
		get
		{
			return mEffectStyle;
		}
		set
		{
			mEffectStyle = value;
		}
	}

	public Effect effectStyle
	{
		get
		{
			if (mEffectStyle == Effect.OutlineShader)
			{
				return Effect.Outline;
			}
			if (mEffectStyle == Effect.OutlineShader8)
			{
				return Effect.Outline8;
			}
			return mEffectStyle;
		}
		set
		{
			Effect effect = value;
			if (effect == Effect.OutlineShader)
			{
				effect = Effect.Outline;
			}
			if (effect == Effect.OutlineShader8)
			{
				effect = Effect.Outline8;
			}
			if (mEffectStyle != effect)
			{
				mEffectStyle = effect;
				shouldBeProcessed = true;
			}
		}
	}

	public Color effectColor
	{
		get
		{
			return mEffectColor;
		}
		set
		{
			if (mEffectColor != value)
			{
				mEffectColor = value;
				if (effectStyle != Effect.None)
				{
					shouldBeProcessed = true;
				}
			}
		}
	}

	public Vector2 effectDistance
	{
		get
		{
			return mEffectDistance;
		}
		set
		{
			if (mEffectDistance != value)
			{
				mEffectDistance = value;
				shouldBeProcessed = true;
			}
		}
	}

	[Obsolete("Use 'overflowMethod == UILabel.Overflow.ShrinkContent' instead")]
	public bool shrinkToFit
	{
		get
		{
			return mOverflow == Overflow.ShrinkContent;
		}
		set
		{
			if (value)
			{
				overflowMethod = Overflow.ShrinkContent;
			}
		}
	}

	public string processedText
	{
		get
		{
			if (mLastWidth != mWidth || mLastHeight != mHeight)
			{
				mLastWidth = mWidth;
				mLastHeight = mHeight;
				mShouldBeProcessed = true;
			}
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return mProcessedText;
		}
	}

	public Vector2 printedSize
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return mCalculatedSize;
		}
	}

	public override Vector2 localSize
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return base.localSize;
		}
	}

	private bool isValid
	{
		get
		{
			if (!(mFont != null))
			{
				return mTrueTypeFont != null;
			}
			return true;
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		int instanceID = GetInstanceID();
		if (!GlobalDic.ContainsKey(instanceID))
		{
			GlobalDic.Add(instanceID, new WeakRef<UILabel>(this));
		}
		SetActiveFont(trueTypeFont);
	}

	protected override void OnDisable()
	{
		SetActiveFont(null);
		GlobalDic.Remove(GetInstanceID());
		base.OnDisable();
	}

	protected void SetActiveFont(Font fnt)
	{
	}

	public static void OnFontChanged(Font font)
	{
		PoolingList<int> poolingList = default(PoolingList<int>);
		foreach (KeyValuePair<int, WeakRef<UILabel>> item in GlobalDic)
		{
			int key = item.Key;
			UILabel value = item.Value.Value;
			if (value == null)
			{
				poolingList.Add(key);
				continue;
			}
			Font font2 = value.trueTypeFont;
			if (font2 == font)
			{
				font2.RequestCharactersInTexture(value.mText, value.mPrintedSize, value.mFontStyle);
			}
		}
		for (int i = 0; i < poolingList.Count; i++)
		{
			GlobalDic.Remove(poolingList[i]);
		}
		poolingList.Clear();
		foreach (KeyValuePair<int, WeakRef<UILabel>> item2 in GlobalDic)
		{
			UILabel value2 = item2.Value.Value;
			if (!(value2 == null) && value2.trueTypeFont == font)
			{
				value2.RemoveFromPanel();
				value2.CreatePanel();
			}
		}
	}

	public override Vector3[] GetSides(Transform relativeTo)
	{
		if (shouldBeProcessed)
		{
			ProcessText();
		}
		return base.GetSides(relativeTo);
	}

	private void DestroyOutlineShaderMaterial()
	{
		if (!(mOutlineShaderMaterial == null))
		{
			VespaObject.DestroyObject(mOutlineShaderMaterial);
			mOutlineShaderMaterial = null;
		}
	}

	protected override void OnAnchor()
	{
		if (mOverflow == Overflow.ResizeFreely)
		{
			if (base.isFullyAnchored)
			{
				mOverflow = Overflow.ShrinkContent;
			}
		}
		else if (mOverflow == Overflow.ResizeHeight && topAnchor.target != null && bottomAnchor.target != null)
		{
			mOverflow = Overflow.ShrinkContent;
		}
		base.OnAnchor();
	}

	private void ProcessAndRequest()
	{
		if (ambigiousFont != null)
		{
			ProcessText();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!mTexRebuildAdded)
		{
			mTexRebuildAdded = true;
			Font.textureRebuilt += NguiFontRebuild.OnFontChanged;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (mLineWidth > 0f)
		{
			mMaxLineWidth = Mathf.RoundToInt(mLineWidth);
			mLineWidth = 0f;
		}
		if (!mMultiline)
		{
			mMaxLineCount = 1;
			mMultiline = true;
		}
		mPremultiply = material != null && material.shader != null && material.shader.name.Contains("Premultiplied");
		ProcessAndRequest();
	}

	public override void MarkAsChanged()
	{
		shouldBeProcessed = true;
		base.MarkAsChanged();
	}

	public void ProcessText()
	{
		ProcessText(legacyMode: false, full: true);
	}

	private void ProcessText(bool legacyMode, bool full)
	{
		if (!isValid)
		{
			return;
		}
		mChanged = true;
		shouldBeProcessed = false;
		float num = mDrawRegion.z - mDrawRegion.x;
		float num2 = mDrawRegion.w - mDrawRegion.y;
		NGUIText.rectWidth = ((!legacyMode) ? base.width : ((mMaxLineWidth != 0) ? mMaxLineWidth : 1000000));
		NGUIText.rectHeight = ((!legacyMode) ? base.height : ((mMaxLineHeight != 0) ? mMaxLineHeight : 1000000));
		NGUIText.regionWidth = ((num != 1f) ? Mathf.RoundToInt((float)NGUIText.rectWidth * num) : NGUIText.rectWidth);
		NGUIText.regionHeight = ((num2 != 1f) ? Mathf.RoundToInt((float)NGUIText.rectHeight * num2) : NGUIText.rectHeight);
		mPrintedSize = Mathf.Abs(legacyMode ? Mathf.RoundToInt(base.cachedTransform.localScale.x) : defaultFontSize);
		mScale = 1f;
		if (NGUIText.regionWidth < 1 || NGUIText.regionHeight < 0)
		{
			mProcessedText = "";
			return;
		}
		bool flag = trueTypeFont != null;
		if (flag && keepCrisp)
		{
			UIRoot uIRoot = base.root;
			if (uIRoot != null)
			{
				mDensity = ((uIRoot != null) ? uIRoot.pixelSizeAdjustment : 1f);
			}
		}
		else
		{
			mDensity = 1f;
		}
		if (full)
		{
			UpdateNGUIText();
		}
		if (mOverflow == Overflow.ResizeFreely)
		{
			NGUIText.rectWidth = 1000000;
			NGUIText.regionWidth = 1000000;
		}
		if (mOverflow == Overflow.ResizeFreely || mOverflow == Overflow.ResizeHeight)
		{
			NGUIText.rectHeight = 1000000;
			NGUIText.regionHeight = 1000000;
		}
		if (mPrintedSize > 0)
		{
			bool flag2 = keepCrisp;
			int num3 = mPrintedSize;
			while (num3 > 0)
			{
				if (flag2)
				{
					mPrintedSize = num3;
					NGUIText.fontSize = mPrintedSize;
				}
				else
				{
					mScale = (float)num3 / (float)mPrintedSize;
					NGUIText.fontScale = (flag ? mScale : ((float)mFontSize / (float)mFont.defaultSize * mScale));
				}
				NGUIText.Update(request: false);
				bool flag3 = NGUIText.WrapText(mText, out mProcessedText, keepCharCount: true);
				if (mOverflow == Overflow.ShrinkContent && !flag3)
				{
					if (--num3 <= 1)
					{
						break;
					}
					num3--;
					continue;
				}
				if (mOverflow == Overflow.ResizeFreely)
				{
					mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
					mWidth = Mathf.Max(minWidth, Mathf.RoundToInt(mCalculatedSize.x));
					if (num != 1f)
					{
						mWidth = Mathf.RoundToInt((float)mWidth / num);
					}
					mHeight = Mathf.Max(minHeight, Mathf.RoundToInt(mCalculatedSize.y));
					if (num2 != 1f)
					{
						mHeight = Mathf.RoundToInt((float)mHeight / num2);
					}
					if ((mWidth & 1) == 1)
					{
						mWidth++;
					}
					if ((mHeight & 1) == 1)
					{
						mHeight++;
					}
				}
				else if (mOverflow == Overflow.ResizeHeight)
				{
					mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
					mHeight = Mathf.Max(minHeight, Mathf.RoundToInt(mCalculatedSize.y));
					if (num2 != 1f)
					{
						mHeight = Mathf.RoundToInt((float)mHeight / num2);
					}
					if ((mHeight & 1) == 1)
					{
						mHeight++;
					}
				}
				else
				{
					mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
				}
				if (legacyMode)
				{
					base.width = Mathf.RoundToInt(mCalculatedSize.x);
					base.height = Mathf.RoundToInt(mCalculatedSize.y);
					base.cachedTransform.localScale = Vector3.one;
				}
				break;
			}
		}
		else
		{
			base.cachedTransform.localScale = Vector3.one;
			mProcessedText = "";
			mScale = 1f;
		}
		if (full)
		{
			NGUIText.bitmapFont = null;
			NGUIText.dynamicFont = null;
		}
	}

	public override void MakePixelPerfect()
	{
		if (ambigiousFont != null)
		{
			Vector3 localPosition = base.cachedTransform.localPosition;
			localPosition.x = Mathf.RoundToInt(localPosition.x);
			localPosition.y = Mathf.RoundToInt(localPosition.y);
			localPosition.z = Mathf.RoundToInt(localPosition.z);
			base.cachedTransform.localPosition = localPosition;
			base.cachedTransform.localScale = Vector3.one;
			if (mOverflow == Overflow.ResizeFreely)
			{
				AssumeNaturalSize();
				return;
			}
			int a = base.width;
			int a2 = base.height;
			Overflow overflow = mOverflow;
			if (overflow != Overflow.ResizeHeight)
			{
				mWidth = 100000;
			}
			mHeight = 100000;
			mOverflow = Overflow.ShrinkContent;
			ProcessText(legacyMode: false, full: true);
			mOverflow = overflow;
			int a3 = Mathf.RoundToInt(mCalculatedSize.x);
			int a4 = Mathf.RoundToInt(mCalculatedSize.y);
			a3 = Mathf.Max(a3, base.minWidth);
			a4 = Mathf.Max(a4, base.minHeight);
			mWidth = Mathf.Max(a, a3);
			mHeight = Mathf.Max(a2, a4);
			MarkAsChanged();
		}
		else
		{
			base.MakePixelPerfect();
		}
	}

	public void AssumeNaturalSize()
	{
		if (ambigiousFont != null)
		{
			mWidth = 100000;
			mHeight = 100000;
			ProcessText(legacyMode: false, full: true);
			mWidth = Mathf.RoundToInt(mCalculatedSize.x);
			mHeight = Mathf.RoundToInt(mCalculatedSize.y);
			if ((mWidth & 1) == 1)
			{
				mWidth++;
			}
			if ((mHeight & 1) == 1)
			{
				mHeight++;
			}
			MarkAsChanged();
		}
	}

	[Obsolete("Use UILabel.GetCharacterAtPosition instead")]
	public int GetCharacterIndex(Vector3 worldPos)
	{
		return GetCharacterIndexAtPosition(worldPos, precise: false);
	}

	[Obsolete("Use UILabel.GetCharacterAtPosition instead")]
	public int GetCharacterIndex(Vector2 localPos)
	{
		return GetCharacterIndexAtPosition(localPos, precise: false);
	}

	public int GetCharacterIndexAtPosition(Vector3 worldPos, bool precise)
	{
		Vector2 localPos = base.cachedTransform.InverseTransformPoint(worldPos);
		return GetCharacterIndexAtPosition(localPos, precise);
	}

	public int GetCharacterIndexAtPosition(Vector2 localPos, bool precise)
	{
		if (isValid)
		{
			string value = processedText;
			if (string.IsNullOrEmpty(value))
			{
				return 0;
			}
			UpdateNGUIText();
			if (precise)
			{
				NGUIText.PrintExactCharacterPositions(value, mTempVerts, mTempIndices);
			}
			else
			{
				NGUIText.PrintApproximateCharacterPositions(value, mTempVerts, mTempIndices);
			}
			if (mTempVerts.size > 0)
			{
				ApplyOffset(mTempVerts, 0);
				int result = (precise ? NGUIText.GetExactCharacterIndex(mTempVerts, mTempIndices, localPos) : NGUIText.GetApproximateCharacterIndex(mTempVerts, mTempIndices, localPos));
				mTempVerts.Clear();
				mTempIndices.Clear();
				NGUIText.bitmapFont = null;
				NGUIText.dynamicFont = null;
				return result;
			}
			NGUIText.bitmapFont = null;
			NGUIText.dynamicFont = null;
		}
		return 0;
	}

	public string GetWordAtPosition(Vector3 worldPos)
	{
		int characterIndexAtPosition = GetCharacterIndexAtPosition(worldPos, precise: true);
		return GetWordAtCharacterIndex(characterIndexAtPosition);
	}

	public string GetWordAtPosition(Vector2 localPos)
	{
		int characterIndexAtPosition = GetCharacterIndexAtPosition(localPos, precise: true);
		return GetWordAtCharacterIndex(characterIndexAtPosition);
	}

	public string GetWordAtCharacterIndex(int characterIndex)
	{
		if (characterIndex != -1 && characterIndex < mText.Length)
		{
			int num = mText.LastIndexOfAny(new char[2] { ' ', '\n' }, characterIndex) + 1;
			int num2 = mText.IndexOfAny(new char[4] { ' ', '\n', ',', '.' }, characterIndex);
			if (num2 == -1)
			{
				num2 = mText.Length;
			}
			if (num != num2)
			{
				int num3 = num2 - num;
				if (num3 > 0)
				{
					return NGUIText.StripSymbols(mText.Substring(num, num3));
				}
			}
		}
		return null;
	}

	public string GetUrlAtPosition(Vector3 worldPos)
	{
		return GetUrlAtCharacterIndex(GetCharacterIndexAtPosition(worldPos, precise: true));
	}

	public string GetUrlAtPosition(Vector2 localPos)
	{
		return GetUrlAtCharacterIndex(GetCharacterIndexAtPosition(localPos, precise: true));
	}

	public string GetUrlAtCharacterIndex(int characterIndex)
	{
		if (characterIndex != -1 && characterIndex < mText.Length - 6)
		{
			int num = ((mText[characterIndex] != '[' || mText[characterIndex + 1] != 'u' || mText[characterIndex + 2] != 'r' || mText[characterIndex + 3] != 'l' || mText[characterIndex + 4] != '=') ? mText.LastIndexOf("[url=", characterIndex) : characterIndex);
			if (num == -1)
			{
				return null;
			}
			num += 5;
			int num2 = mText.IndexOf("]", num);
			if (num2 == -1)
			{
				return null;
			}
			int num3 = mText.IndexOf("[/url]", num2);
			if (num3 == -1 || characterIndex <= num3)
			{
				return mText.Substring(num, num2 - num);
			}
		}
		return null;
	}

	public int GetCharacterIndex(int currentIndex, KeyCode key)
	{
		if (isValid)
		{
			string text = processedText;
			if (string.IsNullOrEmpty(text))
			{
				return 0;
			}
			int num = defaultFontSize;
			UpdateNGUIText();
			NGUIText.PrintApproximateCharacterPositions(text, mTempVerts, mTempIndices);
			if (mTempVerts.size > 0)
			{
				ApplyOffset(mTempVerts, 0);
				for (int i = 0; i < mTempIndices.size; i++)
				{
					if (mTempIndices[i] == currentIndex)
					{
						Vector2 pos = mTempVerts[i];
						switch (key)
						{
						case KeyCode.UpArrow:
							pos.y += (float)num + effectiveSpacingY;
							break;
						case KeyCode.DownArrow:
							pos.y -= (float)num + effectiveSpacingY;
							break;
						case KeyCode.Home:
							pos.x -= 1000f;
							break;
						case KeyCode.End:
							pos.x += 1000f;
							break;
						}
						int approximateCharacterIndex = NGUIText.GetApproximateCharacterIndex(mTempVerts, mTempIndices, pos);
						if (approximateCharacterIndex == currentIndex)
						{
							break;
						}
						mTempVerts.Clear();
						mTempIndices.Clear();
						return approximateCharacterIndex;
					}
				}
				mTempVerts.Clear();
				mTempIndices.Clear();
			}
			NGUIText.bitmapFont = null;
			NGUIText.dynamicFont = null;
			switch (key)
			{
			case KeyCode.UpArrow:
			case KeyCode.Home:
				return 0;
			case KeyCode.DownArrow:
			case KeyCode.End:
				return text.Length;
			}
		}
		return currentIndex;
	}

	public void PrintOverlay(int start, int end, UIGeometry caret, UIGeometry highlight, Color caretColor, Color highlightColor)
	{
		caret?.Clear();
		highlight?.Clear();
		if (!isValid)
		{
			return;
		}
		string text = processedText;
		UpdateNGUIText();
		int size = caret.verts.size;
		Vector2 item = new Vector2(0.5f, 0.5f);
		float num = finalAlpha;
		if (highlight != null && start != end)
		{
			int size2 = highlight.verts.size;
			NGUIText.PrintCaretAndSelection(text, start, end, caret.verts, highlight.verts);
			if (highlight.verts.size > size2)
			{
				ApplyOffset(highlight.verts, size2);
				Color32 item2 = new Color(highlightColor.r, highlightColor.g, highlightColor.b, highlightColor.a * num);
				highlight.uvs.AddCount(item, highlight.verts.size - size2);
				highlight.cols.AddCount(item2, highlight.verts.size - size2);
			}
		}
		else
		{
			NGUIText.PrintCaretAndSelection(text, start, end, caret.verts, null);
		}
		ApplyOffset(caret.verts, size);
		Color32 item3 = new Color(caretColor.r, caretColor.g, caretColor.b, caretColor.a * num);
		caret.uvs.AddCount(item, caret.verts.size - size);
		caret.cols.AddCount(item3, caret.verts.size - size);
		NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;
	}

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		if (!isValid)
		{
			return;
		}
		int offset = verts.size;
		Color color = base.color;
		color.a = finalAlpha;
		if (mFont != null && mFont.premultipliedAlphaShader)
		{
			color = NGUITools.ApplyPMA(color);
		}
		if (QualitySettings.activeColorSpace == ColorSpace.Linear)
		{
			color.r = Mathf.Pow(color.r, 2.2f);
			color.g = Mathf.Pow(color.g, 2.2f);
			color.b = Mathf.Pow(color.b, 2.2f);
		}
		string obj = processedText;
		int size = verts.size;
		UpdateNGUIText();
		NGUIText.tint = color;
		NGUIText.Print(obj, verts, uvs, cols);
		NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;
		ApplyOffset(verts, size);
		if (mFont != null && mFont.packedFontShader)
		{
			return;
		}
		if (effectStyle == Effect.OutlineShader || effectStyle == Effect.OutlineShader8)
		{
			if (mOutlineShaderMaterial == null)
			{
				mOutlineShaderMaterial = new Material(material);
				mOutlineShaderMaterial.hideFlags = HideFlags.HideAndDontSave;
				if (effectStyle == Effect.OutlineShader)
				{
					mOutlineShaderMaterial.shader = Shader.Find("Vespa/NGUI/TextOutline");
				}
				else
				{
					mOutlineShaderMaterial.shader = Shader.Find("Vespa/NGUI/TextOutline8");
				}
			}
			if (mOutlineShaderMaterial.mainTexture != null)
			{
				Vector3 vector = (panel.worldToLocal2.Value * base.cachedTransform.localToWorldMatrix).MultiplyVector(mEffectDistance);
				mOutlineShaderMaterial.SetFloat("_OutlineX", vector.x);
				mOutlineShaderMaterial.SetFloat("_OutlineY", vector.y);
			}
		}
		else
		{
			DestroyOutlineShaderMaterial();
		}
		if (effectStyle != Effect.None && effectStyle != Effect.OutlineShader && effectStyle != Effect.OutlineShader8)
		{
			FillEffect2(verts, uvs, cols, ref offset);
		}
		if (onPostFill != null)
		{
			onPostFill(this, offset, verts, uvs, cols);
		}
	}

	private void FillEffect2(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols, ref int offset)
	{
		int size = verts.size;
		List<Vector2> list = new List<Vector2>();
		list.Add(mEffectDistance);
		if (effectStyle == Effect.Outline || effectStyle == Effect.Outline8)
		{
			list.Add(new Vector2(0f - mEffectDistance.x, mEffectDistance.y));
			list.Add(new Vector2(mEffectDistance.x, 0f - mEffectDistance.y));
			list.Add(new Vector2(0f - mEffectDistance.x, 0f - mEffectDistance.y));
			if (effectStyle == Effect.Outline8)
			{
				list.Add(new Vector2(0f - mEffectDistance.x, 0f));
				list.Add(new Vector2(mEffectDistance.x, 0f));
				list.Add(new Vector2(0f, mEffectDistance.y));
				list.Add(new Vector2(0f, 0f - mEffectDistance.y));
			}
		}
		ApplyShadows(verts, uvs, cols, offset, size, list);
		offset += (size - offset) * list.Count;
	}

	private void FillEffect(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols, ref int offset)
	{
		int size = verts.size;
		Vector2 vector = default(Vector2);
		vector.x = mEffectDistance.x;
		vector.y = mEffectDistance.y;
		ApplyShadow(verts, uvs, cols, offset, size, vector.x, 0f - vector.y);
		if (effectStyle == Effect.Outline || effectStyle == Effect.Outline8)
		{
			offset = size;
			size = verts.size;
			ApplyShadow(verts, uvs, cols, offset, size, 0f - vector.x, vector.y);
			offset = size;
			size = verts.size;
			ApplyShadow(verts, uvs, cols, offset, size, vector.x, 0f - vector.y);
			offset = size;
			size = verts.size;
			ApplyShadow(verts, uvs, cols, offset, size, 0f - vector.x, 0f - vector.y);
			if (effectStyle == Effect.Outline8)
			{
				offset = size;
				size = verts.size;
				ApplyShadow(verts, uvs, cols, offset, size, 0f - vector.x, 0f);
				offset = size;
				size = verts.size;
				ApplyShadow(verts, uvs, cols, offset, size, vector.x, 0f);
				offset = size;
				size = verts.size;
				ApplyShadow(verts, uvs, cols, offset, size, 0f, vector.y);
				offset = size;
				size = verts.size;
				ApplyShadow(verts, uvs, cols, offset, size, 0f, 0f - vector.y);
			}
		}
	}

	public Vector2 ApplyOffset(RenderingList<Vector3> verts, int start)
	{
		Vector2 vector = base.pivotOffset;
		float f = Mathf.Lerp(0f, -mWidth, vector.x);
		float f2 = Mathf.Lerp(mHeight, 0f, vector.y) + Mathf.Lerp(mCalculatedSize.y - (float)mHeight, 0f, vector.y);
		f = Mathf.Round(f);
		f2 = Mathf.Round(f2);
		for (int i = start; i < verts.size; i++)
		{
			verts.buffer[i].x += f;
			verts.buffer[i].y += f2;
		}
		return new Vector2(f, f2);
	}

	public void ApplyShadows(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols, int start, int end, List<Vector2> shadowList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(shadowList))
		{
			return;
		}
		int count = shadowList.Count;
		int num = end - start;
		uvs.ReserveCount(num * count);
		for (int i = 0; i < count; i++)
		{
			uvs.AddArray(uvs.buffer, start, num);
		}
		verts.AddEmpty(num * count);
		cols.AddEmpty(num * count);
		Array.Copy(verts.buffer, start, verts.buffer, start + count * num, num);
		Array.Copy(cols.buffer, start, cols.buffer, start + count * num, num);
		int num2 = start + count * num;
		Vector3[] buffer = verts.buffer;
		for (int j = 0; j < count; j++)
		{
			Vector2 vector = shadowList[j];
			int num3 = start + num * j;
			for (int k = 0; k < num; k++)
			{
				int num4 = num2 + k;
				int num5 = num3 + k;
				Vector3 vector2 = buffer[num4];
				vector2.x += vector.x;
				vector2.y += vector.y;
				buffer[num5] = vector2;
			}
		}
		if (bitmapFont != null && bitmapFont.premultipliedAlphaShader)
		{
			Color c = mEffectColor;
			c.a *= finalAlpha;
			Color32 color = NGUITools.ApplyPMA(c);
			Color32[] buffer2 = cols.buffer;
			for (int l = 0; l < count; l++)
			{
				_ = shadowList[l];
				int num6 = start + num * l;
				for (int m = 0; m < num; m++)
				{
					int num7 = num2 + m;
					int num8 = num6 + m;
					Color32 color2 = buffer2[num7];
					Color32 color3 = color;
					int a = color2.a;
					a = a * color.a / 255;
					color3.a = (byte)a;
					Color c2 = color3;
					buffer2[num8] = NGUITools.ApplyPMA(c2);
				}
			}
			return;
		}
		Color color4 = mEffectColor;
		color4.a *= finalAlpha;
		Color32 color5 = color4;
		Color32[] buffer3 = cols.buffer;
		for (int n = 0; n < count; n++)
		{
			_ = shadowList[n];
			int num9 = start + num * n;
			for (int num10 = 0; num10 < num; num10++)
			{
				int num11 = num2 + num10;
				int num12 = num9 + num10;
				Color32 color6 = buffer3[num11];
				Color32 color7 = color5;
				int a2 = color6.a;
				a2 = a2 * color5.a / 255;
				color7.a = (byte)a2;
				buffer3[num12] = color7;
			}
		}
	}

	public void ApplyShadow(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols, int start, int end, float x, float y)
	{
		Color color = mEffectColor;
		color.a *= finalAlpha;
		Color32 color2 = ((bitmapFont != null && bitmapFont.premultipliedAlphaShader) ? NGUITools.ApplyPMA(color) : color);
		verts.ReserveCount(end - start);
		uvs.ReserveCount(end - start);
		cols.ReserveCount(end - start);
		verts.AddArray(verts.buffer, start, end - start);
		uvs.AddArray(uvs.buffer, start, end - start);
		cols.AddArray(cols.buffer, start, end - start);
		for (int i = start; i < end; i++)
		{
			Vector3 vector = verts.buffer[i];
			vector.x += x;
			vector.y += y;
			verts.buffer[i] = vector;
			Color32 color3 = cols.buffer[i];
			if (color3.a == byte.MaxValue)
			{
				cols.buffer[i] = color2;
				continue;
			}
			Color color4 = color;
			color4.a = (float)(int)color3.a / 255f * color.a;
			cols.buffer[i] = ((bitmapFont != null && bitmapFont.premultipliedAlphaShader) ? NGUITools.ApplyPMA(color4) : color4);
		}
	}

	public int CalculateOffsetToFit(string text)
	{
		UpdateNGUIText();
		NGUIText.encoding = false;
		NGUIText.symbolStyle = NGUIText.SymbolStyle.None;
		int result = NGUIText.CalculateOffsetToFit(text);
		NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;
		return result;
	}

	public void SetCurrentProgress()
	{
		if (UIProgressBar.current != null)
		{
			text = UIProgressBar.current.value.ToString("F");
		}
	}

	public void SetCurrentPercent()
	{
		if (UIProgressBar.current != null)
		{
			text = Mathf.RoundToInt(UIProgressBar.current.value * 100f) + "%";
		}
	}

	public void SetCurrentSelection()
	{
		if (UIPopupList.current != null)
		{
			text = (UIPopupList.current.isLocalized ? Localization.Get(UIPopupList.current.value) : UIPopupList.current.value);
		}
	}

	public bool Wrap(string text, out string final)
	{
		return Wrap(text, out final, 1000000);
	}

	public bool Wrap(string text, out string final, int height)
	{
		UpdateNGUIText();
		NGUIText.rectHeight = height;
		NGUIText.regionHeight = height;
		bool result = NGUIText.WrapText(text, out final);
		NGUIText.bitmapFont = null;
		NGUIText.dynamicFont = null;
		return result;
	}

	public void UpdateNGUIText(bool useValidFontSize = false)
	{
		Font font = trueTypeFont;
		bool flag = font != null;
		NGUIText.fontSize = mPrintedSize;
		if (useValidFontSize && NGUIText.fontSize == 0)
		{
			NGUIText.fontSize = defaultFontSize;
		}
		NGUIText.fontStyle = mFontStyle;
		NGUIText.rectWidth = mWidth;
		NGUIText.rectHeight = mHeight;
		NGUIText.regionWidth = Mathf.RoundToInt((float)mWidth * (mDrawRegion.z - mDrawRegion.x));
		NGUIText.regionHeight = Mathf.RoundToInt((float)mHeight * (mDrawRegion.w - mDrawRegion.y));
		NGUIText.gradient = mApplyGradient && (mFont == null || !mFont.packedFontShader);
		NGUIText.gradientTop = mGradientTop;
		NGUIText.gradientBottom = mGradientBottom;
		NGUIText.encoding = mEncoding;
		NGUIText.premultiply = mPremultiply;
		NGUIText.symbolStyle = mSymbols;
		NGUIText.maxLines = mMaxLineCount;
		NGUIText.spacingX = effectiveSpacingX;
		NGUIText.spacingY = effectiveSpacingY;
		NGUIText.fontScale = (flag ? mScale : ((float)mFontSize / (float)mFont.defaultSize * mScale));
		if (mFont != null)
		{
			NGUIText.bitmapFont = mFont;
			while (true)
			{
				UIFont replacement = NGUIText.bitmapFont.replacement;
				if (replacement == null)
				{
					break;
				}
				NGUIText.bitmapFont = replacement;
			}
			if (NGUIText.bitmapFont.isDynamic)
			{
				NGUIText.dynamicFont = NGUIText.bitmapFont.dynamicFont;
				NGUIText.bitmapFont = null;
			}
			else
			{
				NGUIText.dynamicFont = null;
			}
		}
		else
		{
			NGUIText.dynamicFont = font;
			NGUIText.bitmapFont = null;
		}
		if (flag && keepCrisp)
		{
			UIRoot uIRoot = base.root;
			if (uIRoot != null)
			{
				NGUIText.pixelDensity = ((uIRoot != null) ? uIRoot.pixelSizeAdjustment : 1f);
			}
		}
		else
		{
			NGUIText.pixelDensity = 1f;
		}
		if (mDensity != NGUIText.pixelDensity)
		{
			ProcessText(legacyMode: false, full: false);
			NGUIText.rectWidth = mWidth;
			NGUIText.rectHeight = mHeight;
			NGUIText.regionWidth = Mathf.RoundToInt((float)mWidth * (mDrawRegion.z - mDrawRegion.x));
			NGUIText.regionHeight = Mathf.RoundToInt((float)mHeight * (mDrawRegion.w - mDrawRegion.y));
		}
		if (alignment == NGUIText.Alignment.Automatic)
		{
			switch (base.pivot)
			{
			case Pivot.TopLeft:
			case Pivot.Left:
			case Pivot.BottomLeft:
				NGUIText.alignment = NGUIText.Alignment.Left;
				break;
			case Pivot.TopRight:
			case Pivot.Right:
			case Pivot.BottomRight:
				NGUIText.alignment = NGUIText.Alignment.Right;
				break;
			default:
				NGUIText.alignment = NGUIText.Alignment.Center;
				break;
			}
		}
		else
		{
			NGUIText.alignment = alignment;
		}
		NGUIText.Update();
	}
}
