using System;
using System.Globalization;
using NGame2.NUI.NWindow;
using UnityEngine;

public static class SprkBattleKeyboard
{
	internal struct Binding
	{
		internal KeyCode Key;
		internal int Modifiers;

		internal Binding(KeyCode key, int modifiers = 0)
		{
			Key = key;
			Modifiers = modifiers;
		}

		internal bool SameAs(Binding other)
		{
			return Key != KeyCode.None && Key == other.Key && Modifiers == other.Modifiers;
		}

		internal string Label
		{
			get
			{
				if (Key == KeyCode.None)
				{
					return SprkText.Get("Unbound");
				}
				return ((Modifiers & 1) != 0 ? "Ctrl+" : "")
					+ ((Modifiers & 2) != 0 ? "Alt+" : "")
					+ ((Modifiers & 4) != 0 ? "Shift+" : "")
					+ ((Modifiers & 8) != 0 ? "Meta+" : "") + KeyName(Key);
			}
		}

		// Compact form for the in-battle key badges: modifiers as single letters (Shift+1 -> S1).
		internal string ShortLabel
		{
			get
			{
				if (Key == KeyCode.None)
				{
					return "";
				}
				return ((Modifiers & 1) != 0 ? "C" : "")
					+ ((Modifiers & 2) != 0 ? "A" : "")
					+ ((Modifiers & 4) != 0 ? "S" : "")
					+ ((Modifiers & 8) != 0 ? "M" : "") + KeyName(Key);
			}
		}
	}

	internal static string KeyName(KeyCode key)
	{
		if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
		{
			return ((int)key - (int)KeyCode.Alpha0).ToString(CultureInfo.InvariantCulture);
		}
		if (key >= KeyCode.Keypad0 && key <= KeyCode.Keypad9)
		{
			return "Num" + ((int)key - (int)KeyCode.Keypad0).ToString(CultureInfo.InvariantCulture);
		}
		switch (key)
		{
			case KeyCode.BackQuote: return "`";
			case KeyCode.Minus: return "-";
			case KeyCode.Equals: return "=";
			case KeyCode.LeftBracket: return "[";
			case KeyCode.RightBracket: return "]";
			case KeyCode.Backslash: return "\\";
			case KeyCode.Semicolon: return ";";
			case KeyCode.Quote: return "'";
			case KeyCode.Comma: return ",";
			case KeyCode.Period: return ".";
			case KeyCode.Slash: return "/";
			case KeyCode.Return: return "Enter";
			case KeyCode.KeypadEnter: return "NumEnter";
			case KeyCode.KeypadPlus: return "Num+";
			case KeyCode.KeypadMinus: return "Num-";
			case KeyCode.KeypadMultiply: return "Num*";
			case KeyCode.KeypadDivide: return "Num/";
			case KeyCode.KeypadPeriod: return "Num.";
			case KeyCode.UpArrow: return "Up";
			case KeyCode.DownArrow: return "Down";
			case KeyCode.LeftArrow: return "Left";
			case KeyCode.RightArrow: return "Right";
			case KeyCode.PageUp: return "PgUp";
			case KeyCode.PageDown: return "PgDn";
			case KeyCode.CapsLock: return "Caps";
			default: return key.ToString();
		}
	}

	private const string Prefix = "Sprk.BattleKeyboard.";
	internal static readonly string[] PresetNames = { "Preset A", "Preset B", "Preset C", "Custom" };
	private static readonly Binding[][] presets = CreatePresets();
	internal static readonly KeyCode[] CaptureKeys = CreateCaptureKeys();
	private static Binding[] custom;
	private static bool loaded;
	private static bool enabled;
	private static bool hints;
	private static int preset;
	private static int revision;
	private static GameOptionWindow settingsWindow;
	private static int suppressedThroughFrame = -1;

	public static bool ControlsEnabled
	{
		get { EnsureLoaded(); return enabled; }
		set
		{
			EnsureLoaded();
			enabled = value;
			revision++;
			SprkTestPrefs.SetInt(Prefix + "Enabled", value ? 1 : 0);
			SprkTestPrefs.Save();
		}
	}

	// Key badges drawn on the battle skill slots (default on; only shown while controls are on).
	public static bool KeyHintsEnabled
	{
		get { EnsureLoaded(); return hints; }
		set
		{
			EnsureLoaded();
			hints = value;
			revision++;
			SprkTestPrefs.SetInt(Prefix + "Hints", value ? 1 : 0);
			SprkTestPrefs.Save();
		}
	}

	// Bumped on every preference change so battle badges refresh immediately.
	internal static int Revision => revision;

	// Includes the entire game settings window, not just the keyboard editor.
	public static bool SettingsOpen => SprkBattleKeyboardPanel.Current != null
		|| (settingsWindow != null && settingsWindow.gameObject.activeInHierarchy && settingsWindow.IsOpen());

	public static bool IsPressed(int heroIndex, int actionIndex)
	{
		if (!ValidSlot(heroIndex, actionIndex) || !ControlsEnabled || SettingsOpen
			|| Time.frameCount <= suppressedThroughFrame || !Application.isFocused
			|| UICamera.inputHasFocus || UIInput.selection != null || TouchScreenKeyboard.visible)
		{
			return false;
		}
		Binding binding = GetBinding(heroIndex * 4 + actionIndex);
		return binding.Key != KeyCode.None && binding.Modifiers == HeldModifiers() && Input.GetKeyDown(binding.Key);
	}

	public static string GetBindingLabel(int heroIndex, int actionIndex)
	{
		return ValidSlot(heroIndex, actionIndex) ? GetBinding(heroIndex * 4 + actionIndex).Label : "";
	}

	public static void Setup(GameOptionWindow window)
	{
		settingsWindow = window;
		SprkBattleKeyboardPanel.SetupRow(window);
	}

	public static bool HandleSettingsBack(GameOptionWindow window)
	{
		return SprkBattleKeyboardPanel.HandleBack(window);
	}

	internal static int SelectedPreset
	{
		get { EnsureLoaded(); return preset; }
		set
		{
			EnsureLoaded();
			preset = Mathf.Clamp(value, 0, 3);
			revision++;
			SprkTestPrefs.SetInt(Prefix + "Preset", preset);
			SprkTestPrefs.Save();
		}
	}

	internal static Binding GetBinding(int slot)
	{
		EnsureLoaded();
		return preset == 3 ? custom[slot] : presets[preset][slot];
	}

	internal static int FindDuplicate(int slot, Binding binding)
	{
		EnsureLoaded();
		for (int i = 0; i < custom.Length; i++)
		{
			if (i != slot && custom[i].SameAs(binding))
			{
				return i;
			}
		}
		return -1;
	}

	internal static void AssignCustom(int slot, Binding binding)
	{
		EnsureLoaded();
		int duplicate = FindDuplicate(slot, binding);
		if (duplicate >= 0)
		{
			custom[duplicate] = default(Binding);
		}
		custom[slot] = binding;
		SaveCustom();
	}

	// Replaces the custom layout with a copy of preset A/B/C so it can be tweaked.
	internal static void CopyPresetToCustom(int source)
	{
		EnsureLoaded();
		if (source < 0 || source >= presets.Length)
		{
			return;
		}
		custom = (Binding[])presets[source].Clone();
		SaveCustom();
	}

	private static void SaveCustom()
	{
		revision++;
		string[] entries = new string[16];
		for (int i = 0; i < entries.Length; i++)
		{
			entries[i] = ((int)custom[i].Key).ToString(CultureInfo.InvariantCulture) + ":"
				+ custom[i].Modifiers.ToString(CultureInfo.InvariantCulture);
		}
		SprkTestPrefs.SetString(Prefix + "Custom.v1", string.Join(";", entries));
		SprkTestPrefs.Save();
	}

	internal static void SuppressClosingFrame()
	{
		// Do not let a closing/capture key leak into battle later in this frame.
		suppressedThroughFrame = Time.frameCount + 1;
	}

	internal static int HeldModifiers()
	{
		return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 1 : 0)
			| (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr) ? 2 : 0)
			| (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 4 : 0)
			| (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)
				|| Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows) ? 8 : 0);
	}

	private static bool ValidSlot(int heroIndex, int actionIndex)
	{
		return heroIndex >= 0 && heroIndex < 4 && actionIndex >= 0 && actionIndex < 4;
	}

	private static bool CanBind(KeyCode key)
	{
		return key > KeyCode.None && key < KeyCode.Mouse0 && key != KeyCode.Escape
			&& key != KeyCode.Backspace && key != KeyCode.Delete
			&& key != KeyCode.LeftControl && key != KeyCode.RightControl
			&& key != KeyCode.LeftAlt && key != KeyCode.RightAlt && key != KeyCode.AltGr
			&& key != KeyCode.LeftShift && key != KeyCode.RightShift
			&& key != KeyCode.LeftCommand && key != KeyCode.RightCommand
			&& key != KeyCode.LeftWindows && key != KeyCode.RightWindows;
	}

	private static KeyCode[] CreateCaptureKeys()
	{
		var keys = new System.Collections.Generic.List<KeyCode>();
		foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
		{
			if (CanBind(key) && !keys.Contains(key))
			{
				keys.Add(key);
			}
		}
		return keys.ToArray();
	}

	private static Binding[][] CreatePresets()
	{
		KeyCode[][] rows = {
			new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
				KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8,
				KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I },
			new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
				KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
				KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
				KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 },
			new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
				KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
				KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V }
		};
		var result = new Binding[3][];
		int[] modifiers = { 0, 2, 4, 1 };
		for (int p = 0; p < result.Length; p++)
		{
			result[p] = new Binding[16];
			for (int i = 0; i < 16; i++)
			{
				result[p][i] = new Binding(rows[p][i], p == 1 ? modifiers[i / 4] : 0);
			}
		}
		return result;
	}

	private static void EnsureLoaded()
	{
		if (loaded)
		{
			return;
		}
		enabled = SprkTestPrefs.GetInt(Prefix + "Enabled", 0) == 1;
		hints = SprkTestPrefs.GetInt(Prefix + "Hints", 1) == 1;
		revision++;
		preset = Mathf.Clamp(SprkTestPrefs.GetInt(Prefix + "Preset", 0), 0, 3);
		custom = (Binding[])presets[0].Clone();
		string saved = SprkTestPrefs.GetString(Prefix + "Custom.v1", "");
		string[] entries = saved.Split(';');
		if (entries.Length == 16)
		{
			for (int i = 0; i < entries.Length; i++)
			{
				string[] parts = entries[i].Split(':');
				custom[i] = default(Binding);
				if (parts.Length == 2 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int key)
					&& int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int modifiers)
					&& modifiers >= 0 && modifiers <= 15 && Array.IndexOf(CaptureKeys, (KeyCode)key) >= 0)
				{
					custom[i] = new Binding((KeyCode)key, modifiers);
					for (int earlier = 0; earlier < i; earlier++)
					{
						if (custom[earlier].SameAs(custom[i]))
						{
							custom[i] = default(Binding);
							break;
						}
					}
				}
			}
		}
		loaded = true;
	}
}
