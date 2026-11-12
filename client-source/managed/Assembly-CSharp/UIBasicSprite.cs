using System;
using NVespa;
using UnityEngine;

public abstract class UIBasicSprite : UIWidget
{
	public enum Type
	{
		Simple,
		Sliced,
		Tiled,
		Filled,
		Advanced
	}

	public enum FillDirection
	{
		Horizontal,
		Vertical,
		Radial90,
		Radial180,
		Radial360
	}

	public enum AdvancedType
	{
		Invisible,
		Sliced,
		Tiled
	}

	public enum Flip
	{
		Nothing,
		Horizontally,
		Vertically,
		Both
	}

	[HideInInspector]
	[SerializeField]
	protected Type mType;

	[HideInInspector]
	[SerializeField]
	protected FillDirection mFillDirection = FillDirection.Radial360;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	protected float mFillAmount = 1f;

	[HideInInspector]
	[SerializeField]
	protected bool mInvert;

	[HideInInspector]
	[SerializeField]
	protected Flip mFlip;

	[NonSerialized]
	private Rect mInnerUV;

	[NonSerialized]
	private Rect mOuterUV;

	public AdvancedType centerType = AdvancedType.Sliced;

	public AdvancedType leftType = AdvancedType.Sliced;

	public AdvancedType rightType = AdvancedType.Sliced;

	public AdvancedType bottomType = AdvancedType.Sliced;

	public AdvancedType topType = AdvancedType.Sliced;

	protected static Vector3[] mTempPos = new Vector3[16];

	protected static Vector2[] mTempUVs = new Vector2[16];

	private static readonly int[][] _slicedFill_PosArray = new int[9][]
	{
		new int[2],
		new int[2] { 1, 0 },
		new int[2] { 2, 0 },
		new int[2] { 0, 1 },
		new int[2] { 1, 1 },
		new int[2] { 2, 1 },
		new int[2] { 0, 2 },
		new int[2] { 1, 2 },
		new int[2] { 2, 2 }
	};

	private static readonly int[][] _slicedFill_CenterInvisible_PosArray = new int[8][]
	{
		new int[2],
		new int[2] { 1, 0 },
		new int[2] { 2, 0 },
		new int[2] { 0, 1 },
		new int[2] { 2, 1 },
		new int[2] { 0, 2 },
		new int[2] { 1, 2 },
		new int[2] { 2, 2 }
	};

	public virtual Type type
	{
		get
		{
			return mType;
		}
		set
		{
			if (mType != value)
			{
				mType = value;
				MarkAsChanged();
			}
		}
	}

	public Flip flip
	{
		get
		{
			return mFlip;
		}
		set
		{
			if (mFlip != value)
			{
				mFlip = value;
				MarkAsChanged();
			}
		}
	}

	public FillDirection fillDirection
	{
		get
		{
			return mFillDirection;
		}
		set
		{
			if (mFillDirection != value)
			{
				mFillDirection = value;
				mChanged = true;
			}
		}
	}

	public virtual float fillAmount
	{
		get
		{
			return mFillAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mFillAmount != num)
			{
				mFillAmount = num;
				mChanged = true;
			}
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
				return Mathf.Max(base.minHeight, ((num & 1) == 1) ? (num + 1) : num);
			}
			return base.minHeight;
		}
	}

	public bool invert
	{
		get
		{
			return mInvert;
		}
		set
		{
			if (mInvert != value)
			{
				mInvert = value;
				mChanged = true;
			}
		}
	}

	public bool hasBorder
	{
		get
		{
			Vector4 vector = border;
			if (vector.x == 0f && vector.y == 0f && vector.z == 0f)
			{
				return vector.w != 0f;
			}
			return true;
		}
	}

	public virtual bool premultipliedAlpha => false;

	public virtual float pixelSize => 1f;

	protected Vector4 drawingUVs => mFlip switch
	{
		Flip.Horizontally => new Vector4(mOuterUV.xMax, mOuterUV.yMin, mOuterUV.xMin, mOuterUV.yMax), 
		Flip.Vertically => new Vector4(mOuterUV.xMin, mOuterUV.yMax, mOuterUV.xMax, mOuterUV.yMin), 
		Flip.Both => new Vector4(mOuterUV.xMax, mOuterUV.yMax, mOuterUV.xMin, mOuterUV.yMin), 
		_ => new Vector4(mOuterUV.xMin, mOuterUV.yMin, mOuterUV.xMax, mOuterUV.yMax), 
	};

	protected Color32 drawingColor
	{
		get
		{
			Color color = base.color;
			color.a = finalAlpha;
			if (premultipliedAlpha)
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

	protected void Fill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols, Rect outer, Rect inner)
	{
		mOuterUV = outer;
		mInnerUV = inner;
		switch (type)
		{
		case Type.Simple:
			SimpleFill(verts, uvs, cols);
			break;
		case Type.Sliced:
			SlicedFill(verts, uvs, cols);
			break;
		case Type.Filled:
			FilledFill(verts, uvs, cols);
			break;
		case Type.Tiled:
			TiledFill(verts, uvs, cols);
			break;
		case Type.Advanced:
			AdvancedFill(verts, uvs, cols);
			break;
		}
	}

	private void SimpleFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		Vector4 vector = drawingDimensions;
		Vector4 vector2 = drawingUVs;
		Color32 item = drawingColor;
		verts.ReserveCount(4);
		verts.AddUnsafe(new Vector3(vector.x, vector.y));
		verts.AddUnsafe(new Vector3(vector.x, vector.w));
		verts.AddUnsafe(new Vector3(vector.z, vector.w));
		verts.AddUnsafe(new Vector3(vector.z, vector.y));
		uvs.ReserveCount(4);
		uvs.AddUnsafe(new Vector2(vector2.x, vector2.y));
		uvs.AddUnsafe(new Vector2(vector2.x, vector2.w));
		uvs.AddUnsafe(new Vector2(vector2.z, vector2.w));
		uvs.AddUnsafe(new Vector2(vector2.z, vector2.y));
		cols.AddCount(item, 4);
	}

	private void SlicedFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		Vector4 vector = border * pixelSize;
		if (vector.x == 0f && vector.y == 0f && vector.z == 0f && vector.w == 0f)
		{
			SimpleFill(verts, uvs, cols);
			return;
		}
		Color32 c = drawingColor;
		Vector4 vector2 = drawingDimensions;
		mTempPos[0].x = vector2.x;
		mTempPos[0].y = vector2.y;
		mTempPos[3].x = vector2.z;
		mTempPos[3].y = vector2.w;
		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			mTempPos[1].x = mTempPos[0].x + vector.z;
			mTempPos[2].x = mTempPos[3].x - vector.x;
			mTempUVs[3].x = mOuterUV.xMin;
			mTempUVs[2].x = mInnerUV.xMin;
			mTempUVs[1].x = mInnerUV.xMax;
			mTempUVs[0].x = mOuterUV.xMax;
		}
		else
		{
			mTempPos[1].x = mTempPos[0].x + vector.x;
			mTempPos[2].x = mTempPos[3].x - vector.z;
			mTempUVs[0].x = mOuterUV.xMin;
			mTempUVs[1].x = mInnerUV.xMin;
			mTempUVs[2].x = mInnerUV.xMax;
			mTempUVs[3].x = mOuterUV.xMax;
		}
		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			mTempPos[1].y = mTempPos[0].y + vector.w;
			mTempPos[2].y = mTempPos[3].y - vector.y;
			mTempUVs[3].y = mOuterUV.yMin;
			mTempUVs[2].y = mInnerUV.yMin;
			mTempUVs[1].y = mInnerUV.yMax;
			mTempUVs[0].y = mOuterUV.yMax;
		}
		else
		{
			mTempPos[1].y = mTempPos[0].y + vector.y;
			mTempPos[2].y = mTempPos[3].y - vector.w;
			mTempUVs[0].y = mOuterUV.yMin;
			mTempUVs[1].y = mInnerUV.yMin;
			mTempUVs[2].y = mInnerUV.yMax;
			mTempUVs[3].y = mOuterUV.yMax;
		}
		SlicedFill_Case2(verts, uvs, cols, c);
	}

	private void SlicedFill_Case1(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols, Color32 c)
	{
		verts.ReserveCount(36);
		uvs.ReserveCount(36);
		cols.ReserveCount(36);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (centerType != AdvancedType.Invisible || j != 1 || i != 1)
				{
					int num = j + 1;
					int num2 = i + 1;
					verts.AddUnsafe(new Vector3(mTempPos[j].x, mTempPos[i].y));
					verts.AddUnsafe(new Vector3(mTempPos[j].x, mTempPos[num2].y));
					verts.AddUnsafe(new Vector3(mTempPos[num].x, mTempPos[num2].y));
					verts.AddUnsafe(new Vector3(mTempPos[num].x, mTempPos[i].y));
					uvs.AddUnsafe(new Vector2(mTempUVs[j].x, mTempUVs[i].y));
					uvs.AddUnsafe(new Vector2(mTempUVs[j].x, mTempUVs[num2].y));
					uvs.AddUnsafe(new Vector2(mTempUVs[num].x, mTempUVs[num2].y));
					uvs.AddUnsafe(new Vector2(mTempUVs[num].x, mTempUVs[i].y));
					cols.AddUnsafe(c);
					cols.AddUnsafe(c);
					cols.AddUnsafe(c);
					cols.AddUnsafe(c);
				}
			}
		}
	}

	private void SlicedFill_Case2(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols, Color32 c)
	{
		int[][] array = ((centerType == AdvancedType.Invisible) ? _slicedFill_CenterInvisible_PosArray : _slicedFill_PosArray);
		int num = verts.Count;
		int num2 = uvs.Count;
		int num3 = cols.Count;
		verts.AddEmpty(array.Length * 4);
		uvs.AddEmpty(array.Length * 4);
		cols.AddEmpty(array.Length * 4);
		Vector3[] buffer = verts.buffer;
		Vector2[] buffer2 = uvs.buffer;
		Color32[] buffer3 = cols.buffer;
		for (int i = 0; i < array.Length; i++)
		{
			int num4 = array[i][0];
			int num5 = array[i][1];
			int num6 = num4 + 1;
			int num7 = num5 + 1;
			buffer[num] = new Vector3(mTempPos[num4].x, mTempPos[num5].y);
			buffer[num + 1] = new Vector3(mTempPos[num4].x, mTempPos[num7].y);
			buffer[num + 2] = new Vector3(mTempPos[num6].x, mTempPos[num7].y);
			buffer[num + 3] = new Vector3(mTempPos[num6].x, mTempPos[num5].y);
			buffer2[num2] = new Vector2(mTempUVs[num4].x, mTempUVs[num5].y);
			buffer2[num2 + 1] = new Vector2(mTempUVs[num4].x, mTempUVs[num7].y);
			buffer2[num2 + 2] = new Vector2(mTempUVs[num6].x, mTempUVs[num7].y);
			buffer2[num2 + 3] = new Vector2(mTempUVs[num6].x, mTempUVs[num5].y);
			buffer3[num3] = c;
			buffer3[num3 + 1] = c;
			buffer3[num3 + 2] = c;
			buffer3[num3 + 3] = c;
			num += 4;
			num2 += 4;
			num3 += 4;
		}
	}

	private void TiledFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		Texture texture = mainTexture;
		if (texture == null)
		{
			return;
		}
		Vector2 vector = new Vector2(mInnerUV.width * (float)texture.width, mInnerUV.height * (float)texture.height);
		vector *= pixelSize;
		if (texture == null || vector.x < 2f || vector.y < 2f)
		{
			return;
		}
		Color32 item = drawingColor;
		Vector4 vector2 = drawingDimensions;
		Vector4 vector3 = default(Vector4);
		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			vector3.x = mInnerUV.xMax;
			vector3.z = mInnerUV.xMin;
		}
		else
		{
			vector3.x = mInnerUV.xMin;
			vector3.z = mInnerUV.xMax;
		}
		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			vector3.y = mInnerUV.yMax;
			vector3.w = mInnerUV.yMin;
		}
		else
		{
			vector3.y = mInnerUV.yMin;
			vector3.w = mInnerUV.yMax;
		}
		float x = vector2.x;
		float num = vector2.y;
		float x2 = vector3.x;
		float y = vector3.y;
		for (; num < vector2.w; num += vector.y)
		{
			x = vector2.x;
			float num2 = num + vector.y;
			float y2 = vector3.w;
			if (num2 > vector2.w)
			{
				y2 = Mathf.Lerp(vector3.y, vector3.w, (vector2.w - num) / vector.y);
				num2 = vector2.w;
			}
			for (; x < vector2.z; x += vector.x)
			{
				float num3 = x + vector.x;
				float x3 = vector3.z;
				if (num3 > vector2.z)
				{
					x3 = Mathf.Lerp(vector3.x, vector3.z, (vector2.z - x) / vector.x);
					num3 = vector2.z;
				}
				verts.ReserveCount(4);
				verts.AddUnsafe(new Vector3(x, num));
				verts.AddUnsafe(new Vector3(x, num2));
				verts.AddUnsafe(new Vector3(num3, num2));
				verts.AddUnsafe(new Vector3(num3, num));
				uvs.ReserveCount(4);
				uvs.AddUnsafe(new Vector2(x2, y));
				uvs.AddUnsafe(new Vector2(x2, y2));
				uvs.AddUnsafe(new Vector2(x3, y2));
				uvs.AddUnsafe(new Vector2(x3, y));
				cols.AddCount(item, 4);
			}
		}
	}

	private void FilledFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		if (mFillAmount < 0.001f)
		{
			return;
		}
		Vector4 v = drawingDimensions;
		Vector4 u = drawingUVs;
		Color32 color = drawingColor;
		if (mFillDirection == FillDirection.Horizontal || mFillDirection == FillDirection.Vertical)
		{
			if (mFillDirection == FillDirection.Horizontal)
			{
				float num = (u.z - u.x) * mFillAmount;
				if (mInvert)
				{
					v.x = v.z - (v.z - v.x) * mFillAmount;
					u.x = u.z - num;
				}
				else
				{
					v.z = v.x + (v.z - v.x) * mFillAmount;
					u.z = u.x + num;
				}
			}
			else if (mFillDirection == FillDirection.Vertical)
			{
				float num2 = (u.w - u.y) * mFillAmount;
				if (mInvert)
				{
					v.y = v.w - (v.w - v.y) * mFillAmount;
					u.y = u.w - num2;
				}
				else
				{
					v.w = v.y + (v.w - v.y) * mFillAmount;
					u.w = u.y + num2;
				}
			}
		}
		if (mFillAmount < 1f)
		{
			if (mFillDirection == FillDirection.Radial90)
			{
				FilledFill_Radial90(v, u, color, verts, uvs, cols);
				return;
			}
			if (mFillDirection == FillDirection.Radial180)
			{
				FilledFill_Radial180(v, u, color, verts, uvs, cols);
				return;
			}
			if (mFillDirection == FillDirection.Radial360)
			{
				FilledFill_Radial360(v, u, color, verts, uvs, cols);
				return;
			}
		}
		mTempPos[0] = new Vector3(v.x, v.y);
		mTempPos[1] = new Vector3(v.x, v.w);
		mTempPos[2] = new Vector3(v.z, v.w);
		mTempPos[3] = new Vector3(v.z, v.y);
		mTempUVs[0] = new Vector2(u.x, u.y);
		mTempUVs[1] = new Vector2(u.x, u.w);
		mTempUVs[2] = new Vector2(u.z, u.w);
		mTempUVs[3] = new Vector2(u.z, u.y);
		verts.AddArray(mTempPos, 4);
		uvs.AddArray(mTempUVs, 4);
		cols.AddCount(color, 4);
	}

	private void FilledFill_Radial90(Vector4 v, Vector4 u, Color32 c, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		mTempPos[0] = new Vector3(v.x, v.y);
		mTempPos[1] = new Vector3(v.x, v.w);
		mTempPos[2] = new Vector3(v.z, v.w);
		mTempPos[3] = new Vector3(v.z, v.y);
		mTempUVs[0] = new Vector2(u.x, u.y);
		mTempUVs[1] = new Vector2(u.x, u.w);
		mTempUVs[2] = new Vector2(u.z, u.w);
		mTempUVs[3] = new Vector2(u.z, u.y);
		if (RadialCut(0, mTempPos, mTempUVs, mFillAmount, mInvert, 0))
		{
			verts.AddArray(mTempPos, 4);
			uvs.AddArray(mTempUVs, 4);
			cols.AddCount(c, 4);
		}
	}

	private void FilledFill_Radial180(Vector4 v, Vector4 u, Color32 c, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		for (int i = 0; i < 2; i++)
		{
			float t = 0f;
			float t2 = 1f;
			float t3;
			float t4;
			if (i == 0)
			{
				t3 = 0f;
				t4 = 0.5f;
			}
			else
			{
				t3 = 0.5f;
				t4 = 1f;
			}
			mTempPos[0] = new Vector3(Mathf.Lerp(v.x, v.z, t3), Mathf.Lerp(v.y, v.w, t));
			mTempPos[1] = new Vector3(mTempPos[0].x, Mathf.Lerp(v.y, v.w, t2));
			mTempPos[2] = new Vector3(Mathf.Lerp(v.x, v.z, t4), mTempPos[1].y);
			mTempPos[3] = new Vector3(mTempPos[2].x, mTempPos[0].y);
			mTempUVs[0] = new Vector2(Mathf.Lerp(u.x, u.z, t3), Mathf.Lerp(u.y, u.w, t));
			mTempUVs[1] = new Vector2(mTempUVs[0].x, Mathf.Lerp(u.y, u.w, t2));
			mTempUVs[2] = new Vector2(Mathf.Lerp(u.x, u.z, t4), mTempUVs[1].y);
			mTempUVs[3] = new Vector2(mTempUVs[2].x, mTempUVs[0].y);
			float value = ((!mInvert) ? (fillAmount * 2f - (float)i) : (mFillAmount * 2f - (float)(1 - i)));
			if (RadialCut(0, mTempPos, mTempUVs, Mathf.Clamp01(value), !mInvert, NGUIMath.RepeatIndex(i + 3, 4)))
			{
				verts.AddArray(mTempPos, 4);
				uvs.AddArray(mTempUVs, 4);
				cols.AddCount(c, 4);
			}
		}
	}

	private void FilledFill_Radial360(Vector4 v, Vector4 u, Color32 c, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			int num2 = num;
			int num3 = num + 1;
			int num4 = num + 2;
			int num5 = num + 3;
			float x;
			float x2;
			float x3;
			float x4;
			if (i < 2)
			{
				x = v.x;
				x2 = (v.x + v.z) * 0.5f;
				x3 = u.x;
				x4 = (u.x + u.z) * 0.5f;
			}
			else
			{
				x = (v.x + v.z) * 0.5f;
				x2 = v.z;
				x3 = (u.x + u.z) * 0.5f;
				x4 = u.z;
			}
			float y;
			float y2;
			float y3;
			float y4;
			if (i == 0 || i == 3)
			{
				y = v.y;
				y2 = (v.y + v.w) * 0.5f;
				y3 = u.y;
				y4 = (u.y + u.w) * 0.5f;
			}
			else
			{
				y = (v.y + v.w) * 0.5f;
				y2 = v.w;
				y3 = (u.y + u.w) * 0.5f;
				y4 = u.w;
			}
			mTempPos[num2] = new Vector3(x, y);
			mTempPos[num3] = new Vector3(mTempPos[num2].x, y2);
			mTempPos[num4] = new Vector3(x2, mTempPos[num3].y);
			mTempPos[num5] = new Vector3(mTempPos[num4].x, mTempPos[num2].y);
			mTempUVs[num2] = new Vector2(x3, y3);
			mTempUVs[num3] = new Vector2(mTempUVs[num2].x, y4);
			mTempUVs[num4] = new Vector2(x4, mTempUVs[num3].y);
			mTempUVs[num5] = new Vector2(mTempUVs[num4].x, mTempUVs[num2].y);
			float value = (mInvert ? (mFillAmount * 4f - (float)NGUIMath.RepeatIndex(i + 2, 4)) : (mFillAmount * 4f - (float)(3 - NGUIMath.RepeatIndex(i + 2, 4))));
			if (RadialCut(num, mTempPos, mTempUVs, Mathf.Clamp01(value), mInvert, NGUIMath.RepeatIndex(i + 2, 4)))
			{
				num += 4;
			}
		}
		if (num > 0)
		{
			verts.AddArray(mTempPos, num);
			uvs.AddArray(mTempUVs, num);
			cols.AddCount(c, num);
		}
	}

	private void AdvancedFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		Texture texture = mainTexture;
		if (texture == null)
		{
			return;
		}
		Vector4 vector = border * pixelSize;
		if (vector.x == 0f && vector.y == 0f && vector.z == 0f && vector.w == 0f)
		{
			SimpleFill(verts, uvs, cols);
			return;
		}
		Color32 color = drawingColor;
		Vector4 vector2 = drawingDimensions;
		Vector2 vector3 = new Vector2(mInnerUV.width * (float)texture.width, mInnerUV.height * (float)texture.height);
		vector3 *= pixelSize;
		if (vector3.x < 1f)
		{
			vector3.x = 1f;
		}
		if (vector3.y < 1f)
		{
			vector3.y = 1f;
		}
		mTempPos[0].x = vector2.x;
		mTempPos[0].y = vector2.y;
		mTempPos[3].x = vector2.z;
		mTempPos[3].y = vector2.w;
		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			mTempPos[1].x = mTempPos[0].x + vector.z;
			mTempPos[2].x = mTempPos[3].x - vector.x;
			mTempUVs[3].x = mOuterUV.xMin;
			mTempUVs[2].x = mInnerUV.xMin;
			mTempUVs[1].x = mInnerUV.xMax;
			mTempUVs[0].x = mOuterUV.xMax;
		}
		else
		{
			mTempPos[1].x = mTempPos[0].x + vector.x;
			mTempPos[2].x = mTempPos[3].x - vector.z;
			mTempUVs[0].x = mOuterUV.xMin;
			mTempUVs[1].x = mInnerUV.xMin;
			mTempUVs[2].x = mInnerUV.xMax;
			mTempUVs[3].x = mOuterUV.xMax;
		}
		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			mTempPos[1].y = mTempPos[0].y + vector.w;
			mTempPos[2].y = mTempPos[3].y - vector.y;
			mTempUVs[3].y = mOuterUV.yMin;
			mTempUVs[2].y = mInnerUV.yMin;
			mTempUVs[1].y = mInnerUV.yMax;
			mTempUVs[0].y = mOuterUV.yMax;
		}
		else
		{
			mTempPos[1].y = mTempPos[0].y + vector.y;
			mTempPos[2].y = mTempPos[3].y - vector.w;
			mTempUVs[0].y = mOuterUV.yMin;
			mTempUVs[1].y = mInnerUV.yMin;
			mTempUVs[2].y = mInnerUV.yMax;
			mTempUVs[3].y = mOuterUV.yMax;
		}
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			for (int j = 0; j < 3; j++)
			{
				if (centerType == AdvancedType.Invisible && i == 1 && j == 1)
				{
					continue;
				}
				int num2 = j + 1;
				if (i == 1 && j == 1)
				{
					if (centerType == AdvancedType.Tiled)
					{
						float x = mTempPos[i].x;
						float x2 = mTempPos[num].x;
						float y = mTempPos[j].y;
						float y2 = mTempPos[num2].y;
						float x3 = mTempUVs[i].x;
						float y3 = mTempUVs[j].y;
						for (float num3 = y; num3 < y2; num3 += vector3.y)
						{
							float num4 = x;
							float num5 = mTempUVs[num2].y;
							float num6 = num3 + vector3.y;
							if (num6 > y2)
							{
								num5 = Mathf.Lerp(y3, num5, (y2 - num3) / vector3.y);
								num6 = y2;
							}
							for (; num4 < x2; num4 += vector3.x)
							{
								float num7 = num4 + vector3.x;
								float num8 = mTempUVs[num].x;
								if (num7 > x2)
								{
									num8 = Mathf.Lerp(x3, num8, (x2 - num4) / vector3.x);
									num7 = x2;
								}
								Fill(verts, uvs, cols, num4, num7, num3, num6, x3, num8, y3, num5, color);
							}
						}
					}
					else if (centerType == AdvancedType.Sliced)
					{
						Fill(verts, uvs, cols, mTempPos[i].x, mTempPos[num].x, mTempPos[j].y, mTempPos[num2].y, mTempUVs[i].x, mTempUVs[num].x, mTempUVs[j].y, mTempUVs[num2].y, color);
					}
				}
				else if (i == 1)
				{
					if ((j == 0 && bottomType == AdvancedType.Tiled) || (j == 2 && topType == AdvancedType.Tiled))
					{
						float x4 = mTempPos[i].x;
						float x5 = mTempPos[num].x;
						float y4 = mTempPos[j].y;
						float y5 = mTempPos[num2].y;
						float x6 = mTempUVs[i].x;
						float y6 = mTempUVs[j].y;
						float y7 = mTempUVs[num2].y;
						for (float num9 = x4; num9 < x5; num9 += vector3.x)
						{
							float num10 = num9 + vector3.x;
							float num11 = mTempUVs[num].x;
							if (num10 > x5)
							{
								num11 = Mathf.Lerp(x6, num11, (x5 - num9) / vector3.x);
								num10 = x5;
							}
							Fill(verts, uvs, cols, num9, num10, y4, y5, x6, num11, y6, y7, color);
						}
					}
					else if ((j == 0 && bottomType != AdvancedType.Invisible) || (j == 2 && topType != AdvancedType.Invisible))
					{
						Fill(verts, uvs, cols, mTempPos[i].x, mTempPos[num].x, mTempPos[j].y, mTempPos[num2].y, mTempUVs[i].x, mTempUVs[num].x, mTempUVs[j].y, mTempUVs[num2].y, color);
					}
				}
				else if (j == 1)
				{
					if ((i == 0 && leftType == AdvancedType.Tiled) || (i == 2 && rightType == AdvancedType.Tiled))
					{
						float x7 = mTempPos[i].x;
						float x8 = mTempPos[num].x;
						float y8 = mTempPos[j].y;
						float y9 = mTempPos[num2].y;
						float x9 = mTempUVs[i].x;
						float x10 = mTempUVs[num].x;
						float y10 = mTempUVs[j].y;
						for (float num12 = y8; num12 < y9; num12 += vector3.y)
						{
							float num13 = mTempUVs[num2].y;
							float num14 = num12 + vector3.y;
							if (num14 > y9)
							{
								num13 = Mathf.Lerp(y10, num13, (y9 - num12) / vector3.y);
								num14 = y9;
							}
							Fill(verts, uvs, cols, x7, x8, num12, num14, x9, x10, y10, num13, color);
						}
					}
					else if ((i == 0 && leftType != AdvancedType.Invisible) || (i == 2 && rightType != AdvancedType.Invisible))
					{
						Fill(verts, uvs, cols, mTempPos[i].x, mTempPos[num].x, mTempPos[j].y, mTempPos[num2].y, mTempUVs[i].x, mTempUVs[num].x, mTempUVs[j].y, mTempUVs[num2].y, color);
					}
				}
				else if ((j == 0 && bottomType != AdvancedType.Invisible) || (j == 2 && topType != AdvancedType.Invisible) || (i == 0 && leftType != AdvancedType.Invisible) || (i == 2 && rightType != AdvancedType.Invisible))
				{
					Fill(verts, uvs, cols, mTempPos[i].x, mTempPos[num].x, mTempPos[j].y, mTempPos[num2].y, mTempUVs[i].x, mTempUVs[num].x, mTempUVs[j].y, mTempUVs[num2].y, color);
				}
			}
		}
	}

	private static bool RadialCut(int offset, Vector3[] xy, Vector2[] uv, float fill, bool invert, int corner)
	{
		if (fill < 0.001f)
		{
			return false;
		}
		if ((corner & 1) == 1)
		{
			invert = !invert;
		}
		if (!invert && fill > 0.999f)
		{
			return true;
		}
		float num = Mathf.Clamp01(fill);
		if (invert)
		{
			num = 1f - num;
		}
		num *= MathF.PI / 2f;
		float cos = Mathf.Cos(num);
		float sin = Mathf.Sin(num);
		RadialCut(offset, xy, uv, cos, sin, invert, corner);
		return true;
	}

	private static void RadialCut(int offset, Vector2[] xy, float cos, float sin, bool invert, int corner)
	{
		int num = offset + corner;
		int num2 = offset + NGUIMath.RepeatIndex(corner + 1, 4);
		int num3 = offset + NGUIMath.RepeatIndex(corner + 2, 4);
		int num4 = offset + NGUIMath.RepeatIndex(corner + 3, 4);
		if ((corner & 1) == 1)
		{
			if (sin > cos)
			{
				cos /= sin;
				sin = 1f;
				if (invert)
				{
					xy[num2].x = Mathf.Lerp(xy[num].x, xy[num3].x, cos);
					xy[num3].x = xy[num2].x;
				}
			}
			else if (cos > sin)
			{
				sin /= cos;
				cos = 1f;
				if (!invert)
				{
					xy[num3].y = Mathf.Lerp(xy[num].y, xy[num3].y, sin);
					xy[num4].y = xy[num3].y;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}
			if (!invert)
			{
				xy[num4].x = Mathf.Lerp(xy[num].x, xy[num3].x, cos);
			}
			else
			{
				xy[num2].y = Mathf.Lerp(xy[num].y, xy[num3].y, sin);
			}
			return;
		}
		if (cos > sin)
		{
			sin /= cos;
			cos = 1f;
			if (!invert)
			{
				xy[num2].y = Mathf.Lerp(xy[num].y, xy[num3].y, sin);
				xy[num3].y = xy[num2].y;
			}
		}
		else if (sin > cos)
		{
			cos /= sin;
			sin = 1f;
			if (invert)
			{
				xy[num3].x = Mathf.Lerp(xy[num].x, xy[num3].x, cos);
				xy[num4].x = xy[num3].x;
			}
		}
		else
		{
			cos = 1f;
			sin = 1f;
		}
		if (invert)
		{
			xy[num4].y = Mathf.Lerp(xy[num].y, xy[num3].y, sin);
		}
		else
		{
			xy[num2].x = Mathf.Lerp(xy[num].x, xy[num3].x, cos);
		}
	}

	private static void RadialCut(int offset, Vector3[] xy, float cos, float sin, bool invert, int corner)
	{
		int num = offset + corner;
		int num2 = offset + NGUIMath.RepeatIndex(corner + 1, 4);
		int num3 = offset + NGUIMath.RepeatIndex(corner + 2, 4);
		int num4 = offset + NGUIMath.RepeatIndex(corner + 3, 4);
		if ((corner & 1) == 1)
		{
			if (sin > cos)
			{
				cos /= sin;
				sin = 1f;
				if (invert)
				{
					xy[num2].x = Mathf.Lerp(xy[num].x, xy[num3].x, cos);
					xy[num3].x = xy[num2].x;
				}
			}
			else if (cos > sin)
			{
				sin /= cos;
				cos = 1f;
				if (!invert)
				{
					xy[num3].y = Mathf.Lerp(xy[num].y, xy[num3].y, sin);
					xy[num4].y = xy[num3].y;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}
			if (!invert)
			{
				xy[num4].x = Mathf.Lerp(xy[num].x, xy[num3].x, cos);
			}
			else
			{
				xy[num2].y = Mathf.Lerp(xy[num].y, xy[num3].y, sin);
			}
			return;
		}
		if (cos > sin)
		{
			sin /= cos;
			cos = 1f;
			if (!invert)
			{
				xy[num2].y = Mathf.Lerp(xy[num].y, xy[num3].y, sin);
				xy[num3].y = xy[num2].y;
			}
		}
		else if (sin > cos)
		{
			cos /= sin;
			sin = 1f;
			if (invert)
			{
				xy[num3].x = Mathf.Lerp(xy[num].x, xy[num3].x, cos);
				xy[num4].x = xy[num3].x;
			}
		}
		else
		{
			cos = 1f;
			sin = 1f;
		}
		if (invert)
		{
			xy[num4].y = Mathf.Lerp(xy[num].y, xy[num3].y, sin);
		}
		else
		{
			xy[num2].x = Mathf.Lerp(xy[num].x, xy[num3].x, cos);
		}
	}

	private static void RadialCut(int offset, Vector3[] xy, Vector2[] uv, float cos, float sin, bool invert, int corner)
	{
		int num = offset + corner;
		int num2 = offset + NGUIMath.RepeatIndex(corner + 1, 4);
		int num3 = offset + NGUIMath.RepeatIndex(corner + 2, 4);
		int num4 = offset + NGUIMath.RepeatIndex(corner + 3, 4);
		if ((corner & 1) == 1)
		{
			if (sin > cos)
			{
				cos /= sin;
				sin = 1f;
				if (invert)
				{
					float num5 = 1f - cos;
					xy[num2].x = xy[num].x * num5 + xy[num3].x * cos;
					xy[num3].x = xy[num2].x;
					uv[num2].x = uv[num].x * num5 + uv[num3].x * cos;
					uv[num3].x = uv[num2].x;
				}
			}
			else if (cos > sin)
			{
				sin /= cos;
				cos = 1f;
				if (!invert)
				{
					float num6 = 1f - sin;
					xy[num3].y = xy[num].y * num6 + xy[num3].y * sin;
					xy[num4].y = xy[num3].y;
					uv[num3].y = uv[num].y * num6 + uv[num3].y * sin;
					uv[num4].y = uv[num3].y;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}
			if (!invert)
			{
				float num7 = 1f - cos;
				xy[num4].x = xy[num].x * num7 + xy[num3].x * cos;
				uv[num4].x = uv[num].x * num7 + uv[num3].x * cos;
			}
			else
			{
				float num8 = 1f - sin;
				xy[num2].y = xy[num].y * num8 + xy[num3].y * sin;
				uv[num2].y = uv[num].y * num8 + uv[num3].y * sin;
			}
			return;
		}
		if (cos > sin)
		{
			sin /= cos;
			cos = 1f;
			if (!invert)
			{
				float num9 = 1f - sin;
				xy[num2].y = xy[num].y * num9 + xy[num3].y * sin;
				xy[num3].y = xy[num2].y;
				uv[num2].y = uv[num].y * num9 + uv[num3].y * sin;
				uv[num3].y = uv[num2].y;
			}
		}
		else if (sin > cos)
		{
			cos /= sin;
			sin = 1f;
			if (invert)
			{
				float num10 = 1f - cos;
				xy[num3].x = xy[num].x * num10 + xy[num3].x * cos;
				xy[num4].x = xy[num3].x;
				uv[num3].x = uv[num].x * num10 + uv[num3].x * cos;
				uv[num4].x = uv[num3].x;
			}
		}
		else
		{
			cos = 1f;
			sin = 1f;
		}
		if (invert)
		{
			float num11 = 1f - sin;
			xy[num4].y = xy[num].y * num11 + xy[num3].y * sin;
			uv[num4].y = uv[num].y * num11 + uv[num3].y * sin;
		}
		else
		{
			float num12 = 1f - cos;
			xy[num2].x = xy[num].x * num12 + xy[num3].x * cos;
			uv[num2].x = uv[num].x * num12 + uv[num3].x * cos;
		}
	}

	private static void Fill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols, float v0x, float v1x, float v0y, float v1y, float u0x, float u1x, float u0y, float u1y, Color col)
	{
		verts.ReserveCount(4);
		verts.AddUnsafe(new Vector3(v0x, v0y));
		verts.AddUnsafe(new Vector3(v0x, v1y));
		verts.AddUnsafe(new Vector3(v1x, v1y));
		verts.AddUnsafe(new Vector3(v1x, v0y));
		uvs.ReserveCount(4);
		uvs.AddUnsafe(new Vector2(u0x, u0y));
		uvs.AddUnsafe(new Vector2(u0x, u1y));
		uvs.AddUnsafe(new Vector2(u1x, u1y));
		uvs.AddUnsafe(new Vector2(u1x, u0y));
		cols.AddCount(col, 4);
	}
}
