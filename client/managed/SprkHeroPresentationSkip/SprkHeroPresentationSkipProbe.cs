using System;
using System.Collections.Generic;
using System.Reflection;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using UnityEngine;

/// <summary>
/// Login-screen fixture probe (automation action "hero-skip-probe"): renders the
/// purify room or the new-hero window from its prefab, applies this feature's
/// hooks and reports the added controls' screen geometry.
/// Modes: "purify-show", "cinema-show".
/// </summary>
public static class SprkHeroPresentationSkipProbe
{
	public static string Run(string mode)
	{
		switch (mode)
		{
		case "purify-show":
			return PurifyShow();
		case "purify-measure":
			return PurifyMeasure();
		case "cinema-show":
			return CinemaShow();
		case "watchdog-arm":
			return WatchdogArm();
		case "watchdog-check":
			Check(!SprkHeroPresentationSkip.IsFastForwarding && Mathf.Approximately(Time.timeScale, 1f), "watchdog left fast-forward on, timeScale=" + Time.timeScale);
			return "PASS: watchdog restored timeScale=" + Time.timeScale;
		default:
			throw new ArgumentException("unknown mode " + mode);
		}
	}

	private static T Show<T>(string name) where T : MonoBehaviour
	{
		// Reuse the shared prefab fixture loader (atlases, panel depths).
		string shown = (string)Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
			.Invoke(null, new object[1] { "show:" + name });
		Check(shown.StartsWith("shown "), shown);
		T found = null;
		foreach (T candidate in UnityEngine.Object.FindObjectsOfType<T>())
		{
			if (candidate.gameObject.activeInHierarchy && candidate.transform.root.GetComponentInChildren<UIRoot>() != null)
			{
				found = candidate;
			}
		}
		Check(found != null, "no active " + name);
		foreach (UIPanel panel in found.GetComponentsInChildren<UIPanel>(includeInactive: true))
		{
			panel.alpha = 1f;
		}
		return found;
	}

	private static string PurifyShow()
	{
		PurifyHeroRoom room = Show<PurifyHeroRoom>("PurifyHeroRoom");
		bool saved = SprkHeroPresentationSkip.SkipAnimations;
		List<string> lines = new List<string>();
		try
		{
			SprkHeroPresentationSkip.SkipAnimations = false;
			SprkHeroPresentationSkip.AttachPurifyRoom(room);
			Transform toggle = room.TopTween.transform.Find("SprkSkipAnimationsToggle");
			Check(toggle != null, "toggle not created");
			UILabel label = toggle.GetComponentInChildren<UILabel>();
			Check(label != null && label.text == "Skip Animations: Off", "off label: " + (label == null ? "none" : label.text));
			SprkHeroPresentationSkip.SkipAnimations = true;
			SprkHeroPresentationSkip.AttachPurifyRoom(room);
			Check(room.TopTween.transform.Find("SprkSkipAnimationsToggle") == toggle, "toggle duplicated");
			Check(label.text == "Skip Animations: On", "on label: " + label.text);
			Check(toggle.GetComponent<UIButton>().onClick.Count == 1, "toggle click handler");
			Check(toggle.GetComponent<Collider>() != null, "toggle collider");
		}
		finally
		{
			SprkHeroPresentationSkip.SkipAnimations = saved;
		}
		return "PASS: created; run purify-measure after a few frames";
	}

	// Anchors settle on the frames after the window is instantiated.
	private static string PurifyMeasure()
	{
		PurifyHeroRoom room = null;
		foreach (PurifyHeroRoom candidate in UnityEngine.Object.FindObjectsOfType<PurifyHeroRoom>())
		{
			if (candidate.gameObject.activeInHierarchy)
			{
				room = candidate;
			}
		}
		Check(room != null, "run purify-show first");
		List<string> lines = new List<string>();
		{
			Transform toggle = room.TopTween.transform.Find("SprkSkipAnimationsToggle");
			Check(toggle != null, "toggle missing");
			Rect rect = ScreenRect(toggle.GetComponent<UIWidget>());
			lines.Add("toggle=" + rect + " screen=" + Screen.width + "x" + Screen.height);
			Check(rect.xMin >= 0f && rect.yMin >= 0f && rect.xMax <= Screen.width && rect.yMax <= Screen.height, "toggle off screen " + rect);
			Check(rect.center.x > Screen.width * 0.5f && rect.center.y > Screen.height * 0.5f, "toggle not top-right " + rect);
			foreach (UIButton other in new UIButton[] { room.Button_Purify, room.Button_MaxPurify })
			{
				if (other != null)
				{
					Rect otherRect = ScreenRect(other.GetComponent<UIWidget>());
					lines.Add(other.name + "=" + otherRect);
					Check(!rect.Overlaps(otherRect), "toggle overlaps " + other.name);
				}
			}
			Transform close = room.TopTween.transform.Find("Button_Close");
			if (close != null && close.GetComponent<UIWidget>() != null)
			{
				Rect closeRect = ScreenRect(close.GetComponent<UIWidget>());
				lines.Add("close=" + closeRect);
				Check(!rect.Overlaps(closeRect), "toggle overlaps close");
			}
		}
		return "PASS: " + string.Join("; ", lines.ToArray());
	}

	private static string CinemaShow()
	{
		GetNewHero2 window = Show<GetNewHero2>("GetNewHero2");
		bool saved = SprkHeroPresentationSkip.SkipAnimations;
		try
		{
			SprkHeroPresentationSkip.SkipAnimations = false;
			// As during the cinematic: Close hidden, Skip shown.
			UIHelper.EnableGameObject(window.ExitWidget, enable: false);
			// Awakening/transcending: no Skip button.
			SprkHeroPresentationSkip.OnCinemaStart(window, GetNewHero2.EOpenMode.Awaken);
			Transform skip = window.ExitWidget.transform.parent.Find("SprkSkipButton");
			Check(skip == null || !skip.gameObject.activeSelf, "skip button shown while awakening");
			Check(!SprkHeroPresentationSkip.IsFastForwarding, "awaken fast-forward without request");
			SprkHeroPresentationSkip.OnCinemaEnd(window);
			SprkHeroPresentationSkip.OnCinemaStart(window, GetNewHero2.EOpenMode.Transcend);
			skip = window.ExitWidget.transform.parent.Find("SprkSkipButton");
			Check(skip == null || !skip.gameObject.activeSelf, "skip button shown while transcending");
			SprkHeroPresentationSkip.OnCinemaEnd(window);
			// Other cinematics (new hero) keep the button.
			SprkHeroPresentationSkip.OnCinemaStart(window, GetNewHero2.EOpenMode.NewHero);
			skip = window.ExitWidget.transform.parent.Find("SprkSkipButton");
			Check(skip != null && skip.gameObject.activeInHierarchy, "skip button not shown");
			Check(!SprkHeroPresentationSkip.IsFastForwarding, "fast-forward without request");
			foreach (UILabel label in skip.GetComponentsInChildren<UILabel>(includeInactive: true))
			{
				Check(label.text == "Skip", "skip label: " + label.text);
			}
			UIButton[] buttons = skip.GetComponentsInChildren<UIButton>(includeInactive: true);
			Check(buttons.Length > 0, "skip has no UIButton");
			foreach (UIButton button in buttons)
			{
				Check(button.onClick.Count == 1 && button.onClick[0].isValid, "skip handler " + button.name);
			}
			// Like Close, the copy is a large tap zone; its caption must be on screen.
			Rect rect = ScreenRect(skip.GetComponentInChildren<UILabel>());
			Check(rect.xMin >= 0f && rect.xMax <= Screen.width && rect.yMin >= 0f && rect.yMax <= Screen.height, "skip caption off screen " + rect);
			// Clicking Skip fast-forwards and hides the button.
			EventDelegate.Execute(buttons[0].onClick);
			Check(SprkHeroPresentationSkip.IsFastForwarding && Time.timeScale > 10f, "click did not fast-forward, timeScale=" + Time.timeScale);
			Check(!skip.gameObject.activeSelf, "skip still shown after click");
			SprkHeroPresentationSkip.OnCinemaEnd(window);
			Check(Mathf.Approximately(Time.timeScale, 1f), "speed not restored after click, timeScale=" + Time.timeScale);
			SprkHeroPresentationSkip.OnCinemaStart(window, GetNewHero2.EOpenMode.Awaken);
			// Back during the (awaken) cinematic fast-forwards; the end restores normal speed.
			window.OnSystemBackButton();
			Check(SprkHeroPresentationSkip.IsFastForwarding && Time.timeScale > 10f, "back did not fast-forward, timeScale=" + Time.timeScale);
			Check(!skip.gameObject.activeSelf, "skip still shown while skipping");
			SprkHeroPresentationSkip.OnCinemaEnd(window);
			Check(!SprkHeroPresentationSkip.IsFastForwarding && Mathf.Approximately(Time.timeScale, 1f), "speed not restored, timeScale=" + Time.timeScale);
			// Auto-skip from the purify room's setting, only for awaken/transcend.
			SprkHeroPresentationSkip.SkipAnimations = true;
			SprkHeroPresentationSkip.OnCinemaStart(window, GetNewHero2.EOpenMode.NewHero);
			Check(!SprkHeroPresentationSkip.IsFastForwarding, "auto-skip applied to a new hero");
			SprkHeroPresentationSkip.OnCinemaEnd(window);
			SprkHeroPresentationSkip.OnCinemaStart(window, GetNewHero2.EOpenMode.Transcend);
			Check(SprkHeroPresentationSkip.IsFastForwarding, "auto-skip not applied to transcend");
			SprkHeroPresentationSkip.OnCinemaEnd(window);
			Check(Mathf.Approximately(Time.timeScale, 1f), "speed not restored after auto-skip");
			// Leave the Skip button visible for a screenshot, with the result
			// panels hidden as during the real cinematic (DisableUI).
			SprkHeroPresentationSkip.SkipAnimations = false;
			UIHelper.EnableGameObject(window.HeroInfoComponent, enable: false);
			UIHelper.EnableGameObject(window.StarGrid, enable: false);
			SprkHeroPresentationSkip.OnCinemaStart(window, GetNewHero2.EOpenMode.NewHero);
			UILabel caption = skip.GetComponentInChildren<UILabel>();
			return "PASS: skip=" + rect + " screen=" + Screen.width + "x" + Screen.height + " captionAlpha=" + caption.finalAlpha + " depth=" + caption.depth + " components=" + string.Join(",", Array.ConvertAll(caption.GetComponents<Component>(), (Component c) => c.GetType().Name));
		}
		finally
		{
			SprkHeroPresentationSkip.SkipAnimations = saved;
		}
	}

	// Fast-forward, then hide the window without the end callback, as if it
	// were torn down mid-cinematic; the watchdog must restore normal speed.
	private static string WatchdogArm()
	{
		GetNewHero2 window = Show<GetNewHero2>("GetNewHero2");
		UIHelper.EnableGameObject(window.ExitWidget, enable: false);
		SprkHeroPresentationSkip.OnCinemaStart(window, GetNewHero2.EOpenMode.Awaken);
		window.OnSystemBackButton();
		Check(SprkHeroPresentationSkip.IsFastForwarding && Time.timeScale > 10f, "not fast-forwarding, timeScale=" + Time.timeScale);
		window.gameObject.SetActive(false);
		return "PASS: armed timeScale=" + Time.timeScale + "; run watchdog-check";
	}

	private static Rect ScreenRect(UIWidget widget)
	{
		Check(widget != null, "no widget");
		Camera camera = NGUITools.FindCameraForLayer(widget.gameObject.layer);
		Check(camera != null, "no UI camera for " + widget.name);
		Vector3[] corners = widget.worldCorners;
		Vector3 min = camera.WorldToScreenPoint(corners[0]);
		Vector3 max = camera.WorldToScreenPoint(corners[2]);
		return Rect.MinMaxRect(Mathf.Round(min.x), Mathf.Round(min.y), Mathf.Round(max.x), Mathf.Round(max.y));
	}

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Hero presentation skip: " + message);
		}
	}
}
