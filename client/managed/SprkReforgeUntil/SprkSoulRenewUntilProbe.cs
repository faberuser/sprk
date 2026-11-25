using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;
using UnityEngine;

/// Automation probe for "Rearrange until": renders the Rearrange Stats popup
/// from its prefab on the Login screen (through the ticket-search probe's
/// `show:` mode), attaches a fixture soul weapon and drives the hooks.
public static class SprkSoulRenewUntilProbe
{
	private const int FixtureSlot = 987654;

	private static RenewSoulWeaponStatPopup popup;

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Rearrange until: " + message);
		}
	}

	private static string Show()
	{
		string text = (string)Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
			.Invoke(null, new object[1] { "show:RenewSoulWeaponStatPopup" });
		GameObject gameObject = GameObject.Find("TicketSearchShow_RenewSoulWeaponStatPopup");
		Check(gameObject != null, "popup not shown: " + text);
		// Skip the opening tween: full size and opacity.
		UITweener[] componentsInChildren = gameObject.GetComponentsInChildren<UITweener>(includeInactive: true);
		foreach (UITweener uITweener in componentsInChildren)
		{
			uITweener.enabled = false;
		}
		gameObject.transform.Find("Panel/Root").localScale = Vector3.one;
		UIPanel[] componentsInChildren2 = gameObject.GetComponentsInChildren<UIPanel>(includeInactive: true);
		foreach (UIPanel uIPanel in componentsInChildren2)
		{
			uIPanel.alpha = 1f;
		}
		popup = gameObject.GetComponent<RenewSoulWeaponStatPopup>();
		Check(popup != null, "no RenewSoulWeaponStatPopup");
		SoulWeaponInfo value = new SoulWeaponInfo
		{
			EquipItemSlotIndex = FixtureSlot,
			OptionRatio1 = 699,
			OptionRatio2 = 301,
			OptionBonusRatio1 = 171,
			OptionBonusRatio2 = 170
		};
		typeof(CommonRenewSoulWeaponStatPopup).GetProperty("SoulWeaponInfo").SetValue(popup, value, null);
		SprkSoulRenewUntil.Save(FixtureSlot, SprkSoulRenewUntil.Any);
		SprkSoulRenewUntil.Opened(popup);
		return text;
	}

	private static Transform Root => popup.transform.Find("Panel/Root");

	private static Bounds BoundsOf(Transform t)
	{
		return NGUIMath.CalculateRelativeWidgetBounds(Root, t, considerInactive: false);
	}

	private static string B(Bounds b)
	{
		return "[" + b.min.x.ToString("0") + ".." + b.max.x.ToString("0") + " x " + b.min.y.ToString("0") + ".." + b.max.y.ToString("0") + "]";
	}

	private static bool Overlap(Bounds a, Bounds b)
	{
		return a.min.x < b.max.x - 1f && b.min.x < a.max.x - 1f && a.min.y < b.max.y - 1f && b.min.y < a.max.y - 1f;
	}

	private static string LabelText(string path)
	{
		Transform transform = Root.Find(path);
		UILabel uILabel = ((transform == null) ? null : transform.GetComponent<UILabel>());
		return (uILabel == null) ? null : uILabel.text;
	}

	private static bool Same(SprkSoulRenewUntil.Target a, SprkSoulRenewUntil.Target b)
	{
		return a.ToString() == b.ToString();
	}

	private static string Test()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Show();
		Transform root = Root;
		UIButton component = root.Find("ActionCost/Button").GetComponent<UIButton>();
		UIButton targetButton = SprkSoulRenewUntil.TargetButton;
		Check(targetButton != null, "no Target Stats button");
		Check(LabelText("ActionCost/SprkSoulRenewUntilTarget/Label") == "Target Stats", "target button text " + LabelText("ActionCost/SprkSoulRenewUntilTarget/Label"));
		Bounds bounds = BoundsOf(component.transform);
		Bounds bounds2 = BoundsOf(targetButton.transform);
		Bounds bounds3 = BoundsOf(root.Find("Sprite_Frame"));
		stringBuilder.Append("buttons target=").Append(B(bounds2)).Append(" renew=")
			.Append(B(bounds))
			.Append(" frame=")
			.Append(B(bounds3));
		Check(!Overlap(bounds, bounds2), "buttons overlap");
		Check(bounds2.min.x > bounds3.min.x + 20f && bounds.max.x < bounds3.max.x - 20f, "buttons leave the frame");
		// Opening the editor hides the native stat rows and gauge.
		EventDelegate.Execute(targetButton.onClick);
		Check(SprkSoulRenewUntil.Editing, "editor did not open");
		Check(!root.Find("StatcomponentRoot/01Max_Atk").gameObject.activeSelf && !popup.transform.Find("Panel/StatProgressRoot").gameObject.activeSelf, "native rows still shown");
		Check(LabelText("StatcomponentRoot/Label_Title") == "Target Stats", "title " + LabelText("StatcomponentRoot/Label_Title"));
		Check(LabelText("ActionCost/SprkSoulRenewUntilTarget/Label") == "Done", "editing button text");
		// Layout: rows do not overlap each other, the buttons or the frame.
		List<KeyValuePair<string, Bounds>> list = new List<KeyValuePair<string, Bounds>>();
		foreach (Transform item in SprkSoulRenewUntil.Editor.transform)
		{
			list.Add(new KeyValuePair<string, Bounds>(item.name, BoundsOf(item)));
		}
		Bounds bounds4 = BoundsOf(root.Find("StatcomponentRoot/Sprite_UpgradeValue"));
		// The title's text, not its (taller) label box.
		UILabel component2 = root.Find("StatcomponentRoot/Label_Title").GetComponent<UILabel>();
		Bounds bounds5 = new Bounds(root.InverseTransformPoint(component2.transform.position), new Vector3(component2.printedSize.x, component2.printedSize.y, 0f));
		stringBuilder.Append("\nbox=").Append(B(bounds4));
		foreach (KeyValuePair<string, Bounds> item2 in list)
		{
			stringBuilder.Append("\n").Append(item2.Key).Append("=")
				.Append(B(item2.Value));
			Check(item2.Value.min.x >= bounds3.min.x + 20f && item2.Value.max.x <= bounds3.max.x - 20f, item2.Key + " leaves the frame " + B(item2.Value));
			Check(!Overlap(item2.Value, bounds) && !Overlap(item2.Value, bounds2), item2.Key + " overlaps the buttons " + B(item2.Value));
			Check(!Overlap(item2.Value, bounds5), item2.Key + " overlaps the title " + B(item2.Value) + " " + B(bounds5));
			foreach (KeyValuePair<string, Bounds> item3 in list)
			{
				bool flag = item2.Key.StartsWith("Preset") && item3.Key.StartsWith("Preset");
				bool flag2 = item3.Key == item2.Key + "Value" || item2.Key == item3.Key + "Value";
				if (string.CompareOrdinal(item2.Key, item3.Key) < 0 && !flag2 && !(flag && item2.Key == item3.Key))
				{
					Check(!Overlap(item2.Value, item3.Value), item2.Key + " overlaps " + item3.Key + " " + B(item2.Value) + " " + B(item3.Value));
				}
			}
		}
		// Slider positions map back to the same values; ATK bonus is anchored
		// left and HP bonus right.
		SprkSoulSlider splitSlider = SprkSoulRenewUntil.SplitSlider;
		SprkSoulSlider atkBonusSlider = SprkSoulRenewUntil.AtkBonusSlider;
		SprkSoulSlider hpBonusSlider = SprkSoulRenewUntil.HpBonusSlider;
		foreach (int num in new int[5] { 300, 301, 500, 699, 700 })
		{
			Check(splitSlider.ValueAt(splitSlider.X(num)) == num, "split maps " + num + " to " + splitSlider.ValueAt(splitSlider.X(num)));
		}
		foreach (int num2 in new int[4] { 0, 1, 150, 200 })
		{
			Check(atkBonusSlider.ValueAt(atkBonusSlider.X(num2)) == num2 && hpBonusSlider.ValueAt(hpBonusSlider.X(num2)) == num2, "bonus maps " + num2);
		}
		Check(splitSlider.ValueAt(-10000f) == 300 && splitSlider.ValueAt(10000f) == 700, "split clamps");
		Check(atkBonusSlider.X(0) < atkBonusSlider.X(200) && hpBonusSlider.X(0) > hpBonusSlider.X(200), "bonus anchors");
		Check(BoundsOf(atkBonusSlider.transform).max.x < BoundsOf(hpBonusSlider.transform).min.x && Mathf.Abs(atkBonusSlider.transform.localPosition.y - hpBonusSlider.transform.localPosition.y) < 1f, "bonuses share a line, ATK left");
		// One split handle: ATK within ±1.0% of it.
		splitSlider.Set(550);
		atkBonusSlider.Set(150);
		hpBonusSlider.Set(150);
		SprkSoulRenewUntil.Target current = SprkSoulRenewUntil.Current;
		Check(current.ToString() == "540,560,150,150", "bruiser target " + current);
		double num3 = 21.0 * 51.0 * 51.0 / (401.0 * 201.0 * 201.0);
		Check(Math.Abs(current.Chance - num3) < 1E-12, "chance " + current.Chance);
		string text2 = LabelText("SprkSoulRenewUntil/Odds");
		Check(text2.Contains(SprkSoulRenewUntil.ChanceText(num3)) && text2.Contains(Math.Ceiling(1.0 / num3).ToString("N0")), "odds " + text2);
		Check(LabelText("SprkSoulRenewUntil/SplitValue") == "ATK 54.0–56.0% / HP 44.0–46.0%", "split text " + LabelText("SprkSoulRenewUntil/SplitValue"));
		Check(LabelText("SprkSoulRenewUntil/AtkBonusValue") == "15.0%+" && LabelText("SprkSoulRenewUntil/HpBonusValue") == "15.0%+", "bonus text");
		Check(Same(SprkSoulRenewUntil.Load(FixtureSlot), current), "not saved");
		Check(LabelText("ActionCost/Button/Label") == "Rearrange Until", "renew label " + LabelText("ActionCost/Button/Label"));
		// "-"/"+" move 0.1% (one unit), clamped at the ends; the band clips there.
		foreach (KeyValuePair<string, string> item4 in new KeyValuePair<string, string>[6]
		{
			new KeyValuePair<string, string>("SplitMore", "541,561,150,150"),
			new KeyValuePair<string, string>("SplitLess", "540,560,150,150"),
			new KeyValuePair<string, string>("AtkBonusMore", "540,560,151,150"),
			new KeyValuePair<string, string>("AtkBonusLess", "540,560,150,150"),
			new KeyValuePair<string, string>("HpBonusMore", "540,560,150,151"),
			new KeyValuePair<string, string>("HpBonusLess", "540,560,150,150")
		})
		{
			UIEventListener.Get(SprkSoulRenewUntil.Editor.transform.Find(item4.Key).gameObject).onClick(null);
			Check(SprkSoulRenewUntil.Current.ToString() == item4.Value, item4.Key + " gave " + SprkSoulRenewUntil.Current);
		}
		splitSlider.Set(700);
		Check(SprkSoulRenewUntil.Current.AtkMin == 690 && SprkSoulRenewUntil.Current.AtkMax == 700, "max end " + SprkSoulRenewUntil.Current);
		splitSlider.Nudge(1);
		Check(splitSlider.Value == 700, "nudge past the end");
		splitSlider.Nudge(-1);
		Check(SprkSoulRenewUntil.Current.AtkMin == 689 && SprkSoulRenewUntil.Current.AtkMax == 700, "near the end " + SprkSoulRenewUntil.Current);
		Check(SprkSoulRenewUntil.SplitHandle(SprkSoulRenewUntil.Load(FixtureSlot)) == 699, "clipped band reloads at its handle");
		splitSlider.Set(300);
		Check(SprkSoulRenewUntil.Current.AtkMin == 300 && SprkSoulRenewUntil.Current.AtkMax == 310, "min end " + SprkSoulRenewUntil.Current);
		// Older two-handle saves load as the band around their middle.
		Check(SprkSoulRenewUntil.Normalized(new SprkSoulRenewUntil.Target(530, 570, 0, 0)).ToString() == "540,560,0,0", "old save");
		// Presets set every slider; "Clear" frees the split and restores the native Rearrange.
		EventDelegate.Execute(SprkSoulRenewUntil.Editor.transform.Find("Preset0").GetComponent<UIButton>().onClick);
		Check(SprkSoulRenewUntil.Current.ToString() == "690,700,200,200", "Max ATK preset " + SprkSoulRenewUntil.Current);
		Check(splitSlider.Value == 700 && atkBonusSlider.Value == 200 && hpBonusSlider.Value == 200, "sliders follow the preset");
		EventDelegate.Execute(SprkSoulRenewUntil.Editor.transform.Find("Preset1").GetComponent<UIButton>().onClick);
		Check(SprkSoulRenewUntil.Current.ToString() == "300,310,200,200", "Max HP preset " + SprkSoulRenewUntil.Current);
		EventDelegate.Execute(SprkSoulRenewUntil.Editor.transform.Find("Preset3").GetComponent<UIButton>().onClick);
		Check(SprkSoulRenewUntil.Current.IsAny && SprkSoulRenewUntil.Load(FixtureSlot).IsAny && splitSlider.Free, "Clear preset");
		Check(LabelText("SprkSoulRenewUntil/SplitValue") == "Any" && !splitSlider.Band.enabled, "free split shown");
		Check(LabelText("SprkSoulRenewUntil/AtkBonusValue") == "Any" && LabelText("SprkSoulRenewUntil/HpBonusValue") == "Any", "any bonus text");
		atkBonusSlider.Set(100);
		Check(SprkSoulRenewUntil.Current.ToString() == "300,700,100,0", "bonus alone keeps any split " + SprkSoulRenewUntil.Current);
		atkBonusSlider.Set(0);
		Check(LabelText("ActionCost/Button/Label") != "Rearrange Until" && root.Find("ActionCost/Button/Label").GetComponent<NVespa.NNGUIExtension.UILocalizeEx>().enabled, "renew label restored: " + LabelText("ActionCost/Button/Label"));
		Check(!SprkSoulRenewUntil.TryStart(popup.SoulWeaponInfo), "no target must keep the native flow");
		// Closing restores the native view.
		EventDelegate.Execute(targetButton.onClick);
		Check(!SprkSoulRenewUntil.Editing, "editor did not close");
		Check(root.Find("StatcomponentRoot/01Max_Atk").gameObject.activeSelf && popup.transform.Find("Panel/StatProgressRoot").gameObject.activeSelf, "native rows not restored");
		Check(LabelText("StatcomponentRoot/Label_Title") != "Target Stats" && root.Find("StatcomponentRoot/Label_Title").GetComponent<NVespa.NNGUIExtension.UILocalizeEx>().enabled, "title not restored: " + LabelText("StatcomponentRoot/Label_Title"));
		Check(BoundsOf(root.Find("StatcomponentRoot/Sprite_UpgradeValue")).size.y < bounds4.size.y, "box not restored");
		// Saved targets come back when the popup reopens for the weapon.
		SprkSoulRenewUntil.SetTarget(new SprkSoulRenewUntil.Target(490, 510, 150, 150));
		SprkSoulRenewUntil.Closing(popup);
		SprkSoulRenewUntil.Opened(popup);
		Check(SprkSoulRenewUntil.Current.ToString() == "490,510,150,150", "reload " + SprkSoulRenewUntil.Current);
		Check(LabelText("ActionCost/SprkSoulRenewUntilTarget/Label") == "Targets: On", "target on text");
		Check(SprkSoulRenewUntil.Load(FixtureSlot + 1).IsAny, "other weapons keep no target");
		SprkSoulRenewUntil.Save(FixtureSlot, SprkSoulRenewUntil.Any);
		// Wire format through the game's own request encoder and response parser.
		Dictionary<string, object> dictionary = SprkSoulRenewUntil.EncodePreview(FixtureSlot, new SprkSoulRenewUntil.Target(530, 570, 150, 0));
		Check((string)dictionary["SprkAtkMin"] == "530" && (string)dictionary["SprkAtkMax"] == "570" && (string)dictionary["SprkAtkBonusMin"] == "150" && (string)dictionary["SprkHpBonusMin"] == "0" && (string)dictionary["SprkPreview"] == "1" && !dictionary.ContainsKey("SprkMaxTries"), "preview request " + MiniJSON.Json.Serialize(dictionary));
		Check(!JM_NShared_SoulWeaponRenewOption_Request.EncodeJsonObject(new NShared.SoulWeaponRenewOption.Request()).ContainsKey("SprkAtkMin"), "native request carries a target");
		NShared.SoulWeaponRenewOption.Response response = JM_NShared_SoulWeaponRenewOption_Response.Parse("{\"BaseResult\":\"Success\",\"Result\":\"Success\",\"SprkTries\":37,\"SprkHit\":false,\"SprkTicketsUsed\":2,\"SprkGemsSpent\":7000,\"SprkGuaranteed\":false,\"SprkPityMisses\":40,\"SprkPityLimit\":456}");
		string text3 = SprkSoulRenewUntil.RunSummary(response);
		Check(text3 != null && text3.Contains("37") && text3.Contains("7,000") && text3.Contains("Rearrange tickets: 2") && text3.Contains("40/456") && text3.Contains("last roll"), "run summary " + text3);
		Check(SprkSoulRenewUntil.RunSummary(JM_NShared_SoulWeaponRenewOption_Response.Parse("{\"BaseResult\":\"Success\",\"Result\":\"Success\"}")) == null, "native response summarized");
		stringBuilder.Append("\nsummary=").Append(text3.Replace("\n", " | "));
		return "PASS: " + stringBuilder;
	}

	public static string Run(string mode)
	{
		if (mode == "test")
		{
			return Test();
		}
		if (mode == "show-editor" || mode == "show-buttons")
		{
			Show();
			SprkSoulRenewUntil.SetTarget(new SprkSoulRenewUntil.Target(540, 560, 150, 150));
			if (mode == "show-editor")
			{
				SprkSoulRenewUntil.ToggleEditor();
			}
			return "shown " + mode + " target=" + SprkSoulRenewUntil.Current;
		}
		if (mode == "hide")
		{
			if (popup != null)
			{
				SprkSoulRenewUntil.Save(FixtureSlot, SprkSoulRenewUntil.Any);
				SprkSoulRenewUntil.Closing(popup);
				UnityEngine.Object.DestroyImmediate(popup.gameObject);
			}
			return "hidden";
		}
		return "unknown mode " + mode;
	}
}
