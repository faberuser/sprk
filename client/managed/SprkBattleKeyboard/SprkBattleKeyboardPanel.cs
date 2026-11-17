using System;
using System.Collections.Generic;
using System.Linq;
using NGame2;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;

using UnityEngine;

// Keyboard editor drawn with the native Game Settings parts (PayShop frame and title bar,
// Common_Box_3 option rows, Common_CheckBox toggles and Common_Button_Tab tabs).
public sealed class SprkBattleKeyboardPanel : MonoBehaviour
{
	internal static SprkBattleKeyboardPanel Current;
	private static GameOptionWindow lastBackWindow;
	private static int lastBackFrame = -1;
	private const string RowName = "SPRK Battle Keyboard";
	private const string Title = "Hero Skill Keyboard Controls";
	private const int FrameWidth = 1480;
	private const int FrameHeight = 980;
	private const int RowWidth = 1380;
	private const int KeyWidth = 230;
	private const int KeyHeight = 62;
	private static readonly float[] Columns = { -270f, 0f, 270f, 540f };
	private static readonly float[] Tabs = { -465f, -155f, 155f, 465f };
	private static readonly string[] ActionNames = { "Skill 1", "Skill 2", "Skill 3", "Soul Weapon" };
	private static readonly Color Ink = new Color(0.33f, 0.24f, 0.13f);
	private static readonly Color Capturing = new Color(1f, 0.86f, 0.35f);
	private static readonly Color Muted = new Color(0.62f, 0.58f, 0.52f);
	private GameOptionWindow window;
	private GameOptionButtonComponent row;
	private UIRoot uiRoot;
	private Transform content;
	private readonly List<UIAtlas> atlases = new List<UIAtlas>();
	private UILabel titleStyle;
	private UILabel nameStyle;
	private UILabel toggleStyle;
	private UILabel tabStyle;
	private UILabel buttonStyle;
	private readonly UILabel[] bindingLabels = new UILabel[16];
	private readonly UISprite[] bindingButtons = new UISprite[16];
	private readonly UISprite[] presetButtons = new UISprite[4];
	private readonly UILabel[] presetLabels = new UILabel[4];
	private UISprite controlsOn;
	private UISprite controlsOff;
	private UISprite hintsOn;
	private UISprite hintsOff;
	private UISprite captureFrame;
	private UILabel status;
	private UILabel confirmLabel;
	private GameObject clearButton;
	private GameObject cancelButton;
	private GameObject confirmButton;
	private GameObject copyButton;
	private UILabel copyLabel;
	private UIWidget backdrop;
	private int captureSlot = -1;
	private int captureFrameCount;
	private int duplicateSlot = -1;
	private SprkBattleKeyboard.Binding pending;
	private string message = "";
	private Vector2 screenSize;

	internal static void SetupRow(GameOptionWindow window)
	{
		if (window == null || window.MainTabs == null)
		{
			return;
		}
		foreach (GameOptionWindow.TabUIInfo tab in window.MainTabs)
		{
			if (tab.GameOptionTab != GameOptionTab.Battle || tab.GameOptionGrid == null)
			{
				continue;
			}
			GameOptionButtonComponent row = tab.GameOptionGrid.GetComponentsInChildren<GameOptionButtonComponent>(true)
				.FirstOrDefault(r => r.name == RowName);
			if (row == null)
			{
				row = UIHelper.AddGridItem(tab.GameOptionGrid, GameOptionButtonComponent.PrefabAuto);
			}
			row.name = RowName;
			row.enabled = false;
			if (row.ParentRoot != null)
			{
				if (row.ParentRoot.Root != null)
				{
					row.ParentRoot.Root.SetActive(true);
				}
				if (row.ParentRoot.Label_OptionName != null)
				{
					row.ParentRoot.Label_OptionName.text = Title;
				}
				if (row.ParentRoot.ToolTipImage != null)
				{
					row.ParentRoot.ToolTipImage.SetActive(false);
				}
			}
			if (row.ChildRoot != null && row.ChildRoot.Root != null
				&& row.ChildRoot.Root != (row.ParentRoot == null ? null : row.ParentRoot.Root))
			{
				row.ChildRoot.Root.SetActive(false);
			}
			RefreshRow(row);
			if (row.Button != null)
			{
				row.Button.onClick.Clear();
				GameOptionButtonComponent target = row;
				EventDelegate.Add(row.Button.onClick, delegate { Open(window, target); });
			}
			tab.GameOptionGrid.RepositionGrid();
		}
	}

	private static void RefreshRow(GameOptionButtonComponent row)
	{
		if (row != null && row.Label_Button != null)
		{
			row.Label_Button.text = SprkBattleKeyboard.ControlsEnabled ? "Configure (On)" : "Configure (Off)";
		}
	}

	private static void Open(GameOptionWindow window, GameOptionButtonComponent row)
	{
		if (Current != null || window == null)
		{
			return;
		}
		UIRoot root = NGUITools.FindInParents<UIRoot>(window.gameObject);
		if (root == null)
		{
			return;
		}
		int layer = row.Button != null ? row.Button.gameObject.layer : window.gameObject.layer;
		Transform parent = root.transform;
		if (GlobalUIRoot.isValidInstance && GlobalUIRoot.instance.UIRoot != null
			&& GlobalUIRoot.instance.ContentsGroup != null
			&& NGUITools.FindCameraForLayer(GlobalUIRoot.instance.gameObject.layer) != null)
		{
			root = GlobalUIRoot.instance.UIRoot;
			parent = GlobalUIRoot.instance.ContentsGroup.transform;
			layer = GlobalUIRoot.instance.gameObject.layer;
		}
		GameObject go = new GameObject("SPRK Battle Keyboard Settings");
		go.layer = layer;
		go.transform.SetParent(parent, false);
		SprkBattleKeyboardPanel panel = go.AddComponent<SprkBattleKeyboardPanel>();
		panel.window = window;
		panel.row = row;
		panel.uiRoot = root;
		Current = panel;
		try
		{
			panel.Build();
		}
		catch
		{
			panel.Close();
			throw;
		}
	}

	private void CollectStyles()
	{
		foreach (UISprite sprite in window.GetComponentsInChildren<UISprite>(true))
		{
			if (sprite.atlas != null && !atlases.Contains(sprite.atlas))
			{
				atlases.Add(sprite.atlas);
			}
		}
		UILabel[] labels = window.GetComponentsInChildren<UILabel>(true)
			.Where(l => l.bitmapFont != null || l.trueTypeFont != null).ToArray();
		Func<string, UILabel> under = part => labels.FirstOrDefault(l => PathOf(l.transform).Contains(part));
		titleStyle = under("Sprite_Title/");
		nameStyle = labels.FirstOrDefault(l => l.name == "OptionName");
		toggleStyle = labels.FirstOrDefault(l => l.name.StartsWith("Label_Toggle", StringComparison.Ordinal));
		tabStyle = under("Toggle_Battle/");
		buttonStyle = row.Label_Button != null && (row.Label_Button.bitmapFont != null || row.Label_Button.trueTypeFont != null)
			? row.Label_Button : toggleStyle;
	}

	private void Build()
	{
		CollectStyles();
		UIPanel panel = gameObject.AddComponent<UIPanel>();
		int depth = 0;
		foreach (UIPanel other in UIPanel.list)
		{
			if (other != null && other != panel)
			{
				depth = Math.Max(depth, other.depth);
			}
		}
		panel.depth = depth + 20;

		// Dimmed backdrop like the native Outside_ClickClose, swallowing clicks.
		UISprite dim = Sprite(transform, "Modal Blocker", "blank_white", 1920, 1080, Vector2.zero, 0);
		if (dim.atlas == null)
		{
			UnityEngine.Object.DestroyImmediate(dim);
			UITexture texture = dim.gameObject.AddComponent<UITexture>();
			texture.mainTexture = Texture2D.whiteTexture;
			backdrop = texture;
		}
		else
		{
			backdrop = dim;
		}
		backdrop.color = new Color(0f, 0f, 0f, 0.63f);
		NGUITools.AddWidgetCollider(backdrop.gameObject);
		UIEventListener.Get(backdrop.gameObject).onClick = delegate { };

		content = Child(transform, "Content").transform;
		UISprite fill = Sprite(content, "Frame Fill", "Common_Box_Text_2", FrameWidth, FrameHeight, Vector2.zero, 1);
		NGUITools.AddWidgetCollider(fill.gameObject);
		UIEventListener.Get(fill.gameObject).onClick = delegate { };
		Sprite(content, "Frame", "PayShop_Frame", FrameWidth, FrameHeight, Vector2.zero, 40);
		Sprite(content, "Title Bar", "PayShop_Title_Name", 1280, 90, new Vector2(0f, 430f), 41);
		Label(content, Title, 35, new Vector2(0f, 430f), 42, UIWidget.Pivot.Center, titleStyle, 1100);
		UISprite close = Sprite(content, "Close", "Btn_PayShop_Close", 70, 70, new Vector2(670f, 430f), 43, false);
		Clickable(close.gameObject, Close, 12);

		OptionRow("Keyboard Controls", 330f, out controlsOn, out controlsOff, delegate (bool on)
		{
			SprkBattleKeyboard.ControlsEnabled = on;
		});
		OptionRow("Key Hints in Battle", 238f, out hintsOn, out hintsOff, delegate (bool on)
		{
			SprkBattleKeyboard.KeyHintsEnabled = on;
		});

		Sprite(content, "Tab Deco", "Common_Deco_Top_1", 1420, 100, new Vector2(0f, 135f), 2);
		for (int p = 0; p < 4; p++)
		{
			int selected = p;
			presetButtons[p] = Sprite(content, "Layout " + p, "Common_Button_Tab_Disable", 300, 96, new Vector2(Tabs[p], 135f), 4);
			presetLabels[p] = Label(content, SprkBattleKeyboard.PresetNames[p], 30, new Vector2(Tabs[p], 135f), 6, UIWidget.Pivot.Center, tabStyle, 260);
			Clickable(presetButtons[p].gameObject, delegate
			{
				if (captureSlot >= 0) return;
				SprkBattleKeyboard.SelectedPreset = selected;
				message = "";
				Sync();
			});
		}

		for (int a = 0; a < 4; a++)
		{
			UILabel header = Label(content, ActionNames[a], 28, new Vector2(Columns[a], 62f), 6, UIWidget.Pivot.Center, null, KeyWidth);
			header.color = Ink;
			header.effectStyle = UILabel.Effect.None;
		}
		// Drawn under the key cap so only its glow border shows around the slot being captured.
		captureFrame = Sprite(content, "Capture Frame", "Common_Select_Frame", KeyWidth + 26, KeyHeight + 24, Vector2.zero, 6);
		for (int h = 0; h < 4; h++)
		{
			float y = -86f * h;
			Sprite(content, "Hero Row " + h, "Common_Box_3", RowWidth, 78, new Vector2(0f, y), 3);
			Label(content, "Hero " + (h + 1), 30, new Vector2(-660f, y), 6, UIWidget.Pivot.Left, nameStyle, 240);
			for (int a = 0; a < 4; a++)
			{
				int slot = h * 4 + a;
				bindingButtons[slot] = Sprite(content, "Binding " + slot, "Common_CheckBox", KeyWidth, KeyHeight, new Vector2(Columns[a], y), 7);
				bindingLabels[slot] = Label(bindingButtons[slot].transform, "", 30, Vector2.zero, 8, UIWidget.Pivot.Center, toggleStyle, KeyWidth - 20);
				bindingLabels[slot].supportEncoding = false;
				Clickable(bindingButtons[slot].gameObject, delegate { BeginCapture(slot); });
			}
		}

		status = Label(content, "", 26, new Vector2(0f, -338f), 6, UIWidget.Pivot.Center, null, 1300);
		status.color = Ink;
		status.effectStyle = UILabel.Effect.None;
		status.overflowMethod = UILabel.Overflow.ShrinkContent;
		status.height = 70;

		clearButton = Button("Clear Binding", "Common_Button7_Cancel", new Vector2(-540f, -425f), 300, delegate
		{
			if (captureSlot < 0) return;
			SprkBattleKeyboard.AssignCustom(captureSlot, default(SprkBattleKeyboard.Binding));
			EndCapture(SlotLabel(captureSlot) + " is now unbound.");
		}, out _);
		cancelButton = Button("Cancel", "Common_Button6_Normal", new Vector2(-245f, -425f), 250,
			delegate { EndCapture("Capture cancelled. No bindings changed."); }, out _);
		confirmButton = Button("Reassign", "Common_Button6_Emphasis", new Vector2(35f, -425f), 270, delegate
		{
			if (captureSlot < 0 || duplicateSlot < 0) return;
			string previous = SlotLabel(duplicateSlot);
			SprkBattleKeyboard.AssignCustom(captureSlot, pending);
			EndCapture(pending.Label + " moved to " + SlotLabel(captureSlot) + ". " + previous + " is now unbound.");
		}, out confirmLabel);
		copyButton = Button("Copy to Custom", "Common_Button6_Normal", new Vector2(-520f, -425f), 340, delegate
		{
			int source = SprkBattleKeyboard.SelectedPreset;
			if (captureSlot >= 0 || source >= 3) return;
			SprkBattleKeyboard.CopyPresetToCustom(source);
			SprkBattleKeyboard.SelectedPreset = 3;
			message = SprkBattleKeyboard.PresetNames[source] + " copied to Custom. Click a key to change it.";
			Sync();
		}, out copyLabel);
		Button("Done", "Common_Button6_Emphasis", new Vector2(560f, -425f), 260, Close, out _);
		Resize();
		Sync();
	}

	private void OptionRow(string name, float y, out UISprite on, out UISprite off, Action<bool> set)
	{
		Sprite(content, name + " Row", "Common_Box_3", RowWidth, 84, new Vector2(0f, y), 3);
		Label(content, name, 32, new Vector2(-660f, y), 6, UIWidget.Pivot.Left, nameStyle, 700);
		on = Toggle("On", new Vector2(330f, y), delegate
		{
			if (captureSlot >= 0) return;
			set(true);
			Sync();
		});
		off = Toggle("Off", new Vector2(590f, y), delegate
		{
			if (captureSlot >= 0) return;
			set(false);
			Sync();
		});
	}

	private UISprite Toggle(string text, Vector2 position, Action action)
	{
		UISprite box = Sprite(content, "Toggle " + text, "Common_CheckBox", 60, 60, position, 7);
		UISprite check = Sprite(box.transform, "Checked", "Common_Check", 124, 106, new Vector2(5f, 24f), 8, false);
		Label(content, text, 30, position + new Vector2(-48f, 0f), 6, UIWidget.Pivot.Right, toggleStyle, 200);
		// One hit area covering the caption and the box, like the native toggles.
		UIWidget hit = Widget<UIWidget>(content, "Toggle " + text + " Input", 220, 76, position + new Vector2(-80f, 0f), 10);
		Clickable(hit.gameObject, action);
		return check;
	}

	private void BeginCapture(int slot)
	{
		if (SprkBattleKeyboard.SelectedPreset != 3)
		{
			message = "Presets are read-only. Select Custom, or use Copy to Custom to start from this preset.";
			Sync();
			return;
		}
		captureSlot = slot;
		captureFrameCount = Time.frameCount;
		duplicateSlot = -1;
		message = "Press a key for " + SlotLabel(slot) + " (Ctrl / Alt / Shift optional).\n"
			+ "Esc cancels. Backspace or Delete unbinds.";
		Sync();
	}

	private void EndCapture(string text)
	{
		captureSlot = -1;
		duplicateSlot = -1;
		message = text;
		SprkBattleKeyboard.SuppressClosingFrame();
		Sync();
	}

	private static string SlotLabel(int slot)
	{
		return "Hero " + (slot / 4 + 1) + " " + ActionNames[slot % 4];
	}

	private string IdleMessage()
	{
		if (!SprkBattleKeyboard.ControlsEnabled)
		{
			return "Keyboard controls are off. Turn them on to use these keys in battle.";
		}
		if (SprkBattleKeyboard.SelectedPreset != 3)
		{
			return "Heroes are numbered left to right as shown in battle. Use Copy to Custom to edit this layout.";
		}
		return "Heroes are numbered left to right as shown in battle. Click a key to change it.";
	}

	private void Sync()
	{
		bool enabled = SprkBattleKeyboard.ControlsEnabled;
		bool hints = SprkBattleKeyboard.KeyHintsEnabled;
		SetChecked(controlsOn, enabled);
		SetChecked(controlsOff, !enabled);
		SetChecked(hintsOn, hints);
		SetChecked(hintsOff, !hints);
		int preset = SprkBattleKeyboard.SelectedPreset;
		for (int p = 0; p < presetButtons.Length; p++)
		{
			bool active = p == preset;
			presetButtons[p].spriteName = active ? "Common_Button_Tab_Enable" : "Common_Button_Tab_Disable";
			presetLabels[p].alpha = active ? 1f : 0.8f;
		}
		bool custom = preset == 3;
		for (int i = 0; i < bindingLabels.Length; i++)
		{
			SprkBattleKeyboard.Binding binding = SprkBattleKeyboard.GetBinding(i);
			bool capturing = i == captureSlot;
			bindingLabels[i].text = capturing ? "Press a key..." : binding.Label;
			Style(bindingLabels[i], toggleStyle);
			if (capturing)
			{
				bindingLabels[i].color = Capturing;
			}
			else if (binding.Key == KeyCode.None)
			{
				bindingLabels[i].color = Muted;
			}
			bindingButtons[i].alpha = custom || capturing ? 1f : 0.85f;
		}
		captureFrame.gameObject.SetActive(captureSlot >= 0);
		if (captureSlot >= 0)
		{
			captureFrame.transform.localPosition = bindingButtons[captureSlot].transform.localPosition;
		}
		status.text = string.IsNullOrEmpty(message) ? IdleMessage() : message;
		clearButton.SetActive(captureSlot >= 0);
		cancelButton.SetActive(captureSlot >= 0);
		confirmButton.SetActive(duplicateSlot >= 0);
		confirmLabel.text = "Reassign";
		copyButton.SetActive(captureSlot < 0 && !custom);
		copyLabel.text = "Copy to Custom";
		RefreshRow(row);
	}

	private static void SetChecked(UISprite check, bool value)
	{
		if (check != null)
		{
			check.alpha = value ? 1f : 0f;
		}
	}

	internal static bool HandleBack(GameOptionWindow owner)
	{
		// Both the global back dispatcher and Update can observe the same Escape.
		if (lastBackWindow == owner && lastBackFrame == Time.frameCount) return true;
		if (Current == null || Current.window != owner) return false;
		lastBackWindow = owner;
		lastBackFrame = Time.frameCount;
		if (Current.captureSlot >= 0) Current.EndCapture("Capture cancelled. No bindings changed.");
		else Current.Close();
		return true;
	}

	private void Update()
	{
		if (window == null || !window.gameObject.activeInHierarchy || !window.IsOpen())
		{
			Close();
			return;
		}
		if (screenSize.x != Screen.width || screenSize.y != Screen.height)
		{
			Resize();
		}

		if (captureSlot < 0 || Time.frameCount <= captureFrameCount || !Application.isFocused)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
		{
			SprkBattleKeyboard.AssignCustom(captureSlot, default(SprkBattleKeyboard.Binding));
			EndCapture(SlotLabel(captureSlot) + " is now unbound.");
			return;
		}
		foreach (KeyCode key in SprkBattleKeyboard.CaptureKeys)
		{
			if (!Input.GetKeyDown(key)) continue;
			pending = new SprkBattleKeyboard.Binding(key, SprkBattleKeyboard.HeldModifiers());
			duplicateSlot = SprkBattleKeyboard.FindDuplicate(captureSlot, pending);
			if (duplicateSlot >= 0)
			{
				message = pending.Label + " is already used by " + SlotLabel(duplicateSlot) + ".\n"
					+ "Reassign moves it here and unbinds " + SlotLabel(duplicateSlot) + ", or press another key.";
				Sync();
			}
			else
			{
				SprkBattleKeyboard.AssignCustom(captureSlot, pending);
				EndCapture(SlotLabel(captureSlot) + " set to " + pending.Label + ".");
			}
			break;
		}
	}

	private void Resize()
	{
		screenSize = new Vector2(Screen.width, Screen.height);
		float height = uiRoot.activeHeight;
		float width = height * Screen.width / Mathf.Max(1f, Screen.height);
		content.localScale = Vector3.one * Mathf.Min(height / 1080f, width / (FrameWidth + 80f));
		backdrop.width = Mathf.CeilToInt(width) + 4;
		backdrop.height = Mathf.CeilToInt(height) + 4;
		NGUITools.AddWidgetCollider(backdrop.gameObject);
	}

	private void Close()
	{
		SprkBattleKeyboard.SuppressClosingFrame();
		RefreshRow(row);
		gameObject.SetActive(false);
		UnityEngine.Object.Destroy(gameObject);
	}

	private void OnDisable()
	{
		if (Current == this)
		{
			Current = null;
			SprkBattleKeyboard.SuppressClosingFrame();
		}
	}

	private static string PathOf(Transform t)
	{
		string path = t.name;
		for (t = t.parent; t != null; t = t.parent)
		{
			path = t.name + "/" + path;
		}
		return path;
	}

	private GameObject Child(Transform parent, string name)
	{
		GameObject go = new GameObject(name);
		go.layer = gameObject.layer;
		go.transform.SetParent(parent, false);
		return go;
	}

	private T Widget<T>(Transform parent, string name, int width, int height, Vector2 position, int depth) where T : UIWidget
	{
		GameObject go = Child(parent, name);
		go.transform.localPosition = position;
		T widget = go.AddComponent<T>();
		widget.width = width;
		widget.height = height;
		widget.depth = depth;
		return widget;
	}

	private UISprite Sprite(Transform parent, string name, string spriteName, int width, int height, Vector2 position, int depth, bool sliced = true)
	{
		UISprite sprite = Widget<UISprite>(parent, name, width, height, position, depth);
		sprite.atlas = atlases.FirstOrDefault(a => a.GetSprite(spriteName) != null);
		sprite.spriteName = spriteName;
		sprite.type = sliced ? UIBasicSprite.Type.Sliced : UIBasicSprite.Type.Simple;
		sprite.width = width;
		sprite.height = height;
		return sprite;
	}

	private static void Style(UILabel label, UILabel source)
	{
		if (source == null)
		{
			label.color = Color.white;
			label.applyGradient = false;
			label.effectStyle = UILabel.Effect.Outline8;
			label.effectColor = Color.black;
			return;
		}
		label.color = source.color;
		label.applyGradient = source.applyGradient;
		label.gradientTop = source.gradientTop;
		label.gradientBottom = source.gradientBottom;
		label.effectStyle = source.effectStyle;
		label.effectColor = source.effectColor;
		label.effectDistance = source.effectDistance;
		label.spacingX = source.spacingX;
		label.fontStyle = source.fontStyle;
	}

	private UILabel Label(Transform parent, string text, int size, Vector2 position, int depth, UIWidget.Pivot pivot, UILabel style, int width)
	{
		UILabel label = Widget<UILabel>(parent, "Label", width, size + 16, position, depth);
		UILabel font = style ?? nameStyle ?? toggleStyle ?? titleStyle ?? buttonStyle;
		if (font != null)
		{
			label.bitmapFont = font.bitmapFont;
			if (font.bitmapFont == null) label.trueTypeFont = font.trueTypeFont;
		}
		label.fontSize = size;
		label.pivot = pivot;
		label.alignment = pivot == UIWidget.Pivot.Left ? NGUIText.Alignment.Left
			: pivot == UIWidget.Pivot.Right ? NGUIText.Alignment.Right : NGUIText.Alignment.Center;
		label.overflowMethod = UILabel.Overflow.ShrinkContent;
		label.transform.localPosition = position;
		Style(label, style);
		label.text = text;
		return label;
	}

	private void Clickable(GameObject go, Action action, int padding = 0)
	{
		NGUITools.AddWidgetCollider(go);
		BoxCollider collider = go.GetComponent<BoxCollider>();
		if (padding > 0 && collider != null)
		{
			collider.size += new Vector3(padding * 2, padding * 2, 0f);
		}
		UIButtonScale scale = go.AddComponent<UIButtonScale>();
		scale.tweenTarget = go.transform;
		scale.hover = Vector3.one;
		scale.pressed = new Vector3(0.95f, 0.95f, 1f);
		scale.duration = 0.08f;
		UIEventListener.Get(go).onClick = delegate { action(); };
	}

	private GameObject Button(string text, string spriteName, Vector2 position, int width, Action action, out UILabel label)
	{
		UISprite sprite = Sprite(content, text, spriteName, width, 80, position, 7);
		label = Label(sprite.transform, text, 30, Vector2.zero, 8, UIWidget.Pivot.Center, buttonStyle, width - 30);
		Clickable(sprite.gameObject, action);
		return sprite.gameObject;
	}
}
