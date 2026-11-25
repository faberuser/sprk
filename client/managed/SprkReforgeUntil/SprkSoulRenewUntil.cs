using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;
using Math = System.Math;

/// "Rearrange until" for soul weapons: "Target Stats" in the Rearrange Stats
/// popup opens sliders for the ATK/HP split and the minimum ATK and HP bonus.
/// With a target set, Rearrange rolls until the stats meet it. The server runs
/// the rolls (`renew_until` in soul.rs); the last roll is kept or taken through
/// the game's normal confirmation. Targets are saved per weapon.
public static class SprkSoulRenewUntil
{
	/// Target in the server's 1/1000 units. HP's share is 1000 minus ATK's.
	internal struct Target
	{
		public int AtkMin;

		public int AtkMax;

		public int AtkBonus;

		public int HpBonus;

		public bool IsAny => AtkMin <= RatioMin && AtkMax >= RatioMax && AtkBonus <= 0 && HpBonus <= 0;

		/// Every value of a roll is uniform and independent (matches the server).
		public double Chance => (double)(AtkMax - AtkMin + 1) / (RatioMax - RatioMin + 1) * (BonusMax - AtkBonus + 1) / (BonusMax + 1) * (BonusMax - HpBonus + 1) / (BonusMax + 1);

		public Target(int atkMin, int atkMax, int atkBonus, int hpBonus)
		{
			AtkMin = atkMin;
			AtkMax = atkMax;
			AtkBonus = atkBonus;
			HpBonus = hpBonus;
		}

		public Target Clamped()
		{
			int num = Mathf.Clamp(AtkMin, RatioMin, RatioMax);
			return new Target(num, Mathf.Clamp(AtkMax, num, RatioMax), Mathf.Clamp(AtkBonus, 0, BonusMax), Mathf.Clamp(HpBonus, 0, BonusMax));
		}

		public override string ToString()
		{
			return AtkMin + "," + AtkMax + "," + AtkBonus + "," + HpBonus;
		}
	}

	private sealed class Ask
	{
		public Target Target;

		public bool Preview;

		public int MaxTries;
	}

	private sealed class Extras
	{
		public long PityMisses;

		public long PityLimit;

		public bool IsRun;

		public int Tries;

		public bool Hit;

		public int TicketsUsed;

		public long GemsSpent;

		public bool Guaranteed;
	}

	private struct Budget
	{
		public int Tickets;

		public long Gems;

		public int[] Prices;

		public int RenewCount;
	}

	private struct Preset
	{
		public string Caption;

		public Target Target;
	}

	internal const int RatioMin = 300;

	internal const int RatioMax = 700;

	internal const int BonusMax = 200;

	// One request makes at most this many rolls, and the guarantee lands by
	// then at the latest (matches the server).
	public const int MaxTries = 100000;

	internal static readonly Target Any = new Target(RatioMin, RatioMax, 0, 0);

	/// The split slider's single handle accepts ATK within this much of it
	/// (1/1000 units: ±1.0%), clipped at the ends.
	internal const int SplitTolerance = 10;

	private static readonly Preset[] Presets = new Preset[4]
	{
		new Preset
		{
			Caption = "Max ATK",
			Target = AroundSplit(RatioMax, BonusMax, BonusMax)
		},
		new Preset
		{
			Caption = "Max HP",
			Target = AroundSplit(RatioMin, BonusMax, BonusMax)
		},
		new Preset
		{
			Caption = "Balanced",
			Target = AroundSplit(500, BonusMax, BonusMax)
		},
		new Preset
		{
			Caption = "Clear",
			Target = Any
		}
	};

	private static readonly Color Brown = new Color32(108, 75, 43, byte.MaxValue);

	private static readonly Color AtkColor = new Color32(200, 60, 60, byte.MaxValue);

	private static readonly Color HpColor = new Color32(60, 155, 60, byte.MaxValue);

	private static string Title => SprkText.Get("Rearrange Until");

	private static readonly ConditionalWeakTable<NShared.SoulWeaponRenewOption.Request, Ask> Asks = new ConditionalWeakTable<NShared.SoulWeaponRenewOption.Request, Ask>();

	private static readonly ConditionalWeakTable<NShared.SoulWeaponRenewOption.Response, Extras> Captured = new ConditionalWeakTable<NShared.SoulWeaponRenewOption.Response, Extras>();

	private static Ask armed;

	private static bool busy;

	// ---- Popup state -------------------------------------------------------

	private static RenewSoulWeaponStatPopup popup;

	private static int slot;

	private static Target target = Any;

	private static UIButton targetButton;

	private static UILabel targetLabel;

	private static UILabel renewLabel;

	private static string renewText;

	private static GameObject editor;

	private static UISprite box;

	private static UILabel title;

	private static UILocalizeEx titleLocalizer;

	private static string titleText;

	private static int boxHeight;

	private static readonly SprkReforgeUntil.PopupEdit editing = new SprkReforgeUntil.PopupEdit();

	internal static SprkSoulSlider SplitSlider;

	internal static SprkSoulSlider AtkBonusSlider;

	internal static SprkSoulSlider HpBonusSlider;

	private static UILabel splitValue;

	private static UILabel atkBonusValue;

	private static UILabel hpBonusValue;

	private static UILabel oddsLabel;

	private static readonly List<KeyValuePair<UILabel, string>> captions = new List<KeyValuePair<UILabel, string>>();

	internal static bool Editing => editor != null && editor.activeSelf;

	internal static Target Current => target;

	internal static GameObject Editor => editor;

	internal static UIButton TargetButton => targetButton;

	// ---- Wire format -------------------------------------------------------

	public static void Encode(Dictionary<string, object> json, NShared.SoulWeaponRenewOption.Request request)
	{
		if (json == null || request == null || !Asks.TryGetValue(request, out var ask))
		{
			return;
		}
		json["SprkAtkMin"] = ask.Target.AtkMin.ToString();
		json["SprkAtkMax"] = ask.Target.AtkMax.ToString();
		json["SprkAtkBonusMin"] = ask.Target.AtkBonus.ToString();
		json["SprkHpBonusMin"] = ask.Target.HpBonus.ToString();
		if (ask.Preview)
		{
			json["SprkPreview"] = "1";
		}
		else
		{
			json["SprkMaxTries"] = ask.MaxTries.ToString();
		}
	}

	public static void Capture(IDictionary json, NShared.SoulWeaponRenewOption.Response response)
	{
		if (json == null || response == null || !json.Contains("SprkPityLimit"))
		{
			return;
		}
		Extras extras = new Extras
		{
			PityMisses = Number(json, "SprkPityMisses"),
			PityLimit = Number(json, "SprkPityLimit"),
			IsRun = json.Contains("SprkTries"),
			Tries = (int)Number(json, "SprkTries"),
			Hit = Flag(json, "SprkHit"),
			TicketsUsed = (int)Number(json, "SprkTicketsUsed"),
			GemsSpent = Number(json, "SprkGemsSpent"),
			Guaranteed = Flag(json, "SprkGuaranteed")
		};
		Captured.Remove(response);
		Captured.Add(response, extras);
	}

	private static long Number(IDictionary json, string key)
	{
		if (!json.Contains(key) || json[key] == null)
		{
			return 0L;
		}
		try
		{
			return Convert.ToInt64(json[key], System.Globalization.CultureInfo.InvariantCulture);
		}
		catch
		{
			return 0L;
		}
	}

	private static bool Flag(IDictionary json, string key)
	{
		return json.Contains(key) && json[key] is bool && (bool)json[key];
	}

	// ---- Saved targets -----------------------------------------------------

	private static string PrefKey(int weaponSlot)
	{
		return "Sprk.SoulRenewUntil." + weaponSlot;
	}

	internal static Target Load(int weaponSlot)
	{
		string[] array = SprkTestPrefs.GetString(PrefKey(weaponSlot), string.Empty).Split(',');
		if (array.Length != 4)
		{
			return Any;
		}
		int[] array2 = new int[4];
		for (int i = 0; i < 4; i++)
		{
			if (!int.TryParse(array[i], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out array2[i]))
			{
				return Any;
			}
		}
		return Normalized(new Target(array2[0], array2[1], array2[2], array2[3]));
	}

	internal static bool SplitIsAny(Target t)
	{
		return t.AtkMin <= RatioMin && t.AtkMax >= RatioMax;
	}

	/// The split handle for a target: the middle of its ATK range, or the end
	/// it touches when the range was clipped there.
	internal static int SplitHandle(Target t)
	{
		if (SplitIsAny(t))
		{
			return (RatioMin + RatioMax) / 2;
		}
		if (t.AtkMax >= RatioMax)
		{
			return Math.Min(RatioMax, t.AtkMin + SplitTolerance);
		}
		if (t.AtkMin <= RatioMin)
		{
			return Math.Max(RatioMin, t.AtkMax - SplitTolerance);
		}
		return (t.AtkMin + t.AtkMax) / 2;
	}

	internal static Target AroundSplit(int handle, int atkBonus, int hpBonus)
	{
		return new Target(Math.Max(RatioMin, handle - SplitTolerance), Math.Min(RatioMax, handle + SplitTolerance), atkBonus, hpBonus).Clamped();
	}

	/// Any saved range as the slider would set it (older saves used two handles).
	internal static Target Normalized(Target t)
	{
		t = t.Clamped();
		return SplitIsAny(t) ? t : AroundSplit(SplitHandle(t), t.AtkBonus, t.HpBonus);
	}

	internal static void Save(int weaponSlot, Target value)
	{
		SprkTestPrefs.SetString(PrefKey(weaponSlot), value.IsAny ? string.Empty : value.ToString());
		SprkTestPrefs.Save();
	}

	// ---- Text --------------------------------------------------------------

	internal static string Percent(int value)
	{
		return ((double)value / 10.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%";
	}

	private static string Range(int min, int max)
	{
		return (min == max) ? Percent(min) : (((double)min / 10.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "–" + Percent(max));
	}

	internal static string SplitText(Target t)
	{
		if (SplitIsAny(t))
		{
			return SprkText.Get("Any");
		}
		return SprkText.Format("ATK {0} / HP {1}", Range(t.AtkMin, t.AtkMax), Range(1000 - t.AtkMax, 1000 - t.AtkMin));
	}

	internal static string BonusText(int min)
	{
		return (min <= 0) ? SprkText.Get("Any") : (Percent(min) + "+");
	}

	/// The target in one line, leaving out the parts that accept anything.
	internal static string Describe(Target t)
	{
		List<string> list = new List<string>();
		if (t.AtkMin > RatioMin || t.AtkMax < RatioMax)
		{
			list.Add(SplitText(t));
		}
		if (t.AtkBonus > 0)
		{
			list.Add(SprkText.Format("ATK Bonus {0}", BonusText(t.AtkBonus)));
		}
		if (t.HpBonus > 0)
		{
			list.Add(SprkText.Format("HP Bonus {0}", BonusText(t.HpBonus)));
		}
		return string.Join(", ", list.ToArray());
	}

	internal static string OddsText(Target t)
	{
		if (t.IsAny)
		{
			return SprkText.Get("No target: Rearrange works as usual.");
		}
		double chance = t.Chance;
		return SprkText.Format("Chance per roll: {0} · about {1} rolls on average", ChanceText(chance), Math.Ceiling(1.0 / chance).ToString("N0"));
	}

	internal static string ChanceText(double chance)
	{
		double num = chance * 100.0;
		if (num >= 99.95)
		{
			return (num >= 100.0) ? "100%" : "~100%";
		}
		string format = (num >= 10.0) ? "0" : ((num >= 0.1) ? "0.0#" : "0.00##");
		return num.ToString(format, System.Globalization.CultureInfo.InvariantCulture) + "%";
	}

	// ---- Popup hooks -------------------------------------------------------

	public static void Opened(RenewSoulWeaponStatPopup renewPopup)
	{
		if (renewPopup == null)
		{
			return;
		}
		CloseEditor();
		popup = renewPopup;
		slot = (popup.SoulWeaponInfo == null) ? 0 : popup.SoulWeaponInfo.EquipItemSlotIndex;
		target = Load(slot);
		try
		{
			EnsureUi();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SprkSoulRenewUntil: " + ex);
		}
		Paint();
	}

	public static void Closing(RenewSoulWeaponStatPopup renewPopup)
	{
		if (renewPopup != null && renewPopup == popup)
		{
			CloseEditor();
		}
	}

	private static Transform Root()
	{
		return (popup == null) ? null : popup.transform.Find("Panel/Root");
	}

	private static UIButton RenewButton()
	{
		Transform transform = Root();
		transform = ((transform == null) ? null : transform.Find("ActionCost/Button"));
		return (transform == null) ? null : transform.GetComponent<UIButton>();
	}

	private static void DropLocalizers(GameObject gameObject)
	{
		MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			if (monoBehaviour != null && monoBehaviour.GetType().Name.Contains("Localize"))
			{
				UnityEngine.Object.DestroyImmediate(monoBehaviour);
			}
		}
	}

	/// Builds the controls once per popup instance: a "Target Stats" button
	/// beside Rearrange, and the hidden target editor over the stat area.
	private static void EnsureUi()
	{
		Transform root = Root();
		UIButton uIButton = RenewButton();
		if (root == null || uIButton == null)
		{
			return;
		}
		if (renewLabel == null || renewLabel.transform.parent != uIButton.transform)
		{
			renewLabel = uIButton.GetComponentInChildren<UILabel>(includeInactive: true);
			// The prefab's own text, used only when its localizer has no key.
			renewText = (renewLabel == null) ? null : renewLabel.text;
		}
		if (targetButton == null || targetButton.transform.parent != uIButton.transform.parent)
		{
			BuildButtons(uIButton);
		}
		if (editor == null || editor.transform.parent != root)
		{
			BuildEditor(root, uIButton);
		}
	}

	private static UISprite ButtonSprite(UIButton button)
	{
		return ((button.tweenTarget != null) ? button.tweenTarget.GetComponent<UISprite>() : null) ?? button.GetComponent<UISprite>();
	}

	private static void Resize(UIButton button, int width, int height)
	{
		UISprite uISprite = ButtonSprite(button);
		if (uISprite != null)
		{
			uISprite.width = width;
			uISprite.height = height;
		}
		BoxCollider component = button.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.size = new Vector3(width, height, component.size.z);
		}
		UILabel componentInChildren = button.GetComponentInChildren<UILabel>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.width = Mathf.Max(10, width - 60);
		}
	}

	/// A copy of the Rearrange button, without its click target or localizer.
	private static UIButton CopyButton(UIButton source, Transform parent, string name, EventDelegate.Callback onClick)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(source.gameObject, parent, worldPositionStays: false);
		gameObject.name = name;
		DropLocalizers(gameObject);
		UIButton component = gameObject.GetComponent<UIButton>();
		component.onClick.Clear();
		EventDelegate.Add(component.onClick, onClick);
		component.isEnabled = true;
		gameObject.SetActive(value: true);
		return component;
	}

	/// Rearrange moves right and "Target Stats" takes the left half.
	private static void BuildButtons(UIButton renew)
	{
		UISprite uISprite = ButtonSprite(renew);
		int height = (uISprite == null) ? 104 : uISprite.height;
		Vector3 localPosition = renew.transform.localPosition;
		renew.transform.localPosition = new Vector3(180f, localPosition.y, localPosition.z);
		Resize(renew, 340, height);
		targetButton = CopyButton(renew, renew.transform.parent, "SprkSoulRenewUntilTarget", ToggleEditor);
		targetButton.transform.localPosition = new Vector3(-180f, localPosition.y, localPosition.z);
		targetLabel = targetButton.GetComponentInChildren<UILabel>(includeInactive: true);
		NGUITools.MarkParentAsChanged(targetButton.gameObject);
	}

	private static UILabel AddLabel(UILabel template, Transform parent, string name, UIWidget.Pivot pivot, float x, float y, int width, int size, Color color)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(template.gameObject, parent, worldPositionStays: false);
		gameObject.name = name;
		DropLocalizers(gameObject);
		for (int num = gameObject.transform.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.DestroyImmediate(gameObject.transform.GetChild(num).gameObject);
		}
		UILabel component = gameObject.GetComponent<UILabel>();
		component.pivot = pivot;
		component.overflowMethod = UILabel.Overflow.ShrinkContent;
		component.width = width;
		component.height = size + 4;
		component.fontSize = size;
		component.color = color;
		component.effectStyle = UILabel.Effect.None;
		component.supportEncoding = true;
		component.depth = 420;
		gameObject.transform.localPosition = new Vector3(x, y, 0f);
		gameObject.transform.localScale = Vector3.one;
		gameObject.SetActive(value: true);
		return component;
	}

	private static UISprite AddSprite(UISprite template, Transform parent, string name, UIWidget.Pivot pivot, int width, int height, Color color, int depth)
	{
		UISprite uISprite = NGUITools.AddWidget<UISprite>(parent.gameObject, depth);
		uISprite.name = name;
		uISprite.atlas = template.atlas;
		uISprite.spriteName = template.spriteName;
		uISprite.type = UIBasicSprite.Type.Sliced;
		uISprite.pivot = pivot;
		uISprite.width = width;
		uISprite.height = height;
		uISprite.color = color;
		return uISprite;
	}

	/// A slider row's drag area, track, fills and handle. A split slider fills
	/// red (ATK) left of its handle and green (HP) right of it and marks the
	/// accepted band; a bonus slider fills from its anchor end to the handle.
	private static SprkSoulSlider AddSlider(Transform parent, string name, float x, float y, int width, UISprite white, Color fill, bool reversed, bool split)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.layer = parent.gameObject.layer;
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		gameObject.transform.localPosition = new Vector3(x, y, 0f);
		SprkSoulSlider sprkSoulSlider = gameObject.AddComponent<SprkSoulSlider>();
		sprkSoulSlider.Width = width;
		sprkSoulSlider.Reversed = reversed;
		// An invisible widget gives the drag area its size and depth; the touch
		// area is taller than the drawn row, for fingers.
		int num = width + (split ? 60 : 20);
		UIWidget uIWidget = gameObject.AddComponent<UIWidget>();
		uIWidget.width = num;
		uIWidget.height = 40;
		uIWidget.depth = 430;
		NGUITools.AddWidgetCollider(gameObject);
		gameObject.GetComponent<BoxCollider>().size = new Vector3(num, 60f, 0f);
		AddSprite(white, gameObject.transform, "Track", UIWidget.Pivot.Center, width, 14, new Color(Brown.r, Brown.g, Brown.b, 0.3f), 421).transform.localPosition = Vector3.zero;
		sprkSoulSlider.Fill = AddSprite(white, gameObject.transform, "Fill", UIWidget.Pivot.Left, 10, 14, split ? AtkColor : fill, 422);
		if (split)
		{
			sprkSoulSlider.Fill2 = AddSprite(white, gameObject.transform, "Fill2", UIWidget.Pivot.Left, 10, 14, HpColor, 422);
			sprkSoulSlider.Band = AddSprite(white, gameObject.transform, "Band", UIWidget.Pivot.Left, 10, 24, new Color32(240, 190, 60, 230), 423);
			sprkSoulSlider.BandWidth = SplitTolerance;
		}
		sprkSoulSlider.Thumb = AddThumb(white, gameObject.transform, "Thumb");
		return sprkSoulSlider;
	}

	private static Transform AddThumb(UISprite white, Transform parent, string name)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.layer = parent.gameObject.layer;
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		AddSprite(white, gameObject.transform, "Edge", UIWidget.Pivot.Center, 16, 36, Brown, 425);
		AddSprite(white, gameObject.transform, "Face", UIWidget.Pivot.Center, 10, 30, new Color32(253, 233, 212, byte.MaxValue), 426);
		return gameObject.transform;
	}

	private static void AddNudge(UILabel template, Transform parent, string name, string text, float x, float y, Func<SprkSoulSlider> slider, int delta)
	{
		UILabel uILabel = AddLabel(template, parent, name, UIWidget.Pivot.Center, x, y, 50, 36, Brown);
		uILabel.height = 40;
		uILabel.text = text;
		uILabel.depth = 431;
		NGUITools.AddWidgetCollider(uILabel.gameObject);
		uILabel.GetComponent<BoxCollider>().size = new Vector3(60f, 60f, 0f);
		UIEventListener.Get(uILabel.gameObject).onClick = delegate
		{
			slider()?.Nudge(delta);
		};
	}

	private static UILabel AddCaption(UILabel template, Transform parent, string name, string caption, float y)
	{
		UILabel uILabel = AddLabel(template, parent, name, UIWidget.Pivot.Left, -310f, y, 210, 24, Brown);
		captions.Add(new KeyValuePair<UILabel, string>(uILabel, caption));
		return AddLabel(template, parent, name + "Value", UIWidget.Pivot.Right, 310f, y, 410, 24, Brown);
	}

	/// The editor: the split row, one row with both bonuses (ATK anchored left,
	/// HP anchored right), presets and the odds, laid out over the stat area
	/// (the "Current Stats" box grows to hold them). Every slider has "-"/"+"
	/// for 0.1% steps.
	private static void BuildEditor(Transform root, UIButton renew)
	{
		Transform transform = root.Find("StatcomponentRoot/01Max_Atk/Label");
		UILabel uILabel = ((transform == null) ? null : transform.GetComponent<UILabel>());
		Transform transform2 = popup.transform.Find("Panel/Outside_ClickClose");
		UISprite uISprite = ((transform2 == null) ? null : transform2.GetComponent<UISprite>());
		if (uILabel == null || uISprite == null || uISprite.atlas == null)
		{
			return;
		}
		captions.Clear();
		editor = new GameObject("SprkSoulRenewUntil");
		editor.layer = root.gameObject.layer;
		editor.transform.SetParent(root, worldPositionStays: false);
		Transform parent = editor.transform;
		splitValue = AddCaption(uILabel, parent, "Split", "ATK / HP split", 104f);
		SplitSlider = AddSlider(parent, "SplitSlider", 0f, 68f, 560, uISprite, AtkColor, reversed: false, split: true);
		// "-" lowers ATK (moves left), "+" raises it.
		AddNudge(uILabel, parent, "SplitLess", "-", -335f, 68f, () => SplitSlider, -1);
		AddNudge(uILabel, parent, "SplitMore", "+", 335f, 68f, () => SplitSlider, 1);
		captions.Add(new KeyValuePair<UILabel, string>(AddLabel(uILabel, parent, "AtkBonus", UIWidget.Pivot.Left, -310f, 20f, 170, 24, Brown), "ATK Bonus"));
		atkBonusValue = AddLabel(uILabel, parent, "AtkBonusValue", UIWidget.Pivot.Right, -30f, 20f, 110, 24, Brown);
		captions.Add(new KeyValuePair<UILabel, string>(AddLabel(uILabel, parent, "HpBonus", UIWidget.Pivot.Right, 310f, 20f, 170, 24, Brown), "HP Bonus"));
		hpBonusValue = AddLabel(uILabel, parent, "HpBonusValue", UIWidget.Pivot.Left, 30f, 20f, 110, 24, Brown);
		AtkBonusSlider = AddSlider(parent, "AtkBonusSlider", -180f, -18f, 230, uISprite, AtkColor, reversed: false, split: false);
		HpBonusSlider = AddSlider(parent, "HpBonusSlider", 180f, -18f, 230, uISprite, HpColor, reversed: true, split: false);
		// Each bonus grows away from its anchor: "-" sits at the outer edge
		// and "+" toward the middle, the way the handle moves.
		AddNudge(uILabel, parent, "AtkBonusLess", "-", -335f, -18f, () => AtkBonusSlider, -1);
		AddNudge(uILabel, parent, "AtkBonusMore", "+", -30f, -18f, () => AtkBonusSlider, 1);
		AddNudge(uILabel, parent, "HpBonusMore", "+", 30f, -18f, () => HpBonusSlider, 1);
		AddNudge(uILabel, parent, "HpBonusLess", "-", 335f, -18f, () => HpBonusSlider, -1);
		SplitSlider.Configure(RatioMin, RatioMax, OnSlider);
		AtkBonusSlider.Configure(0, BonusMax, OnSlider);
		HpBonusSlider.Configure(0, BonusMax, OnSlider);
		for (int i = 0; i < Presets.Length; i++)
		{
			Target preset = Presets[i].Target;
			UIButton uIButton = CopyButton(renew, parent, "Preset" + i, delegate
			{
				SetTarget(preset);
			});
			uIButton.transform.localPosition = new Vector3(-237f + 158f * (float)i, -80f, 0f);
			uIButton.transform.localScale = Vector3.one;
			Resize(uIButton, 150, 52);
			UILabel componentInChildren = uIButton.GetComponentInChildren<UILabel>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.fontSize = 22;
				componentInChildren.height = 40;
				componentInChildren.overflowMethod = UILabel.Overflow.ShrinkContent;
				captions.Add(new KeyValuePair<UILabel, string>(componentInChildren, Presets[i].Caption));
			}
			UIWidget[] componentsInChildren = uIButton.GetComponentsInChildren<UIWidget>(includeInactive: true);
			foreach (UIWidget uIWidget in componentsInChildren)
			{
				uIWidget.depth += 330;
			}
		}
		oddsLabel = AddLabel(uILabel, parent, "Odds", UIWidget.Pivot.Center, 0f, -146f, 700, 22, Brown);
		Transform transform3 = root.Find("StatcomponentRoot/Sprite_UpgradeValue");
		box = ((transform3 == null) ? null : transform3.GetComponent<UISprite>());
		boxHeight = ((box == null) ? 0 : box.height);
		editor.SetActive(value: false);
	}

	private static void OnSlider()
	{
		if (SplitSlider == null)
		{
			return;
		}
		int value = AtkBonusSlider.Value;
		int value2 = HpBonusSlider.Value;
		SetTarget(SplitSlider.Free ? new Target(RatioMin, RatioMax, value, value2) : AroundSplit(SplitSlider.Value, value, value2), fromSlider: true);
	}

	internal static void SetTarget(Target value, bool fromSlider = false)
	{
		target = value.Clamped();
		Save(slot, target);
		if (!fromSlider && SplitSlider != null)
		{
			SplitSlider.Show(SplitHandle(target), SplitIsAny(target));
			AtkBonusSlider.Show(target.AtkBonus, free: false);
			HpBonusSlider.Show(target.HpBonus, free: false);
		}
		Paint();
	}

	internal static void ToggleEditor()
	{
		if (busy)
		{
			return;
		}
		if (Editing)
		{
			CloseEditor();
		}
		else
		{
			OpenEditor();
		}
		Paint();
	}

	internal static void OpenEditor()
	{
		Transform transform = Root();
		if (editor == null || transform == null)
		{
			return;
		}
		Transform transform2 = transform.Find("StatcomponentRoot");
		if (transform2 != null)
		{
			foreach (Transform item in transform2)
			{
				if (item.name != "Label_Title" && item.name != "Sprite_UpgradeValue")
				{
					editing.SetActive(item.gameObject, active: false);
				}
			}
			// The title's localizer would write "Current Stats" back (on its
			// first Start or a language reload), so it pauses while editing.
			Transform transform3 = transform2.Find("Label_Title");
			title = ((transform3 == null) ? null : transform3.GetComponent<UILabel>());
			if (title != null)
			{
				titleText = title.text;
				UILocalizeEx component = title.GetComponent<UILocalizeEx>();
				if (component != null && component.enabled)
				{
					component.enabled = false;
					titleLocalizer = component;
				}
				title.text = SprkText.Get("Target Stats");
			}
		}
		Transform transform4 = popup.transform.Find("Panel/StatProgressRoot");
		Transform transform5 = popup.transform.Find("Panel/Btn_Tip");
		editing.SetActive((transform4 == null) ? null : transform4.gameObject, active: false);
		editing.SetActive((transform5 == null) ? null : transform5.gameObject, active: false);
		if (box != null)
		{
			box.height = 290;
		}
		editor.SetActive(value: true);
		SetTarget(target);
	}

	internal static void CloseEditor()
	{
		editing.Restore();
		if (title != null)
		{
			Restore(title, titleLocalizer, titleText);
		}
		title = null;
		titleText = null;
		titleLocalizer = null;
		if (box != null && boxHeight > 0)
		{
			box.height = boxHeight;
		}
		if (editor != null)
		{
			editor.SetActive(value: false);
		}
	}

	private static void Paint()
	{
		foreach (KeyValuePair<UILabel, string> caption in captions)
		{
			if (caption.Key != null)
			{
				caption.Key.text = SprkText.Get(caption.Value);
			}
		}
		if (targetLabel != null)
		{
			targetLabel.text = (Editing ? SprkText.Get("Done") : (target.IsAny ? SprkText.Get("Target Stats") : SprkText.Get("Targets: On")));
		}
		if (renewLabel != null)
		{
			// With a target the label says so; without one the game's localizer
			// writes its own text again (it may not have run yet when the popup
			// opens, so no text is saved for later).
			UILocalizeEx component = renewLabel.GetComponent<UILocalizeEx>();
			if (!target.IsAny)
			{
				if (component != null)
				{
					component.enabled = false;
				}
				renewLabel.text = Title;
			}
			else if (component == null || !component.enabled)
			{
				Restore(renewLabel, component, renewText);
			}
		}
		if (splitValue != null)
		{
			splitValue.text = SplitText(target);
			atkBonusValue.text = BonusText(target.AtkBonus);
			hpBonusValue.text = BonusText(target.HpBonus);
			oddsLabel.text = OddsText(target);
		}
	}

	/// The game's text again: from the localizer's key when it has one (it may
	/// not have started yet), otherwise `fallback`; then the localizer resumes.
	private static void Restore(UILabel label, UILocalizeEx localizer, string fallback)
	{
		string text = ((localizer != null && localizer.PlatformLink.IsValid()) ? localizer.PlatformLink.GetLocalize() : fallback);
		if (text != null)
		{
			label.text = text;
		}
		if (localizer != null)
		{
			localizer.enabled = true;
		}
	}

	// ---- Rearrange flow ----------------------------------------------------

	/// Replaces the native Rearrange (from the popup or a result's Retry) while
	/// a target is set for this weapon: odds first, then the number of tries.
	public static bool TryStart(SoulWeaponInfo info)
	{
		if (info == null || armed != null)
		{
			return false;
		}
		Target wanted = ((popup != null && slot == info.EquipItemSlotIndex) ? target : Load(info.EquipItemSlotIndex));
		if (wanted.IsAny)
		{
			return false;
		}
		if (busy)
		{
			return true;
		}
		if (Editing)
		{
			CloseEditor();
			Paint();
		}
		NShared.SoulWeaponRenewOption.Request request = new NShared.SoulWeaponRenewOption.Request
		{
			EquipItemSlotIndex = info.EquipItemSlotIndex
		};
		Asks.Add(request, new Ask
		{
			Target = wanted,
			Preview = true
		});
		busy = true;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponRenewOption.Response response)
		{
			busy = false;
			if (response == null)
			{
				return;
			}
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SoulWeaponRenewOption.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (Captured.TryGetValue(response, out var value))
			{
				Confirm(info, wanted, SprkReforgeUntil.Remaining(value.PityMisses, value.PityLimit));
			}
		});
		return true;
	}

	private static Budget ReadBudget(SoulWeaponInfo info)
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SoulWeaponRenewTicketItemIndex");
		ItemInfo itemInfo = instance.ItemManager.Get(valueInt);
		return new Budget
		{
			Tickets = (itemInfo == null) ? 0 : ((int)Math.Min(itemInfo.Count, MaxTries)),
			Gems = instance.UserManager.TotalGem,
			Prices = (GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("SoulWeaponRenewGem") ?? new int[0]),
			RenewCount = info.RenewOptionCount
		};
	}

	private static int Price(Budget budget, int tryIndex)
	{
		return budget.Prices[Math.Min(budget.RenewCount + tryIndex, budget.Prices.Length - 1)];
	}

	/// Mirrors the server: tickets first, then gems at the weapon's price.
	private static int Affordable(Budget budget, int limit)
	{
		long num = budget.Gems;
		int i;
		for (i = 0; i < limit; i++)
		{
			if (i >= budget.Tickets)
			{
				if (budget.Prices.Length == 0)
				{
					break;
				}
				int num2 = Price(budget, i);
				if (num2 > num)
				{
					break;
				}
				num -= num2;
			}
		}
		return i;
	}

	private static long Rubies(Budget budget, int tries)
	{
		long num = 0L;
		for (int i = Math.Min(tries, budget.Tickets); i < tries && budget.Prices.Length != 0; i++)
		{
			num += Price(budget, i);
		}
		return num;
	}

	private static string ConfirmText(Target wanted, double chance, Budget budget, int tries, long remaining)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Describe(wanted));
		stringBuilder.Append("\n").Append(SprkText.Format("{0} per try; chance for {1} attempt(s): {2}", ChanceText(chance), tries, ChanceText(SprkReforgeUntil.Within(chance, tries, remaining))));
		int num = Math.Min(tries, budget.Tickets);
		stringBuilder.Append((num > 0) ? ("\n" + SprkText.Format("Uses at most {0} Rearrange ticket(s) + rubies below", num)) : SprkText.Get("\nUses at most the rubies shown below"));
		stringBuilder.Append(SprkReforgeUntil.GuaranteeLine(remaining));
		return stringBuilder.ToString();
	}

	private static void Confirm(SoulWeaponInfo info, Target wanted, long remaining)
	{
		Budget budget = ReadBudget(info);
		int limit = ((remaining > 0) ? ((int)Math.Min(remaining, MaxTries)) : MaxTries);
		int max = Affordable(budget, limit);
		if (max <= 0)
		{
			MessagePopup.Open(Title, SprkText.Get("Not enough Rearrange tickets or rubies."));
			return;
		}
		double chance = wanted.Chance;
		int initial = ((budget.Tickets > 0) ? Math.Min(budget.Tickets, max) : Mathf.Clamp((int)Math.Ceiling(1.0 / chance), 1, max));
		SprkReforgeUntil.PopupEdit edit = new SprkReforgeUntil.PopupEdit();
		CountMessagePopup countMessagePopup = CountMessagePopup.Open(Title, string.Empty, SprkText.Get("Rearrange"), 1, max, initial, delegate(int tries)
		{
			edit.Restore();
			Run(info, wanted, Mathf.Clamp(tries, 1, max));
		}, delegate
		{
			edit.Restore();
		});
		if (countMessagePopup == null)
		{
			return;
		}
		UILabel uILabel = SprkReforgeUntil.PopupLabel(countMessagePopup, "Panel/Root/Label_Message");
		UILabel uILabel2 = SprkReforgeUntil.PopupLabel(countMessagePopup, "Panel/Root/Value (1)/Label_Price");
		edit.Set(SprkReforgeUntil.PopupLabel(countMessagePopup, "Panel/Root/Title/Label_Title"), Title);
		edit.Set(SprkReforgeUntil.PopupLabel(countMessagePopup, "Panel/Root/Button_Ok/Label_Action"), SprkText.Get("Rearrange"));
		edit.Set(uILabel, ConfirmText(wanted, chance, budget, initial, remaining));
		edit.Set(uILabel2, Rubies(budget, initial).ToString("N0"));
		Transform title = countMessagePopup.transform.Find("Panel/Root/Title/Label_Title");
		Transform button = countMessagePopup.transform.Find("Panel/Root/Button_Ok");
		edit.MoveDown((uILabel == null) ? null : uILabel.transform, title, button, 0.055f);
		edit.MoveDown(countMessagePopup.transform.Find("Panel/Root/Count"), title, button, 0.1f);
		edit.Mirror = countMessagePopup.gameObject.AddComponent<SprkReforgeUntil.PopupMirror>();
		edit.Mirror.Popup = countMessagePopup;
		edit.Mirror.Owner = edit;
		edit.Mirror.Message = uILabel;
		edit.Mirror.Price = uILabel2;
		edit.Mirror.MessageText = (int n) => ConfirmText(wanted, chance, budget, Mathf.Clamp(n, 1, max), remaining);
		edit.Mirror.PriceText = (int n) => Rubies(budget, Mathf.Clamp(n, 1, max)).ToString("N0");
	}

	/// Sends the game's own Rearrange request with the target attached, so its
	/// usual response handling applies the costs and opens the confirmation.
	private static void Run(SoulWeaponInfo info, Target wanted, int tries)
	{
		if (busy)
		{
			return;
		}
		armed = new Ask
		{
			Target = wanted,
			MaxTries = tries
		};
		try
		{
			SingletonAuto<SoulWeaponWebManager>.instance.RequestSoulWeaponRenewOption(info, useCoupon: false);
		}
		finally
		{
			armed = null;
		}
	}

	public static void Arm(NShared.SoulWeaponRenewOption.Request request)
	{
		if (armed != null && request != null)
		{
			Asks.Add(request, armed);
			armed = null;
		}
	}

	public static void Done(NShared.SoulWeaponRenewOption.Response response)
	{
		string text = RunSummary(response);
		if (text != null)
		{
			MessagePopup.Open(Title, text);
		}
	}

	/// The request a preview sends, through the game's own encoder.
	internal static Dictionary<string, object> EncodePreview(int weaponSlot, Target wanted)
	{
		NShared.SoulWeaponRenewOption.Request request = new NShared.SoulWeaponRenewOption.Request
		{
			EquipItemSlotIndex = weaponSlot
		};
		Asks.Add(request, new Ask
		{
			Target = wanted,
			Preview = true
		});
		return JM_NShared_SoulWeaponRenewOption_Request.EncodeJsonObject(request);
	}

	/// What a run reports, or null for a native Rearrange.
	internal static string RunSummary(NShared.SoulWeaponRenewOption.Response response)
	{
		if (response == null || !Captured.TryGetValue(response, out var value) || !value.IsRun)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(SprkText.Format(value.Hit ? "Target reached. Attempts: {0}." : "Target not reached. Attempts: {0}.", value.Tries));
		if (!value.Hit)
		{
			stringBuilder.Append(SprkText.Get("\nThe last roll is shown."));
		}
		List<string> list = new List<string>();
		if (value.TicketsUsed > 0)
		{
			list.Add(SprkText.Format("Rearrange tickets: {0}", value.TicketsUsed));
		}
		if (value.GemsSpent > 0)
		{
			list.Add(SprkText.Format("Rubies: {0}", value.GemsSpent.ToString("N0")));
		}
		if (list.Count > 0)
		{
			stringBuilder.Append(SprkText.Format("\nUsed {0}.", string.Join(", ", list.ToArray())));
		}
		stringBuilder.Append(SprkReforgeUntil.PityResult(value.Hit, value.Guaranteed, value.PityMisses, value.PityLimit));
		return stringBuilder.ToString();
	}
}

/// A horizontal slider over integer values with one handle. `Reversed` puts
/// `Min` at the right end. A plain slider fills from its `Min` end to the
/// handle. A split slider (with `Fill2`) fills red from the left end to the
/// handle and green from the handle to the right end, and `Band` marks the
/// values within `BandWidth` of the handle, unless the slider is `Free` (no
/// value chosen yet).
public sealed class SprkSoulSlider : MonoBehaviour
{
	public int Min;

	public int Max = 1;

	public int Value;

	public bool Reversed;

	public bool Free;

	public float Width = 560f;

	public UISprite Fill;

	public UISprite Fill2;

	public UISprite Band;

	public int BandWidth;

	public Transform Thumb;

	private Action changed;

	public void Configure(int min, int max, Action onChanged)
	{
		Min = min;
		Max = max;
		Value = min;
		changed = onChanged;
		Refresh();
	}

	public float X(int value)
	{
		float num = (float)(value - Min) / (float)(Max - Min);
		if (Reversed)
		{
			num = 1f - num;
		}
		return (0f - Width) / 2f + Width * num;
	}

	public int ValueAt(float localX)
	{
		float num = (localX + Width / 2f) / Width;
		if (Reversed)
		{
			num = 1f - num;
		}
		return Mathf.Clamp(Mathf.RoundToInt((float)Min + num * (float)(Max - Min)), Min, Max);
	}

	/// Shows a value without reporting a change.
	public void Show(int value, bool free)
	{
		Value = Mathf.Clamp(value, Min, Max);
		Free = free;
		Refresh();
	}

	/// The player chose a value.
	public void Set(int value)
	{
		Value = Mathf.Clamp(value, Min, Max);
		Free = false;
		Refresh();
		changed?.Invoke();
	}

	public void Nudge(int delta)
	{
		Set(Value + delta);
	}

	private float TouchX()
	{
		Camera camera = UICamera.currentCamera;
		if (camera == null || UICamera.currentTouch == null)
		{
			return X(Value);
		}
		return base.transform.InverseTransformPoint(camera.ScreenToWorldPoint(UICamera.currentTouch.pos)).x;
	}

	private void OnPress(bool pressed)
	{
		if (pressed)
		{
			Set(ValueAt(TouchX()));
		}
	}

	private void OnDrag(Vector2 delta)
	{
		Set(ValueAt(TouchX()));
	}

	private static void Span(UISprite sprite, float a, float b)
	{
		if (!(sprite == null))
		{
			float num = Mathf.Min(a, b);
			float num2 = Mathf.Max(a, b);
			sprite.transform.localPosition = new Vector3(num, sprite.transform.localPosition.y, 0f);
			sprite.width = Mathf.Max(2, Mathf.RoundToInt(num2 - num));
			sprite.enabled = num2 - num >= 1f;
		}
	}

	public void Refresh()
	{
		float num = X(Value);
		if (Thumb != null)
		{
			Thumb.localPosition = new Vector3(num, 0f, 0f);
		}
		if (Fill2 != null)
		{
			Span(Fill, (0f - Width) / 2f, num);
			Span(Fill2, num, Width / 2f);
			if (Band != null)
			{
				Span(Band, X(Mathf.Clamp(Value - BandWidth, Min, Max)), X(Mathf.Clamp(Value + BandWidth, Min, Max)));
				Band.enabled = Band.enabled && !Free;
			}
		}
		else
		{
			Span(Fill, X(Min), num);
		}
	}
}
