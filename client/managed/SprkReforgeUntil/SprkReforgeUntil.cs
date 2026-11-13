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
public static class SprkReforgeUntil
{
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

	private const string Title = "Reforge Until";

	public const int MaxTries = 1000;

	private static readonly ConditionalWeakTable<NShared.RenewEquipOption.Request, Ask> Asks = new ConditionalWeakTable<NShared.RenewEquipOption.Request, Ask>();

	private static readonly ConditionalWeakTable<NShared.RenewEquipOption.Response, Extras> Captured = new ConditionalWeakTable<NShared.RenewEquipOption.Response, Extras>();

	private static readonly List<Entry> Entries = new List<Entry>();

	private static int targetEquipSlot;

	private static OperationType target;

	private static bool explained;

	private static bool busy;

	private static bool poolView;

	private static EquipOptionRenew window;

	private static UIButton button;

	private static UILabel buttonLabel;

	private static StatGaugeComponent poolPanel;

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

	public static void Capture(IDictionary json, NShared.RenewEquipOption.Response response)
	{
		if (json == null || response == null || (!json.Contains("SprkHitWeight") && !json.Contains("SprkTries")))
		{
			return;
		}
		Extras extras = new Extras
		{
			HitWeight = Number(json, "SprkHitWeight"),
			TotalWeight = Number(json, "SprkTotalWeight"),
			Tries = (int)Number(json, "SprkTries"),
			CouponsUsed = (int)Number(json, "SprkCouponsUsed"),
			GemsSpent = Number(json, "SprkGemsSpent"),
			Hit = json.Contains("SprkHit") && json["SprkHit"] is bool && (bool)json["SprkHit"]
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

	private static void TogglePoolView()
	{
		if (busy || window == null)
		{
			return;
		}
		poolView = !poolView && Entries.Count > 0;
		Paint();
	}

	private static void Pick(OperationType type)
	{
		if (busy || window == null || window.SelectedEquipItemInfo == null || !poolView)
		{
			return;
		}
		poolView = false;
		if (target == type && targetEquipSlot == window.SelectedEquipItemInfo.SlotIndex)
		{
			ClearTarget();
			return;
		}
		target = type;
		targetEquipSlot = window.SelectedEquipItemInfo.SlotIndex;
		Paint();
		if (!explained)
		{
			explained = true;
			MessagePopup.Open(Title, "Target: " + StatName(target) + "\n\nTap Change Option on a stat line to reforge it until this stat appears.\nTo reforge normally again, open the target button and tap " + StatName(target) + " again.");
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
			titleLabel.text = "Stat Pool";
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
				SetStateLabels(statGaugeInfo2, "Select", "Target");
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
			buttonLabel.text = (poolView ? "Back" : ((target != OperationType.None) ? ("Target:\n" + StatName(target)) : "Target\nStat"));
		}
	}

	/// Replaces the normal reforge when a target is selected for this item.
	public static bool TryStart(BlacksmithManagement manager, int equipSlot, int optionSlot)
	{
		if (target == OperationType.None || targetEquipSlot != equipSlot || manager == null)
		{
			return false;
		}
		if (busy)
		{
			return true;
		}
		OperationType wanted = target;
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
				MessagePopup.Open(Title, StatName(wanted) + " cannot appear on this line.");
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (Captured.TryGetValue(response, out var value) && value.TotalWeight > 0 && value.HitWeight > 0)
			{
				Confirm(manager, equipSlot, optionSlot, wanted, (double)value.HitWeight / (double)value.TotalWeight);
			}
		});
		return true;
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

	private static string Describe(OperationType wanted, double chance, Budget budget, int tries)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(StatName(wanted)).Append(": ").Append(Percent(chance))
			.Append(" per try, ")
			.Append(Percent(1.0 - Math.Pow(1.0 - chance, tries)))
			.Append(" within ")
			.Append(tries)
			.Append((tries == 1) ? " try" : " tries");
		List<string> list = new List<string>();
		int num = Math.Min(tries, budget.Coupons);
		if (num > 0)
		{
			list.Add(num + ((num == 1) ? " coupon" : " coupons"));
		}
		foreach (KeyValuePair<int, int> perTryItem in budget.PerTryItems)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(perTryItem.Key);
			list.Add(((data == null) ? perTryItem.Key.ToString() : LocalizationManager.GetStringDirect(data.Name)) + " ×" + ((long)perTryItem.Value * (long)tries).ToString("N0"));
		}
		if (list.Count > 0)
		{
			stringBuilder.Append("\nUses at most ").Append(string.Join(", ", list.ToArray()));
			if (budget.Prices.Length != 0)
			{
				stringBuilder.Append(" + rubies shown below");
			}
		}
		else if (budget.Prices.Length != 0)
		{
			stringBuilder.Append("\nUses at most the rubies shown below");
		}
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

	private static void Confirm(BlacksmithManagement manager, int equipSlot, int optionSlot, OperationType wanted, double chance)
	{
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipSlot);
		if (equipItemInfo == null)
		{
			return;
		}
		Budget budget = ReadBudget(equipItemInfo, optionSlot);
		if (budget.MaxTries <= 0)
		{
			MessagePopup.Open(Title, "Not enough reforge coupons or rubies.");
			return;
		}
		int num = ((budget.Coupons > 0) ? Math.Min(budget.Coupons, budget.MaxTries) : Math.Min(budget.MaxTries, Math.Max(1, (int)Math.Ceiling(1.0 / chance))));
		PopupEdit edit = new PopupEdit();
		CountMessagePopup popup = CountMessagePopup.Open(Title, string.Empty, "Reforge", 1, budget.MaxTries, num, delegate(int tries)
		{
			edit.Restore();
			Run(manager, equipSlot, optionSlot, wanted, Mathf.Clamp(tries, 1, budget.MaxTries));
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
		edit.Set(PopupLabel(popup, "Panel/Root/Button_Ok/Label_Action"), "Reforge");
		edit.Set(message, Describe(wanted, chance, budget, num));
		edit.Set(price, Rubies(budget, num).ToString("N0"));
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
		edit.Mirror.MessageText = (int n) => Describe(wanted, chance, budget, Mathf.Clamp(n, 1, budget.MaxTries));
		edit.Mirror.PriceText = (int n) => Rubies(budget, Mathf.Clamp(n, 1, budget.MaxTries)).ToString("N0");
	}

	private static void Run(BlacksmithManagement manager, int equipSlot, int optionSlot, OperationType wanted, int tries)
	{
		if (busy || manager == null)
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
		manager.SprkSendRenewEquipOption(request, delegate(NShared.RenewEquipOption.Response response)
		{
			if (Captured.TryGetValue(response, out var value))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(StatName(wanted)).Append(value.Hit ? " found after " : " not found after ").Append(value.Tries)
					.Append((value.Tries == 1) ? " try." : " tries.");
				if (!value.Hit)
				{
					stringBuilder.Append("\nThe last roll is shown.");
				}
				List<string> list = new List<string>();
				if (value.CouponsUsed > 0)
				{
					list.Add(value.CouponsUsed + ((value.CouponsUsed == 1) ? " coupon" : " coupons"));
				}
				if (value.GemsSpent > 0)
				{
					list.Add(value.GemsSpent.ToString("N0") + " rubies");
				}
				if (list.Count > 0)
				{
					stringBuilder.Append("\nUsed ").Append(string.Join(" and ", list.ToArray())).Append('.');
				}
				MessagePopup.Open(Title, stringBuilder.ToString());
			}
		});
	}
}
