using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NNguiExtension;

[Serializable]
[RequireComponent(typeof(UISprite))]
public class UISpriteAnimation : MonoBehaviour
{
	private const int DEFAULT_INDEX = -1;

	public UISpriteAnimInfoContainer[] Container;

	private Vector3 _originPos;

	private UISprite _sprite;

	private Func<float> _deltaTimer;

	private Coroutine _coroutine;

	private Dictionary<int, int> frameRateContainer = new Dictionary<int, int>();

	private Dictionary<int, List<UISpriteAnimInfo>> SpriteNameConstianer = new Dictionary<int, List<UISpriteAnimInfo>>();

	public int CurrentIndex { get; private set; }

	private void Start()
	{
		CurrentIndex = -1;
		_sprite = GetComponent<UISprite>();
		_originPos = _sprite.transform.localPosition;
		UISpriteAnimInfoContainer[] container = Container;
		foreach (UISpriteAnimInfoContainer uISpriteAnimInfoContainer in container)
		{
			AddSpriteList(uISpriteAnimInfoContainer.Index, uISpriteAnimInfoContainer.AnimInfoList, uISpriteAnimInfoContainer.FrameRate);
		}
	}

	public bool AddSpriteList(int index, UISpriteAnimInfo[] spriteList, int frameRate)
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
		SpriteNameConstianer[index] = new List<UISpriteAnimInfo>(spriteList);
		frameRateContainer[index] = frameRate;
		return true;
	}

	public void SetState(int index)
	{
		if (SpriteNameConstianer.ContainsKey(index))
		{
			StopAnimation();
			CurrentIndex = index;
			_coroutine = ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coAnimation());
		}
	}

	public void StopAnimation()
	{
		CurrentIndex = -1;
		if (_coroutine != null)
		{
			VespaCoroutine.Stop(_coroutine);
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

	public void SetTimer(Func<float> timer)
	{
		_deltaTimer = timer;
	}

	private IEnumerator coAnimation()
	{
		List<UISpriteAnimInfo> spriteList = SpriteNameConstianer[CurrentIndex];
		int num = frameRateContainer[CurrentIndex];
		float interval = 1f / (float)num;
		float timer = 0f;
		int index = 0;
		int sprCount = __Extension_Method_ICollection__.COUNT(spriteList);
		do
		{
			timer -= DeltaTime();
			if (timer < 0f)
			{
				timer = interval;
				if (!String_Extension_Method.IsNullOrEmpty(spriteList[index].SpriteName))
				{
					_sprite.spriteName = spriteList[index].SpriteName;
					_sprite.MakePixelPerfect();
				}
				_sprite.transform.localPosition = _originPos + spriteList[index].PositionAdd;
				int num2 = index + 1;
				index = num2;
				index %= sprCount;
			}
			yield return null;
		}
		while (sprCount > 0);
	}

	protected virtual float DeltaTime()
	{
		if (_deltaTimer == null)
		{
			return Time.deltaTime;
		}
		return Vespa_System_Action_Extension_Method.fire(_deltaTimer);
	}
}
