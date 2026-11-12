using System;
using UnityEngine;

[Serializable]
public class UISpriteNumber : MonoBehaviour
{
	public bool Invert;

	public string[] NumSpriteArray;

	public UISprite[] SpriteArray;

	public UISprite[] SpriteArray2;

	private int _value;

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				UpdateSprites(SpriteArray);
				UpdateSprites(SpriteArray2);
			}
		}
	}

	private void UpdateSprites(UISprite[] spriteArray)
	{
		if (__Extension_Method_ICollection__.COUNT(spriteArray) == 0)
		{
			return;
		}
		int num = Value;
		int num2 = 10;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(spriteArray); i++)
		{
			int num3 = num % num2;
			num /= num2;
			if (__Extension_Method_ICollection__.COUNT(NumSpriteArray) > num3)
			{
				int num4 = i;
				if (Invert)
				{
					num4 = __Extension_Method_ICollection__.COUNT(spriteArray) - 1 - i;
				}
				spriteArray[num4].spriteName = NumSpriteArray[num3];
				continue;
			}
			break;
		}
	}

	private void Start()
	{
		UpdateSprites(SpriteArray);
		UpdateSprites(SpriteArray2);
	}
}
