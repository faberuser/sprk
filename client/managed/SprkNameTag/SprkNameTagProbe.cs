using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NGame2.NUI;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NChatting;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

/// <summary>
/// Prefab-fixture probe (docs/client-automation.md) for name tags, run on the
/// Login screen. Checks marker handling on plain labels, then renders real
/// friend list slots and a chat line with fixture tags and checks where the
/// badges land. `screens-show` leaves them on screen for a screenshot.
/// </summary>
public static class SprkNameTagProbe
{
	private static GameObject shown;

	private static readonly Dictionary<string, List<UIAtlas>> atlases = new Dictionary<string, List<UIAtlas>>();

	private static string Marked(long tag, string nick)
	{
		return SprkNameTag.MarkStart + tag.ToString() + SprkNameTag.MarkEnd + nick;
	}

	public static string Run(string mode)
	{
		string text = Fixtures();
		string text2 = Labels();
		string text3 = Screens(mode == "screens-show");
		return "PASS " + text + "; " + text2 + "; " + text3;
	}

	/// <summary>Two tags: one with a (large, then shrunk) icon, one text-only.</summary>
	private static string Fixtures()
	{
		Texture2D texture2D = Leaf(200);
		byte[] png = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		Texture2D texture2D2 = SprkNameTag.DecodeIcon(png);
		Check(texture2D2 != null, "icon did not decode");
		Check(texture2D2.width == SprkNameTag.MaxIconSide && texture2D2.height == SprkNameTag.MaxIconSide, "icon not shrunk: " + texture2D2.width + "x" + texture2D2.height);
		SprkNameTag.SetFixture(1L, "WAO", new Color32(168, 230, 161, byte.MaxValue), new Color32(30, 31, 34, 235), texture2D2);
		SprkNameTag.SetFixture(2L, "GM", new Color32(byte.MaxValue, 215, 90, byte.MaxValue), new Color32(120, 30, 40, 235), null);
		return "icon 200px -> " + texture2D2.width + "px";
	}

	private static string Labels()
	{
		UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		GameObject gameObject0 = VespaResource.Load<GameObject>(FriendSlotComponent.PrefabPath);
		Check(gameObject0 != null, "friend slot prefab missing");
		UILabel uILabel = gameObject0.GetComponentsInChildren<UILabel>(includeInactive: true).FirstOrDefault((UILabel l) => l.bitmapFont != null || l.trueTypeFont != null);
		Check(uILabel != null, "no font in the friend slot prefab");
		GameObject gameObject = new GameObject("NameTagProbe_Labels");
		gameObject.layer = uIRoot.gameObject.layer;
		gameObject.transform.SetParent(uIRoot.transform, worldPositionStays: false);
		gameObject.AddComponent<UIPanel>();
		try
		{
			UILabel uILabel2 = NGUITools.AddWidget<UILabel>(gameObject);
			uILabel2.bitmapFont = uILabel.bitmapFont;
			uILabel2.trueTypeFont = uILabel.trueTypeFont;
			uILabel2.fontSize = 24;
			uILabel2.overflowMethod = UILabel.Overflow.ResizeFreely;
			uILabel2.text = Marked(1L, "Raider");
			SprkNameTagBadge badge = uILabel2.SprkNameTagBadge as SprkNameTagBadge;
			Check(uILabel2.text == "Raider", "marker left in text: " + uILabel2.text);
			Check(badge != null && badge.TagId == 1, "no badge for a tagged name");
			badge.Refresh();
			Check(badge.Root != null && badge.Root.activeSelf, "badge not shown");
			uILabel2.text = "[ff0000]" + Marked(2L, "Raider") + "[-]";
			Check(uILabel2.text == "[ff0000]Raider[-]" && badge.TagId == 2, "coloured name: " + uILabel2.text + " tag " + badge.TagId);
			uILabel2.text = "Bye " + Marked(1L, "Raider") + "!";
			Check(uILabel2.text == "Bye Raider!" && badge.TagId == 0, "name in a sentence: " + uILabel2.text + " tag " + badge.TagId);
			badge.Refresh();
			Check(!badge.Root.activeSelf, "badge kept for a sentence");
			uILabel2.text = Marked(1L, "Raider\nLv. 3");
			Check(badge.TagId == 0, "badge on a multi-line label");
			uILabel2.text = Marked(1L, "Raider");
			uILabel2.text = "Raider";
			Check(badge.TagId == 0, "badge kept after plain text (recycled list item)");
			uILabel2.text = Marked(424242L, "Raider");
			badge.Refresh();
			Check(uILabel2.text == "Raider" && !badge.Root.activeSelf, "unknown tag drawn before the server answered");
			uILabel2.text = SprkNameTag.MarkStart + "x" + SprkNameTag.MarkEnd + "Raider";
			Check(uILabel2.text == "Raider" && badge.TagId == 0, "malformed marker: " + uILabel2.text);
			Check(SprkNameTag.Carry(Marked(1L, "Old"), "New") == Marked(1L, "New"), "nick change dropped the tag");
			Check(SprkNameTag.Carry("Old", "New") == "New", "nick change invented a tag");
			Check(SprkNameTag.Strip("a" + Marked(1L, "b")) == "ab", "strip");
			return "label rules ok";
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(gameObject);
		}
	}

	/// <summary>Friend slots (tag with icon, text-only tag, none) and a chat line.</summary>
	private static string Screens(bool keep)
	{
		if (shown != null)
		{
			UnityEngine.Object.DestroyImmediate(shown);
		}
		UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		shown = new GameObject("NameTagShow");
		shown.layer = uIRoot.gameObject.layer;
		shown.transform.SetParent(uIRoot.transform, worldPositionStays: false);
		UIPanel uIPanel = shown.AddComponent<UIPanel>();
		uIPanel.depth = 600;
		GameObject gameObject = VespaResource.Load<GameObject>(FriendSlotComponent.PrefabPath);
		Check(gameObject != null, "friend slot prefab missing");
		GameObject gameObject2 = VespaResource.Load<GameObject>("Prefab2/Component/Chatting/ChattingComponent");
		Check(gameObject2 != null, "chat prefab missing");
		string[] array = new string[3]
		{
			Marked(1L, "Raider93900"),
			Marked(2L, "Raider51531"),
			"Raider12345"
		};
		FriendSlotComponent[] array2 = new FriendSlotComponent[array.Length];
		float num = 0f;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject3 = UnityEngine.Object.Instantiate(gameObject, shown.transform, worldPositionStays: false);
			gameObject3.SetActive(value: true);
			array2[i] = gameObject3.GetComponent<FriendSlotComponent>();
			if (i > 0)
			{
				gameObject3.transform.localPosition = array2[0].transform.localPosition - new Vector3(0f, num * (float)i, 0f);
			}
			else
			{
				num = NGUIMath.CalculateRelativeWidgetBounds(gameObject3.transform).size.y + 12f;
			}
		}
		GameObject gameObject4 = UnityEngine.Object.Instantiate(gameObject2, shown.transform, worldPositionStays: false);
		gameObject4.SetActive(value: true);
		gameObject4.transform.localPosition = array2[0].transform.localPosition - new Vector3(0f, num * (float)array.Length + 20f, 0f);
		ChattingComponent component = gameObject4.GetComponent<ChattingComponent>();
		AssignAtlases(shown);
		FieldInfo field = typeof(FriendSlotComponent).GetField("_nameLabel", BindingFlags.Instance | BindingFlags.NonPublic);
		List<string> list = new List<string>();
		for (int j = 0; j < array.Length; j++)
		{
			array2[j].SetName(array[j]);
			UILabel uILabel = (UILabel)field.GetValue(array2[j]);
			list.Add(Measure("friend" + (j + 1), uILabel, (j < 2) ? (j + 1) : 0));
		}
		component.SetSenderName(Marked(1L, "Raider93900"));
		// SetChat's own text path (it also needs a ChatInfo for item links).
		component.ChatLabel.gameObject.SetActive(value: true);
		UIHelper.SetLabelText(component.ChatLabel, "hello " + Marked(2L, "spoof") + " world");
		Check(component.ChatLabel.text == "hello spoof world", "chat text kept a marker: " + component.ChatLabel.text);
		Check(!(component.ChatLabel.SprkNameTagBadge is SprkNameTagBadge sprkNameTagBadge) || sprkNameTagBadge.TagId == 0, "chat text got a badge");
		list.Add(Measure("chat", component.NameLabel, 1));
		// Chat anchors ":" and the message after the name label: they must clear the badge.
		gameObject4.GetComponentsInChildren<UIRect>(includeInactive: true).ToList().ForEach((UIRect r) => r.UpdateAnchors());
		SprkNameTagBadge chatBadge = (SprkNameTagBadge)component.NameLabel.SprkNameTagBadge;
		Transform transform = gameObject4.transform.Find("NonChatRoot/Label_Splitter");
		if (transform != null)
		{
			float x = NGUIMath.CalculateAbsoluteWidgetBounds(chatBadge.Root.transform).max.x;
			float x2 = NGUIMath.CalculateAbsoluteWidgetBounds(transform).min.x;
			Check(x2 >= x - 0.001f, "chat ':' under the badge: " + x2 + " < " + x);
			list.Add("chat ':' clears the badge");
		}
		{
			UILabel nameLabel = component.NameLabel;
			UIWidget splitter = (transform != null) ? transform.GetComponent<UIWidget>() : null;
			Func<UIRect, string> anchor = (UIRect r) => (r == null) ? "-" : (r.leftAnchor.target != null ? r.leftAnchor.target.name + "/" + r.leftAnchor.relative + "+" + r.leftAnchor.absolute : "none");
			list.Add("chat name overflow " + nameLabel.overflowMethod + " w " + nameLabel.width + " pivot " + nameLabel.pivot + " anchor " + anchor(nameLabel) + "; splitter anchor " + anchor(splitter) + " x " + (splitter ? splitter.transform.localPosition.x.ToString("0") : "-") + "; chat anchor " + anchor(component.ChatLabel));
		}
		string text = string.Join("; ", list);
		if (!keep)
		{
			UnityEngine.Object.DestroyImmediate(shown);
			return text;
		}
		uIPanel.depth = 5000;
		uIPanel.alpha = 1f;
		Camera camera = NGUITools.FindCameraForLayer(shown.layer);
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(shown.transform);
		Vector3 vector = camera.WorldToScreenPoint(bounds.max) - camera.WorldToScreenPoint(bounds.min);
		Check(vector.y > 1f && vector.x > 1f, "slots not on screen");
		shown.transform.localScale *= Mathf.Min((float)Screen.height * 0.92f / vector.y, (float)Screen.width * 0.92f / vector.x);
		Bounds bounds2 = NGUIMath.CalculateAbsoluteWidgetBounds(shown.transform);
		shown.transform.position += uIRoot.transform.position - bounds2.center;
		return text + "; shown";
	}

	/// <summary>Checks the badge sits right after the printed name, centred on its line.</summary>
	private static string Measure(string what, UILabel label, long tag)
	{
		Check(label != null, what + " has no name label");
		Check(label.text.IndexOf(SprkNameTag.MarkStart) < 0, what + " text kept a marker");
		SprkNameTagBadge sprkNameTagBadge = label.SprkNameTagBadge as SprkNameTagBadge;
		if (tag == 0)
		{
			Check(sprkNameTagBadge == null || sprkNameTagBadge.TagId == 0, what + " untagged name got a badge");
			return what + " plain";
		}
		Check(sprkNameTagBadge != null && sprkNameTagBadge.TagId == tag, what + " badge missing");
		sprkNameTagBadge.Refresh();
		Check(sprkNameTagBadge.Root != null && sprkNameTagBadge.Root.activeSelf, what + " badge hidden");
		UITexture uITexture = sprkNameTagBadge.Root.GetComponentsInChildren<UITexture>(includeInactive: true).First((UITexture t) => t.name == "Pill");
		UILabel uILabel = sprkNameTagBadge.Root.GetComponentInChildren<UILabel>(includeInactive: true);
		// Text corners in the label's space, from NGUI's own character positions.
		Vector3[] worldCorners = label.worldCorners;
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(uITexture.transform);
		Vector3 vector = label.transform.InverseTransformPoint(bounds.min);
		Vector3 vector2 = label.transform.InverseTransformPoint(bounds.max);
		Vector2 printedSize = label.printedSize;
		Vector2 pivotOffset = label.pivotOffset;
		float num = (0f - pivotOffset.x) * (float)label.width;
		float num2 = Mathf.Lerp(label.height, 0f, pivotOffset.y) + Mathf.Lerp(printedSize.y - (float)label.height, 0f, pivotOffset.y);
		float num3 = num2 - printedSize.y * 0.5f;
		float num4 = (vector.y + vector2.y) * 0.5f;
		Check(Mathf.Abs(num4 - num3) <= printedSize.y * 0.25f, what + " badge off the line: centre " + num4 + " vs text " + num3);
		Check(vector2.y - vector.y <= printedSize.y * 1.1f, what + " badge taller than the line: " + (vector2.y - vector.y) + " > " + printedSize.y);
		Check(uILabel != null && uILabel.text == SprkNameTag.Get(tag).Label, what + " badge text");
		Check(uITexture.depth >= label.depth, what + " badge below the name");
		// The badge sits inside the padding: after the name, within the printed text.
		float num5 = alignedStart(label, printedSize);
		float paddingWidth = sprkNameTagBadge.PaddingWidth * sprkNameTagBadge.Scale;
		Check(paddingWidth > 0f, what + " name not padded");
		float num6 = num5 + printedSize.x - paddingWidth;
		Check(vector.x >= num6 - 1f, what + " badge overlaps the name: starts " + vector.x + " < name end " + num6);
		Check(vector2.x <= num5 + printedSize.x + 1f, what + " badge beyond the label's text: ends " + vector2.x + " > " + (num5 + printedSize.x));
		return what + " '" + label.text.TrimEnd() + "' scale " + sprkNameTagBadge.Scale.ToString("0.00") + " name end " + num6.ToString("0") + " printed " + printedSize.x.ToString("0") + "x" + printedSize.y.ToString("0") + " from x=" + num.ToString("0") + ", badge x " + vector.x.ToString("0") + ".." + vector2.x.ToString("0") + " h " + (vector2.y - vector.y).ToString("0") + " (corners " + worldCorners.Length + ")";
	}

	private static float alignedStart(UILabel label, Vector2 printedSize)
	{
		Vector2 pivotOffset = label.pivotOffset;
		float num = (0f - pivotOffset.x) * (float)label.width;
		NGUIText.Alignment alignment = label.alignment;
		if (alignment == NGUIText.Alignment.Automatic)
		{
			alignment = ((pivotOffset.x < 0.25f) ? NGUIText.Alignment.Left : ((pivotOffset.x > 0.75f) ? NGUIText.Alignment.Right : NGUIText.Alignment.Center));
		}
		return alignment switch
		{
			NGUIText.Alignment.Center => num + ((float)label.width - printedSize.x) * 0.5f,
			NGUIText.Alignment.Right => num + (float)label.width - printedSize.x,
			_ => num,
		};
	}

	/// <summary>A leaf-like green blob on transparency, as an uploaded icon would be.</summary>
	private static Texture2D Leaf(int side)
	{
		Texture2D texture2D = new Texture2D(side, side, TextureFormat.RGBA32, mipChain: false);
		Color32[] array = new Color32[side * side];
		float num = (float)side * 0.5f;
		for (int i = 0; i < side; i++)
		{
			for (int j = 0; j < side; j++)
			{
				float num2 = ((float)j - num) / num;
				float num3 = ((float)i - num) / num;
				float num4 = (num2 + num3) * 0.7071f;
				float num5 = (num2 - num3) * 0.7071f;
				bool flag = num4 * num4 / 0.81f + num5 * num5 / 0.25f <= 1f;
				array[i * side + j] = (flag ? new Color32(110, 200, 90, byte.MaxValue) : new Color32(0, 0, 0, 0));
			}
		}
		texture2D.SetPixels32(array);
		texture2D.Apply();
		return texture2D;
	}

	private static void AssignAtlases(GameObject root)
	{
		bool patchDownloaded = Define.PatchDownloaded;
		Define.PatchDownloaded = true;
		try
		{
			AtlasAssignerBase[] componentsInChildren = root.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Assign();
			}
		}
		finally
		{
			Define.PatchDownloaded = patchDownloaded;
		}
		foreach (string item in (from a in root.GetComponentsInChildren<UISpriteAtlasAssigner>(includeInactive: true)
			where !string.IsNullOrEmpty(a.AtlasName) && (a.Target == null || a.Target.atlas == null)
			select a.AtlasName).Distinct())
		{
			if (atlases.ContainsKey(item))
			{
				continue;
			}
			List<UIAtlas> list = (atlases[item] = new List<UIAtlas>());
			string path = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle/HighAndMidSpec/NguiAtlas_UI_Prefab_" + item + ".unity3d");
			AssetBundle assetBundle = (File.Exists(path) ? AssetBundle.LoadFromFile(path) : null);
			if (!(assetBundle == null))
			{
				list.AddRange(from g in assetBundle.LoadAllAssets<GameObject>()
					select g.GetComponent<UIAtlas>() into a
					where a != null
					select a);
			}
		}
		List<UIAtlas> source = atlases.Values.SelectMany((List<UIAtlas> l) => l).ToList();
		UISprite[] componentsInChildren2 = root.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite sprite in componentsInChildren2)
		{
			if (sprite.atlas == null && !string.IsNullOrEmpty(sprite.spriteName))
			{
				UIAtlas uIAtlas = source.FirstOrDefault((UIAtlas a) => a.GetSprite(sprite.spriteName) != null);
				if (uIAtlas != null)
				{
					string spriteName = sprite.spriteName;
					sprite.atlas = uIAtlas;
					sprite.spriteName = spriteName;
				}
			}
		}
	}

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Name tag: " + message);
		}
	}
}
