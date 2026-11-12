using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

public class SpriteAnimation
{
	private const int DEFAULT_INDEX = -1;

	private Vector3 _originPos;

	private UISprite _sprite;

	private MonoBehaviour _mono;

	private Func<float> _deltaTimer;

	private Coroutine _coroutine;

	private Dictionary<int, int> frameRateContainer = new Dictionary<int, int>();

	private Dictionary<int, List<SpriteAnimInfo>> SpriteNameConstianer = new Dictionary<int, List<SpriteAnimInfo>>();

	public int Index { get; private set; }

	public SpriteAnimation(MonoBehaviour mono, Func<float> deltaTime, UISprite spr)
	{
		Index = -1;
		_originPos = spr.transform.localPosition;
		_sprite = spr;
		_mono = mono;
		_deltaTimer = deltaTime;
	}

	public bool AddSpriteList(int index, List<SpriteAnimInfo> spriteList, int frameRate)
	{
		if (index < 0)
		{
			VespaDebug.LogError("index < 0");
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(spriteList) == 0)
		{
			VespaDebug.LogError("spriteList.COUNT() == 0");
			return false;
		}
		if (frameRate == 0)
		{
			VespaDebug.LogError("Frame == 0");
			return false;
		}
		SpriteNameConstianer[index] = new List<SpriteAnimInfo>(spriteList);
		frameRateContainer[index] = frameRate;
		return true;
	}

	public void SetState(int index)
	{
		if (SpriteNameConstianer.ContainsKey(index))
		{
			StopAnimation();
			Index = index;
			_coroutine = ExtensionMethod_VespaCoroutine.StartCoroutineThis(_mono, coAnimation());
		}
	}

	public void StopAnimation()
	{
		Index = -1;
		if (_coroutine != null)
		{
			_mono.StopCoroutine(_coroutine);
		}
	}

	public void SetActive(bool active)
	{
		if (!active)
		{
			StopAnimation();
		}
		_sprite.gameObject.SetActive(active);
	}

	private IEnumerator coAnimation()
	{
		List<SpriteAnimInfo> spriteList = SpriteNameConstianer[Index];
		int num = frameRateContainer[Index];
		float interval = 1f / (float)num;
		float timer = 0f;
		int index = 0;
		int sprCount = __Extension_Method_ICollection__.COUNT(spriteList);
		do
		{
			timer -= Vespa_System_Action_Extension_Method.fire(_deltaTimer);
			if (timer < 0f)
			{
				timer = interval;
				_sprite.spriteName = spriteList[index].SpriteName;
				_sprite.transform.localPosition = _originPos + spriteList[index].PosAddVal;
				_sprite.MakePixelPerfect();
				int num2 = index + 1;
				index = num2;
				index %= sprCount;
			}
			yield return null;
		}
		while (sprCount > 0);
	}
}
