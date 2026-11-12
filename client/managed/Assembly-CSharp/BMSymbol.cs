using System;
using UnityEngine;

[Serializable]
public class BMSymbol
{
	public string sequence;

	public string spriteName;

	private UISpriteData mSprite;

	private bool mIsValid;

	private int mLength;

	private int mOffsetX;

	private int mOffsetY;

	private int mWidth;

	private int mHeight;

	private int mAdvance;

	private Rect mUV;

	public int length
	{
		get
		{
			if (mLength == 0)
			{
				mLength = sequence.Length;
			}
			return mLength;
		}
	}

	public int offsetX => mOffsetX;

	public int offsetY => mOffsetY;

	public int width => mWidth;

	public int height => mHeight;

	public int advance => mAdvance;

	public Rect uvRect => mUV;

	public void MarkAsChanged()
	{
		mIsValid = false;
	}

	public bool Validate(UIAtlas atlas)
	{
		if (atlas == null)
		{
			return false;
		}
		if (!mIsValid)
		{
			if (string.IsNullOrEmpty(spriteName))
			{
				return false;
			}
			mSprite = ((atlas != null) ? atlas.GetSprite(spriteName) : null);
			if (mSprite != null)
			{
				Texture texture = atlas.texture;
				if (texture == null)
				{
					mSprite = null;
				}
				else
				{
					mUV = new Rect(mSprite.X, mSprite.Y, mSprite.Width, mSprite.Height);
					mUV = NGUIMath.ConvertToTexCoords(mUV, texture.width, texture.height);
					mOffsetX = mSprite.PaddingLeft;
					mOffsetY = mSprite.PaddingTop;
					mWidth = mSprite.Width;
					mHeight = mSprite.Height;
					mAdvance = mSprite.Width + (mSprite.PaddingLeft + mSprite.PaddingRight);
					mIsValid = true;
				}
			}
		}
		return mSprite != null;
	}
}
