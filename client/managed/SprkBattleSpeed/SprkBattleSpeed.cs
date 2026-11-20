using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NNguiExtension;
using NVespa;
using UnityEngine;

/// <summary>
/// Selectable battle speed for the native x1.5 buttons (SystemButtonAuto and
/// SystemButtonSpeed). The native buttons keep their Normal/Fast state, which
/// still drives the glow effect and BattleManagement's fast-speed gate; this
/// class remembers which multiplier "Fast" means and shows it with the speed
/// sprites of the battle UI atlas, as the native AUTO and x1.5 buttons do.
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

	/// <summary>
	/// Sprite of the battle UI atlas (NguiAtlas_UI_Prefab_Battle4) showing a speed:
	/// btn_sprk_speed_{off|x0.25|...|x2}_{on|off}, baked once from the native x1.5 art
	/// (see client/overrides/README.md).
	/// <paramref name="percent"/> 0 is the "Off" label.
	/// </summary>
	public static string SpriteName(int percent, bool on)
	{
		return "btn_sprk_speed_" + (percent == 0 ? "off" : Multiplier(percent)) + (on ? "_on" : "_off");
	}

	/// <summary>What the main button shows: the selected speed while Fast is on, otherwise "Off".</summary>
	public static string ButtonSprite(bool on)
	{
		int percent = Percent;
		return SpriteName(on && percent != NormalPercent ? percent : 0, on);
	}

	/// <summary>Shows the selected speed on a native speed button. Safe to call on every Open.</summary>
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
/// Shows the selected speed on the native x1.5 button and owns the drop-down list
/// of speeds. The native state machine keeps switching the button between its
/// btn_x1.5_on/off sprites; each frame this swaps in the matching speed sprite.
/// An atlas without speed sprites (older assets) keeps the native x1.5 art.
/// </summary>
public sealed class SprkBattleSpeedButton : MonoBehaviour
{
	private const float ItemScale = 0.8f;

	private const int ItemGap = 4;

	private UISprite target;

	private UISpriteSelector selector;

	private GameObject menu;

	private readonly List<KeyValuePair<int, UISprite>> items = new List<KeyValuePair<int, UISprite>>();

	private Action<int> onSelect;

	public bool MenuOpen => menu != null;

	public void Initialize(UISprite sprite, UISpriteSelector spriteSelector)
	{
		target = sprite;
		selector = spriteSelector;
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
		return on ? "btn_x1.5_on" : "btn_x1.5_off";
	}

	private bool IsOnSprite(string spriteName)
	{
		return spriteName != null && (spriteName == StateSprite(on: true) || spriteName.EndsWith("_on", StringComparison.Ordinal));
	}

	private static bool Show(UISprite sprite, string name)
	{
		if (sprite == null || sprite.atlas == null || sprite.atlas.GetSprite(name) == null)
		{
			return false;
		}
		if (sprite.spriteName != name)
		{
			sprite.spriteName = name;
		}
		return true;
	}

	private void LateUpdate()
	{
		if (target == null)
		{
			return;
		}
		Show(target, SprkBattleSpeed.ButtonSprite(IsOnSprite(target.spriteName)));
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
			UISprite sprite = item.AddComponent<UISprite>();
			sprite.atlas = atlas;
			sprite.type = UIBasicSprite.Type.Simple;
			sprite.pivot = UIWidget.Pivot.Center;
			sprite.width = itemWidth;
			sprite.height = itemHeight;
			sprite.depth = 10 + i;
			NGUITools.AddWidgetCollider(item);
			UIButton button = item.AddComponent<UIButton>();
			button.tweenTarget = item;
			button.hover = new Color(0.9f, 0.9f, 0.9f, 1f);
			button.pressed = new Color(0.7f, 0.7f, 0.7f, 1f);
			EventDelegate.Add(button.onClick, delegate
			{
				Select(percent);
			});
			items.Add(new KeyValuePair<int, UISprite>(percent, sprite));
		}
		PaintMenu();
	}

	private void PaintMenu()
	{
		int selected = SprkBattleSpeed.Percent;
		foreach (KeyValuePair<int, UISprite> item in items)
		{
			bool active = item.Key == selected;
			// Without speed sprites the list still works, as plain on/off buttons.
			if (!Show(item.Value, SprkBattleSpeed.SpriteName(item.Key, active)))
			{
				Show(item.Value, StateSprite(active));
			}
		}
	}

	private void Select(int percent)
	{
		Action<int> select = onSelect;
		CloseMenu();
		select?.Invoke(percent);
	}
}
