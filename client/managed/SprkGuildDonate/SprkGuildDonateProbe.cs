using System;
using System.Collections.Generic;
using System.Reflection;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using UnityEngine;

/// Automation probe for the guild Donate button: renders the guild
/// Construction window from its prefab on the Login screen (through the
/// ticket-search probe's `show:` mode) and checks where the button and its
/// Wood / Stone / Metal choices sit.
public static class SprkGuildDonateProbe
{
	private static GuildConstruction window;

	private static SprkGuildDonate donate;

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Guild donate: " + message);
		}
	}

	private static void Show()
	{
		string text = (string)Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
			.Invoke(null, new object[1] { "show:GuildConstruction" });
		GameObject gameObject = GameObject.Find("TicketSearchShow_GuildConstruction");
		Check(gameObject != null, "window not shown: " + text);
		foreach (UITweener tweener in gameObject.GetComponentsInChildren<UITweener>(includeInactive: true))
		{
			tweener.enabled = false;
		}
		foreach (UIPanel panel in gameObject.GetComponentsInChildren<UIPanel>(includeInactive: true))
		{
			panel.alpha = 1f;
		}
		window = gameObject.GetComponent<GuildConstruction>();
		Check(window != null, "no GuildConstruction");
		window.Label_NumMember.text = "2/26";
		// The button copies a progress row's Max button; the fixture has no rows yet.
		Transform grid = window.Grid_ProgressBars.GetComponentInChildren<UIGrid>(includeInactive: true).transform;
		GameObject row = UnityEngine.Object.Instantiate(VespaResource.Load<GameObject>(ProgressComponent.PrefabPath), grid, worldPositionStays: false);
		row.SetActive(value: true);
		SprkGuildDonate.Attach(window, show: true);
		donate = window.GetComponent<SprkGuildDonate>();
		Check(donate != null && donate.ButtonRoot != null, "no Donate button");
	}

	private static Bounds Box(GameObject target)
	{
		Transform root = window.Label_NumMember.transform.parent.parent;
		return NGUIMath.CalculateRelativeWidgetBounds(root, target.transform, considerInactive: false);
	}

	private static string Rect(Bounds b)
	{
		return "[" + b.min.x.ToString("0") + ".." + b.max.x.ToString("0") + " x " + b.min.y.ToString("0") + ".." + b.max.y.ToString("0") + "]";
	}

	private static bool Overlaps(Bounds a, Bounds b)
	{
		return a.min.x < b.max.x && b.min.x < a.max.x && a.min.y < b.max.y && b.min.y < a.max.y;
	}

	private static int MinDepth(GameObject root)
	{
		int depth = int.MaxValue;
		foreach (UIWidget widget in root.GetComponentsInChildren<UIWidget>(includeInactive: true))
		{
			depth = Math.Min(depth, widget.depth);
		}
		return depth;
	}

	private static void Click(GameObject root)
	{
		UIButton button = root.GetComponent<UIButton>();
		Check(button != null && button.isEnabled, root.name + " not clickable");
		Check(button.onClick.Count == 1, root.name + " has " + button.onClick.Count + " click handlers");
		EventDelegate.Execute(button.onClick);
	}

	private static string Test()
	{
		Show();
		GameObject buttonRoot = donate.ButtonRoot;
		Bounds member = Box(window.Label_NumMember.transform.parent.gameObject);
		Bounds tabs = Box(window.Toggle_Construction.transform.parent.gameObject);
		Bounds button = Box(buttonRoot);
		Check(buttonRoot.activeInHierarchy, "button hidden");
		Check(buttonRoot.GetComponentInChildren<UILabel>().text == SprkText.Get("Donate"), "button text " + buttonRoot.GetComponentInChildren<UILabel>().text);
		Check(button.max.x <= member.min.x && member.min.x - button.max.x <= 24f, "button not just left of the member count: " + Rect(button) + " member=" + Rect(member));
		Check(Mathf.Abs(button.center.y - member.center.y) <= 2f, "button not level with the member count: " + Rect(button) + " member=" + Rect(member));
		Check(button.size.y <= member.size.y, "button taller than the member box");
		Check(!Overlaps(button, tabs), "button overlaps the tabs");
		Check(MinDepth(buttonRoot) > MinDepth(window.Label_NumMember.transform.parent.gameObject), "button drawn under the member box");
		Check(donate.Choices.Count == 3, "choices " + donate.Choices.Count);
		foreach (GameObject choice in donate.Choices)
		{
			Check(!choice.activeSelf, "choice shown before Donate is pressed: " + choice.name);
		}
		// Donate opens the choices: Wood, Stone, Metal from left to right in the header.
		Click(buttonRoot);
		Check(donate.Choosing, "Donate did not open the choices");
		string[] names = new string[3] { "GUILD_MATERIAL_WOOD", "GUILD_MATERIAL_STONE", "GUILD_MATERIAL_METAL" };
		List<Bounds> placed = new List<Bounds> { button, member, tabs };
		float previous = float.MinValue;
		string layout = "";
		for (int i = 0; i < 3; i++)
		{
			GameObject choice = donate.Choices[i];
			Check(choice.activeInHierarchy, "choice hidden: " + choice.name);
			string label = choice.GetComponentInChildren<UILabel>().text;
			Check(label == LocalizationManager.GetStringDirect(names[i]) && !string.IsNullOrEmpty(label) && label != names[i], "choice " + i + " text " + label);
			Check(choice.GetComponentInChildren<UILabel>().width < choice.GetComponent<UIWidget>().width, "choice label wider than its button");
			Bounds b = Box(choice);
			Check(b.center.x > previous, "choices out of order");
			previous = b.center.x;
			Check(Mathf.Abs(b.center.y - member.center.y) <= 2f, "choice not level with the member count: " + Rect(b));
			Check(b.size.y <= member.size.y, "choice taller than the member box");
			foreach (Bounds other in placed)
			{
				Check(!Overlaps(b, other), choice.name + " overlaps " + Rect(other) + ": " + Rect(b));
			}
			Check(MinDepth(choice) > MinDepth(window.Label_NumMember.transform.parent.gameObject), "choice drawn under the member box");
			placed.Add(b);
			layout = layout + " " + label + "=" + Rect(b);
		}
		// Donate again closes them.
		Click(buttonRoot);
		Check(!donate.Choosing && !donate.Choices[0].activeSelf, "Donate did not close the choices");
		// Picking a material closes the choices and asks to confirm that material.
		Click(buttonRoot);
		try
		{
			Click(donate.Choices[1]);
		}
		catch (Exception ex) when (!(ex.Message.StartsWith("Guild donate")))
		{
			// The confirm popup may not open on the Login screen; the pick is what is checked.
		}
		Check(donate.Offered == GuildConstructionResource.Stone, "Stone pick offered " + donate.Offered);
		Check(!donate.Choosing && !donate.Choices[1].activeSelf, "pick left the choices open");
		foreach (MessagePopup popup in UnityEngine.Object.FindObjectsOfType<MessagePopup>())
		{
			UnityEngine.Object.Destroy(popup.gameObject);
		}
		// The Weekly Status tab keeps the button (the member count stays too).
		window.Root_Construction.gameObject.SetActive(value: false);
		SprkGuildDonate.Attach(window, show: false);
		Check(buttonRoot.activeInHierarchy, "button hidden on Weekly Status");
		window.Root_Construction.gameObject.SetActive(value: true);
		// Closing the window closes the choices.
		Click(buttonRoot);
		donate.enabled = false;
		Check(!donate.Choosing && !donate.Choices[0].activeSelf, "disable left the choices open");
		donate.enabled = true;
		return "PASS: button=" + Rect(button) + " member=" + Rect(member) + " tabs=" + Rect(tabs) + layout;
	}

	public static string Run(string mode)
	{
		switch (mode)
		{
		case "test":
			return Test();
		case "show":
			Show();
			return "shown " + Rect(Box(donate.ButtonRoot));
		case "show-open":
			Show();
			donate.OnClick();
			return "shown open";
		case "hide":
			Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
				.Invoke(null, new object[1] { "hide" });
			return "hidden";
		default:
			if (mode != null && mode.StartsWith("dump:"))
			{
				return (string)Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
					.Invoke(null, new object[1] { "dumpshown:" + mode.Substring(5) });
			}
			throw new ArgumentException("guild donate probe mode: " + mode);
		}
	}
}
