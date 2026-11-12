using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NNguiExtension;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI String Sprite (Extension)")]
public class UIStringSprite : UIWidget
{
	private class TextSpriteData
	{
		public UISpriteData Sprite;

		public int Width;

		public int Height;

		public TextSpriteData(UISpriteData sprite, int fontSize, float scaleX, float scaleY)
		{
			Sprite = sprite;
			if (fontSize > 0)
			{
				Width = Mathf.RoundToInt((float)(fontSize * Sprite.Width / Sprite.Height) * scaleX);
				Height = Mathf.RoundToInt((float)fontSize * scaleY);
			}
			else
			{
				Width = Mathf.RoundToInt((float)Sprite.Width * scaleX);
				Height = Mathf.RoundToInt((float)sprite.Height * scaleY);
			}
		}
	}

	private class SpriteLineData
	{
		public int Width;

		public int Height;

		public List<TextSpriteData> List = new List<TextSpriteData>();

		public void Add(TextSpriteData spriteData, int spacingX)
		{
			List.Add(spriteData);
			if (List.Count > 0)
			{
				Width += spacingX;
			}
			Width += spriteData.Width;
			Height = Mathf.Max(Height, spriteData.Height);
		}
	}

	private class SpriteLineCache
	{
		private string _printText;

		private UIAtlas _atlas;

		private int _fontSize;

		private float _scaleX;

		private float _scaleY;

		private int _spacingX;

		private List<SpriteLineData> _spriteLineCache;

		public List<SpriteLineData> GetSpriteLineFromPrintText(string printText, UIAtlas atlas, int fontSize, float scaleX, float scaleY, int spacingX)
		{
			if (_printText == printText && _atlas == atlas && _fontSize == fontSize && _scaleX == scaleX && _scaleY == scaleY && _spacingX == spacingX)
			{
				return _spriteLineCache;
			}
			_printText = printText;
			_atlas = atlas;
			_fontSize = fontSize;
			_scaleX = scaleX;
			_scaleY = scaleY;
			_spacingX = spacingX;
			_spriteLineCache = CreateSpriteLineFromPrintText();
			return _spriteLineCache;
		}

		private List<SpriteLineData> CreateSpriteLineFromPrintText()
		{
			if (String_Extension_Method.IsNullOrEmpty(_printText))
			{
				return null;
			}
			List<SpriteLineData> list = new List<SpriteLineData>();
			SpriteLineData spriteLineData = new SpriteLineData();
			list.Add(spriteLineData);
			for (int i = 0; i < _printText.Length; i++)
			{
				char c = _printText[i];
				if (c == '\n')
				{
					spriteLineData = new SpriteLineData();
					list.Add(spriteLineData);
					continue;
				}
				UISpriteData sprite = _atlas.GetSprite(c.ToString());
				if (sprite != null)
				{
					TextSpriteData spriteData = new TextSpriteData(sprite, _fontSize, _scaleX, _scaleY);
					spriteLineData.Add(spriteData, _spacingX);
				}
			}
			return list;
		}
	}

	[HideInInspector]
	[SerializeField]
	private UIAtlas mAtlas;

	[HideInInspector]
	[SerializeField]
	private NGUIText.Alignment mAlignment;

	[HideInInspector]
	[SerializeField]
	private float mScaleX = 1f;

	[HideInInspector]
	[SerializeField]
	private float mScaleY = 1f;

	[HideInInspector]
	[SerializeField]
	private int mFontSize;

	[HideInInspector]
	[SerializeField]
	private string mPrintText;

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

	private SpriteLineCache _spriteLineCache = new SpriteLineCache();

	private Color32 drawingColor
	{
		get
		{
			Color color = base.color;
			color.a = finalAlpha;
			if (_premultipliedAlpha)
			{
				color = NGUITools.ApplyPMA(color);
			}
			if (QualitySettings.activeColorSpace == ColorSpace.Linear)
			{
				color.r = Mathf.Pow(color.r, 2.2f);
				color.g = Mathf.Pow(color.g, 2.2f);
				color.b = Mathf.Pow(color.b, 2.2f);
			}
			return color;
		}
	}

	public override Material material
	{
		get
		{
			if (!(mAtlas != null))
			{
				return null;
			}
			return mAtlas.spriteMaterial;
		}
	}

	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if (mAtlas != value)
			{
				RemoveFromPanel();
				mAtlas = value;
				MarkAsChanged();
			}
		}
	}

	public string printText
	{
		get
		{
			return mPrintText;
		}
		set
		{
			if (String_Extension_Method.IsNullOrEmpty(value))
			{
				if (!String_Extension_Method.IsNullOrEmpty(mPrintText))
				{
					mPrintText = "";
					mChanged = true;
				}
			}
			else if (mPrintText != value)
			{
				mPrintText = value;
				mChanged = true;
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

	private float _pixelSize
	{
		get
		{
			if (!(mAtlas != null))
			{
				return 1f;
			}
			return mAtlas.pixelSize / mAtlas.finalAtlasScale;
		}
	}

	public override Vector4 drawingDimensions
	{
		get
		{
			Vector2 vector = base.pivotOffset;
			float num = (0f - vector.x) * (float)mWidth;
			float num2 = (0f - vector.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			Vector4 vector2 = ((mAtlas != null) ? (border * _pixelSize) : Vector4.zero);
			float num5 = vector2.x + vector2.z;
			float num6 = vector2.y + vector2.w;
			float x = Mathf.Lerp(num, num3 - num5, mDrawRegion.x);
			float y = Mathf.Lerp(num2, num4 - num6, mDrawRegion.y);
			float z = Mathf.Lerp(num + num5, num3, mDrawRegion.z);
			float w = Mathf.Lerp(num2 + num6, num4, mDrawRegion.w);
			return new Vector4(x, y, z, w);
		}
	}

	private bool _premultipliedAlpha
	{
		get
		{
			if (mAtlas != null)
			{
				return mAtlas.premultipliedAlpha;
			}
			return false;
		}
	}

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		if (!(mainTexture == null))
		{
			int size = verts.size;
			FillPrintText(verts, uvs, cols);
			if (onPostFill != null)
			{
				onPostFill(this, size, verts, uvs, cols);
			}
		}
	}

	private void FillPrintText(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		Texture texture = mainTexture;
		if (texture == null)
		{
			return;
		}
		List<SpriteLineData> spriteLineFromPrintText = _spriteLineCache.GetSpriteLineFromPrintText(mPrintText, atlas, mFontSize, mScaleX, mScaleY, spacingX);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(spriteLineFromPrintText))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < spriteLineFromPrintText.Count; i++)
		{
			SpriteLineData spriteLineData = spriteLineFromPrintText[i];
			num = Mathf.Max(num, spriteLineData.Width);
			if (i > 0)
			{
				num2 += spacingY;
			}
			num2 += spriteLineData.Height;
			num3 += __Extension_Method_ICollection__.COUNT(spriteLineData.List);
		}
		base.width = num;
		base.height = num2;
		Vector4 vector = drawingDimensions;
		Color32 color = drawingColor;
		int num4 = verts.Count;
		int num5 = uvs.Count;
		int num6 = cols.Count;
		verts.AddEmpty(num3 * 4);
		uvs.AddEmpty(num3 * 4);
		cols.AddEmpty(num3 * 4);
		Vector3[] buffer = verts.buffer;
		Vector2[] buffer2 = uvs.buffer;
		Color32[] buffer3 = cols.buffer;
		Color color2 = color * gradientBottom;
		Color color3 = color * gradientTop;
		float num7 = vector.w;
		for (int j = 0; j < spriteLineFromPrintText.Count; j++)
		{
			SpriteLineData spriteLineData2 = spriteLineFromPrintText[j];
			float num8 = vector.x;
			if (mAlignment == NGUIText.Alignment.Center)
			{
				num8 += (float)((num - spriteLineData2.Width) / 2);
			}
			if (mAlignment == NGUIText.Alignment.Right)
			{
				num8 += (float)(num - spriteLineData2.Width);
			}
			for (int k = 0; k < spriteLineData2.List.Count; k++)
			{
				TextSpriteData textSpriteData = spriteLineData2.List[k];
				buffer[num4] = new Vector3(num8, num7 - (float)textSpriteData.Height);
				buffer[num4 + 1] = new Vector3(num8, num7);
				buffer[num4 + 2] = new Vector3(num8 + (float)textSpriteData.Width, num7);
				buffer[num4 + 3] = new Vector3(num8 + (float)textSpriteData.Width, num7 - (float)textSpriteData.Height);
				num4 += 4;
				Rect rect = new Rect(textSpriteData.Sprite.X, textSpriteData.Sprite.Y, textSpriteData.Sprite.Width, textSpriteData.Sprite.Height);
				rect = NGUIMath.ConvertToTexCoords(rect, texture.width, texture.height);
				buffer2[num5] = new Vector2(rect.xMin, rect.yMin);
				buffer2[num5 + 1] = new Vector2(rect.xMin, rect.yMax);
				buffer2[num5 + 2] = new Vector2(rect.xMax, rect.yMax);
				buffer2[num5 + 3] = new Vector2(rect.xMax, rect.yMin);
				num5 += 4;
				if (mApplyGradient)
				{
					buffer3[num6] = color2;
					buffer3[num6 + 1] = color3;
					buffer3[num6 + 2] = color3;
					buffer3[num6 + 3] = color2;
				}
				else
				{
					buffer3[num6] = color;
					buffer3[num6 + 1] = color;
					buffer3[num6 + 2] = color;
					buffer3[num6 + 3] = color;
				}
				num6 += 4;
				num8 += (float)(textSpriteData.Width + spacingX);
			}
			num7 -= (float)(spriteLineData2.Height + spacingY);
		}
	}
}
