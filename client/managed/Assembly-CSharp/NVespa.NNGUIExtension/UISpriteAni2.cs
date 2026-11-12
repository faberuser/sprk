using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/UI/Sprite Ani2 (Vespa)")]
public class UISpriteAni2 : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	protected int mFPS = 30;

	[HideInInspector]
	[SerializeField]
	protected string mPrefix = "";

	[HideInInspector]
	[SerializeField]
	protected bool mLoop = true;

	[HideInInspector]
	[SerializeField]
	protected bool mReverse;

	[HideInInspector]
	[SerializeField]
	protected bool mSnap = true;

	protected UISprite mSprite;

	protected float mDelta;

	protected int mIndex;

	protected bool mActive = true;

	protected List<string> mSpriteNames = new List<string>();

	public int frames => mSpriteNames.Count;

	public int framesPerSecond
	{
		get
		{
			return mFPS;
		}
		set
		{
			mFPS = value;
		}
	}

	public string namePrefix
	{
		get
		{
			return mPrefix;
		}
		set
		{
			if (mPrefix != value)
			{
				mPrefix = value;
				RebuildSpriteList();
			}
		}
	}

	public bool loop
	{
		get
		{
			return mLoop;
		}
		set
		{
			mLoop = value;
		}
	}

	public bool isPlaying => mActive;

	protected virtual void Start()
	{
		RebuildSpriteList();
	}

	protected virtual void Update()
	{
		if (!mActive || mSpriteNames.Count == 0 || !Application.isPlaying || (float)mFPS < Mathf.Epsilon)
		{
			return;
		}
		mDelta += RealTime.deltaTime;
		float num = 1f / (float)mFPS;
		if (num > mDelta)
		{
			return;
		}
		int num2 = (int)(mDelta / num);
		mDelta -= num * (float)num2;
		mIndex += num2;
		if (mIndex >= mSpriteNames.Count)
		{
			mActive = loop;
			if (loop)
			{
				mIndex %= mSpriteNames.Count;
			}
			else
			{
				mIndex = 0;
			}
		}
		if (mActive)
		{
			mSprite.spriteName = GetSpriteName(mIndex);
			if (mSnap)
			{
				mSprite.MakePixelPerfect();
			}
		}
	}

	public void RebuildSpriteList()
	{
		if (mSprite == null)
		{
			mSprite = GetComponent<UISprite>();
		}
		mSpriteNames.Clear();
		if (!(mSprite != null) || !(mSprite.atlas != null))
		{
			return;
		}
		int i = 0;
		for (int spriteCount = mSprite.atlas.GetSpriteCount(); i < spriteCount; i++)
		{
			UISpriteData spriteByIndex = mSprite.atlas.GetSpriteByIndex(i);
			if (string.IsNullOrEmpty(mPrefix) || spriteByIndex.name.StartsWith(mPrefix))
			{
				mSpriteNames.Add(spriteByIndex.name);
			}
		}
		mSpriteNames.Sort();
	}

	public void Reset()
	{
		mActive = true;
		mIndex = 0;
		if (mSprite != null && mSpriteNames.Count > 0)
		{
			mSprite.spriteName = GetSpriteName(mIndex);
			if (mSnap)
			{
				mSprite.MakePixelPerfect();
			}
		}
	}

	private string GetSpriteName(int i_index)
	{
		if (mReverse)
		{
			return mSpriteNames[mSpriteNames.Count - mIndex - 1];
		}
		return mSpriteNames[mIndex];
	}
}
