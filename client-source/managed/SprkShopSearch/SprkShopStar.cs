using System;
using System.Linq;
using NShared;
using UnityEngine;

public sealed class SprkShopStar : MonoBehaviour
{
	private NewPayShopProductInfo info;

	private GameObject root;

	private UISprite sprite;

	private UILabel glyph;

	private bool shown;

	private static readonly Color On = new Color(1f, 0.84f, 0.25f, 1f);

	private static readonly Color Off = new Color(0.42f, 0.4f, 0.38f, 0.45f);

	public void Bind(Component owner, UIWidget anchor, NewPayShopProductInfo product, int size, float inset)
	{
		info = product;
		if (root == null)
		{
			if (anchor == null)
			{
				anchor = owner.GetComponentInChildren<UIWidget>(includeInactive: true);
			}
			Build(owner, anchor, size, inset);
		}
		Refresh();
	}

	private void Build(Component owner, UIWidget anchor, int size, float inset)
	{
		int num = 0;
		UIPanel uIPanel = anchor.panel ?? NGUITools.FindInParents<UIPanel>(anchor.gameObject);
		UIWidget[] componentsInChildren = owner.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			if (uIWidget.panel == null || uIWidget.panel == uIPanel)
			{
				num = Math.Max(num, uIWidget.depth);
			}
		}
		root = new GameObject("SprkBookmark");
		root.layer = anchor.gameObject.layer;
		root.transform.SetParent(anchor.transform, worldPositionStays: false);
		Vector3[] localCorners = anchor.localCorners;
		root.transform.localPosition = new Vector3(localCorners[2].x - inset, localCorners[2].y - inset, 0f);
		int num2 = size + 22;
		UIWidget uIWidget2 = root.AddComponent<UIWidget>();
		uIWidget2.width = num2;
		uIWidget2.height = num2;
		uIWidget2.depth = num + 10;
		if (SprkShopIcons.Star(out var atlas, out var spriteName))
		{
			sprite = NGUITools.AddWidget<UISprite>(root);
			sprite.atlas = atlas;
			sprite.spriteName = spriteName;
			sprite.type = UIBasicSprite.Type.Simple;
			sprite.width = size;
			sprite.height = size;
			sprite.depth = num + 11;
		}
		else
		{
			glyph = NGUITools.AddWidget<UILabel>(root);
			UILabel uILabel = owner.GetComponentsInChildren<UILabel>(includeInactive: true).FirstOrDefault((UILabel l) => l.bitmapFont != null || l.trueTypeFont != null);
			if (uILabel != null)
			{
				glyph.bitmapFont = uILabel.bitmapFont;
				glyph.trueTypeFont = uILabel.trueTypeFont;
			}
			glyph.fontSize = size;
			glyph.width = size + 16;
			glyph.height = size + 16;
			glyph.depth = num + 11;
			glyph.overflowMethod = UILabel.Overflow.ShrinkContent;
			glyph.alignment = NGUIText.Alignment.Center;
			glyph.effectStyle = UILabel.Effect.Outline;
			glyph.effectColor = new Color(0.2f, 0.13f, 0.05f, 1f);
		}
		root.AddComponent<BoxCollider>().size = new Vector3(num2, num2, 1f);
		UIButton uIButton = root.AddComponent<UIButton>();
		uIButton.tweenTarget = null;
		EventDelegate.Add(uIButton.onClick, OnClick);
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(anchor.gameObject);
		if (uIScrollView != null)
		{
			root.AddComponent<UIDragScrollView>().scrollView = uIScrollView;
		}
	}

	private void OnClick()
	{
		if (info != null)
		{
			SprkShopSearch.Toggle(info.Index);
			Refresh();
		}
	}

	private void Refresh()
	{
		bool flag = (shown = info != null && SprkShopSearch.IsBookmarked(info.Index));
		root.SetActive(info != null);
		if (sprite != null)
		{
			sprite.color = (flag ? On : Off);
		}
		if (glyph != null)
		{
			glyph.text = (flag ? "★" : "☆");
			glyph.color = (flag ? On : Off);
		}
	}

	private void Update()
	{
		if (root != null && info != null && SprkShopSearch.IsBookmarked(info.Index) != shown)
		{
			Refresh();
		}
	}
}
