using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NWindow.NPayShop;
using UnityEngine;

public sealed class SprkShopSearchBar : MonoBehaviour
{
	private static readonly FieldInfo CurrentField = typeof(PayShop).GetField("_currentCategory", BindingFlags.Instance | BindingFlags.NonPublic);

	private PayShop shop;

	private Transform tagRoot;

	private UISprite frame;

	private UILabel tagLabel;

	private GameObject bar;

	private UISprite field;

	private UIInput input;

	private UILabel inputLabel;

	private UILabel title;

	private UILabel count;

	private UILabel hint;

	private GameObject clear;

	private string shown;

	private bool silent;

	private float pending = -1f;

	private bool styled;

	private Vector3 laidOut = new Vector3(float.NaN, 0f, 0f);

	private readonly HashSet<Behaviour> hiddenWidgets = new HashSet<Behaviour>();

	private readonly HashSet<Collider> hiddenColliders = new HashSet<Collider>();

	private string Current => (CurrentField.GetValue(shop) as string) ?? "";

	public void Bind(PayShop window)
	{
		shop = window;
		shown = null;
	}

	private void Build()
	{
		PayShopTagList payShopTagList = shop.PayShopTagList;
		tagRoot = payShopTagList.transform.Find("Root");
		if (tagRoot == null)
		{
			return;
		}
		frame = tagRoot.Find("Sprite_TagFrame")?.GetComponent<UISprite>();
		tagLabel = tagRoot.GetComponentsInChildren<UILabel>(includeInactive: true).FirstOrDefault((UILabel l) => l.name.Contains("TagLabel"));
		if (!(frame == null) && !(tagLabel == null))
		{
			bar = new GameObject("SprkShopSearchBar");
			bar.layer = tagRoot.gameObject.layer;
			bar.transform.SetParent(tagRoot, worldPositionStays: false);
			title = NGUITools.AddWidget<UILabel>(bar);
			Copy(tagLabel, title);
			title.name = "Title";
			GameObject gameObject = new GameObject("Input");
			gameObject.layer = bar.layer;
			gameObject.transform.SetParent(bar.transform, worldPositionStays: false);
			field = gameObject.AddComponent<UISprite>();
			field.atlas = frame.atlas;
			field.spriteName = frame.spriteName;
			field.type = UIBasicSprite.Type.Sliced;
			field.width = 400;
			field.height = 60;
			field.depth = frame.depth + 2;
			gameObject.AddComponent<BoxCollider>();
			inputLabel = NGUITools.AddWidget<UILabel>(gameObject);
			inputLabel.name = "Label_Search";
			Copy(tagLabel, inputLabel);
			inputLabel.pivot = UIWidget.Pivot.Left;
			inputLabel.alignment = NGUIText.Alignment.Left;
			inputLabel.overflowMethod = UILabel.Overflow.ClampContent;
			inputLabel.supportEncoding = false;
			inputLabel.height = 44;
			inputLabel.depth = field.depth + 1;
			input = gameObject.AddComponent<UIInput>();
			input.label = inputLabel;
			input.characterLimit = 40;
			input.inputType = UIInput.InputType.Standard;
			input.validation = UIInput.Validation.None;
			input.onReturnKey = UIInput.OnReturnKey.Submit;
			input.selectAllTextOnFocus = false;
			input.activeTextColor = tagLabel.color;
			input.caretColor = tagLabel.color;
			input.selectionColor = new Color(0.85f, 0.68f, 0.36f, 0.45f);
			input.value = "";
			EventDelegate.Add(input.onChange, OnChanged);
			EventDelegate.Add(input.onSubmit, OnSubmitted);
			count = NGUITools.AddWidget<UILabel>(gameObject);
			count.name = "Label_Count";
			Copy(tagLabel, count);
			count.fontSize = Math.Max(18, tagLabel.fontSize - 6);
			count.pivot = UIWidget.Pivot.Right;
			count.alignment = NGUIText.Alignment.Right;
			count.width = 180;
			count.height = 40;
			count.depth = field.depth + 1;
			count.color = new Color(tagLabel.color.r, tagLabel.color.g, tagLabel.color.b, 0.7f);
			clear = new GameObject("Button_Clear");
			clear.layer = bar.layer;
			clear.transform.SetParent(gameObject.transform, worldPositionStays: false);
			if (SprkShopIcons.Find(new string[2] { "PayShop_Tag_Close_Icon", "Common_Close" }, out var atlas, out var sprite))
			{
				UISprite uISprite = clear.AddComponent<UISprite>();
				uISprite.atlas = atlas;
				uISprite.spriteName = sprite;
				uISprite.MakePixelPerfect();
				uISprite.depth = field.depth + 2;
			}
			else
			{
				UILabel uILabel = clear.AddComponent<UILabel>();
				Copy(tagLabel, uILabel);
				uILabel.text = "X";
				uILabel.width = 40;
				uILabel.height = 40;
				uILabel.depth = field.depth + 2;
			}
			clear.AddComponent<BoxCollider>().size = new Vector3(56f, 56f, 1f);
			UIButton uIButton = clear.AddComponent<UIButton>();
			uIButton.tweenTarget = clear;
			EventDelegate.Add(uIButton.onClick, Clear);
			Transform transform = shop.PayShopProductList.transform;
			Transform transform2 = transform.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform t) => t.name == "DragScrollView");
			hint = NGUITools.AddWidget<UILabel>(((transform2 != null) ? transform2.parent : transform).gameObject);
			hint.name = "SprkSearchHint";
			Copy(tagLabel, hint);
			hint.width = 1100;
			hint.height = 120;
			hint.depth = frame.depth + 5;
			hint.overflowMethod = UILabel.Overflow.ShrinkContent;
			hint.alignment = NGUIText.Alignment.Center;
			hint.pivot = UIWidget.Pivot.Center;
			hint.transform.localPosition = ((transform2 != null) ? transform2.localPosition : Vector3.zero);
			bar.SetActive(value: false);
			hint.gameObject.SetActive(value: false);
		}
	}

	private void Layout()
	{
		Vector3[] worldCorners = frame.worldCorners;
		Vector3 vector = tagRoot.InverseTransformPoint(worldCorners[0]);
		Vector3 vector2 = tagRoot.InverseTransformPoint(worldCorners[2]);
		Vector3 vector3 = new Vector3(vector.x, vector2.x, (vector.y + vector2.y) * 0.5f);
		if (!(vector3 == laidOut))
		{
			laidOut = vector3;
			float z = vector3.z;
			Vector3 localPosition = tagRoot.InverseTransformPoint(tagLabel.transform.position);
			title.transform.localPosition = localPosition;
			float num = localPosition.x + ((tagLabel.pivot == UIWidget.Pivot.Left) ? ((float)tagLabel.width) : ((tagLabel.pivot == UIWidget.Pivot.Right) ? 0f : ((float)tagLabel.width * 0.5f))) + 110f;
			float num2 = vector2.x - 26f;
			int num3 = Math.Max(200, (int)(num2 - num));
			field.transform.localPosition = new Vector3(num + (float)num3 * 0.5f, z, 0f);
			field.width = num3;
			field.GetComponent<BoxCollider>().size = new Vector3(num3, field.height, 1f);
			inputLabel.width = Math.Max(80, num3 - 260);
			inputLabel.transform.localPosition = new Vector3((float)(-num3) * 0.5f + 30f, 0f, 0f);
			count.transform.localPosition = new Vector3((float)num3 * 0.5f - 80f, 0f, 0f);
			clear.transform.localPosition = new Vector3((float)num3 * 0.5f - 38f, 0f, 0f);
		}
	}

	private IEnumerable<Transform> Content()
	{
		PayShopTagList tagList = shop.PayShopTagList;
		if (tagList.HotTagRoot != null)
		{
			yield return tagList.HotTagRoot.transform;
		}
		Component[] array = new Component[4] { tagList.HotTagPos, tagList.SelectedTagPos, tagList.AllTagPos, tagList.Sprite_OpenArrow };
		foreach (Component component in array)
		{
			if (component != null)
			{
				yield return component.transform;
			}
		}
		Transform transform = tagRoot.Find("OpenAllTagButton");
		if (transform != null)
		{
			yield return transform;
		}
		yield return tagLabel.transform;
	}

	private void HideNative()
	{
		foreach (Transform item in Content())
		{
			if (item == null || item.IsChildOf(bar.transform))
			{
				continue;
			}
			UIWidget[] componentsInChildren = item.GetComponentsInChildren<UIWidget>(includeInactive: true);
			foreach (UIWidget uIWidget in componentsInChildren)
			{
				if (uIWidget.enabled)
				{
					uIWidget.enabled = false;
					hiddenWidgets.Add(uIWidget);
				}
			}
			Collider[] componentsInChildren2 = item.GetComponentsInChildren<Collider>(includeInactive: true);
			foreach (Collider collider in componentsInChildren2)
			{
				if (collider.enabled)
				{
					collider.enabled = false;
					hiddenColliders.Add(collider);
				}
			}
		}
		Collider component = frame.GetComponent<Collider>();
		if (component != null && component.enabled)
		{
			component.enabled = false;
			hiddenColliders.Add(component);
		}
	}

	private void RestoreNative()
	{
		foreach (Behaviour hiddenWidget in hiddenWidgets)
		{
			if (hiddenWidget != null)
			{
				hiddenWidget.enabled = true;
			}
		}
		foreach (Collider hiddenCollider in hiddenColliders)
		{
			if (hiddenCollider != null)
			{
				hiddenCollider.enabled = true;
			}
		}
		hiddenWidgets.Clear();
		hiddenColliders.Clear();
	}

	private void OnDisable()
	{
		RestoreNative();
	}

	private static void Copy(UILabel from, UILabel to)
	{
		to.bitmapFont = from.bitmapFont;
		to.trueTypeFont = from.trueTypeFont;
		to.fontSize = from.fontSize;
		to.fontStyle = from.fontStyle;
		to.color = from.color;
		to.effectStyle = from.effectStyle;
		to.effectColor = from.effectColor;
		to.effectDistance = from.effectDistance;
		to.pivot = from.pivot;
		to.alignment = from.alignment;
		to.width = from.width;
		to.height = from.height;
		to.depth = from.depth + 1;
		to.overflowMethod = from.overflowMethod;
		to.applyGradient = false;
	}

	private void MatchStyle()
	{
		if (!styled && !(field == null))
		{
			UISprite uISprite = shop.PayShopTagList.GetComponentsInChildren<UISprite>(includeInactive: true).FirstOrDefault((UISprite s) => s.name == "Sprite_BG" && s.atlas != null && s.transform.parent != null && s.transform.parent.name == "Group_Default");
			if (!(uISprite == null))
			{
				field.atlas = uISprite.atlas;
				field.spriteName = uISprite.spriteName;
				field.type = UIBasicSprite.Type.Sliced;
				field.color = uISprite.color;
				field.height = Math.Max(50, uISprite.height);
				field.GetComponent<BoxCollider>().size = new Vector3(field.width, field.height, 1f);
				styled = true;
			}
		}
	}

	private void OnChanged()
	{
		if (silent || input == null)
		{
			return;
		}
		string current = Current;
		if (SprkShopSearch.IsVirtual(current))
		{
			string text = input.value ?? "";
			if (!(text == SprkShopSearch.Query(current)))
			{
				SprkShopSearch.SetQuery(current, text);
				clear.SetActive(text.Length > 0);
				pending = Time.unscaledTime + 0.25f;
			}
		}
	}

	private void OnSubmitted()
	{
		OnChanged();
		if (pending >= 0f)
		{
			Apply();
		}
	}

	private void Clear()
	{
		SprkShopSearch.SetQuery(Current, "");
		SetSilently("");
		clear.SetActive(value: false);
		Apply();
	}

	private void SetSilently(string value)
	{
		silent = true;
		try
		{
			input.value = value;
		}
		finally
		{
			silent = false;
		}
	}

	private void Apply()
	{
		pending = -1f;
		string current = Current;
		if (!SprkShopSearch.IsVirtual(current))
		{
			return;
		}
		try
		{
			shop.PayShopProductList.CreateList(current);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Update()
	{
		if (shop == null)
		{
			return;
		}
		if (bar == null)
		{
			Build();
			if (bar == null)
			{
				return;
			}
		}
		string current = Current;
		bool flag = SprkShopSearch.IsVirtual(current);
		if (flag)
		{
			HideNative();
		}
		else if (hiddenWidgets.Count > 0 || hiddenColliders.Count > 0)
		{
			RestoreNative();
		}
		if (current != shown)
		{
			bar.SetActive(flag);
			if (flag)
			{
				bool flag2 = current == "SprkSearch";
				title.text = (flag2 ? "SEARCH" : "SAVED");
				input.defaultText = (flag2 ? "Search packages, items and descriptions" : "Filter bookmarked packages");
				string text = SprkShopSearch.Query(current);
				SetSilently(text);
				clear.SetActive(text.Length > 0);
				if (input.isSelected)
				{
					input.isSelected = false;
				}
			}
			pending = -1f;
			shown = current;
		}
		if (!flag)
		{
			if (hint.gameObject.activeSelf)
			{
				hint.gameObject.SetActive(value: false);
			}
			return;
		}
		MatchStyle();
		Layout();
		if (pending >= 0f && Time.unscaledTime >= pending)
		{
			Apply();
		}
		bool flag3 = current == "SprkSearch";
		bool flag4 = SprkShopSearch.Query(current).Trim().Length > 0;
		int num = ((SprkShopSearch.LastCategory == current) ? SprkShopSearch.LastCount : (-1));
		count.text = ((!flag4 || num < 0) ? (flag3 ? "" : (SprkShopSearch.BookmarkCount + " saved")) : ((num == 1) ? "1 result" : (num + " results")));
		string text2 = null;
		if (flag3 && !flag4)
		{
			text2 = "Type a package, item or description to search every shop category.";
		}
		else if (num <= 0 && flag4)
		{
			text2 = "No packages match \"" + SprkShopSearch.Query(current).Trim() + "\".";
		}
		else if (!flag3 && num <= 0)
		{
			text2 = "No bookmarks yet. Tap the star on any package to save it here.";
		}
		hint.text = text2 ?? "";
		if (hint.gameObject.activeSelf != (text2 != null))
		{
			hint.gameObject.SetActive(text2 != null);
		}
	}
}
