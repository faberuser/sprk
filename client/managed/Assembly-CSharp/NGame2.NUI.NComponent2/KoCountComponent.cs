using System;
using System.Collections;
using System.Collections.Generic;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class KoCountComponent : LinkFactoryComponent, LinkFactoryComponent.IReleaseEvent
{
	private const string PrefabPath = "Resources/Prefab2/Component/PhaseStepReward/KoCountComponent";

	public static PrefabAuto<KoCountComponent> PrefabAuto = new PrefabAuto<KoCountComponent>("Resources/Prefab2/Component/PhaseStepReward/KoCountComponent");

	[Header("SettingInEditor")]
	public Transform UnusedRoot;

	public UIGrid NumberGrid;

	public Transform LeftPos;

	public Transform RightPos;

	public string[] NumberSpriteNameArray;

	public List<UISprite> NumberSpriteList;

	[Tooltip("NumberSpriteList와 동일한 순서")]
	public List<UISprite> NumberShadowSpriteList;

	[Header("Options")]
	public bool IsInvert;

	public bool IsAnimate;

	public int AnimateDurationFPS;

	private long _value = -1L;

	private int _multiple = 1;

	private long _maximumNumber;

	private int _curDigitCount;

	private float _gridCellWidth;

	private int _spriteWidth;

	private int _spriteHeight;

	private Coroutine ModifyCorotuine;

	public int Multiple
	{
		get
		{
			return _multiple;
		}
		set
		{
			if (_multiple != value)
			{
				_multiple = value;
				InitMultiple();
			}
		}
	}

	public long Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				if (IsAnimate)
				{
					ModifyValue(value);
				}
				else
				{
					SetValue(value);
				}
			}
		}
	}

	public void Open(long startValue = 0L)
	{
		if (__Extension_Method_ICollection__.COUNT(NumberSpriteList) != 0 && __Extension_Method_ICollection__.COUNT(NumberSpriteNameArray) == 10)
		{
			float num = Mathf.Pow(10f, __Extension_Method_ICollection__.COUNT(NumberSpriteList));
			_maximumNumber = (long)Math.Truncate(num);
			_curDigitCount = 0;
			_gridCellWidth = NumberGrid.cellWidth;
			_spriteWidth = NumberSpriteList[0].width;
			_spriteHeight = NumberSpriteList[0].height;
			Value = startValue;
			AddReleaseEvent(this);
		}
	}

	public virtual void OnRelease()
	{
		UpdateSpriteList(NumberSpriteList, 0);
		if (ModifyCorotuine != null)
		{
			StopCoroutine(ModifyCorotuine);
		}
		_curDigitCount = 0;
		_value = -1L;
	}

	private void InitMultiple()
	{
		NumberGrid.cellWidth = _gridCellWidth * (float)_multiple;
		foreach (UISprite numberSprite in NumberSpriteList)
		{
			numberSprite.width = _spriteWidth * _multiple;
			numberSprite.height = _spriteHeight * _multiple;
		}
		foreach (UISprite numberShadowSprite in NumberShadowSpriteList)
		{
			numberShadowSprite.width = _spriteWidth * _multiple;
			numberShadowSprite.height = _spriteHeight * _multiple;
		}
		NumberGrid.Reposition();
	}

	private void SetValue(long newValue)
	{
		if (_value != newValue)
		{
			if (newValue < 0)
			{
				newValue = 0L;
			}
			else if (newValue >= _maximumNumber)
			{
				newValue = _maximumNumber - 1;
			}
			_value = newValue;
			int digitCount = GetDigitCount(_value);
			UpdateSprites(NumberSpriteList, digitCount);
			UpdateSpriteList(NumberSpriteList, digitCount);
			UpdateSprites(NumberShadowSpriteList, digitCount);
			LeftPos.transform.position = NumberSpriteList[0].transform.position;
			RightPos.transform.position = NumberSpriteList[_curDigitCount - 1].transform.position;
		}
	}

	private void ModifyValue(long newValue)
	{
		if (ModifyCorotuine != null)
		{
			StopCoroutine(ModifyCorotuine);
		}
		ModifyCorotuine = StartCoroutine(ModifyCoroutine(newValue, AnimateDurationFPS));
	}

	private IEnumerator ModifyCoroutine(long newValue, int durationFPS)
	{
		if (durationFPS == 0)
		{
			durationFPS = 1;
		}
		WaitForSeconds wait = new WaitForSeconds(1f / (float)durationFPS);
		long stepAmount;
		try
		{
			float value = (float)(newValue - _value) / (float)durationFPS;
			stepAmount = Convert.ToInt64(value);
		}
		catch (Exception)
		{
			SetValue(newValue);
			yield break;
		}
		if (_value > newValue)
		{
			stepAmount = ((stepAmount == 0L) ? (-1) : stepAmount);
			while (_value > newValue)
			{
				SetValue(_value + stepAmount);
				yield return wait;
			}
		}
		else
		{
			stepAmount = ((stepAmount == 0L) ? 1 : stepAmount);
			while (_value < newValue)
			{
				SetValue(_value + stepAmount);
				yield return wait;
			}
		}
		SetValue(newValue);
	}

	private void UpdateSprites(List<UISprite> spriteArray, int digitCount)
	{
		if (__Extension_Method_ICollection__.COUNT(spriteArray) == 0)
		{
			return;
		}
		long num = _value;
		long num2 = 10L;
		for (int i = 0; i < digitCount; i++)
		{
			long num3 = num % num2;
			num /= num2;
			int index = digitCount - 1 - i;
			if (IsInvert)
			{
				index = i;
			}
			spriteArray[index].spriteName = NumberSpriteNameArray[num3];
		}
	}

	private int GetDigitCount(long value)
	{
		if (value == 0L)
		{
			return 1;
		}
		return (int)Math.Truncate(Mathf.Log10(value)) + 1;
	}

	private void UpdateSpriteList(List<UISprite> spriteArray, int digitCount)
	{
		if (__Extension_Method_ICollection__.COUNT(spriteArray) == 0)
		{
			return;
		}
		int num = digitCount - _curDigitCount;
		if (num == 0)
		{
			return;
		}
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				int index = _curDigitCount + i;
				UISprite uISprite = spriteArray[index];
				UIHelper.EnableGameObject(uISprite, enable: true);
				uISprite.transform.SetParent(NumberGrid.transform);
			}
		}
		else
		{
			num *= -1;
			for (int j = 0; j < num; j++)
			{
				int index2 = _curDigitCount - j - 1;
				UISprite uISprite2 = spriteArray[index2];
				UIHelper.EnableGameObject(uISprite2, enable: false);
				uISprite2.transform.SetParent(UnusedRoot);
			}
		}
		_curDigitCount = digitCount;
		NumberGrid.Reposition();
	}
}
