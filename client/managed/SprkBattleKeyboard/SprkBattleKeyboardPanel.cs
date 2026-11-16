using System;
using System.Collections.Generic;
using System.Linq;
using NGame2;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;

using UnityEngine;

public sealed class SprkBattleKeyboardPanel : MonoBehaviour
{
	internal static SprkBattleKeyboardPanel Current;
	private static GameOptionWindow lastBackWindow;
	private static int lastBackFrame = -1;
	private const string RowName = "SPRK Battle Keyboard";
	private GameOptionWindow window;
	private GameOptionButtonComponent row;
	private UIRoot uiRoot;
	private Transform content;
	private UILabel fontStyle;
	private readonly List<UIAtlas> atlases = new List<UIAtlas>();
	private readonly UILabel[] bindingLabels = new UILabel[16];
	private readonly UISprite[] bindingButtons = new UISprite[16];
	private readonly UISprite[] presetButtons = new UISprite[4];
	private UILabel enabledLabel;
	private UILabel status;
	private UILabel confirmLabel;
	private GameObject clearButton;
	private GameObject cancelButton;
	private GameObject confirmButton;
	private UITexture backdrop;
	private int captureSlot = -1;
	private int captureFrame;
	private int duplicateSlot = -1;
	private SprkBattleKeyboard.Binding pending;
	private string message = "Select Custom to edit individual bindings. Custom starts with Preset A.";
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
					row.ParentRoot.Label_OptionName.text = "Hero Skill Keyboard Controls";
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
			row.Label_Button.text = SprkBattleKeyboard.ControlsEnabled ? "On - Configure" : "Off - Configure";
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

	private void Build()
	{
		fontStyle = row.Label_Button;
		if (fontStyle == null || (fontStyle.bitmapFont == null && fontStyle.trueTypeFont == null))
		{
			fontStyle = window.GetComponentsInChildren<UILabel>(true)
				.FirstOrDefault(l => l.bitmapFont != null || l.trueTypeFont != null);
		}
		foreach (UISprite sprite in window.GetComponentsInChildren<UISprite>(true))
		{
			if (sprite.atlas != null && !atlases.Contains(sprite.atlas))
			{
				atlases.Add(sprite.atlas);
			}
		}
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
		backdrop = Widget<UITexture>(transform, "Modal Blocker", 1920, 1080, Vector2.zero, 0);
		backdrop.mainTexture = Texture2D.whiteTexture;
		backdrop.color = new Color(0f, 0f, 0f, 0.85f);
		NGUITools.AddWidgetCollider(backdrop.gameObject);
		UIEventListener.Get(backdrop.gameObject).onClick = delegate { };
		content = Child(transform, "Content").transform;
		UISprite frame = Sprite(content, "Frame", "Common_Box_Text_2", 1120, 820, Vector2.zero, 1);
		NGUITools.AddWidgetCollider(frame.gameObject);
		UIEventListener.Get(frame.gameObject).onClick = delegate { };
		Sprite(content, "Title Bar", "PayShop_Title_Name", 1020, 75, new Vector2(0, 350), 2);
		Label(content, "Hero Skill Keyboard Controls", 34, 980, 60, new Vector2(0, 350));
		enabledLabel = Button("Enable", "", new Vector2(0, 270), 480, delegate
		{
			if (captureSlot >= 0) return;
			SprkBattleKeyboard.ControlsEnabled = !SprkBattleKeyboard.ControlsEnabled;
			Sync();
		}, out _);
		for (int p = 0; p < 4; p++)
		{
			int selected = p;
			Button("Layout " + p, SprkBattleKeyboard.PresetNames[p], new Vector2(-345 + p * 230, 195), 210, delegate
			{
				if (captureSlot >= 0) return;
				SprkBattleKeyboard.SelectedPreset = selected;
				message = selected == 3
					? "Click a binding, then press a key with optional Ctrl / Alt / Shift / Meta."
					: "Preset layout (read-only). Select Custom to edit your saved custom bindings.";
				Sync();
			}, out presetButtons[p]);
		}
		Label(content, "Hero", 24, 130, 40, new Vector2(-450, 125));
		string[] actions = { "Skill 1", "Skill 2", "Skill 3", "Soul Weapon" };
		for (int a = 0; a < 4; a++)
		{
			Label(content, actions[a], 24, 200, 40, new Vector2(-275 + a * 210, 125));
		}
		for (int h = 0; h < 4; h++)
		{
			Label(content, "Hero " + (h + 1), 26, 130, 55, new Vector2(-450, 65 - h * 75));
			for (int a = 0; a < 4; a++)
			{
				int slot = h * 4 + a;
				bindingLabels[slot] = Button("Binding " + slot, "", new Vector2(-275 + a * 210, 65 - h * 75), 195,
					delegate { BeginCapture(slot); }, out bindingButtons[slot]);
			}
		}
		status = Label(content, "", 23, 1030, 100, new Vector2(0, -235));
		Button("Clear", "Clear / Unbind", new Vector2(-370, -325), 225, delegate
		{
			if (captureSlot < 0) return;
			SprkBattleKeyboard.AssignCustom(captureSlot, default(SprkBattleKeyboard.Binding));
			EndCapture("Binding cleared. This action no longer has a keyboard shortcut.");
		}, out UISprite clear);
		clearButton = clear.gameObject;
		Button("Cancel Capture", "Cancel Capture", new Vector2(-115, -325), 245,
			delegate { EndCapture("Capture cancelled. No bindings changed."); }, out UISprite cancel);
		cancelButton = cancel.gameObject;
		confirmLabel = Button("Reassign", "Reassign", new Vector2(145, -325), 235, delegate
		{
			if (captureSlot < 0 || duplicateSlot < 0) return;
			string previous = SlotLabel(duplicateSlot);
			SprkBattleKeyboard.AssignCustom(captureSlot, pending);
			EndCapture("Reassigned " + pending.Label + ". " + previous + " is now unbound.");
		}, out UISprite confirm);
		confirmButton = confirm.gameObject;
		Button("Done", "Done", new Vector2(400, -325), 200, Close, out _);
		Label(content, "Changes save immediately. Exact modifiers required. Disabled while typing or settings are open.",
			20, 1050, 45, new Vector2(0, -385));
		Resize();
		Sync();
	}

	private void BeginCapture(int slot)
	{
		if (SprkBattleKeyboard.SelectedPreset != 3)
		{
			message = "Select Custom first. Switching presets never overwrites your custom bindings.";
			Sync();
			return;
		}
		captureSlot = slot;
		captureFrame = Time.frameCount;
		duplicateSlot = -1;
		message = "Capturing " + SlotLabel(slot) + ": press a key (optional Ctrl / Alt / Shift / Meta).\n"
			+ "Escape cancels. Backspace / Delete or Clear unbinds. Modifier-only and mouse bindings are not supported.";
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
		return "Hero " + (slot / 4 + 1) + " / " + (slot % 4 == 3 ? "Soul Weapon" : "Skill " + (slot % 4 + 1));
	}

	private void Sync()
	{
		enabledLabel.text = SprkBattleKeyboard.ControlsEnabled ? "Keyboard Controls: ON" : "Keyboard Controls: OFF (default)";
		for (int p = 0; p < presetButtons.Length; p++)
		{
			presetButtons[p].color = p == SprkBattleKeyboard.SelectedPreset ? new Color(1f, 0.85f, 0.5f) : Color.white;
		}
		for (int i = 0; i < bindingLabels.Length; i++)
		{
			bindingLabels[i].text = i == captureSlot ? "Press key..." : SprkBattleKeyboard.GetBinding(i).Label;
			bindingButtons[i].color = i == captureSlot ? new Color(1f, 0.85f, 0.5f) : Color.white;
		}
		status.text = message;
		clearButton.SetActive(captureSlot >= 0);
		cancelButton.SetActive(captureSlot >= 0);
		confirmButton.SetActive(duplicateSlot >= 0);
		confirmLabel.text = "Reassign";
		RefreshRow(row);
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

		if (captureSlot < 0 || Time.frameCount <= captureFrame || !Application.isFocused)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
		{
			SprkBattleKeyboard.AssignCustom(captureSlot, default(SprkBattleKeyboard.Binding));
			EndCapture("Binding cleared. This action no longer has a keyboard shortcut.");
			return;
		}
		foreach (KeyCode key in SprkBattleKeyboard.CaptureKeys)
		{
			if (!Input.GetKeyDown(key)) continue;
			pending = new SprkBattleKeyboard.Binding(key, SprkBattleKeyboard.HeldModifiers());
			duplicateSlot = SprkBattleKeyboard.FindDuplicate(captureSlot, pending);
			if (duplicateSlot >= 0)
			{
				message = pending.Label + " is already assigned to " + SlotLabel(duplicateSlot) + ".\n"
					+ "Reassign will unbind that slot. Click Reassign, press another key, or cancel.";
				Sync();
			}
			else
			{
				SprkBattleKeyboard.AssignCustom(captureSlot, pending);
				EndCapture("Saved " + pending.Label + " for " + SlotLabel(captureSlot) + ".");
			}
			break;
		}
	}

	private void Resize()
	{
		screenSize = new Vector2(Screen.width, Screen.height);
		float height = uiRoot.activeHeight;
		float width = height * Screen.width / Mathf.Max(1f, Screen.height);
		content.localScale = Vector3.one * Mathf.Min(height / 1080f, width / 1180f);
		backdrop.width = Mathf.CeilToInt(width);
		backdrop.height = Mathf.CeilToInt(height);
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

	private UISprite Sprite(Transform parent, string name, string spriteName, int width, int height, Vector2 position, int depth)
	{
		UISprite sprite = Widget<UISprite>(parent, name, width, height, position, depth);
		sprite.atlas = atlases.FirstOrDefault(a => a.GetSprite(spriteName) != null);
		sprite.spriteName = spriteName;
		sprite.type = UIBasicSprite.Type.Sliced;
		sprite.width = width;
		sprite.height = height;
		return sprite;
	}

	private UILabel Label(Transform parent, string text, int size, int width, int height, Vector2 position)
	{
		UILabel label = Widget<UILabel>(parent, "Label", width, height, position, 5);
		if (fontStyle != null)
		{
			label.bitmapFont = fontStyle.bitmapFont;
			if (fontStyle.bitmapFont == null) label.trueTypeFont = fontStyle.trueTypeFont;
			label.color = fontStyle.color;
			label.applyGradient = fontStyle.applyGradient;
			label.gradientTop = fontStyle.gradientTop;
			label.gradientBottom = fontStyle.gradientBottom;
			label.effectStyle = fontStyle.effectStyle;
			label.effectColor = fontStyle.effectColor;
			label.effectDistance = fontStyle.effectDistance;
			label.fontStyle = fontStyle.fontStyle;
		}
		label.fontSize = size;
		label.alignment = NGUIText.Alignment.Center;
		label.overflowMethod = UILabel.Overflow.ShrinkContent;
		label.text = text;
		return label;
	}

	private UILabel Button(string name, string text, Vector2 position, int width, Action action, out UISprite sprite)
	{
		sprite = Sprite(content, name, "Common_Button6_Normal", width, 55, position, 3);
		NGUITools.AddWidgetCollider(sprite.gameObject);
		UIEventListener.Get(sprite.gameObject).onClick = delegate { action(); };
		return Label(sprite.transform, text, 25, width - 16, 48, Vector2.zero);
	}
}
