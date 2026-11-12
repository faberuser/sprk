using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGame2.NEditor;

public class ReplaceSpriteToolsBehaviour : MonoBehaviour
{
	[Serializable]
	public class AtlasSpriteData
	{
		public UIAtlas Atlas;

		public string Sprite;

		public void OnSelectAtlas(UnityEngine.Object obj)
		{
			Atlas = obj as UIAtlas;
		}

		public void OnSelectSprite(string spriteName)
		{
			Sprite = spriteName;
		}

		public void CopyFromGameObject(GameObject go)
		{
			if (go == null)
			{
				return;
			}
			UISprite component = go.GetComponent<UISprite>();
			if (component != null)
			{
				CopyFromSprite(component);
				return;
			}
			UIButton component2 = go.GetComponent<UIButton>();
			if (component2 != null)
			{
				CopyFromButton(component2);
			}
		}

		private void CopyFromSprite(UISprite sprite)
		{
			Atlas = sprite.atlas;
			Sprite = sprite.spriteName;
		}

		private void CopyFromButton(UIButton button)
		{
			GameObject tweenTarget = button.tweenTarget;
			if (!(tweenTarget == null))
			{
				UISprite component = tweenTarget.GetComponent<UISprite>();
				if (!(component == null))
				{
					Atlas = component.atlas;
					Sprite = button.normalSprite;
				}
			}
		}
	}

	[Serializable]
	public class AdjustWidget
	{
		public float Left;

		public float Right;

		public float Bottom;

		public float Top;
	}

	[Serializable]
	public class ConvertEntry
	{
		public AtlasSpriteData From = new AtlasSpriteData();

		public AtlasSpriteData To = new AtlasSpriteData();

		public AdjustWidget AdjustOnApply = new AdjustWidget();

		public void Apply(UISprite sprite)
		{
			sprite.atlas = To.Atlas;
			sprite.spriteName = To.Sprite;
			if (AdjustOnApply.Left > Mathf.Epsilon || AdjustOnApply.Right > Mathf.Epsilon || AdjustOnApply.Bottom > Mathf.Epsilon || AdjustOnApply.Top > Mathf.Epsilon)
			{
				NGUIMath.AdjustWidget(sprite, AdjustOnApply.Left, AdjustOnApply.Bottom, AdjustOnApply.Right, AdjustOnApply.Top);
			}
		}
	}

	[HideInInspector]
	public List<ConvertEntry> ConvertList;

	public ConvertEntry GetConvertEntry(UIAtlas atlas, string spriteName)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ConvertList); i++)
		{
			ConvertEntry convertEntry = ConvertList[i];
			if (convertEntry.From.Atlas == atlas && convertEntry.From.Sprite == spriteName)
			{
				return convertEntry;
			}
		}
		return null;
	}

	public ConvertEntry GetConvertEntry(UISprite sprite)
	{
		return GetConvertEntry(sprite.atlas, sprite.spriteName);
	}

	public bool IsReplaceTarget(UIButton button)
	{
		UIAtlas buttonAtlas = GetButtonAtlas(button);
		if (GetConvertEntry(buttonAtlas, button.normalSprite) != null)
		{
			return true;
		}
		if (GetConvertEntry(buttonAtlas, button.hoverSprite) != null)
		{
			return true;
		}
		if (GetConvertEntry(buttonAtlas, button.pressedSprite) != null)
		{
			return true;
		}
		if (GetConvertEntry(buttonAtlas, button.disabledSprite) != null)
		{
			return true;
		}
		return false;
	}

	public static UIAtlas GetButtonAtlas(UIButton button)
	{
		UISprite component = button.tweenTarget.GetComponent<UISprite>();
		if (component == null)
		{
			return null;
		}
		return component.atlas;
	}
}
