using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Atlas")]
public class UIAtlas : MonoBehaviour
{
	private enum Coordinates
	{
		Pixels,
		TexCoords
	}

	[HideInInspector]
	[SerializeField]
	private Material material;

	[HideInInspector]
	[SerializeField]
	private List<UISpriteData> mSprites = new List<UISpriteData>();

	[HideInInspector]
	[SerializeField]
	private float mPixelSize = 1f;

	[HideInInspector]
	[SerializeField]
	private int mAtlasWidth;

	[HideInInspector]
	[SerializeField]
	private int mAtlasHeight;

	[HideInInspector]
	[SerializeField]
	private UIAtlas mReplacement;

	private int mPMA = -1;

	private Dictionary<string, int> mSpriteIndices = new Dictionary<string, int>();

	private bool _adjustedAtlasSize;

	private float _atlasScale = 1f;

	public float finalAtlasScale
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.finalAtlasScale;
			}
			if (_adjustedAtlasSize)
			{
				return _atlasScale;
			}
			Texture texture = this.texture;
			if (texture == null)
			{
				return 1f;
			}
			if (mAtlasWidth == 0)
			{
				_atlasScale = 1f;
			}
			else
			{
				_atlasScale = Mathf.Clamp((float)texture.width / (float)mAtlasWidth, 0.1f, 10f);
			}
			_adjustedAtlasSize = true;
			return _atlasScale;
		}
	}

	public Material spriteMaterial
	{
		get
		{
			if (!(mReplacement != null))
			{
				return material;
			}
			return mReplacement.spriteMaterial;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.spriteMaterial = value;
				return;
			}
			if (material == null)
			{
				mPMA = 0;
				material = value;
				return;
			}
			MarkAsChanged();
			mPMA = -1;
			material = value;
			MarkAsChanged();
		}
	}

	public bool premultipliedAlpha
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.premultipliedAlpha;
			}
			if (mPMA == -1)
			{
				Material material = spriteMaterial;
				mPMA = ((material != null && material.shader != null && material.shader.name.Contains("Premultiplied")) ? 1 : 0);
			}
			return mPMA == 1;
		}
	}

	public Texture texture
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.texture;
			}
			if (material != null)
			{
				return material.mainTexture;
			}
			return null;
		}
	}

	public float pixelSize
	{
		get
		{
			if (mReplacement != null)
			{
				return mReplacement.pixelSize;
			}
			return mPixelSize;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.pixelSize = value;
				return;
			}
			float num = Mathf.Clamp(value, 0.25f, 4f);
			if (mPixelSize != num)
			{
				mPixelSize = num;
				MarkAsChanged();
			}
		}
	}

	public int atlasWidth
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mAtlasWidth;
			}
			return mReplacement.atlasWidth;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.atlasWidth = value;
			}
			else if (mAtlasWidth != value)
			{
				mAtlasWidth = value;
				MarkAsChanged();
			}
		}
	}

	public int atlasHeight
	{
		get
		{
			if (!(mReplacement != null))
			{
				return mAtlasHeight;
			}
			return mReplacement.atlasHeight;
		}
		set
		{
			if (mReplacement != null)
			{
				mReplacement.atlasHeight = value;
			}
			else if (mAtlasHeight != value)
			{
				mAtlasHeight = value;
				MarkAsChanged();
			}
		}
	}

	public UIAtlas replacement
	{
		get
		{
			return mReplacement;
		}
		set
		{
			UIAtlas uIAtlas = value;
			if (uIAtlas == this)
			{
				uIAtlas = null;
			}
			if (mReplacement != uIAtlas)
			{
				if (uIAtlas != null && uIAtlas.replacement == this)
				{
					uIAtlas.replacement = null;
				}
				if (mReplacement != null)
				{
					MarkAsChanged();
				}
				mReplacement = uIAtlas;
				if (uIAtlas != null)
				{
					material = null;
				}
				MarkAsChanged();
			}
		}
	}

	public int GetSpriteCount()
	{
		if (mReplacement != null)
		{
			return mReplacement.GetSpriteCount();
		}
		return __Extension_Method_ICollection__.COUNT(mSprites);
	}

	public UISpriteData GetSpriteByIndex(int index)
	{
		if (mReplacement != null)
		{
			return mReplacement.GetSpriteByIndex(index);
		}
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(mSprites, index))
		{
			return null;
		}
		mSprites[index].atlasScale = finalAtlasScale;
		return mSprites[index];
	}

	public void ClearSprite()
	{
		if (mReplacement != null)
		{
			mReplacement.ClearSprite();
		}
		else
		{
			mSprites.Clear();
		}
	}

	public void AddSprite(UISpriteData sd)
	{
		if (mReplacement != null)
		{
			mReplacement.AddSprite(sd);
			return;
		}
		sd.atlasScale = finalAtlasScale;
		mSprites.Add(sd);
	}

	public void RemoveSpriteByIndex(int index)
	{
		if (mReplacement != null)
		{
			mReplacement.RemoveSpriteByIndex(index);
		}
		else if (Vespa_Generic_IList_Extension_Method.IsValidIndex(mSprites, index))
		{
			mSprites.RemoveAt(index);
		}
	}

	public void SortSprite(Comparison<UISpriteData> comp)
	{
		if (mReplacement != null)
		{
			mReplacement.SortSprite(comp);
		}
		else
		{
			mSprites.Sort(comp);
		}
	}

	public UISpriteData GetSprite(string name)
	{
		if (mReplacement != null)
		{
			return mReplacement.GetSprite(name);
		}
		if (!string.IsNullOrEmpty(name))
		{
			if (mSprites.Count == 0)
			{
				return null;
			}
			if (mSpriteIndices.Count != mSprites.Count)
			{
				MarkSpriteListAsChanged();
			}
			if (mSpriteIndices.TryGetValue(name, out var value))
			{
				if (value > -1 && value < mSprites.Count)
				{
					mSprites[value].atlasScale = finalAtlasScale;
					return mSprites[value];
				}
				MarkSpriteListAsChanged();
				if (mSpriteIndices.TryGetValue(name, out value))
				{
					mSprites[value].atlasScale = finalAtlasScale;
					return mSprites[value];
				}
				return null;
			}
			int i = 0;
			for (int count = mSprites.Count; i < count; i++)
			{
				UISpriteData uISpriteData = mSprites[i];
				if (!string.IsNullOrEmpty(uISpriteData.name) && name == uISpriteData.name)
				{
					MarkSpriteListAsChanged();
					uISpriteData.atlasScale = finalAtlasScale;
					return uISpriteData;
				}
			}
		}
		return null;
	}

	public string GetRandomSprite(string startsWith)
	{
		if (GetSprite(startsWith) == null)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < GetSpriteCount(); i++)
			{
				UISpriteData spriteByIndex = GetSpriteByIndex(i);
				if (spriteByIndex.name.StartsWith(startsWith))
				{
					list.Add(spriteByIndex.name);
				}
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return startsWith;
	}

	public void MarkSpriteListAsChanged()
	{
		mSpriteIndices.Clear();
		int i = 0;
		for (int count = mSprites.Count; i < count; i++)
		{
			mSpriteIndices[mSprites[i].name] = i;
		}
	}

	public void SortAlphabetically()
	{
		mSprites.Sort((UISpriteData s1, UISpriteData s2) => s1.name.CompareTo(s2.name));
	}

	public BetterList<string> GetListOfSprites()
	{
		if (mReplacement != null)
		{
			return mReplacement.GetListOfSprites();
		}
		BetterList<string> betterList = new BetterList<string>();
		int i = 0;
		for (int count = mSprites.Count; i < count; i++)
		{
			UISpriteData uISpriteData = mSprites[i];
			if (uISpriteData != null && !string.IsNullOrEmpty(uISpriteData.name))
			{
				betterList.Add(uISpriteData.name);
			}
		}
		return betterList;
	}

	public BetterList<string> GetListOfSprites(string match)
	{
		if ((bool)mReplacement)
		{
			return mReplacement.GetListOfSprites(match);
		}
		if (string.IsNullOrEmpty(match))
		{
			return GetListOfSprites();
		}
		BetterList<string> betterList = new BetterList<string>();
		int i = 0;
		for (int count = mSprites.Count; i < count; i++)
		{
			UISpriteData uISpriteData = mSprites[i];
			if (uISpriteData != null && !string.IsNullOrEmpty(uISpriteData.name) && string.Equals(match, uISpriteData.name, StringComparison.OrdinalIgnoreCase))
			{
				betterList.Add(uISpriteData.name);
				return betterList;
			}
		}
		string[] array = match.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = array[j].ToLower();
		}
		int k = 0;
		for (int count2 = mSprites.Count; k < count2; k++)
		{
			UISpriteData uISpriteData2 = mSprites[k];
			if (uISpriteData2 == null || string.IsNullOrEmpty(uISpriteData2.name))
			{
				continue;
			}
			string text = uISpriteData2.name.ToLower();
			int num = 0;
			for (int l = 0; l < array.Length; l++)
			{
				if (text.Contains(array[l]))
				{
					num++;
				}
			}
			if (num == array.Length)
			{
				betterList.Add(uISpriteData2.name);
			}
		}
		return betterList;
	}

	private bool References(UIAtlas atlas)
	{
		if (atlas == null)
		{
			return false;
		}
		if (atlas == this)
		{
			return true;
		}
		if (!(mReplacement != null))
		{
			return false;
		}
		return mReplacement.References(atlas);
	}

	public static bool CheckIfRelated(UIAtlas a, UIAtlas b)
	{
		if (a == null || b == null)
		{
			return false;
		}
		if (!(a == b) && !a.References(b))
		{
			return b.References(a);
		}
		return true;
	}

	public void MarkAsChanged()
	{
		if (mReplacement != null)
		{
			mReplacement.MarkAsChanged();
		}
		UISprite[] array = NGUITools.FindActive<UISprite>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UISprite uISprite = array[i];
			if (CheckIfRelated(this, uISprite.atlas))
			{
				UIAtlas atlas = uISprite.atlas;
				uISprite.atlas = null;
				uISprite.atlas = atlas;
			}
		}
		UIFont[] array2 = Resources.FindObjectsOfTypeAll(typeof(UIFont)) as UIFont[];
		int j = 0;
		for (int num2 = array2.Length; j < num2; j++)
		{
			UIFont uIFont = array2[j];
			if (CheckIfRelated(this, uIFont.atlas))
			{
				UIAtlas atlas2 = uIFont.atlas;
				uIFont.atlas = null;
				uIFont.atlas = atlas2;
			}
		}
		UILabel[] array3 = NGUITools.FindActive<UILabel>();
		int k = 0;
		for (int num3 = array3.Length; k < num3; k++)
		{
			UILabel uILabel = array3[k];
			if (uILabel.bitmapFont != null && CheckIfRelated(this, uILabel.bitmapFont.atlas))
			{
				UIFont bitmapFont = uILabel.bitmapFont;
				uILabel.bitmapFont = null;
				uILabel.bitmapFont = bitmapFont;
			}
		}
	}

	private void AdjustSpriteData()
	{
		if (!(replacement != null) && atlasWidth != 0 && atlasHeight != 0 && !(texture == null) && (texture.width != atlasWidth || texture.height != atlasHeight) && !_adjustedAtlasSize)
		{
			_adjustedAtlasSize = true;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(mSprites); i++)
			{
				mSprites[i].AdjustByAtlasSize(atlasWidth, atlasHeight, texture.width, texture.height);
			}
			atlasWidth = texture.width;
			atlasHeight = texture.height;
		}
	}
}
