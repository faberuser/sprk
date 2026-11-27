using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.RenewEquipOption;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;
using Math = System.Math;

/// "Reforge until": "Target Stat" in the reforge window shows the item's stat
/// pool (the game builds that list but its layout keeps it hidden); picking a
/// stat makes it the target, and "Change Option" on a stat line then rerolls
/// that line until the target appears. The server runs the rolls (see
/// `renew_until` in the server); the last roll is confirmed through the game's
/// normal reforge result screen.
///
/// Technomagic Reforge sends the same request and gets the same feature: a
/// "Target Stat" button beside its Change Option button opens the pool inside
/// the window's option list, and Change Option on the selected line runs the
/// rolls; the native Technomagic result screen confirms the last one.
public static class SprkReforgeUntil
{
	/// Sends a reforge request through the hosting window's normal flow and
	/// calls back once the response has been applied.
	public delegate void Sender(NShared.RenewEquipOption.Request request, Action<NShared.RenewEquipOption.Response> onApplied);

	private enum HostKind
	{
		None,
		Normal,
		Technomagic
	}

	private sealed class Ask
	{
		public int Target;

		public bool Preview;

		public int MaxTries;
	}

	private sealed class Extras
	{
		public long HitWeight;

		public long TotalWeight;

		public int Tries;

		public int CouponsUsed;

		public long GemsSpent;

		public bool Hit;

		public long PityMisses;

		public long PityLimit;

		public bool Guaranteed;
	}

	private sealed class Entry
	{
		public OperationType Type;

		public int Min;

		public int Max;
	}

	private struct Budget
	{
		public int Coupons;

		public int MaxTries;

		public int[] Prices;

		public int RenewCount;

		public List<KeyValuePair<int, int>> PerTryItems;
	}

	/// Text and localizers changed on the shared, pooled count popup; undone on close.
	public sealed class PopupEdit
	{
		public readonly List<KeyValuePair<UILabel, string>> Texts = new List<KeyValuePair<UILabel, string>>();

		public readonly List<Behaviour> Disabled = new List<Behaviour>();

		public PopupMirror Mirror;

		public readonly List<KeyValuePair<Transform, Vector3>> Moved = new List<KeyValuePair<Transform, Vector3>>();

		public readonly List<KeyValuePair<GameObject, bool>> Shown = new List<KeyValuePair<GameObject, bool>>();

		public void SetActive(GameObject gameObject, bool active)
		{
			if (gameObject != null)
			{
				Shown.Add(new KeyValuePair<GameObject, bool>(gameObject, gameObject.activeSelf));
				gameObject.SetActive(active);
			}
		}

		/// Shifts a part of the popup down by a fraction of the title-to-button distance.
		public void MoveDown(Transform part, Transform title, Transform button, float fraction)
		{
			if (part == null || title == null || button == null)
			{
				return;
			}
			Moved.Add(new KeyValuePair<Transform, Vector3>(part, part.localPosition));
			part.position += new Vector3(0f, (button.position.y - title.position.y) * fraction, 0f);
		}

		public void Set(UILabel label, string text)
		{
			if (label == null)
			{
				return;
			}
			Texts.Add(new KeyValuePair<UILabel, string>(label, label.text));
			MonoBehaviour[] components = label.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in components)
			{
				if (monoBehaviour != null && monoBehaviour.enabled && monoBehaviour.GetType().Name.Contains("Localize"))
				{
					monoBehaviour.enabled = false;
					Disabled.Add(monoBehaviour);
				}
			}
			label.text = text;
		}

		public void Restore()
		{
			PopupMirror mirror = Mirror;
			Mirror = null;
			if (mirror != null)
			{
				// Destroy only takes effect after this frame's LateUpdate, which
				// would otherwise write the reforge texts back once more.
				mirror.Owner = null;
				mirror.Popup = null;
				mirror.enabled = false;
				UnityEngine.Object.Destroy(mirror);
			}
			foreach (KeyValuePair<UILabel, string> text in Texts)
			{
				if (text.Key != null)
				{
					text.Key.text = text.Value;
				}
			}
			foreach (Behaviour item in Disabled)
			{
				if (item != null)
				{
					item.enabled = true;
				}
			}
			foreach (KeyValuePair<Transform, Vector3> item2 in Moved)
			{
				if (item2.Key != null)
				{
					item2.Key.localPosition = item2.Value;
				}
			}
			for (int i = Shown.Count - 1; i >= 0; i--)
			{
				if (Shown[i].Key != null)
				{
					Shown[i].Key.SetActive(Shown[i].Value);
				}
			}
			Texts.Clear();
			Disabled.Clear();
			Moved.Clear();
			Shown.Clear();
		}
	}

	/// Keeps the count popup's message and ruby total in step with the chosen
	/// number of tries. It runs after the popup's own price update, which would
	/// otherwise reset the total to a flat per-item price.
	public sealed class PopupMirror : MonoBehaviour
	{
		public CountMessagePopup Popup;

		public UILabel Message;

		public UILabel Price;

		public Func<int, string> MessageText;

		public Func<int, string> PriceText;

		public PopupEdit Owner;

		// The popup's close button and outside tap close it without the cancel
		// callback, so the edit is undone here whichever way it closes.
		private void OnDisable()
		{
			Owner?.Restore();
		}

		private void LateUpdate()
		{
			if (Popup == null)
			{
				return;
			}
			if (!Popup.IsOpen())
			{
				Owner?.Restore();
				return;
			}
			int number = Popup.GetNumber();
			if (Message != null && MessageText != null)
			{
				string text = MessageText(number);
				if (Message.text != text)
				{
					Message.text = text;
				}
			}
			if (Price != null && PriceText != null)
			{
				string text2 = PriceText(number);
				if (Price.text != text2)
				{
					Price.text = text2;
				}
			}
		}
	}

	private static string Title => SprkText.Get("Reforge Until");

	// The guarantee lands by the 100th try, so a run never needs more (matches the server).
	public const int MaxTries = 100;

	private static readonly ConditionalWeakTable<NShared.RenewEquipOption.Request, Ask> Asks = new ConditionalWeakTable<NShared.RenewEquipOption.Request, Ask>();

	private static readonly ConditionalWeakTable<NShared.RenewEquipOption.Response, Extras> Captured = new ConditionalWeakTable<NShared.RenewEquipOption.Response, Extras>();

	private static readonly List<Entry> Entries = new List<Entry>();

	private static int targetEquipSlot;

	private static OperationType target;

	private static bool explained;

	private static bool explainedOwned;

	private static bool busy;

	private static bool poolView;

	private static EquipOptionRenew window;

	private static UIButton button;

	private static UILabel buttonLabel;

	private static StatGaugeComponent poolPanel;

	private static HostKind host;

	private static ValanceOptionRenewWindow tmWindow;

	private static StatGaugeComponent tmStats;

	private static StatGaugeComponent tmSkills;

	private static StatGaugeComponent tmPool;

	private static UIButton tmButton;

	private static UILabel tmButtonLabel;

	// Where the option list was scrolled when the pool opened, restored on closing.
	private static UIScrollView tmScrollView;

	private static Vector3 tmScrollPosition;

	private static Vector2 tmClipOffset;

	/// Sends a Technomagic skill reroll through the window's normal flow.
	public delegate void SkillSender(NShared.RenewEquipSkill.Request request, Action<NShared.RenewEquipSkill.Response> onApplied);

	/// One additional skill a Technomagic skill line can roll. Several option rows
	/// can show the same skill (type and value); the pool lists it once.
	private sealed class SkillEntry
	{
		public int Option;

		public OperationType Type;

		public int Value;

		public string Text;
	}

	private static readonly List<SkillEntry> SkillEntries = new List<SkillEntry>();

	private static readonly ConditionalWeakTable<NShared.RenewEquipSkill.Request, Ask> SkillAsks = new ConditionalWeakTable<NShared.RenewEquipSkill.Request, Ask>();

	private static readonly ConditionalWeakTable<NShared.RenewEquipSkill.Response, Extras> SkillCaptured = new ConditionalWeakTable<NShared.RenewEquipSkill.Response, Extras>();

	// The targeted skill, as one of its option rows (0 = none).
	private static int skillTarget;

	private static int skillTargetEquipSlot;

	private static bool explainedSkill;

	private static StatGaugeComponent tmSkillPool;

	internal static StatGaugeComponent TechnomagicSkillPool => tmSkillPool;

	internal static int SkillTarget => skillTarget;

	internal static int SkillPoolCount => SkillEntries.Count;

	internal static UIButton TechnomagicButton => tmButton;

	internal static StatGaugeComponent TechnomagicPool => tmPool;

	internal static OperationType Target => target;

	internal static int PoolCount => Entries.Count;

	/// For probes: skip the one-time explanation popups.
	internal static void SkipExplanations()
	{
		explained = true;
		explainedOwned = true;
		explainedSkill = true;
	}

	internal static void ToggleTargetView()
	{
		TogglePoolView();
	}

	public static void Encode(Dictionary<string, object> json, NShared.RenewEquipOption.Request request)
	{
		if (json == null || request == null || !Asks.TryGetValue(request, out var ask))
		{
			return;
		}
		json["SprkTargetOptionType"] = ask.Target.ToString();
		if (ask.Preview)
		{
			json["SprkPreview"] = "1";
		}
		else
		{
			json["SprkMaxTries"] = ask.MaxTries.ToString();
		}
	}

	public static void EncodeSkill(Dictionary<string, object> json, NShared.RenewEquipSkill.Request request)
	{
		if (json == null || request == null || !SkillAsks.TryGetValue(request, out var ask))
		{
			return;
		}
		json["SprkTargetSkillOption"] = ask.Target.ToString();
		if (ask.Preview)
		{
			json["SprkPreview"] = "1";
		}
		else
		{
			json["SprkMaxTries"] = ask.MaxTries.ToString();
		}
	}

	public static void Capture(IDictionary json, NShared.RenewEquipOption.Response response)
	{
		Extras extras = ParseExtras(json);
		if (extras != null && response != null)
		{
			Captured.Remove(response);
			Captured.Add(response, extras);
		}
	}

	public static void CaptureSkill(IDictionary json, NShared.RenewEquipSkill.Response response)
	{
		Extras extras = ParseExtras(json);
		if (extras != null && response != null)
		{
			SkillCaptured.Remove(response);
			SkillCaptured.Add(response, extras);
		}
	}

	private static Extras ParseExtras(IDictionary json)
	{
		if (json == null || (!json.Contains("SprkHitWeight") && !json.Contains("SprkTries")))
		{
			return null;
		}
		return new Extras
		{
			HitWeight = Number(json, "SprkHitWeight"),
			TotalWeight = Number(json, "SprkTotalWeight"),
			Tries = (int)Number(json, "SprkTries"),
			CouponsUsed = (int)Number(json, "SprkCouponsUsed"),
			GemsSpent = Number(json, "SprkGemsSpent"),
			PityMisses = Number(json, "SprkPityMisses"),
			PityLimit = Number(json, "SprkPityLimit"),
			Guaranteed = json.Contains("SprkGuaranteed") && json["SprkGuaranteed"] is bool && (bool)json["SprkGuaranteed"],
			Hit = json.Contains("SprkHit") && json["SprkHit"] is bool && (bool)json["SprkHit"]
		};
	}

	private static long Number(IDictionary json, string key)
	{
		if (!json.Contains(key) || json[key] == null)
		{
			return 0L;
		}
		try
		{
			return Convert.ToInt64(json[key]);
		}
		catch
		{
			return 0L;
		}
	}

	internal static string StatName(OperationType type)
	{
		try
		{
			return GameRoot.LocalizeProxy.GetEnumString(type);
		}
		catch
		{
			return type.ToString();
		}
	}

	public static void ClearTarget()
	{
		target = OperationType.None;
		targetEquipSlot = 0;
		poolView = false;
		Paint();
	}

	/// Called before the reforge window releases its stat panel: the pool view
	/// is a private copy of that panel, so it goes away with it.
	public static void ReleaseGauge()
	{
		poolView = false;
		DestroyPoolPanel();
		if (window != null && window.StatGaugeRoot != null)
		{
			window.StatGaugeRoot.gameObject.SetActive(value: true);
		}
	}

	/// Called when the reforge window rebuilds its (hidden) pool list.
	public static void ReleasePool()
	{
		Entries.Clear();
	}

	public static void AddPoolEntry(OperationType type, int min, int max)
	{
		if (!Entries.Exists((Entry e) => e.Type == type))
		{
			Entries.Add(new Entry
			{
				Type = type,
				Min = min,
				Max = max
			});
		}
	}

	public static void PoolBuilt(EquipOptionRenew renewWindow, int equipSlot)
	{
		window = renewWindow;
		host = HostKind.Normal;
		poolView = false;
		if (target != OperationType.None && (equipSlot != targetEquipSlot || !Entries.Exists((Entry e) => e.Type == target)))
		{
			target = OperationType.None;
			targetEquipSlot = 0;
		}
		EnsureButton();
		Paint();
	}

	private static Transform FindChild(Transform root, string name)
	{
		Transform[] componentsInChildren = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == name)
			{
				return transform;
			}
		}
		return null;
	}

	/// A copy of the game's "Change Option" button, placed in the stat panel's title row.
	private static void EnsureButton()
	{
		if (window == null || window.StatGaugeRoot == null)
		{
			return;
		}
		Transform transform = FindChild(window.StatGaugeRoot, "Button_ConfirmSelect_None");
		if (button == null || button.transform.parent != window.StatGaugeRoot.parent)
		{
			if (transform == null)
			{
				return;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(transform.gameObject, window.StatGaugeRoot.parent, worldPositionStays: true);
			gameObject.name = "SprkReforgeUntil";
			StripBehaviours(gameObject);
			button = gameObject.GetComponent<UIButton>();
			if (button == null)
			{
				button = gameObject.AddComponent<UIButton>();
			}
			button.onClick.Clear();
			EventDelegate.Add(button.onClick, TogglePoolView);
			buttonLabel = gameObject.GetComponentInChildren<UILabel>(includeInactive: true);
			gameObject.SetActive(value: true);
			NGUITools.MarkParentAsChanged(gameObject);
		}
		Transform transform2 = FindChild(window.StatGaugeRoot, "Label_Title");
		if (transform != null && transform2 != null)
		{
			button.transform.position = new Vector3(transform.position.x, transform2.position.y, transform.position.z);
		}
		button.gameObject.SetActive(Entries.Count > 0);
	}

	private static void StripBehaviours(GameObject gameObject)
	{
		MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			if (!(monoBehaviour is UIWidget) && !(monoBehaviour is UIButtonColor) && !(monoBehaviour is UIButtonScale) && !(monoBehaviour is UIPlaySound))
			{
				UnityEngine.Object.DestroyImmediate(monoBehaviour);
			}
		}
	}

	private static EquipItemInfo CurrentItem()
	{
		switch (host)
		{
		case HostKind.Normal:
			return (window == null) ? null : window.SelectedEquipItemInfo;
		case HostKind.Technomagic:
			return (tmWindow == null) ? null : tmWindow.EquipItemInfo;
		default:
			return null;
		}
	}

	private static void TogglePoolView()
	{
		if (busy || CurrentItem() == null)
		{
			return;
		}
		poolView = !poolView && (Entries.Count > 0 || SkillEntries.Count > 0);
		Paint();
	}

	private static SkillEntry SkillOf(int option)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(option);
		if (data == null)
		{
			return null;
		}
		EquipItemInfo item = CurrentItem();
		int value = data.GetOptionValueMax((item == null) ? 5 : item.Star);
		return SkillEntries.Find((SkillEntry e) => e.Option == option || (e.Type == data.OptionType && e.Value == value));
	}

	internal static string SkillName(int option)
	{
		SkillEntry entry = SkillOf(option);
		return (entry == null) ? option.ToString() : entry.Text;
	}

	private static void PickSkill(int option)
	{
		EquipItemInfo item = CurrentItem();
		if (busy || item == null || !poolView)
		{
			return;
		}
		poolView = false;
		if (skillTarget == option && skillTargetEquipSlot == item.SlotIndex)
		{
			skillTarget = 0;
			skillTargetEquipSlot = 0;
			Paint();
			return;
		}
		skillTarget = option;
		skillTargetEquipSlot = item.SlotIndex;
		Paint();
		if (!explainedSkill)
		{
			explainedSkill = true;
			MessagePopup.Open(Title, SprkText.Format("Target: {0}\n\nTap Change Option on an additional skill line to reroll it until this skill appears.\nTo reroll normally again, open the target button and tap {0} again.", SkillName(option)));
		}
	}

	private static void Pick(OperationType type)
	{
		EquipItemInfo item = CurrentItem();
		if (busy || item == null || !poolView)
		{
			return;
		}
		poolView = false;
		if (target == type && targetEquipSlot == item.SlotIndex)
		{
			ClearTarget();
			return;
		}
		target = type;
		targetEquipSlot = item.SlotIndex;
		Paint();
		if (!explainedOwned && HasStat(item, type))
		{
			explainedOwned = true;
			MessagePopup.Open(Title, SprkText.Format("Target: {0}\n\nThis item already has {0}. Tap Change Option on its {0} line to reforge until {0} rolls a higher value (up to its maximum).\nOther lines cannot roll {0}: an item cannot have the same stat twice.", StatName(type)));
		}
		else if (!explained)
		{
			explained = true;
			MessagePopup.Open(Title, SprkText.Format("Target: {0}\n\nTap Change Option on a stat line to reforge it until this stat appears.\nTo reforge normally again, open the target button and tap {0} again.", StatName(target)));
		}
	}

	private static void DestroyPoolPanel()
	{
		if (poolPanel != null)
		{
			poolPanel.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(poolPanel.gameObject);
		}
		poolPanel = null;
	}

	private static void SetStateLabels(StatGaugeInfo row, string none, string selected)
	{
		if (row.StateRoot == null)
		{
			return;
		}
		foreach (StatGaugeInfo.StateRootInfo stateRootInfo in row.StateRoot)
		{
			if (stateRootInfo != null && stateRootInfo.Root != null)
			{
				UILabel componentInChildren = stateRootInfo.Root.GetComponentInChildren<UILabel>(includeInactive: true);
				if (componentInChildren != null)
				{
					componentInChildren.text = ((stateRootInfo.Type == StatGaugeInfo.StateType.Selected) ? selected : none);
				}
			}
		}
	}

	/// The stat pool as rows of the game's own stat panel (name, "+min / +max",
	/// and a Change-Option-style button), in a private copy of that panel with
	/// the rows inside a clipped, scrollable area.
	private static void BuildPoolPanel()
	{
		DestroyPoolPanel();
		StatGaugeComponent statGaugeComponent = ((window == null || window.StatGaugeRoot == null) ? null : window.StatGaugeRoot.GetComponentInChildren<StatGaugeComponent>(includeInactive: true));
		if (statGaugeComponent == null || statGaugeComponent.GaugeGrid == null || statGaugeComponent.GaugeTemplate == null || statGaugeComponent.Sprite_BG == null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(statGaugeComponent.gameObject, window.StatGaugeRoot.parent, worldPositionStays: true);
		gameObject.name = "SprkReforgeUntilPool";
		poolPanel = gameObject.GetComponent<StatGaugeComponent>();
		// Localizers would re-apply "Current Stats"/"Change Option" a frame later;
		// this copy (template included) is ours, so drop them before relabelling.
		MonoBehaviour[] behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour behaviour in behaviours)
		{
			if (behaviour != null && behaviour.GetType().Name.Contains("Localize"))
			{
				UnityEngine.Object.DestroyImmediate(behaviour);
			}
		}
		// The copy carries the window's current rows; keep only the template.
		StatGaugeInfo[] componentsInChildren = poolPanel.GaugeGrid.GetComponentsInChildren<StatGaugeInfo>(includeInactive: true);
		foreach (StatGaugeInfo statGaugeInfo in componentsInChildren)
		{
			if (statGaugeInfo != poolPanel.GaugeTemplate)
			{
				UnityEngine.Object.DestroyImmediate(statGaugeInfo.gameObject);
			}
		}
		Transform titleTransform = FindChild(gameObject.transform, "Label_Title");
		UILabel titleLabel = ((titleTransform == null) ? null : titleTransform.GetComponent<UILabel>());
		if (titleLabel != null)
		{
			titleLabel.text = SprkText.Get("Stat Pool");
		}
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(window.StatGaugeRoot.gameObject);
		GameObject gameObject2 = new GameObject("Scroll");
		gameObject2.layer = gameObject.layer;
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(gameObject2.transform, poolPanel.Sprite_BG.transform);
		UIPanel uIPanel2 = gameObject2.AddComponent<UIPanel>();
		uIPanel2.depth = ((uIPanel == null) ? 10 : (uIPanel.depth + 1));
		uIPanel2.sortingOrder = ((!(uIPanel == null)) ? uIPanel.sortingOrder : 0);
		uIPanel2.clipping = UIDrawCall.Clipping.SoftClip;
		uIPanel2.clipSoftness = new Vector2(0f, 12f);
		uIPanel2.baseClipRegion = new Vector4(bounds.center.x, bounds.center.y, bounds.size.x - 8f, bounds.size.y - 12f);
		UIScrollView uIScrollView = gameObject2.AddComponent<UIScrollView>();
		uIScrollView.movement = UIScrollView.Movement.Vertical;
		uIScrollView.dragEffect = UIScrollView.DragEffect.MomentumAndSpring;
		uIScrollView.restrictWithinPanel = true;
		uIScrollView.disableDragIfFits = true;
		// An invisible backdrop catches drags and the mouse wheel between buttons.
		GameObject gameObject3 = new GameObject("Backdrop");
		gameObject3.layer = gameObject.layer;
		gameObject3.transform.SetParent(gameObject.transform, worldPositionStays: false);
		// Its own panel at the list's depth puts it above the window's background
		// colliders (which would otherwise take the drag/wheel) and below the rows.
		UIPanel backdropPanel = gameObject3.AddComponent<UIPanel>();
		backdropPanel.depth = uIPanel2.depth;
		backdropPanel.sortingOrder = uIPanel2.sortingOrder;
		UIWidget uIWidget = gameObject3.AddComponent<UIWidget>();
		uIWidget.depth = -100;
		uIWidget.width = Mathf.Max(2, Mathf.RoundToInt(bounds.size.x));
		uIWidget.height = Mathf.Max(2, Mathf.RoundToInt(bounds.size.y));
		gameObject3.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, 0f);
		NGUITools.AddWidgetCollider(gameObject3);
		gameObject3.AddComponent<UIDragScrollView>().scrollView = uIScrollView;
		poolPanel.GaugeGrid.transform.SetParent(gameObject2.transform, worldPositionStays: true);
		foreach (Entry entry in Entries)
		{
			OperationType type = entry.Type;
			poolPanel.AddStat(type, entry.Min, entry.Max, 0, 1, "/", delegate
			{
				Pick(type);
			});
		}
		for (int j = 0; j < Entries.Count; j++)
		{
			StatGaugeInfo statGaugeInfo2 = poolPanel.GetStatGaugeInfo(j);
			if (!(statGaugeInfo2 == null))
			{
				bool flag = Entries[j].Type == target;
				SetStateLabels(statGaugeInfo2, SprkText.Get("Select"), SprkText.Get("Target"));
				statGaugeInfo2.SelectRoot(flag ? StatGaugeInfo.StateType.Selected : StatGaugeInfo.StateType.None);
				Collider[] componentsInChildren2 = statGaugeInfo2.GetComponentsInChildren<Collider>(includeInactive: true);
				foreach (Collider collider in componentsInChildren2)
				{
					UIDragScrollView uIDragScrollView = collider.GetComponent<UIDragScrollView>();
					if (uIDragScrollView == null)
					{
						uIDragScrollView = collider.gameObject.AddComponent<UIDragScrollView>();
					}
					uIDragScrollView.scrollView = uIScrollView;
				}
			}
		}
		poolPanel.GaugeGrid.Reposition();
		NGUITools.MarkParentAsChanged(gameObject);
		uIScrollView.ResetPosition();
		// Open with the current target in view (one row of context above it).
		int targetRow = Entries.FindIndex((Entry e) => e.Type == target);
		if (targetRow > 1 && poolPanel.GaugeGrid.cellHeight > 0f)
		{
			uIScrollView.MoveRelative(new Vector3(0f, poolPanel.GaugeGrid.cellHeight * (float)(targetRow - 1), 0f));
			uIScrollView.RestrictWithinBounds(instant: true);
		}
	}

	private static void Paint()
	{
		if (host == HostKind.Technomagic)
		{
			PaintTechnomagic();
			return;
		}
		if (window == null)
		{
			return;
		}
		if (poolView)
		{
			BuildPoolPanel();
		}
		else
		{
			DestroyPoolPanel();
		}
		if (window.StatGaugeRoot != null)
		{
			window.StatGaugeRoot.gameObject.SetActive(!poolView || poolPanel == null);
		}
		if (buttonLabel != null)
		{
			buttonLabel.text = SprkText.Get((poolView ? "Back" : ((target != OperationType.None) ? (SprkText.Format("Target:\n{0}", StatName(target))) : "Target\nStat")));
		}
	}

	/// Technomagic Reforge built its stat panels for `EquipItemInfo` (or none).
	/// Its pool is the item's option list plus option groups, as the server rolls them.
	public static void TechnomagicBuilt(ValanceOptionRenewWindow renewWindow, StatGaugeComponent stats, StatGaugeComponent skills)
	{
		if (renewWindow == null)
		{
			return;
		}
		if (tmWindow != renewWindow)
		{
			TechnomagicReleasing(tmWindow);
		}
		tmWindow = renewWindow;
		tmStats = stats;
		tmSkills = skills;
		host = HostKind.Technomagic;
		poolView = false;
		Entries.Clear();
		EquipItemInfo item = renewWindow.EquipItemInfo;
		ItemData<EquipItemData> data = (item == null) ? null : GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(item.ItemIndex);
		if (data != null && data.SubData != null && stats != null)
		{
			if (data.SubData.OptionIndex != null)
			{
				foreach (int optionIndex in data.SubData.OptionIndex)
				{
					AddTechnomagicEntry(optionIndex, item.Star);
				}
			}
			if (data.SubData.OptionGroupIndex != null)
			{
				foreach (int groupIndex in data.SubData.OptionGroupIndex)
				{
					EquipOptionGroupData group = GameRoot.Instance.EquipOptionGroupDataContainer.GetData(groupIndex);
					if (group != null && group.OptionIndex != null)
					{
						foreach (int optionIndex2 in group.OptionIndex)
						{
							AddTechnomagicEntry(optionIndex2, item.Star);
						}
					}
				}
			}
		}
		if (target != OperationType.None && (item == null || item.SlotIndex != targetEquipSlot || !Entries.Exists((Entry e) => e.Type == target)))
		{
			target = OperationType.None;
			targetEquipSlot = 0;
		}
		// The skill panel exists only for identified items, whose skill lines can be rerolled.
		SkillEntries.Clear();
		if (data != null && data.SubData != null && skills != null)
		{
			List<int> indices = new List<int>(data.SubData.UniqueOptionIndex ?? new int[0]);
			foreach (int groupIndex in data.SubData.UniqueOptionGroupIndex ?? new int[0])
			{
				EquipOptionGroupData group = GameRoot.Instance.EquipOptionGroupDataContainer.GetData(groupIndex);
				if (group != null && group.OptionIndex != null)
				{
					indices.AddRange(group.OptionIndex);
				}
			}
			foreach (int optionIndex in indices)
			{
				AddSkillEntry(optionIndex, item.Star);
			}
		}
		if (skillTarget != 0 && (item == null || item.SlotIndex != skillTargetEquipSlot || SkillOf(skillTarget) == null))
		{
			skillTarget = 0;
			skillTargetEquipSlot = 0;
		}
		EnsureTechnomagicButton();
		Paint();
	}

	private static void AddSkillEntry(int optionIndex, int star)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null || data.Ratio <= 0)
		{
			return;
		}
		int value = data.GetOptionValueMax(star);
		if (SkillEntries.Exists((SkillEntry e) => e.Type == data.OptionType && e.Value == value))
		{
			return;
		}
		SkillEntries.Add(new SkillEntry
		{
			Option = optionIndex,
			Type = data.OptionType,
			Value = value,
			Text = StatData.GetOptionDisplay(data.OptionType, value, opened: true, 0, appendColor: false)
		});
	}

	private static void AddTechnomagicEntry(int optionIndex, int star)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data != null && data.Ratio > 0)
		{
			AddPoolEntry(data.OptionType, data.GetOptionValueMin(star), data.GetOptionValueMax(star));
		}
	}

	/// Called before Technomagic Reforge releases its stat panels: the pool view
	/// hides them and must hand them back visible.
	public static void TechnomagicReleasing(ValanceOptionRenewWindow renewWindow)
	{
		if (renewWindow == null || renewWindow != tmWindow)
		{
			return;
		}
		poolView = false;
		DestroyTechnomagicPool();
		tmStats = null;
		tmSkills = null;
		Entries.Clear();
		SkillEntries.Clear();
		if (tmButton != null)
		{
			tmButton.gameObject.SetActive(value: false);
		}
	}

	public static void TechnomagicClosing(ValanceOptionRenewWindow renewWindow)
	{
		if (renewWindow == null || renewWindow != tmWindow)
		{
			return;
		}
		TechnomagicReleasing(renewWindow);
		target = OperationType.None;
		targetEquipSlot = 0;
		skillTarget = 0;
		skillTargetEquipSlot = 0;
		tmWindow = null;
		host = HostKind.None;
	}

	/// A copy of the window's "Change Option" button, above it.
	private static void EnsureTechnomagicButton()
	{
		UIButton renew = (tmWindow == null) ? null : tmWindow.Button_Renew;
		if (renew == null)
		{
			return;
		}
		if (tmButton == null || tmButton.transform.parent != renew.transform.parent)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(renew.gameObject, renew.transform.parent, worldPositionStays: false);
			gameObject.name = "SprkReforgeUntilTarget";
			// Drop localizers (they would restore "Change Option") and the event ribbon.
			foreach (MonoBehaviour behaviour in gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
			{
				if (behaviour != null && behaviour.GetType().Name.Contains("Localize"))
				{
					UnityEngine.Object.DestroyImmediate(behaviour);
				}
			}
			Transform ribbon = gameObject.transform.Find("EventButtonImage");
			if (ribbon != null)
			{
				UnityEngine.Object.DestroyImmediate(ribbon.gameObject);
			}
			tmButton = gameObject.GetComponent<UIButton>();
			tmButton.onClick.Clear();
			EventDelegate.Add(tmButton.onClick, TogglePoolView);
			tmButtonLabel = gameObject.GetComponentInChildren<UILabel>(includeInactive: true);
			if (tmButtonLabel != null)
			{
				tmButtonLabel.overflowMethod = UILabel.Overflow.ShrinkContent;
			}
			float height = NGUIMath.CalculateRelativeWidgetBounds(renew.transform).size.y;
			gameObject.transform.localPosition = renew.transform.localPosition + new Vector3(0f, height + 12f, 0f);
		}
		// Change Option is disabled until a line is selected; this button is not. A copy
		// made while Change Option was disabled took its gray tint and sprite as "normal"
		// (NGUI reads them from the widget on init), so restore Change Option's own.
		foreach (UIButtonColor color in tmButton.GetComponentsInChildren<UIButtonColor>(includeInactive: true))
		{
			Transform source = (color.transform == tmButton.transform) ? renew.transform : renew.transform.Find(PathFrom(tmButton.transform, color.transform));
			UIButtonColor original = (source == null) ? null : source.GetComponent<UIButtonColor>();
			if (original != null)
			{
				color.defaultColor = original.defaultColor;
				UIButton copyButton = color as UIButton;
				UIButton originalButton = original as UIButton;
				if (copyButton != null && originalButton != null && !string.IsNullOrEmpty(originalButton.normalSprite))
				{
					copyButton.normalSprite = originalButton.normalSprite;
				}
			}
		}
		foreach (UIWidget widget in tmButton.GetComponentsInChildren<UIWidget>(includeInactive: true))
		{
			widget.isGrayscale = false;
		}
		tmButton.isEnabled = true;
		tmButton.gameObject.SetActive(tmWindow.EquipItemInfo != null && tmStats != null && (Entries.Count > 0 || SkillEntries.Count > 0));
		if (tmButton.gameObject.activeInHierarchy)
		{
			foreach (UIButtonColor color2 in tmButton.GetComponentsInChildren<UIButtonColor>(includeInactive: true))
			{
				color2.SetState(UIButtonColor.State.Normal, instant: true);
			}
		}
		NGUITools.MarkParentAsChanged(tmButton.gameObject);
	}

	private static string PathFrom(Transform root, Transform child)
	{
		string path = child.name;
		for (Transform parent = child.parent; parent != null && parent != root; parent = parent.parent)
		{
			path = parent.name + "/" + path;
		}
		return path;
	}

	private static void PaintTechnomagic()
	{
		if (tmWindow == null)
		{
			return;
		}
		if (poolView)
		{
			BuildTechnomagicPool();
		}
		else
		{
			DestroyTechnomagicPool();
		}
		if (tmButtonLabel != null)
		{
			string text;
			if (poolView)
			{
				text = SprkText.Get("Back");
			}
			else if (target != OperationType.None && skillTarget != 0)
			{
				text = SprkText.Format("Targets: {0}", 2);
			}
			else if (target != OperationType.None)
			{
				text = SprkText.Format("Target:\n{0}", StatName(target));
			}
			else if (skillTarget != 0)
			{
				text = SprkText.Format("Target:\n{0}", SkillName(skillTarget));
			}
			else
			{
				text = SprkText.Get("Target\nStat");
			}
			tmButtonLabel.text = text.Replace("\n", " ");
		}
	}

	private static UITable TechnomagicTable()
	{
		return (tmStats == null || tmStats.transform.parent == null) ? null : tmStats.transform.parent.GetComponent<UITable>();
	}

	private static void RestoreTechnomagicScroll()
	{
		UIScrollView scrollView = tmScrollView;
		tmScrollView = null;
		if (scrollView == null)
		{
			return;
		}
		scrollView.DisableSpring();
		scrollView.currentMomentum = Vector3.zero;
		scrollView.transform.localPosition = tmScrollPosition;
		if (scrollView.panel != null)
		{
			scrollView.panel.clipOffset = tmClipOffset;
		}
		scrollView.UpdateScrollbars(recalculateBounds: true);
	}

	private static void DestroyTechnomagicPool()
	{
		bool shown = tmPool != null || tmSkillPool != null;
		if (tmPool != null)
		{
			tmPool.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(tmPool.gameObject);
		}
		tmPool = null;
		if (tmSkillPool != null)
		{
			tmSkillPool.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(tmSkillPool.gameObject);
		}
		tmSkillPool = null;
		if (tmStats != null)
		{
			tmStats.gameObject.SetActive(value: true);
		}
		if (tmSkills != null)
		{
			tmSkills.gameObject.SetActive(value: true);
		}
		if (shown)
		{
			UITable table = TechnomagicTable();
			if (table != null)
			{
				table.Reposition();
			}
			RestoreTechnomagicScroll();
		}
	}

	/// The stat pool as rows of the window's own stat panel ("+min / +max"), in a
	/// copy of that panel placed in the window's scrolling option list, with the
	/// item's stat and skill panels hidden meanwhile.
	/// A copy of one of the window's panels in its option list, emptied of rows.
	private static StatGaugeComponent CopyPanel(StatGaugeComponent source, UITable table, string name, string title)
	{
		if (source == null || source.GaugeGrid == null || source.GaugeTemplate == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(source.gameObject, table.transform, worldPositionStays: false);
		gameObject.name = name;
		StatGaugeComponent copy = gameObject.GetComponent<StatGaugeComponent>();
		foreach (StatGaugeInfo row in copy.GaugeGrid.GetComponentsInChildren<StatGaugeInfo>(includeInactive: true))
		{
			if (row != copy.GaugeTemplate)
			{
				UnityEngine.Object.DestroyImmediate(row.gameObject);
			}
		}
		copy.SetTitle(title);
		return copy;
	}

	private static void FinishPoolRows(StatGaugeComponent panel, int count, Func<int, bool> selected, UIScrollView scrollView)
	{
		for (int i = 0; i < count; i++)
		{
			StatGaugeInfo row = panel.GetStatGaugeInfo(i);
			if (row == null)
			{
				continue;
			}
			row.SelectRoot(selected(i) ? StatGaugeInfo.StateType.Selected : StatGaugeInfo.StateType.None);
			if (scrollView != null)
			{
				foreach (Collider collider in row.GetComponentsInChildren<Collider>(includeInactive: true))
				{
					UIDragScrollView drag = collider.GetComponent<UIDragScrollView>();
					if (drag == null)
					{
						drag = collider.gameObject.AddComponent<UIDragScrollView>();
					}
					drag.scrollView = scrollView;
				}
			}
		}
		panel.gameObject.SetActive(value: true);
		panel.GaugeGrid.Reposition();
	}

	/// The stat pool and the skill pool as rows of the window's own panels, in
	/// copies placed in the window's scrolling option list, with the item's stat
	/// and skill panels hidden meanwhile.
	private static void BuildTechnomagicPool()
	{
		DestroyTechnomagicPool();
		UITable table = TechnomagicTable();
		if (table == null)
		{
			return;
		}
		UIScrollView scrollView = NGUITools.FindInParents<UIScrollView>(table.gameObject);
		if (Entries.Count > 0)
		{
			tmPool = CopyPanel(tmStats, table, "SprkReforgeUntilPool", SprkText.Get("Stat Pool"));
		}
		if (tmPool != null)
		{
			foreach (Entry entry in Entries)
			{
				OperationType type = entry.Type;
				tmPool.AddStat(type, entry.Min, entry.Max, 0, 1, "/", delegate
				{
					Pick(type);
				});
			}
			FinishPoolRows(tmPool, Entries.Count, (int i) => Entries[i].Type == target, scrollView);
		}
		if (SkillEntries.Count > 0)
		{
			tmSkillPool = CopyPanel(tmSkills, table, "SprkReforgeUntilSkillPool", SprkText.Get("Skill Pool"));
		}
		if (tmSkillPool != null)
		{
			SkillEntry wanted = (skillTarget == 0) ? null : SkillOf(skillTarget);
			foreach (SkillEntry entry2 in SkillEntries)
			{
				int option = entry2.Option;
				tmSkillPool.AddExtraSkill(entry2.Text, delegate
				{
					PickSkill(option);
				});
			}
			FinishPoolRows(tmSkillPool, SkillEntries.Count, (int i) => SkillEntries[i] == wanted, scrollView);
		}
		if (tmPool == null && tmSkillPool == null)
		{
			return;
		}
		if (scrollView != null)
		{
			tmScrollView = scrollView;
			tmScrollPosition = scrollView.transform.localPosition;
			tmClipOffset = (scrollView.panel == null) ? Vector2.zero : scrollView.panel.clipOffset;
		}
		tmStats.gameObject.SetActive(value: false);
		if (tmSkills != null)
		{
			tmSkills.gameObject.SetActive(value: false);
		}
		table.Reposition();
		NGUITools.MarkParentAsChanged(table.gameObject);
		if (scrollView != null)
		{
			// Opens where the item's panels were (top); the stat target in view (one row of context above it).
			scrollView.UpdateScrollbars(recalculateBounds: true);
			int targetRow = Entries.FindIndex((Entry e) => e.Type == target);
			if (tmPool != null && targetRow > 1 && tmPool.GaugeGrid.cellHeight > 0f)
			{
				scrollView.MoveRelative(new Vector3(0f, tmPool.GaugeGrid.cellHeight * (float)(targetRow - 1), 0f));
				scrollView.RestrictWithinBounds(instant: true);
			}
		}
	}

	/// Replaces the normal reforge when a target is selected for this item.
	public static bool TryStart(BlacksmithManagement manager, int equipSlot, int optionSlot)
	{
		return manager != null && host == HostKind.Normal && TryStart(manager.SprkSendRenewEquipOption, equipSlot, optionSlot);
	}

	/// Replaces the Technomagic reforge when a target is selected for this item.
	public static bool TryStartTechnomagic(ValanceWebManager manager, int equipSlot, int optionSlot)
	{
		return manager != null && host == HostKind.Technomagic && TryStart(manager.SprkSendValanceOptionRenew, equipSlot, optionSlot);
	}

	/// True when Change Option on this Technomagic item runs "reforge until"
	/// (the window then skips its own confirmation; the count popup asks instead).
	public static bool IsTechnomagicArmed(int equipSlot)
	{
		return host == HostKind.Technomagic && target != OperationType.None && targetEquipSlot == equipSlot;
	}

	private static bool TryStart(Sender send, int equipSlot, int optionSlot)
	{
		if (target == OperationType.None || targetEquipSlot != equipSlot || send == null)
		{
			return false;
		}
		if (busy)
		{
			return true;
		}
		OperationType wanted = target;
		EquipItemInfo item = CurrentItem();
		string blocked = BlockedReason((item != null && item.SlotIndex == equipSlot) ? item : null, optionSlot, wanted);
		if (blocked != null)
		{
			MessagePopup.Open(Title, blocked);
			return true;
		}
		NShared.RenewEquipOption.Request request = new NShared.RenewEquipOption.Request
		{
			EquipItemSlotIndex = equipSlot,
			OptionSlotIndex = optionSlot
		};
		Asks.Add(request, new Ask
		{
			Target = (int)wanted,
			Preview = true
		});
		busy = true;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RenewEquipOption.Response response)
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
			else if (response.Result == ResultType.NoAvailableOption)
			{
				MessagePopup.Open(Title, SprkText.Format("{0} cannot appear on this line.", StatName(wanted)));
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (Captured.TryGetValue(response, out var value) && value.TotalWeight > 0 && value.HitWeight > 0)
			{
				Confirm(send, equipSlot, optionSlot, wanted, (double)value.HitWeight / (double)value.TotalWeight, Remaining(value.PityMisses, value.PityLimit));
			}
		});
		return true;
	}

	/// An item never holds the same stat twice. A stat it already has can only
	/// be targeted on its own line, where rolling it again raises its value, so
	/// that line must not already be at the maximum. The server enforces the
	/// same rules; this explains them before anything is charged.
	private static string BlockedReason(EquipItemInfo item, int optionSlot, OperationType wanted)
	{
		if (item == null)
		{
			return null;
		}
		foreach (EquipItemOptionInfo info in item.GetOptionInfoList())
		{
			EquipOptionData data = (info == null) ? null : GameRoot.Instance.EquipOptionDataContainer.GetData(info.OptionIndex);
			if (data == null || data.OptionType != wanted)
			{
				continue;
			}
			if (info.SlotIndex != optionSlot)
			{
				return SprkText.Format("{0} is already on another line, and an item cannot have the same stat twice.\nReforge that line to raise {0}, or pick another target.", StatName(wanted));
			}
			int step = Mathf.Min(info.OptionStep, data.OptionValueStep);
			if (data.GetOptionValue(item.Star, step) >= data.GetOptionValueMax(item.Star))
			{
				return SprkText.Format("{0} on this line is already at its maximum value.", StatName(wanted));
			}
		}
		return null;
	}

	private static bool HasStat(EquipItemInfo item, OperationType type)
	{
		if (item == null)
		{
			return false;
		}
		foreach (EquipItemOptionInfo info in item.GetOptionInfoList())
		{
			EquipOptionData data = (info == null) ? null : GameRoot.Instance.EquipOptionDataContainer.GetData(info.OptionIndex);
			if (data != null && data.OptionType == type)
			{
				return true;
			}
		}
		return false;
	}

	private static Budget ReadBudget(EquipItemInfo item, int optionSlot)
	{
		Budget result = new Budget
		{
			PerTryItems = new List<KeyValuePair<int, int>>(),
			Prices = new int[0]
		};
		EquipItemData equipItemData = item.EquipItemData;
		EquipItemDetailData equipItemDetailData = ((equipItemData == null) ? null : GameRoot.Instance.EquipItemDetailDataContainer.GetData(equipItemData.DetailIndex));
		if (equipItemDetailData == null)
		{
			return result;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		result.Coupons = ((equipItemDetailData.RenewTicketItemIndex > 0) ? Count(equipItemDetailData.RenewTicketItemIndex) : 0);
		bool flag = equipItemDetailData.RenewCurrencyType == CurrencyType.Gem && equipItemDetailData.RenewPrice != null && equipItemDetailData.RenewPrice.Length != 0;
		if (flag)
		{
			result.Prices = equipItemDetailData.RenewPrice;
		}
		result.RenewCount = item.GetOptionRenewCount(optionSlot);
		int[] array = new int[3] { equipItemDetailData.RenewItemIndex1, equipItemDetailData.RenewItemIndex2, equipItemDetailData.RenewItemIndex3 };
		int[] array2 = new int[3] { equipItemDetailData.RenewItemCount1, equipItemDetailData.RenewItemCount2, equipItemDetailData.RenewItemCount3 };
		for (int i = 0; i < 3; i++)
		{
			if (array2[i] > 0)
			{
				result.PerTryItems.Add(new KeyValuePair<int, int>(array[i], array2[i]));
			}
		}
		// Mirrors the server: coupon first, then rubies at the escalating price;
		// per-try items are always charged.
		long num = (flag ? instance.UserManager.TotalGem : 0);
		int num2 = 0;
		while (num2 < MaxTries)
		{
			bool flag2 = true;
			foreach (KeyValuePair<int, int> perTryItem in result.PerTryItems)
			{
				if ((long)Count(perTryItem.Key) / (long)perTryItem.Value <= num2)
				{
					flag2 = false;
				}
			}
			if (!flag2)
			{
				break;
			}
			if (num2 >= result.Coupons)
			{
				if (flag)
				{
					int num3 = Price(result, num2);
					if (num3 > num)
					{
						break;
					}
					num -= num3;
				}
				else if (equipItemDetailData.RenewTicketItemIndex > 0 || result.PerTryItems.Count == 0)
				{
					break;
				}
			}
			num2++;
		}
		result.MaxTries = num2;
		return result;
	}

	private static int Price(Budget budget, int tryIndex)
	{
		int num = budget.RenewCount + tryIndex;
		return budget.Prices[Math.Min(num, budget.Prices.Length - 1)];
	}

	private static long Rubies(Budget budget, int tries)
	{
		long num = 0L;
		for (int i = Math.Min(tries, budget.Coupons); i < tries && budget.Prices.Length != 0; i++)
		{
			num += Price(budget, i);
		}
		return num;
	}

	private static int Count(int itemIndex)
	{
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(itemIndex);
		return (itemInfo != null) ? ((int)itemInfo.Count) : 0;
	}

	/// Tries left until the guarantee, counting the next try as 1 (0 = unknown).
	internal static long Remaining(long misses, long limit)
	{
		return (limit <= 0) ? 0 : Math.Max(1L, limit - Math.Max(0L, misses));
	}

	/// Chance to hit within `tries`, certain once the guarantee falls inside them.
	internal static double Within(double chance, int tries, long remaining)
	{
		return (remaining > 0 && tries >= remaining) ? 1.0 : (1.0 - Math.Pow(1.0 - chance, tries));
	}

	internal static string GuaranteeLine(long remaining)
	{
		return (remaining > 0) ? SprkText.Format("\nGuaranteed by attempt {0}.", remaining) : string.Empty;
	}

	/// Summary line: a guaranteed hit, or the progress kept for the next run.
	internal static string PityResult(bool hit, bool guaranteed, long misses, long limit)
	{
		if (hit)
		{
			return guaranteed ? SprkText.Get("\nGuaranteed by the attempt limit.") : string.Empty;
		}
		return (limit > 0) ? SprkText.Format("\nGuarantee progress: {0}/{1}.", misses, limit) : string.Empty;
	}

	private static string Describe(string wanted, double chance, Budget budget, int tries, long remaining)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(SprkText.Format("{0}: {1} per try; chance for {2} attempt(s): {3}",
			wanted, Percent(chance), tries, Percent(Within(chance, tries, remaining))));
		List<string> list = new List<string>();
		int num = Math.Min(tries, budget.Coupons);
		if (num > 0)
		{
			list.Add(SprkText.Format("Coupons: {0}", num));
		}
		foreach (KeyValuePair<int, int> perTryItem in budget.PerTryItems)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(perTryItem.Key);
			list.Add(((data == null) ? perTryItem.Key.ToString() : LocalizationManager.GetStringDirect(data.Name)) + " ×" + ((long)perTryItem.Value * (long)tries).ToString("N0"));
		}
		if (list.Count > 0)
		{
			stringBuilder.Append(SprkText.Format("\nUses at most {0}", string.Join(", ", list.ToArray())));
			if (budget.Prices.Length != 0)
			{
				stringBuilder.Append(SprkText.Get(" + rubies shown below"));
			}
		}
		else if (budget.Prices.Length != 0)
		{
			stringBuilder.Append(SprkText.Get("\nUses at most the rubies shown below"));
		}
		stringBuilder.Append(GuaranteeLine(remaining));
		return stringBuilder.ToString();
	}

	internal static string Percent(double value)
	{
		value *= 100.0;
		if (value >= 99.95)
		{
			return (value >= 100.0) ? "100%" : "~100%";
		}
		return ((value < 10.0) ? value.ToString("0.0") : value.ToString("0")) + "%";
	}

	internal static UILabel PopupLabel(CountMessagePopup popup, string path)
	{
		Transform transform = popup.transform.Find(path);
		return (transform == null) ? null : transform.GetComponent<UILabel>();
	}

	private static void Confirm(Sender send, int equipSlot, int optionSlot, OperationType wanted, double chance, long remaining)
	{
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipSlot);
		if (equipItemInfo == null)
		{
			return;
		}
		Budget budget = ReadBudget(equipItemInfo, optionSlot);
		OpenConfirm(StatName(wanted), budget, chance, remaining, SprkText.Get("Not enough reforge coupons or rubies."), delegate(int tries)
		{
			Run(send, equipSlot, optionSlot, wanted, tries);
		});
	}

	/// The count popup for a run: tries, odds and costs; `run` gets the chosen tries.
	private static void OpenConfirm(string wanted, Budget budget, double chance, long remaining, string notEnough, Action<int> run)
	{
		// Tries past the guarantee can never be used: the run stops at the hit.
		if (remaining > 0 && budget.MaxTries > remaining)
		{
			budget.MaxTries = (int)remaining;
		}
		if (budget.MaxTries <= 0)
		{
			MessagePopup.Open(Title, notEnough);
			return;
		}
		int num = ((budget.Coupons > 0) ? Math.Min(budget.Coupons, budget.MaxTries) : Math.Min(budget.MaxTries, Math.Max(1, (int)Math.Ceiling(1.0 / chance))));
		PopupEdit edit = new PopupEdit();
		CountMessagePopup popup = CountMessagePopup.Open(Title, string.Empty, SprkText.Get("Reforge"), 1, budget.MaxTries, num, delegate(int tries)
		{
			edit.Restore();
			run(Mathf.Clamp(tries, 1, budget.MaxTries));
		}, delegate
		{
			edit.Restore();
		});
		if (popup == null)
		{
			return;
		}
		// This prefab's script fields for these labels are unset, so the texts
		// are placed directly on the shared popup and restored when it closes.
		UILabel message = PopupLabel(popup, "Panel/Root/Label_Message");
		UILabel price = PopupLabel(popup, "Panel/Root/Value (1)/Label_Price");
		edit.Set(PopupLabel(popup, "Panel/Root/Title/Label_Title"), Title);
		edit.Set(PopupLabel(popup, "Panel/Root/Button_Ok/Label_Action"), SprkText.Get("Reforge"));
		edit.Set(message, Describe(wanted, chance, budget, num, remaining));
		edit.Set(price, Rubies(budget, num).ToString("N0"));
		// Items without a ruby price (Technomagic) hide the always-zero ruby total.
		if (budget.Prices.Length == 0)
		{
			Transform value = popup.transform.Find("Panel/Root/Value (1)");
			edit.SetActive((value == null) ? null : value.gameObject, active: false);
		}
		// Even spacing between title, message, count row, Max and the buttons
		// (the prefab is laid out for a one-line message).
		Transform title = popup.transform.Find("Panel/Root/Title/Label_Title");
		Transform ok = popup.transform.Find("Panel/Root/Button_Ok");
		edit.MoveDown((message == null) ? null : message.transform, title, ok, 0.055f);
		edit.MoveDown(popup.transform.Find("Panel/Root/Count"), title, ok, 0.1f);
		edit.Mirror = popup.gameObject.AddComponent<PopupMirror>();
		edit.Mirror.Popup = popup;
		edit.Mirror.Owner = edit;
		edit.Mirror.Message = message;
		edit.Mirror.Price = price;
		edit.Mirror.MessageText = (int n) => Describe(wanted, chance, budget, Mathf.Clamp(n, 1, budget.MaxTries), remaining);
		edit.Mirror.PriceText = (int n) => Rubies(budget, Mathf.Clamp(n, 1, budget.MaxTries)).ToString("N0");
	}

	private static void Run(Sender send, int equipSlot, int optionSlot, OperationType wanted, int tries)
	{
		if (busy || send == null)
		{
			return;
		}
		NShared.RenewEquipOption.Request request = new NShared.RenewEquipOption.Request
		{
			EquipItemSlotIndex = equipSlot,
			OptionSlotIndex = optionSlot
		};
		Asks.Add(request, new Ask
		{
			Target = (int)wanted,
			MaxTries = tries
		});
		send(request, delegate(NShared.RenewEquipOption.Response response)
		{
			if (Captured.TryGetValue(response, out var value))
			{
				ShowSummary(StatName(wanted), value);
			}
		});
	}

	private static void ShowSummary(string wanted, Extras value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(SprkText.Format(value.Hit ? "{0} found. Attempts: {1}." : "{0} not found. Attempts: {1}.", wanted, value.Tries));
		if (!value.Hit)
		{
			stringBuilder.Append(SprkText.Get("\nThe last roll is shown."));
		}
		List<string> list = new List<string>();
		if (value.CouponsUsed > 0)
		{
			list.Add(SprkText.Format("Coupons: {0}", value.CouponsUsed));
		}
		if (value.GemsSpent > 0)
		{
			list.Add(SprkText.Format("Rubies: {0}", value.GemsSpent.ToString("N0")));
		}
		if (list.Count > 0)
		{
			stringBuilder.Append(SprkText.Format("\nUsed {0}.", string.Join(", ", list.ToArray())));
		}
		stringBuilder.Append(PityResult(value.Hit, value.Guaranteed, value.PityMisses, value.PityLimit));
		MessagePopup.Open(Title, stringBuilder.ToString());
	}

	/// True when Change Option on this Technomagic item's skill line runs "reroll until".
	public static bool IsTechnomagicSkillArmed(int equipSlot)
	{
		return host == HostKind.Technomagic && skillTarget != 0 && skillTargetEquipSlot == equipSlot;
	}

	/// Replaces the Technomagic skill reroll when a skill target is selected for this item.
	public static bool TryStartTechnomagicSkill(ValanceWebManager manager, int equipSlot, int skillSlot)
	{
		if (manager == null || !IsTechnomagicSkillArmed(equipSlot))
		{
			return false;
		}
		if (busy)
		{
			return true;
		}
		int wanted = skillTarget;
		string name = SkillName(wanted);
		SkillSender send = manager.SprkSendValanceSkillRenew;
		EquipItemInfo item = CurrentItem();
		if (item != null && item.SlotIndex == equipSlot && SameSkill(CurrentSkill(item, skillSlot), wanted))
		{
			MessagePopup.Open(Title, SprkText.Format("This line already has {0}.", name));
			return true;
		}
		NShared.RenewEquipSkill.Request request = new NShared.RenewEquipSkill.Request
		{
			EquipItemSlotIndex = equipSlot,
			SkillSlotIndex = skillSlot
		};
		SkillAsks.Add(request, new Ask
		{
			Target = wanted,
			Preview = true
		});
		busy = true;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RenewEquipSkill.Response response)
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
			else if (response.Result == NShared.RenewEquipSkill.ResultType.NoAvailableOption)
			{
				MessagePopup.Open(Title, SprkText.Format("{0} cannot appear on this line.", name));
			}
			else if (response.Result != NShared.RenewEquipSkill.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (SkillCaptured.TryGetValue(response, out var value) && value.TotalWeight > 0 && value.HitWeight > 0)
			{
				EquipItemInfo current = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipSlot);
				if (current != null)
				{
					OpenConfirm(name, SkillBudget(current), (double)value.HitWeight / (double)value.TotalWeight, Remaining(value.PityMisses, value.PityLimit), SprkText.Get("Not enough skill reroll materials."), delegate(int tries)
					{
						RunSkill(send, equipSlot, skillSlot, wanted, name, tries);
					});
				}
			}
		});
		return true;
	}

	private static int CurrentSkill(EquipItemInfo item, int skillSlot)
	{
		foreach (EquipItemOptionInfo info in item.GetExtraOptionInfoList())
		{
			if (info != null && info.SlotIndex == skillSlot)
			{
				return info.OptionIndex;
			}
		}
		return 0;
	}

	private static bool SameSkill(int option, int wanted)
	{
		if (option == 0)
		{
			return false;
		}
		SkillEntry a = SkillOf(option);
		return a != null && a == SkillOf(wanted);
	}

	/// Skill rerolls cost only the skill reroll material, per try.
	private static Budget SkillBudget(EquipItemInfo item)
	{
		Budget result = new Budget
		{
			PerTryItems = new List<KeyValuePair<int, int>>(),
			Prices = new int[0]
		};
		EquipItemData equipItemData = item.EquipItemData;
		EquipItemDetailData detail = ((equipItemData == null) ? null : GameRoot.Instance.EquipItemDetailDataContainer.GetData(equipItemData.DetailIndex));
		if (detail == null || detail.ExtraSkillRenewItemCount <= 0)
		{
			return result;
		}
		result.PerTryItems.Add(new KeyValuePair<int, int>(detail.ExtraSkillRenewItemIndex, detail.ExtraSkillRenewItemCount));
		result.MaxTries = Math.Min(MaxTries, Count(detail.ExtraSkillRenewItemIndex) / detail.ExtraSkillRenewItemCount);
		return result;
	}

	private static void RunSkill(SkillSender send, int equipSlot, int skillSlot, int wanted, string name, int tries)
	{
		if (busy || send == null)
		{
			return;
		}
		NShared.RenewEquipSkill.Request request = new NShared.RenewEquipSkill.Request
		{
			EquipItemSlotIndex = equipSlot,
			SkillSlotIndex = skillSlot
		};
		SkillAsks.Add(request, new Ask
		{
			Target = wanted,
			MaxTries = tries
		});
		send(request, delegate(NShared.RenewEquipSkill.Response response)
		{
			if (SkillCaptured.TryGetValue(response, out var value))
			{
				ShowSummary(name, value);
			}
		});
	}
}
