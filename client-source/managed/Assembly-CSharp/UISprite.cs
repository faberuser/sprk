using System;
using NVespa;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Sprite")]
public class UISprite : UIBasicSprite
{
	private class DrawingDimensionCache
	{
		public struct Precalculate
		{
			public Vector2 _pivotOffset;

			public float _pixelSize;

			public int _width;

			public int _height;

			public UISpriteData _spriteData;

			public Type _type;

			public Flip _flip;

			public float X0;

			public float X1;

			public float Y0;

			public float Y1;

			public bool Update(Vector2 pivotOffset, float pixelSize, int width, int height, UISpriteData spriteData, Type type, Flip flip)
			{
				if (_pivotOffset == pivotOffset && _pixelSize == pixelSize && _width == width && _height == height && _spriteData == spriteData && _type == type && _flip == flip)
				{
					return false;
				}
				_pivotOffset = pivotOffset;
				_pixelSize = pixelSize;
				_width = width;
				_height = height;
				_spriteData = spriteData;
				_type = type;
				_flip = flip;
				X0 = (0f - pivotOffset.x) * (float)width;
				Y0 = (0f - pivotOffset.y) * (float)height;
				X1 = X0 + (float)width;
				Y1 = Y0 + (float)height;
				if (spriteData != null && type != Type.Tiled)
				{
					int num = Mathf.RoundToInt((float)spriteData.PaddingLeft * pixelSize);
					int num2 = Mathf.RoundToInt((float)spriteData.PaddingBottom * pixelSize);
					int num3 = Mathf.RoundToInt((float)spriteData.PaddingRight * pixelSize);
					int num4 = Mathf.RoundToInt((float)spriteData.PaddingTop * pixelSize);
					int num5 = Mathf.RoundToInt((float)spriteData.Width * pixelSize) + num + num3;
					int num6 = Mathf.RoundToInt((float)spriteData.Height * pixelSize) + num2 + num4;
					float num7 = 1f;
					float num8 = 1f;
					if (num5 > 0 && num6 > 0 && (type == Type.Simple || type == Type.Filled))
					{
						if ((num5 & 1) != 0)
						{
							num3++;
						}
						if ((num6 & 1) != 0)
						{
							num4++;
						}
						num7 = 1f / (float)num5 * (float)width;
						num8 = 1f / (float)num6 * (float)height;
					}
					if (flip == Flip.Horizontally || flip == Flip.Both)
					{
						X0 += (float)num3 * num7;
						X1 -= (float)num * num7;
					}
					else
					{
						X0 += (float)num * num7;
						X1 -= (float)num3 * num7;
					}
					if (flip == Flip.Vertically || flip == Flip.Both)
					{
						Y0 += (float)num4 * num8;
						Y1 -= (float)num2 * num8;
					}
					else
					{
						Y0 += (float)num2 * num8;
						Y1 -= (float)num4 * num8;
					}
				}
				return true;
			}
		}

		private Precalculate _precalculate;

		private UIAtlas _atlas;

		private Vector4 _border;

		private Vector4 _drawRegion;

		private Vector4 _drawingDimensions;

		public Vector4 GetDrawingDimensions(Vector2 pivotOffset, float pixelSize, int width, int height, UISpriteData spriteData, Type type, Flip flip, UIAtlas atlas, Vector4 border, Vector4 drawRegion)
		{
			if (!_precalculate.Update(pivotOffset, pixelSize, width, height, spriteData, type, flip) && _atlas == atlas && _border == border && _drawRegion == drawRegion)
			{
				return _drawingDimensions;
			}
			_atlas = atlas;
			_border = border;
			_drawRegion = drawRegion;
			Vector4 vector = ((atlas != null) ? (border * pixelSize) : Vector4.zero);
			float num = vector.x + vector.z;
			float num2 = vector.y + vector.w;
			float x = _precalculate.X0;
			float x2 = _precalculate.X1;
			float y = _precalculate.Y0;
			float y2 = _precalculate.Y1;
			float x3 = Mathf.Lerp(x, x2 - num, drawRegion.x);
			float y3 = Mathf.Lerp(y, y2 - num2, drawRegion.y);
			float z = Mathf.Lerp(x + num, x2, drawRegion.z);
			float w = Mathf.Lerp(y + num2, y2, drawRegion.w);
			_drawingDimensions = new Vector4(x3, y3, z, w);
			return _drawingDimensions;
		}
	}

	private DrawingDimensionCache _drawingDimensionCache = new DrawingDimensionCache();

	[HideInInspector]
	[SerializeField]
	private UIAtlas mAtlas;

	[HideInInspector]
	[SerializeField]
	private string mSpriteName;

	[HideInInspector]
	[SerializeField]
	private bool mFillCenter = true;

	[NonSerialized]
	protected UISpriteData mSprite;

	[NonSerialized]
	private bool mSpriteSet;

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
				mSpriteSet = false;
				mSprite = null;
				if (string.IsNullOrEmpty(mSpriteName) && mAtlas != null && mAtlas.GetSpriteCount() > 0)
				{
					SetAtlasSprite(mAtlas.GetSpriteByIndex(0));
					mSpriteName = mSprite.name;
				}
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					string text = mSpriteName;
					mSpriteName = "";
					spriteName = text;
					MarkAsChanged();
				}
			}
		}
	}

	public string spriteName
	{
		get
		{
			return mSpriteName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					mSpriteName = "";
					mSprite = null;
					mChanged = true;
					mSpriteSet = false;
				}
			}
			else if (mSpriteName != value)
			{
				mSpriteName = value;
				mSprite = null;
				mChanged = true;
				mSpriteSet = false;
			}
		}
	}

	public bool isValid => GetAtlasSprite() != null;

	[Obsolete("Use 'centerType' instead")]
	public bool fillCenter
	{
		get
		{
			return centerType != AdvancedType.Invisible;
		}
		set
		{
			if (value != (centerType != AdvancedType.Invisible))
			{
				centerType = (value ? AdvancedType.Sliced : AdvancedType.Invisible);
				MarkAsChanged();
			}
		}
	}

	public override Vector4 border
	{
		get
		{
			UISpriteData atlasSprite = GetAtlasSprite();
			if (atlasSprite == null)
			{
				return base.border;
			}
			return new Vector4(atlasSprite.fBorderLeft, atlasSprite.fBorderBottom, atlasSprite.fBorderRight, atlasSprite.fBorderTop);
		}
	}

	public override float pixelSize
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

	public override int minWidth
	{
		get
		{
			if (type == Type.Sliced || type == Type.Advanced)
			{
				Vector4 vector = border * pixelSize;
				int num = Mathf.RoundToInt(vector.x + vector.z);
				UISpriteData atlasSprite = GetAtlasSprite();
				if (atlasSprite != null)
				{
					num += Mathf.RoundToInt((atlasSprite.fPaddingLeft + atlasSprite.fPaddingRight) * pixelSize);
				}
				return Mathf.Max(base.minWidth, ((num & 1) == 1) ? (num + 1) : num);
			}
			return base.minWidth;
		}
	}

	public override int minHeight
	{
		get
		{
			if (type == Type.Sliced || type == Type.Advanced)
			{
				Vector4 vector = border * pixelSize;
				int num = Mathf.RoundToInt(vector.y + vector.w);
				UISpriteData atlasSprite = GetAtlasSprite();
				if (atlasSprite != null)
				{
					num += Mathf.RoundToInt((float)(atlasSprite.PaddingTop + atlasSprite.PaddingBottom) * pixelSize);
				}
				return Mathf.Max(base.minHeight, ((num & 1) == 1) ? (num + 1) : num);
			}
			return base.minHeight;
		}
	}

	public override Vector4 drawingDimensions => _drawingDimensionCache.GetDrawingDimensions(base.pivotOffset, pixelSize, mWidth, mHeight, GetAtlasSprite(), mType, mFlip, mAtlas, border, mDrawRegion);

	public override bool premultipliedAlpha
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

	public UISpriteData GetAtlasSprite()
	{
		if (!mSpriteSet)
		{
			mSprite = null;
		}
		if (mSprite == null && mAtlas != null)
		{
			if (!string.IsNullOrEmpty(mSpriteName))
			{
				UISpriteData sprite = mAtlas.GetSprite(mSpriteName);
				if (sprite == null)
				{
					return null;
				}
				SetAtlasSprite(sprite);
			}
			if (mSprite == null && mAtlas.GetSpriteCount() > 0)
			{
				UISpriteData spriteByIndex = mAtlas.GetSpriteByIndex(0);
				if (spriteByIndex == null)
				{
					return null;
				}
				SetAtlasSprite(spriteByIndex);
				if (mSprite == null)
				{
					Debug.LogError(mAtlas.name + " seems to have a null sprite!");
					return null;
				}
				mSpriteName = mSprite.name;
			}
		}
		return mSprite;
	}

	protected void SetAtlasSprite(UISpriteData sp)
	{
		mChanged = true;
		mSpriteSet = true;
		if (sp != null)
		{
			mSprite = sp;
			mSpriteName = mSprite.name;
		}
		else
		{
			mSpriteName = ((mSprite != null) ? mSprite.name : "");
			mSprite = sp;
		}
	}

	public override void MakePixelPerfect()
	{
		if (!isValid)
		{
			return;
		}
		base.MakePixelPerfect();
		if (mType == Type.Tiled)
		{
			return;
		}
		UISpriteData atlasSprite = GetAtlasSprite();
		if (atlasSprite == null)
		{
			return;
		}
		Texture texture = mainTexture;
		if (!(texture == null) && (mType == Type.Simple || mType == Type.Filled || !atlasSprite.hasBorder) && texture != null)
		{
			int num = Mathf.RoundToInt(pixelSize * (float)(atlasSprite.Width + atlasSprite.PaddingLeft + atlasSprite.PaddingRight));
			int num2 = Mathf.RoundToInt(pixelSize * (float)(atlasSprite.Height + atlasSprite.PaddingTop + atlasSprite.PaddingBottom));
			if ((num & 1) == 1)
			{
				num++;
			}
			if ((num2 & 1) == 1)
			{
				num2++;
			}
			base.width = num;
			base.height = num2;
		}
	}

	protected override void OnInit()
	{
		if (!mFillCenter)
		{
			mFillCenter = true;
			centerType = AdvancedType.Invisible;
		}
		base.OnInit();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (mChanged || !mSpriteSet)
		{
			mSpriteSet = true;
			mSprite = null;
			mChanged = true;
		}
	}

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		Texture texture = mainTexture;
		if (texture == null)
		{
			return;
		}
		if (mSprite == null)
		{
			mSprite = atlas.GetSprite(spriteName);
		}
		if (mSprite != null)
		{
			Rect rect = new Rect(mSprite.fX, mSprite.fY, mSprite.fWidth, mSprite.fHeight);
			Rect rect2 = new Rect(mSprite.fX + mSprite.fBorderLeft, mSprite.fY + mSprite.fBorderTop, mSprite.fWidth - mSprite.fBorderLeft - mSprite.fBorderRight, mSprite.fHeight - mSprite.fBorderBottom - mSprite.fBorderTop);
			rect = NGUIMath.ConvertToTexCoords(rect, texture.width, texture.height);
			rect2 = NGUIMath.ConvertToTexCoords(rect2, texture.width, texture.height);
			int size = verts.size;
			Fill(verts, uvs, cols, rect, rect2);
			if (onPostFill != null)
			{
				onPostFill(this, size, verts, uvs, cols);
			}
		}
	}
}
