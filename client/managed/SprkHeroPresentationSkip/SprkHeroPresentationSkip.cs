using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NVespa;
using UnityEngine;

/// <summary>
/// Skips the slow parts of hero purification and awakening/transcendence:
/// a Skip button (and the back key) fast-forwards the result cinematic, and a
/// per-account "Skip Animations" toggle in the purify room makes purifying
/// instant and fast-forwards the awakening cinematic automatically.
/// Fast-forwarding plays the cinematic to its real end, so the result screen
/// shows the same final pose and camera as when it is watched.
/// </summary>
public static class SprkHeroPresentationSkip
{
	public const float FastForwardSpeed = 20f;

	// Real seconds. A cinematic at 20x ends far sooner; this only guards
	// against the game staying fast-forwarded if the window never reports
	// the end.
	private const float MaxFastForwardSeconds = 8f;

	private const string SkipButtonName = "SprkSkipButton";

	private const string ToggleName = "SprkSkipAnimationsToggle";

	private static readonly LocalAccountBool skipAnimations = new LocalAccountBool(i_default: false, "SprkHeroPresentationSkip.SkipAnimations");

	private static GetNewHero2 cinemaWindow;

	private static GetNewHero2 fastForwardOwner;

	private static float fastForwardStarted;

	private static SprkHeroPresentationSkipWatchdog watchdog;

	public static bool SkipAnimations
	{
		get
		{
			return skipAnimations.Value;
		}
		set
		{
			skipAnimations.Value = value;
		}
	}

	public static bool IsFastForwarding => fastForwardOwner != null;

	// ---------------------------------------------------------------- cinematic

	/// <summary>The result cinematic starts loading or playing.</summary>
	public static void OnCinemaStart(GetNewHero2 window, GetNewHero2.EOpenMode mode)
	{
		if (window == null)
		{
			return;
		}
		cinemaWindow = window;
		bool awakening = mode == GetNewHero2.EOpenMode.Awaken || mode == GetNewHero2.EOpenMode.Transcend;
		// Awakening/transcending shows no Skip button; it is skipped by the
		// purify room's Skip Animations setting (or the back key) instead.
		ShowSkipButton(window, !awakening);
		if (awakening && SkipAnimations)
		{
			Skip(window);
		}
	}

	/// <summary>The cinematic ended (or the window closed): restore normal speed.</summary>
	public static void OnCinemaEnd(GetNewHero2 window)
	{
		if (window == null)
		{
			return;
		}
		ShowSkipButton(window, show: false);
		if ((object)cinemaWindow == window)
		{
			cinemaWindow = null;
		}
		StopFastForward(window);
	}

	/// <summary>Back/Escape while the cinematic plays acts as Skip.</summary>
	public static void OnBack(GetNewHero2 window)
	{
		if (window != null && (object)cinemaWindow == window && !IsFastForwarding)
		{
			Skip(window);
		}
	}

	private static void OnClickSkip()
	{
		if (cinemaWindow != null)
		{
			Skip(cinemaWindow);
		}
	}

	private static void Skip(GetNewHero2 window)
	{
		ShowSkipButton(window, show: false);
		StartFastForward(window);
	}

	private static void StartFastForward(GetNewHero2 owner)
	{
		fastForwardOwner = owner;
		fastForwardStarted = Time.realtimeSinceStartup;
		EnsureWatchdog();
		UnityTimeScaler.SprkSetFastForward(FastForwardSpeed);
	}

	private static void StopFastForward(GetNewHero2 owner)
	{
		if ((object)fastForwardOwner == owner)
		{
			StopFastForward();
		}
	}

	private static void StopFastForward()
	{
		fastForwardOwner = null;
		UnityTimeScaler.SprkSetFastForward(1f);
	}

	private static void EnsureWatchdog()
	{
		if (watchdog == null)
		{
			GameObject go = new GameObject("SprkHeroPresentationSkip");
			Object.DontDestroyOnLoad(go);
			watchdog = go.AddComponent<SprkHeroPresentationSkipWatchdog>();
		}
	}

	/// <summary>Called every frame by the watchdog while fast-forwarding.</summary>
	internal static void CheckFastForward()
	{
		if (UnityTimeScaler.SprkFastForward <= 1f)
		{
			return;
		}
		GetNewHero2 owner = fastForwardOwner;
		if (owner == null || !owner.IsOpen() || !owner.gameObject.activeInHierarchy || Time.realtimeSinceStartup - fastForwardStarted > MaxFastForwardSeconds)
		{
			StopFastForward();
		}
	}

	/// <summary>
	/// The Skip button is a copy of the window's own Close button, at the
	/// bottom-right spot where Close usually appears once the cinematic ends.
	/// </summary>
	private static void ShowSkipButton(GetNewHero2 window, bool show)
	{
		GameObject button = FindOrCreateSkipButton(window, create: show);
		if (button == null)
		{
			return;
		}
		if (show)
		{
			Transform rightPos = window.ExitWidgetRightPos;
			if (rightPos != null)
			{
				Vector3 position = button.transform.localPosition;
				position.x = rightPos.localPosition.x;
				button.transform.localPosition = position;
			}
			SetLabels(button, SprkText.Get("Skip"));
		}
		if (button.activeSelf != show)
		{
			button.SetActive(show);
		}
	}

	private static GameObject FindOrCreateSkipButton(GetNewHero2 window, bool create)
	{
		UIWidget exit = window.ExitWidget;
		if (exit == null || exit.transform.parent == null)
		{
			return null;
		}
		Transform parent = exit.transform.parent;
		Transform existing = parent.Find(SkipButtonName);
		if (existing != null)
		{
			return existing.gameObject;
		}
		if (!create)
		{
			return null;
		}
		GameObject copy = Object.Instantiate(exit.gameObject, parent, worldPositionStays: false);
		copy.name = SkipButtonName;
		copy.SetActive(false);
		RemoveNativeHandlers(copy);
		foreach (UIButton button in copy.GetComponentsInChildren<UIButton>(includeInactive: true))
		{
			button.onClick = new List<EventDelegate>
			{
				new EventDelegate(OnClickSkip)
			};
			button.isEnabled = true;
		}
		return copy;
	}

	// ---------------------------------------------------------------- purify room

	/// <summary>Adds the Skip Animations toggle to the purify room's top-right corner.</summary>
	public static void AttachPurifyRoom(PurifyHeroRoom room)
	{
		if (room == null || room.TopTween == null)
		{
			return;
		}
		Transform parent = room.TopTween.transform;
		Transform existing = parent.Find(ToggleName);
		GameObject toggle = existing != null ? existing.gameObject : CreateToggle(room, parent);
		if (toggle != null)
		{
			RefreshToggle(toggle);
		}
	}

	private static GameObject CreateToggle(PurifyHeroRoom room, Transform parent)
	{
		UIButton source = room.Button_MaxPurify != null ? room.Button_MaxPurify : room.Button_Purify;
		if (source == null)
		{
			return null;
		}
		UISprite sourceSprite = source.GetComponent<UISprite>();
		if (sourceSprite == null && source.tweenTarget != null)
		{
			sourceSprite = source.tweenTarget.GetComponent<UISprite>();
		}
		UILabel sourceLabel = source.GetComponentInChildren<UILabel>(includeInactive: true);
		UIPanel panel = parent.GetComponentInParent<UIPanel>();
		if (sourceSprite == null || sourceLabel == null || panel == null)
		{
			return null;
		}
		const int width = 420;
		const int height = 84;
		const int margin = 36;
		GameObject go = new GameObject(ToggleName);
		go.layer = parent.gameObject.layer;
		go.transform.SetParent(parent, worldPositionStays: false);
		UISprite sprite = go.AddComponent<UISprite>();
		sprite.atlas = sourceSprite.atlas;
		sprite.spriteName = sourceSprite.spriteName;
		sprite.type = sourceSprite.type;
		sprite.pivot = UIWidget.Pivot.Center;
		sprite.depth = sourceSprite.depth;
		sprite.width = width;
		sprite.height = height;
		// Anchored to the screen's top-right corner, mirroring the Close button.
		sprite.SetAnchor(panel.gameObject);
		sprite.leftAnchor.Set(1f, -margin - width);
		sprite.rightAnchor.Set(1f, -margin);
		sprite.topAnchor.Set(1f, -margin);
		sprite.bottomAnchor.Set(1f, -margin - height);
		sprite.UpdateAnchors();
		UIButton button = go.AddComponent<UIButton>();
		button.tweenTarget = go;
		button.defaultColor = source.defaultColor;
		button.hover = source.hover;
		button.pressed = source.pressed;
		button.disabledColor = source.disabledColor;
		button.normalSprite = sourceSprite.spriteName;
		EventDelegate.Add(button.onClick, OnClickToggle);
		GameObject labelGo = new GameObject("Label");
		labelGo.layer = go.layer;
		labelGo.transform.SetParent(go.transform, worldPositionStays: false);
		UILabel label = labelGo.AddComponent<UILabel>();
		label.bitmapFont = sourceLabel.bitmapFont;
		label.trueTypeFont = sourceLabel.trueTypeFont;
		label.fontSize = sourceLabel.fontSize;
		label.fontStyle = sourceLabel.fontStyle;
		label.effectStyle = sourceLabel.effectStyle;
		label.effectColor = sourceLabel.effectColor;
		label.color = sourceLabel.color;
		label.depth = sprite.depth + 1;
		label.alignment = NGUIText.Alignment.Center;
		label.overflowMethod = UILabel.Overflow.ShrinkContent;
		label.maxLineCount = 1;
		label.SetAnchor(go);
		label.leftAnchor.Set(0f, 16f);
		label.rightAnchor.Set(1f, -16f);
		label.bottomAnchor.Set(0f, 6f);
		label.topAnchor.Set(1f, -6f);
		label.UpdateAnchors();
		NGUITools.AddWidgetCollider(go);
		sprite.autoResizeBoxCollider = true;
		return go;
	}

	private static void OnClickToggle()
	{
		SkipAnimations = !SkipAnimations;
		UIButton button = UIButton.current;
		if (button != null)
		{
			RefreshToggle(button.gameObject);
		}
	}

	private static void RefreshToggle(GameObject toggle)
	{
		bool on = SkipAnimations;
		SetLabels(toggle, on ? SprkText.Get("Skip Animations: On") : SprkText.Get("Skip Animations: Off"));
		// Dim the button while the setting is off so its state reads at a glance.
		UISprite sprite = toggle.GetComponent<UISprite>();
		if (sprite != null)
		{
			sprite.color = on ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
		}
		UIButton button = toggle.GetComponent<UIButton>();
		if (button != null)
		{
			button.defaultColor = sprite != null ? sprite.color : button.defaultColor;
			button.UpdateColor(instant: true);
		}
	}

	// ---------------------------------------------------------------- helpers

	private static void SetLabels(GameObject root, string text)
	{
		foreach (UILabel label in root.GetComponentsInChildren<UILabel>(includeInactive: true))
		{
			label.text = text;
		}
	}

	/// <summary>
	/// A copied native button keeps components that would call the original
	/// handler or rewrite its caption on enable; remove them.
	/// </summary>
	private static void RemoveNativeHandlers(GameObject root)
	{
		foreach (UIButtonMessage component in root.GetComponentsInChildren<UIButtonMessage>(includeInactive: true))
		{
			Object.DestroyImmediate(component);
		}
		foreach (UIEventTrigger component in root.GetComponentsInChildren<UIEventTrigger>(includeInactive: true))
		{
			Object.DestroyImmediate(component);
		}
		foreach (UIForwardEvents component in root.GetComponentsInChildren<UIForwardEvents>(includeInactive: true))
		{
			Object.DestroyImmediate(component);
		}
		foreach (UIEventListener component in root.GetComponentsInChildren<UIEventListener>(includeInactive: true))
		{
			Object.DestroyImmediate(component);
		}
		foreach (UILocalize component in root.GetComponentsInChildren<UILocalize>(includeInactive: true))
		{
			Object.DestroyImmediate(component);
		}
		foreach (NVespa.NNGUIExtension.UILocalizeEx component in root.GetComponentsInChildren<NVespa.NNGUIExtension.UILocalizeEx>(includeInactive: true))
		{
			Object.DestroyImmediate(component);
		}
	}
}
