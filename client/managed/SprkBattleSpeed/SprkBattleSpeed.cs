using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NNguiExtension;
using NVespa;
using UnityEngine;

/// <summary>
/// Selectable battle speed for the native x1.5 buttons (SystemButtonAuto and
/// SystemButtonSpeed). The native buttons keep their Normal/Fast state, which
/// still drives the sprite, glow effect and BattleManagement's fast-speed gate;
/// this class remembers which multiplier "Fast" means and draws the button text.
/// </summary>
public static class SprkBattleSpeed
{
	public const int NormalPercent = 100;

	public const int LegacyFastPercent = 150;

	public static readonly int[] Percents = { 25, 50, 75, 100, 125, 150, 175, 200 };

	private static readonly LocalAccountInt stored = new LocalAccountInt(0, "SprkBattleSpeed.Percent");

	public static bool IsValid(int percent)
	{
		return Array.IndexOf(Percents, percent) >= 0;
	}

	/// <summary>Selected speed in percent; 100 is normal speed.</summary>
	public static int Percent
	{
		get
		{
			int value = stored.Value;
			return IsValid(value) ? value : NormalPercent;
		}
		set
		{
			if (IsValid(value))
			{
				stored.Value = value;
			}
		}
	}

	/// <summary>
	/// Loads the account's speed, adopting the old x1.5 toggle the first time,
	/// and returns whether the native button should be in its Fast state.
	/// </summary>
	public static bool Restore(bool legacyFast)
	{
		if (!IsValid(stored.Value))
		{
			stored.Value = legacyFast ? LegacyFastPercent : NormalPercent;
		}
		return Percent != NormalPercent;
	}

	/// <summary>Time scale while the native fast-speed gate is open.</summary>
	public static float Ratio(float fallback)
	{
		int percent = Percent;
		return percent == NormalPercent ? fallback : percent / 100f;
	}

	public static string Multiplier(int percent)
	{
		return "x" + (percent / 100f).ToString("0.##", CultureInfo.InvariantCulture);
	}

	/// <summary>Index into the label sheet (0 = "Off", then <see cref="Percents"/> in order).</summary>
	public static int LabelIndex(int percent)
	{
		return Array.IndexOf(Percents, percent) + 1;
	}

	public static int ButtonLabel(bool on)
	{
		int percent = Percent;
		return on && percent != NormalPercent ? LabelIndex(percent) : 0;
	}

	/// <summary>Adds the text overlay to a native speed button. Safe to call on every Open.</summary>
	public static SprkBattleSpeedButton Attach(UISpriteSelector selector)
	{
		if (selector == null)
		{
			return null;
		}
		UISprite sprite = selector.GetTargetSprite();
		if (sprite == null)
		{
			return null;
		}
		SprkBattleSpeedButton button = sprite.GetComponent<SprkBattleSpeedButton>();
		if (button == null)
		{
			button = sprite.gameObject.AddComponent<SprkBattleSpeedButton>();
		}
		button.Initialize(sprite, selector);
		return button;
	}

	/// <summary>Opens or closes the speed list under the button. <paramref name="setFast"/> updates the native state.</summary>
	public static void ToggleMenu(UISpriteSelector selector, Action<bool> setFast)
	{
		SprkBattleSpeedButton button = Attach(selector);
		if (button != null)
		{
			button.ToggleMenu(delegate(int percent)
			{
				Percent = percent;
				setFast?.Invoke(percent != NormalPercent);
			});
		}
	}
}

/// <summary>
/// Draws a text-free copy of the native x1.5 art with the selected speed's label,
/// and owns the drop-down list of speeds. The art bakes "x1.5" into the sprite, so
/// the text is covered with tiles cut from the plain interior of the same sprite,
/// then a pre-rendered label in the original lettering is laid on top
/// </summary>
public sealed class SprkBattleSpeedButton : MonoBehaviour
{
	private sealed class Face
	{
		// Measured on the 250x129 btn_x1.5_on/off art: plain fill at x 28..44,
		// text and glow inside x 45..205, y 22..107.
		private const float StripLeft = 28f / 250f;

		private const float StripWidth = 16f / 250f;

		private const float CoverLeft = 45f / 250f;

		private const float CoverRight = 205f / 250f;

		private const float CoverTop = 22f / 129f;

		private const float CoverBottom = 107f / 129f;

		private readonly GameObject root;

		private readonly List<UISprite> tiles = new List<UISprite>();

		private readonly UITexture label;

		public Face(Transform parent, int layer)
		{
			root = new GameObject("SprkSpeedFace");
			root.layer = layer;
			root.transform.SetParent(parent, worldPositionStays: false);
			GameObject text = new GameObject("Label_Speed");
			text.layer = layer;
			text.transform.SetParent(root.transform, worldPositionStays: false);
			label = text.AddComponent<UITexture>();
			label.pivot = UIWidget.Pivot.TopLeft;
			root.SetActive(value: false);
		}

		public void Destroy()
		{
			if (root != null)
			{
				UnityEngine.Object.Destroy(root);
			}
		}

		/// <summary>Lays the face over <paramref name="host"/>, which shows <paramref name="spriteName"/>.</summary>
		public bool Paint(UISprite host, string spriteName, bool on, int labelIndex)
		{
			UIAtlas atlas = host.atlas;
			UISpriteData data = atlas != null && !string.IsNullOrEmpty(spriteName) ? atlas.GetSprite(spriteName) : null;
			string fill = data != null ? FillSprite(atlas, data) : null;
			Texture2D sheet = LabelSheet();
			if (fill == null || sheet == null)
			{
				if (root.activeSelf)
				{
					root.SetActive(value: false);
				}
				return false;
			}
			float w = data.fWidth;
			float h = data.fHeight;
			Vector4 drawn = host.drawingDimensions;
			root.transform.localPosition = new Vector3(drawn.x, drawn.w, 0f);
			root.transform.localScale = new Vector3((drawn.z - drawn.x) / w, (drawn.w - drawn.y) / h, 1f);
			int stripWidth = Mathf.Max(1, Mathf.RoundToInt(StripWidth * w));
			int left = Mathf.RoundToInt(CoverLeft * w);
			int top = Mathf.RoundToInt(CoverTop * h);
			int height = Mathf.RoundToInt((CoverBottom - CoverTop) * h);
			int count = Mathf.CeilToInt((Mathf.RoundToInt(CoverRight * w) - left) / (float)stripWidth);
			while (tiles.Count < count)
			{
				GameObject tile = new GameObject("Fill");
				tile.layer = root.layer;
				tile.transform.SetParent(root.transform, worldPositionStays: false);
				UISprite sprite = tile.AddComponent<UISprite>();
				sprite.pivot = UIWidget.Pivot.TopLeft;
				tiles.Add(sprite);
			}
			for (int i = 0; i < tiles.Count; i++)
			{
				UISprite tile = tiles[i];
				bool used = i < count;
				if (tile.gameObject.activeSelf != used)
				{
					tile.gameObject.SetActive(used);
				}
				if (used)
				{
					if (tile.atlas != atlas)
					{
						tile.atlas = atlas;
					}
					tile.spriteName = fill;
					tile.type = UIBasicSprite.Type.Simple;
					tile.width = stripWidth;
					tile.height = height;
					tile.depth = host.depth + 1;
					tile.transform.localPosition = new Vector3(left + i * stripWidth, -top, 0f);
				}
			}
			// Each sheet cell covers the whole button art, so it lays over it 1:1.
			if (label.mainTexture != sheet)
			{
				label.mainTexture = sheet;
			}
			label.uvRect = LabelCell(sheet, on, labelIndex);
			label.depth = host.depth + 2;
			label.width = Mathf.Max(2, Mathf.RoundToInt(w));
			label.height = Mathf.Max(2, Mathf.RoundToInt(h));
			label.transform.localPosition = Vector3.zero;
			label.name = "Label_" + (labelIndex == 0 ? "Off" : SprkBattleSpeed.Multiplier(SprkBattleSpeed.Percents[labelIndex - 1])) + (on ? "_On" : "_Off");
			if (!root.activeSelf)
			{
				root.SetActive(value: true);
			}
			return true;
		}

		private static string FillSprite(UIAtlas atlas, UISpriteData source)
		{
			string name = source.name + "#sprk-speed-fill";
			if (atlas.GetSprite(name) != null)
			{
				return name;
			}
			// SetRect takes unscaled atlas pixels; the f* accessors include atlasScale.
			float scale = source.atlasScale > 0f ? source.atlasScale : 1f;
			float x = source.fX / scale;
			float y = source.fY / scale;
			float width = source.fWidth / scale;
			float height = source.fHeight / scale;
			if (width < 40f || height < 20f)
			{
				return null;
			}
			UISpriteData fill = new UISpriteData();
			fill.CopyFrom(source);
			fill.name = name;
			fill.SetRect(Mathf.RoundToInt(x + StripLeft * width), Mathf.RoundToInt(y + CoverTop * height), Mathf.Max(1, Mathf.RoundToInt(StripWidth * width)), Mathf.RoundToInt((CoverBottom - CoverTop) * height));
			fill.SetBorder(0, 0, 0, 0);
			fill.SetPadding(0, 0, 0, 0);
			atlas.AddSprite(fill);
			atlas.MarkSpriteListAsChanged();
			return atlas.GetSprite(name) != null ? name : null;
		}
	}

	private const float ItemScale = 0.8f;

	private const int ItemGap = 4;

	// 250x129 button art, PAD apart, COLUMNS per row.
	private const string LabelResource = "BattleSpeedLabels.png";

	private const int LabelCount = 9;

	private const int CellWidth = 250;

	private const int CellHeight = 129;

	private const int CellPad = 8;

	private const int CellColumns = 3;

	private static Texture2D labelSheet;

	private static bool labelSheetFailed;

	private UISprite target;

	private UISpriteSelector selector;

	private Face face;

	private GameObject menu;

	private readonly List<KeyValuePair<int, Face>> items = new List<KeyValuePair<int, Face>>();

	private readonly List<UISprite> itemBacks = new List<UISprite>();

	private Action<int> onSelect;

	public bool MenuOpen => menu != null;

	public void Initialize(UISprite sprite, UISpriteSelector spriteSelector)
	{
		target = sprite;
		selector = spriteSelector;
		if (face == null)
		{
			face = new Face(target.transform, target.gameObject.layer);
		}
	}

	private string StateSprite(bool on)
	{
		if (selector != null && selector.SelectList != null)
		{
			string state = on ? "Fast" : "Normal";
			foreach (UISpriteSelector.SelectData item in selector.SelectList)
			{
				if (item != null && item.Name == state && !string.IsNullOrEmpty(item.SpriteName))
				{
					return item.SpriteName;
				}
			}
		}
		string current = target != null ? target.spriteName : null;
		if (string.IsNullOrEmpty(current))
		{
			return null;
		}
		if (on)
		{
			return current.EndsWith("_off", StringComparison.Ordinal) ? current.Substring(0, current.Length - 4) + "_on" : current;
		}
		return current.EndsWith("_on", StringComparison.Ordinal) ? current.Substring(0, current.Length - 3) + "_off" : current;
	}

	private bool IsOnSprite(string spriteName)
	{
		string on = StateSprite(on: true);
		if (on != null && spriteName == on)
		{
			return true;
		}
		return spriteName != null && spriteName.EndsWith("_on", StringComparison.Ordinal);
	}

	private void LateUpdate()
	{
		if (target == null || face == null)
		{
			return;
		}
		bool on = IsOnSprite(target.spriteName);
		face.Paint(target, target.spriteName, on, SprkBattleSpeed.ButtonLabel(on));
		if (menu != null)
		{
			PaintMenu();
		}
	}

	private void OnDisable()
	{
		CloseMenu();
	}

	private void OnDestroy()
	{
		CloseMenu();
	}

	public void ToggleMenu(Action<int> select)
	{
		if (menu != null)
		{
			CloseMenu();
			return;
		}
		onSelect = select;
		OpenMenu();
	}

	public void CloseMenu()
	{
		if (menu != null)
		{
			UnityEngine.Object.Destroy(menu);
		}
		menu = null;
		items.Clear();
		itemBacks.Clear();
		onSelect = null;
	}

	private void OpenMenu()
	{
		if (target == null || target.atlas == null)
		{
			return;
		}
		int layer = target.gameObject.layer;
		menu = new GameObject("SprkSpeedMenu");
		menu.layer = layer;
		menu.transform.SetParent(target.transform, worldPositionStays: false);
		UIPanel panel = menu.AddComponent<UIPanel>();
		// Draw above every other battle window (the damage meter sits beside the
		// button); NGUI orders draw calls by sorting order first, then depth.
		List<UIPanel> panels = UnityEngine.Object.FindObjectsOfType<UIPanel>().Where((UIPanel p) => p != panel).ToList();
		panel.depth = panels.Select((UIPanel p) => p.depth).DefaultIfEmpty(0).Max() + 10;
		panel.sortingOrder = panels.Select((UIPanel p) => p.sortingOrder).DefaultIfEmpty(0).Max() + 1;
		panel.renderQueue = UIPanel.RenderQueue.StartAt;
		panel.startingRenderQueue = 4800;
		GameObject blocker = new GameObject("Outside_ClickClose");
		blocker.layer = layer;
		blocker.transform.SetParent(menu.transform, worldPositionStays: false);
		UIWidget blockerWidget = blocker.AddComponent<UIWidget>();
		blockerWidget.width = 8000;
		blockerWidget.height = 8000;
		blockerWidget.depth = 0;
		NGUITools.AddWidgetCollider(blocker);
		UIEventListener.Get(blocker).onClick = delegate
		{
			CloseMenu();
		};
		int itemWidth = Mathf.RoundToInt(target.width * ItemScale);
		int itemHeight = Mathf.RoundToInt(target.height * ItemScale);
		int[] percents = SprkBattleSpeed.Percents;
		float listTop = -target.height * 0.5f - 8f;
		UIAtlas atlas = target.atlas;
		if (atlas.GetSprite("blank_white") != null)
		{
			GameObject back = new GameObject("Back");
			back.layer = layer;
			back.transform.SetParent(menu.transform, worldPositionStays: false);
			UISprite backSprite = back.AddComponent<UISprite>();
			backSprite.atlas = atlas;
			backSprite.spriteName = "blank_white";
			backSprite.type = UIBasicSprite.Type.Simple;
			backSprite.color = new Color(0f, 0f, 0f, 0.6f);
			backSprite.pivot = UIWidget.Pivot.Top;
			backSprite.width = itemWidth + 12;
			backSprite.height = percents.Length * (itemHeight + ItemGap) - ItemGap + 12;
			backSprite.depth = 1;
			back.transform.localPosition = new Vector3(0f, listTop + 6f, 0f);
			NGUITools.AddWidgetCollider(back);
		}
		for (int i = 0; i < percents.Length; i++)
		{
			int percent = percents[i];
			GameObject item = new GameObject("Speed_" + percent);
			item.layer = layer;
			item.transform.SetParent(menu.transform, worldPositionStays: false);
			item.transform.localPosition = new Vector3(0f, listTop - itemHeight * 0.5f - i * (itemHeight + ItemGap), 0f);
			UISprite back = item.AddComponent<UISprite>();
			back.atlas = atlas;
			back.type = UIBasicSprite.Type.Simple;
			back.pivot = UIWidget.Pivot.Center;
			back.width = itemWidth;
			back.height = itemHeight;
			back.depth = 10 + i * 4;
			NGUITools.AddWidgetCollider(item);
			UIButton button = item.AddComponent<UIButton>();
			button.tweenTarget = item;
			button.hover = new Color(0.9f, 0.9f, 0.9f, 1f);
			button.pressed = new Color(0.7f, 0.7f, 0.7f, 1f);
			EventDelegate.Add(button.onClick, delegate
			{
				Select(percent);
			});
			itemBacks.Add(back);
			items.Add(new KeyValuePair<int, Face>(percent, new Face(item.transform, layer)));
		}
		PaintMenu();
	}

	private void PaintMenu()
	{
		int selected = SprkBattleSpeed.Percent;
		string on = StateSprite(on: true);
		string off = StateSprite(on: false);
		for (int i = 0; i < items.Count; i++)
		{
			int percent = items[i].Key;
			bool active = percent == selected;
			UISprite back = itemBacks[i];
			if (back == null)
			{
				continue;
			}
			back.spriteName = active ? on : off;
			items[i].Value.Paint(back, back.spriteName, active, SprkBattleSpeed.LabelIndex(percent));
		}
	}

	private void Select(int percent)
	{
		Action<int> select = onSelect;
		CloseMenu();
		select?.Invoke(percent);
	}

	private static Texture2D LabelSheet()
	{
		if (labelSheet != null || labelSheetFailed)
		{
			return labelSheet;
		}
		labelSheetFailed = true;
		using (Stream stream = typeof(SprkBattleSpeedButton).Assembly.GetManifestResourceStream(LabelResource))
		{
			if (stream == null)
			{
				Debug.LogError("SprkBattleSpeed: missing " + LabelResource);
				return null;
			}
			using MemoryStream memory = new MemoryStream();
			stream.CopyTo(memory);
			// Mipmaps keep the glow smooth at the button's ~0.45x on-screen scale.
			Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true);
			if (!texture.LoadImage(memory.ToArray(), markNonReadable: true))
			{
				UnityEngine.Object.Destroy(texture);
				Debug.LogError("SprkBattleSpeed: cannot decode " + LabelResource);
				return null;
			}
			texture.name = "SprkBattleSpeedLabels";
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.filterMode = FilterMode.Trilinear;
			texture.anisoLevel = 2;
			labelSheet = texture;
		}
		labelSheetFailed = false;
		return labelSheet;
	}

	private static Rect LabelCell(Texture2D sheet, bool on, int labelIndex)
	{
		int index = (on ? LabelCount : 0) + Mathf.Clamp(labelIndex, 0, LabelCount - 1);
		float x = index % CellColumns * (CellWidth + 2 * CellPad) + CellPad;
		float top = index / CellColumns * (CellHeight + 2 * CellPad) + CellPad;
		return new Rect(x / sheet.width, 1f - (top + CellHeight) / sheet.height, (float)CellWidth / sheet.width, (float)CellHeight / sheet.height);
	}
}
