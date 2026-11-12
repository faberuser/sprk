using System;
using UnityEngine;

[Serializable]
public class UISpriteData
{
	public string name = "Sprite";

	[NonSerialized]
	public float atlasScale = 1f;

	[SerializeField]
	private int x;

	[SerializeField]
	private int y;

	[SerializeField]
	private int width;

	[SerializeField]
	private int height;

	[SerializeField]
	private int borderLeft;

	[SerializeField]
	private int borderRight;

	[SerializeField]
	private int borderTop;

	[SerializeField]
	private int borderBottom;

	[SerializeField]
	private int paddingLeft;

	[SerializeField]
	private int paddingRight;

	[SerializeField]
	private int paddingTop;

	[SerializeField]
	private int paddingBottom;

	public int X => Mathf.RoundToInt(atlasScale * (float)x);

	public int Y => Mathf.RoundToInt(atlasScale * (float)y);

	public int Width => Mathf.RoundToInt(atlasScale * (float)width);

	public int Height => Mathf.RoundToInt(atlasScale * (float)height);

	public int BorderLeft => Mathf.RoundToInt(atlasScale * (float)borderLeft);

	public int BorderRight => Mathf.RoundToInt(atlasScale * (float)borderRight);

	public int BorderTop => Mathf.RoundToInt(atlasScale * (float)borderTop);

	public int BorderBottom => Mathf.RoundToInt(atlasScale * (float)borderBottom);

	public int PaddingLeft => Mathf.RoundToInt(atlasScale * (float)paddingLeft);

	public int PaddingRight => Mathf.RoundToInt(atlasScale * (float)paddingRight);

	public int PaddingTop => Mathf.RoundToInt(atlasScale * (float)paddingTop);

	public int PaddingBottom => Mathf.RoundToInt(atlasScale * (float)paddingBottom);

	public float fX => atlasScale * (float)x;

	public float fY => atlasScale * (float)y;

	public float fWidth => atlasScale * (float)width;

	public float fHeight => atlasScale * (float)height;

	public float fBorderLeft => atlasScale * (float)borderLeft;

	public float fBorderRight => atlasScale * (float)borderRight;

	public float fBorderTop => atlasScale * (float)borderTop;

	public float fBorderBottom => atlasScale * (float)borderBottom;

	public float fPaddingLeft => atlasScale * (float)paddingLeft;

	public float fPaddingRight => atlasScale * (float)paddingRight;

	public float fPaddingTop => atlasScale * (float)paddingTop;

	public float fPaddingBottom => atlasScale * (float)paddingBottom;

	public bool hasBorder => (borderLeft | borderRight | borderTop | borderBottom) != 0;

	public bool hasPadding => (paddingLeft | paddingRight | paddingTop | paddingBottom) != 0;

	public void SetRect(int x, int y, int width, int height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	public void SetPadding(int left, int bottom, int right, int top)
	{
		paddingLeft = left;
		paddingBottom = bottom;
		paddingRight = right;
		paddingTop = top;
	}

	public void SetBorder(int left, int bottom, int right, int top)
	{
		borderLeft = left;
		borderBottom = bottom;
		borderRight = right;
		borderTop = top;
	}

	public void CopyFrom(UISpriteData sd)
	{
		name = sd.name;
		x = sd.x;
		y = sd.y;
		width = sd.width;
		height = sd.height;
		borderLeft = sd.borderLeft;
		borderRight = sd.borderRight;
		borderTop = sd.borderTop;
		borderBottom = sd.borderBottom;
		paddingLeft = sd.paddingLeft;
		paddingRight = sd.paddingRight;
		paddingTop = sd.paddingTop;
		paddingBottom = sd.paddingBottom;
	}

	public void CopyBorderFrom(UISpriteData sd)
	{
		borderLeft = sd.borderLeft;
		borderRight = sd.borderRight;
		borderTop = sd.borderTop;
		borderBottom = sd.borderBottom;
	}

	public void AdjustByAtlasSize(int origWidth, int origHeight, int curWidth, int curHeight)
	{
		x = X * curWidth / origWidth;
		y = Y * curHeight / origHeight;
		width = Width * curWidth / origWidth;
		height = Height * curHeight / origHeight;
		borderLeft = BorderLeft * curWidth / origWidth;
		borderRight = BorderRight * curWidth / origWidth;
		borderTop = BorderTop * curHeight / origHeight;
		borderBottom = BorderBottom * curHeight / origHeight;
		paddingLeft = PaddingLeft * curWidth / origWidth;
		paddingRight = PaddingRight * curWidth / origWidth;
		paddingTop = PaddingTop * curHeight / origHeight;
		paddingBottom = PaddingBottom * curHeight / origHeight;
	}
}
