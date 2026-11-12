using UnityEngine;

namespace NVespa.NNGUIExtension;

public class Tools
{
	public static void AddWidgetCache(Transform tm)
	{
		AddWidgetCache(tm, out var _, out var _);
	}

	public static void AddWidgetCache(Transform tm, out int assignCount, out bool foundPanel)
	{
		if (tm == null)
		{
			assignCount = 0;
			foundPanel = false;
		}
		else
		{
			assignCount = 0;
			foundPanel = false;
			AssignWidgetCacheRecursively(Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<UIWidgetCache>(tm.gameObject), tm, ref assignCount, ref foundPanel);
		}
	}

	private static void AssignWidgetCacheRecursively(UIWidgetCache widgetCache, Transform tm, ref int assignCount, ref bool foundPanel)
	{
		if (tm.GetComponent<UIPanel>() != null)
		{
			foundPanel = true;
			return;
		}
		UIWidgetCache component = tm.GetComponent<UIWidgetCache>();
		if (component != widgetCache)
		{
			VespaObject.DestroyObject(component);
		}
		UIWidget component2 = tm.GetComponent<UIWidget>();
		if (component2 != null)
		{
			component2.widgetCache = widgetCache;
			assignCount++;
		}
		for (int i = 0; i < tm.childCount; i++)
		{
			AssignWidgetCacheRecursively(widgetCache, tm.GetChild(i), ref assignCount, ref foundPanel);
		}
	}

	public static void ChangeLabelEffectFromToRecursively(Transform tm, UILabel.Effect effectFrom, UILabel.Effect effectTo)
	{
		if (!(tm == null))
		{
			UILabel component = tm.gameObject.GetComponent<UILabel>();
			if (component != null && component.rawEffectStyle == effectFrom)
			{
				component.rawEffectStyle = effectTo;
			}
			for (int i = 0; i < tm.childCount; i++)
			{
				ChangeLabelEffectFromToRecursively(tm.GetChild(i), effectFrom, effectTo);
			}
		}
	}

	public static void ChangeLabelEffectRecursively(Transform tm, UILabel.Effect labelEffect)
	{
		if (!(tm == null))
		{
			UILabel component = tm.gameObject.GetComponent<UILabel>();
			if (component != null)
			{
				component.rawEffectStyle = labelEffect;
			}
			for (int i = 0; i < tm.childCount; i++)
			{
				ChangeLabelEffectRecursively(tm.GetChild(i), labelEffect);
			}
		}
	}

	public static void ChangeAnchorExecuteRecursively(Transform tm, UIRect.AnchorUpdate anchorUpdate)
	{
		if (!(tm == null))
		{
			UIRect component = tm.gameObject.GetComponent<UIRect>();
			if (component != null)
			{
				component.updateAnchors = anchorUpdate;
			}
			for (int i = 0; i < tm.childCount; i++)
			{
				ChangeAnchorExecuteRecursively(tm.GetChild(i), anchorUpdate);
			}
		}
	}

	public static void ChangeAnchorNoneRecursively(Transform tm)
	{
		if (!(tm == null))
		{
			UIRect component = tm.gameObject.GetComponent<UIRect>();
			if (component != null)
			{
				component.SetAnchor((Transform)null);
			}
			for (int i = 0; i < tm.childCount; i++)
			{
				ChangeAnchorNoneRecursively(tm.GetChild(i));
			}
		}
	}

	public static void CreateAllSiblingSprite(UIWidget widget)
	{
		UISprite uISprite = widget as UISprite;
		if (uISprite == null)
		{
			return;
		}
		UIAtlas atlas = uISprite.atlas;
		if (atlas == null)
		{
			return;
		}
		for (int num = atlas.GetSpriteCount() - 1; num >= 0; num--)
		{
			UISpriteData spriteByIndex = atlas.GetSpriteByIndex(num);
			if (!(spriteByIndex.name == uISprite.spriteName))
			{
				NGUITools.AddSprite(widget.transform.parent.gameObject, atlas, spriteByIndex.name).name = spriteByIndex.name;
			}
		}
	}

	public static void RearrangeWidgetDepth(UIWidget widget)
	{
		if (!(widget == null))
		{
			int beginDepth = widget.depth + Mathf.Max(1, widget.reserveDepth);
			for (int i = 0; i < widget.transform.childCount; i++)
			{
				beginDepth = RearrangeWidgetDepth(widget.transform.GetChild(i), beginDepth);
			}
		}
	}

	public static int RearrangeWidgetDepth(Transform tm, int beginDepth)
	{
		int num = beginDepth;
		UIWidget component = tm.gameObject.GetComponent<UIWidget>();
		if (component != null)
		{
			component.depth = num;
			num += Mathf.Max(1, component.reserveDepth);
		}
		for (int i = 0; i < tm.childCount; i++)
		{
			num = RearrangeWidgetDepth(tm.GetChild(i), num);
		}
		return num;
	}
}
