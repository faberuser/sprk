using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;
using Math = System.Math;

/// "Enchant until" for Technomagic gear: pick up to 3 target stats from the
/// gear's enchant pool, then Enchant rerolls until one roll contains all of them
/// (in any order). The server runs the rolls (`enchant_until` in valance.rs); the
/// last roll is shown and chosen through the window's normal flow.
public static class SprkEnchantUntil
{
	private sealed class Ask
	{
		public string Targets;

		public bool Preview;

		public int MaxTries;
	}

	private struct CoreOdds
	{
		public int ItemIndex;

		public double Chance;

		public long ReqGold;
	}

	private sealed class Extras
	{
		public double Chance;

		public readonly List<CoreOdds> Cores = new List<CoreOdds>();

		public bool IsRun;

		public int Tries;

		public bool Hit;

		public long GoldSpent;

		public int CoresUsed;
	}

	private struct PoolEntry
	{
		public OperationType Type;

		public string Range;
	}

	private const string Title = "Enchant Until";

	public const int MaxTries = 5000;

	private static readonly Color TargetColor = new Color(1f, 0.78f, 0.2f);

	private static readonly ConditionalWeakTable<NShared.ValanceEnchant.Request, Ask> Asks = new ConditionalWeakTable<NShared.ValanceEnchant.Request, Ask>();

	private static readonly ConditionalWeakTable<NShared.ValanceEnchant.Response, Extras> Captured = new ConditionalWeakTable<NShared.ValanceEnchant.Response, Extras>();

	private static readonly List<OperationType> Targets = new List<OperationType>();

	private static readonly List<PoolEntry> Pool = new List<PoolEntry>();

	private static int targetSlot;

	private static Ask armed;

	private static bool busy;

	private static bool explained;

	private static bool refreshing;

	private static ValanceEnchantWindow window;

	private static UIButton button;

	private static UILabel buttonLabel;

	private static GameObject poolPanel;

	private static bool poolView;

	private static readonly List<KeyValuePair<OperationType, ValanceEnchantOptionSlot>> Rows = new List<KeyValuePair<OperationType, ValanceEnchantOptionSlot>>();

	// ---- Wire format -------------------------------------------------------

	public static void Encode(Dictionary<string, object> json, NShared.ValanceEnchant.Request request)
	{
		if (json == null || request == null || !Asks.TryGetValue(request, out var ask))
		{
			return;
		}
		json["SprkTargetOptionTypes"] = ask.Targets;
		if (ask.Preview)
		{
			json["SprkPreview"] = "1";
		}
		else
		{
			json["SprkMaxTries"] = ask.MaxTries.ToString();
		}
	}

	public static void Capture(IDictionary json, NShared.ValanceEnchant.Response response)
	{
		if (json == null || response == null || (!json.Contains("SprkChance") && !json.Contains("SprkTries")))
		{
			return;
		}
		Extras extras = new Extras
		{
			Chance = Double(json, "SprkChance"),
			IsRun = json.Contains("SprkTries"),
			Tries = (int)Double(json, "SprkTries"),
			Hit = json.Contains("SprkHit") && json["SprkHit"] is bool && (bool)json["SprkHit"],
			GoldSpent = (long)Double(json, "SprkGoldSpent"),
			CoresUsed = (int)Double(json, "SprkCoresUsed")
		};
		if (json.Contains("SprkCores") && json["SprkCores"] is IList list)
		{
			foreach (object item in list)
			{
				if (item is IDictionary dictionary)
				{
					extras.Cores.Add(new CoreOdds
					{
						ItemIndex = (int)Double(dictionary, "ItemIndex"),
						Chance = Double(dictionary, "Chance"),
						ReqGold = (long)Double(dictionary, "ReqGold")
					});
				}
			}
		}
		Captured.Remove(response);
		Captured.Add(response, extras);
	}

	private static double Double(IDictionary json, string key)
	{
		if (!json.Contains(key) || json[key] == null)
		{
			return 0.0;
		}
		try
		{
			return Convert.ToDouble(json[key], System.Globalization.CultureInfo.InvariantCulture);
		}
		catch
		{
			return 0.0;
		}
	}

	private static string TargetText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (OperationType target in Targets)
		{
			if (stringBuilder.Length > 0)
			{
				// Not a comma: the client escapes form values before the form does.
				stringBuilder.Append('_');
			}
			stringBuilder.Append((int)target);
		}
		return stringBuilder.ToString();
	}

	private static string TargetNames()
	{
		List<string> list = new List<string>();
		foreach (OperationType target in Targets)
		{
			list.Add(SprkReforgeUntil.StatName(target));
		}
		return string.Join(" + ", list.ToArray());
	}

	private static string CoreName(int itemIndex)
	{
		string stringDirect = LocalizationManager.GetStringDirect("ITEM_NAME_" + itemIndex);
		return string.IsNullOrEmpty(stringDirect) ? itemIndex.ToString() : stringDirect;
	}

	// ---- Window hooks ------------------------------------------------------

	public static void BeforeRefresh(ValanceEnchantWindow renewWindow)
	{
		if (!refreshing)
		{
			ClosePool(redraw: false);
		}
	}

	public static void AfterRefresh(ValanceEnchantWindow renewWindow)
	{
		window = renewWindow;
		EquipItemInfo equipItemInfo = ((window == null) ? null : window.SprkTargetItem);
		if (equipItemInfo == null || equipItemInfo.SlotIndex != targetSlot)
		{
			Targets.Clear();
			targetSlot = ((equipItemInfo == null) ? 0 : equipItemInfo.SlotIndex);
		}
		LoadPool(equipItemInfo);
		EnsureButton();
		Paint();
	}

	public static void Closing(ValanceEnchantWindow renewWindow)
	{
		ClosePool(redraw: false);
		Targets.Clear();
		targetSlot = 0;
		armed = null;
		window = null;
	}

	/// The gear's enchant pool. Every core shares the same pool for a part, so
	/// the selected core (or any core) identifies it.
	private static void LoadPool(EquipItemInfo item)
	{
		Pool.Clear();
		if (item == null || item.EquipItemData == null)
		{
			return;
		}
		string partType = item.EquipItemData.PartType.ToString();
		string subType = item.EquipItemData.SubType.ToString();
		ItemData itemData = ((window == null) ? null : window.SprkSelectedCore);
		string[] array = new string[5]
		{
			(itemData == null) ? null : itemData.Code,
			"TECHNOMAGIC_ENCHANT_ENERGY",
			"TECHNOMAGIC_ENCHANT_NANO",
			"TECHNOMAGIC_ENCHANT_PULSE",
			"TECHNOMAGIC_ENCHANT_TRANSISTOR"
		};
		ValanceEnchantData valanceEnchantData = null;
		foreach (string text in array)
		{
			if (text != null)
			{
				valanceEnchantData = GameRoot.Instance.ValanceEnchantDataContainer.GetData(new ValanceEnchantDataKey(text, partType, subType));
				if (valanceEnchantData != null)
				{
					break;
				}
			}
		}
		EquipOptionGroupData equipOptionGroupData = ((valanceEnchantData == null) ? null : GameRoot.Instance.EquipOptionGroupDataContainer.GetData(valanceEnchantData.OptionGroupIndex));
		if (equipOptionGroupData == null || equipOptionGroupData.OptionIndex == null)
		{
			return;
		}
		foreach (int key in equipOptionGroupData.OptionIndex)
		{
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(key);
			if (data != null && !Pool.Exists((PoolEntry p) => p.Type == data.OptionType))
			{
				Pool.Add(new PoolEntry
				{
					Type = data.OptionType,
					// Enchants use the zero-star range (see the server's enchant code).
					Range = StatData.GetStatDisplayValue(data.OptionType, data.GetOptionValueMin(0), data.GetOptionValueMax(0))
				});
			}
		}
		Targets.RemoveAll((OperationType t) => !Pool.Exists((PoolEntry p) => p.Type == t));
	}

	// ---- Target button and pool view ---------------------------------------

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

	/// A copy of the panels' "Select" button on the "Enchantment Core" title row.
	private static void EnsureButton()
	{
		if (window == null || window.currentOptionComponent == null || window.currentOptionComponent.optionConfirmButton == null)
		{
			return;
		}
		if (button == null)
		{
			UIButton optionConfirmButton = window.currentOptionComponent.optionConfirmButton;
			GameObject gameObject = UnityEngine.Object.Instantiate(optionConfirmButton.gameObject, window.currentOptionComponent.transform.parent, worldPositionStays: true);
			gameObject.name = "SprkEnchantUntil";
			StripBehaviours(gameObject);
			button = gameObject.GetComponent<UIButton>() ?? gameObject.AddComponent<UIButton>();
			button.onClick.Clear();
			EventDelegate.Add(button.onClick, TogglePool);
			button.isEnabled = true;
			buttonLabel = gameObject.GetComponentInChildren<UILabel>(includeInactive: true);
			gameObject.SetActive(value: true);
			Vector3 position = optionConfirmButton.transform.position;
			UILabel uILabel = CoreTitle();
			button.transform.position = new Vector3(position.x, (uILabel == null) ? position.y : uILabel.transform.position.y, position.z);
			NGUITools.MarkParentAsChanged(gameObject);
		}
		button.gameObject.SetActive(!poolView && window.SprkTargetItem != null && Pool.Count > 0);
	}

	/// The "Enchantment Core" label: the closest label above the first core icon.
	private static UILabel CoreTitle()
	{
		if (window.coreItemRoots == null || window.coreItemRoots.Length == 0 || window.coreItemRoots[0] == null)
		{
			return null;
		}
		Vector3 position = window.coreItemRoots[0].position;
		UILabel result = null;
		float num = float.MaxValue;
		UILabel[] componentsInChildren = window.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel in componentsInChildren)
		{
			float num2 = uILabel.transform.position.y - position.y;
			if (num2 > 0f && num2 < num && Array.IndexOf(window.labelCoreItems, uILabel) < 0 && uILabel.transform.position.x < position.x + 0.2f && !string.IsNullOrEmpty(uILabel.text) && uILabel.GetComponentInParent<ValanceEnchantOptionComponent>() == null)
			{
				num = num2;
				result = uILabel;
			}
		}
		return result;
	}

	private static void Paint()
	{
		if (buttonLabel != null)
		{
			buttonLabel.text = (poolView ? "Done" : ((Targets.Count == 0) ? "Target Stats" : ("Targets: " + Targets.Count)));
		}
		foreach (KeyValuePair<OperationType, ValanceEnchantOptionSlot> row in Rows)
		{
			if (!(row.Value == null))
			{
				bool flag = Targets.Contains(row.Key);
				PoolEntry poolEntry = Pool.Find((PoolEntry p) => p.Type == row.Key);
				row.Value.Init(SprkReforgeUntil.StatName(row.Key), flag ? "Target" : poolEntry.Range, flag ? ValanceEnchantOptionSlot.ValanceEnchantSlotState.Fix : ValanceEnchantOptionSlot.ValanceEnchantSlotState.Active);
				if (row.Value.labelOptionDesc != null)
				{
					row.Value.labelOptionDesc.color = (flag ? TargetColor : Color.white);
				}
				if (row.Value.labelOptionStat != null)
				{
					row.Value.labelOptionStat.color = (flag ? TargetColor : Color.white);
				}
			}
		}
	}

	private static void TogglePool()
	{
		if (busy || window == null)
		{
			return;
		}
		if (poolView)
		{
			ClosePool(redraw: true);
		}
		else
		{
			OpenPool();
		}
		EnsureButton();
		Paint();
	}

	private static void Pick(OperationType type)
	{
		if (!poolView || busy)
		{
			return;
		}
		if (Targets.Contains(type))
		{
			Targets.Remove(type);
		}
		else if (Targets.Count >= 3)
		{
			MessagePopup.Open(Title, "You can target up to 3 stats.\nTap a target again to remove it.");
			return;
		}
		else
		{
			Targets.Add(type);
		}
		Paint();
	}

	private static void SetChildActive(Transform root, string name, bool active)
	{
		Transform transform = FindChild(root, name);
		if (transform != null)
		{
			transform.gameObject.SetActive(active);
		}
	}

	/// The pool as this window's own option rows, in a copy of the "Applied
	/// Enchant Option" panel whose frame is stretched over both option panels,
	/// with the rows in a clipped, scrollable single column.
	private static void OpenPool()
	{
		ValanceEnchantOptionComponent currentOptionComponent = window.currentOptionComponent;
		ValanceEnchantOptionComponent newOptionComponent = window.newOptionComponent;
		if (currentOptionComponent.grid == null || currentOptionComponent.template == null)
		{
			return;
		}
		// The second panel's frame bottom, read before anything is hidden.
		Transform transform = FindChild(newOptionComponent.transform, "Effect_Fix");
		UIWidget uIWidget = ((transform == null) ? null : transform.GetComponent<UIWidget>());
		GameObject gameObject = UnityEngine.Object.Instantiate(currentOptionComponent.gameObject, currentOptionComponent.transform.parent, worldPositionStays: true);
		gameObject.name = "SprkEnchantUntilPool";
		DropLocalizers(gameObject);
		ValanceEnchantOptionComponent component = gameObject.GetComponent<ValanceEnchantOptionComponent>();
		poolPanel = gameObject;
		poolView = true;
		// Freeze the copy in the panel's normal look, whatever state it was copied in.
		if (component.anim != null)
		{
			component.anim.enabled = false;
		}
		Transform transform2 = gameObject.transform;
		SetChildActive(transform2, "EnchantOption", active: true);
		SetChildActive(transform2, "EnchantOption_None", active: false);
		SetChildActive(transform2, "Effect_Preview", active: false);
		SetChildActive(transform2, "Effect_Setting", active: false);
		SetChildActive(transform2, "GUI_Animation", active: false);
		SetChildActive(transform2, "Effect_Fix", active: true);
		ValanceEnchantOptionSlot[] componentsInChildren = component.grid.GetComponentsInChildren<ValanceEnchantOptionSlot>(includeInactive: true);
		foreach (ValanceEnchantOptionSlot valanceEnchantOptionSlot in componentsInChildren)
		{
			if (valanceEnchantOptionSlot != component.template)
			{
				UnityEngine.Object.DestroyImmediate(valanceEnchantOptionSlot.gameObject);
			}
		}
		if (component.titleLabel != null)
		{
			component.titleLabel.text = "Stat Pool (up to 3)";
		}
		if (component.optionConfirmButton != null)
		{
			GameObject gameObject2 = component.optionConfirmButton.gameObject;
			gameObject2.SetActive(value: true);
			component.optionConfirmButton.isEnabled = true;
			component.optionConfirmButton.onClick.Clear();
			EventDelegate.Add(component.optionConfirmButton.onClick, TogglePool);
			UILabel componentInChildren = gameObject2.GetComponentInChildren<UILabel>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.text = "Done";
			}
		}
		// Stretch the frame (and its sprite) down to the second panel's bottom.
		Transform transform3 = FindChild(transform2, "Effect_Fix");
		UIWidget uIWidget2 = ((transform3 == null) ? null : transform3.GetComponent<UIWidget>());
		float num = 0f;
		float num2 = -142f;
		if (uIWidget2 != null)
		{
			Vector3 vector = transform2.InverseTransformPoint(uIWidget2.worldCorners[1]);
			num = vector.y;
			num2 = ((uIWidget == null) ? (num - 2f * (float)uIWidget2.height) : transform2.InverseTransformPoint(uIWidget.worldCorners[0]).y);
			int num3 = Mathf.Max(uIWidget2.height, Mathf.RoundToInt(num - num2));
			float num4 = (num + num2) * 0.5f;
			// The frame is drawn through a fixed-size 3D widget; lift its sprite out
			// so it renders as a plain, resizable sprite, and drop the 3D effect.
			UISprite uISprite = null;
			foreach (Transform item2 in transform3)
			{
				if (item2.GetComponent<UISprite>() != null)
				{
					uISprite = item2.GetComponent<UISprite>();
					break;
				}
			}
			if (uISprite != null)
			{
				uISprite.transform.SetParent(transform3.parent, worldPositionStays: true);
				transform3.gameObject.SetActive(value: false);
				uISprite.height = num3;
				Vector3 localPosition = uISprite.transform.localPosition;
				Vector3 vector2 = uISprite.transform.parent.InverseTransformPoint(transform2.TransformPoint(new Vector3(0f, num4, 0f)));
				uISprite.transform.localPosition = new Vector3(localPosition.x, vector2.y, localPosition.z);
				NGUITools.MarkParentAsChanged(uISprite.gameObject);
			}
		}
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(currentOptionComponent.gameObject);
		int num5 = ((uIPanel == null) ? 10 : (uIPanel.depth + 1));
		int sortingOrder = ((!(uIPanel == null)) ? uIPanel.sortingOrder : 0);
		UIGrid grid = component.grid;
		grid.maxPerLine = 0;
		grid.arrangement = UIGrid.Arrangement.Vertical;
		float y = transform2.InverseTransformPoint(grid.transform.position).y;
		float num6 = y + grid.cellHeight * 0.5f;
		float num7 = num2 + 10f;
		float num8 = (float)((component.template != null && component.template.GetComponentInChildren<UIWidget>(includeInactive: true) != null) ? component.template.GetComponentInChildren<UIWidget>(includeInactive: true).width : 600) + 8f;
		GameObject gameObject3 = new GameObject("Scroll");
		gameObject3.layer = gameObject.layer;
		gameObject3.transform.SetParent(transform2, worldPositionStays: false);
		UIPanel uIPanel2 = gameObject3.AddComponent<UIPanel>();
		uIPanel2.depth = num5;
		uIPanel2.sortingOrder = sortingOrder;
		uIPanel2.clipping = UIDrawCall.Clipping.SoftClip;
		uIPanel2.clipSoftness = new Vector2(0f, 8f);
		float num9 = transform2.InverseTransformPoint(grid.transform.position).x;
		uIPanel2.baseClipRegion = new Vector4(num9, (num6 + num7) * 0.5f, num8, Mathf.Max(10f, num6 - num7));
		UIScrollView uIScrollView = gameObject3.AddComponent<UIScrollView>();
		uIScrollView.movement = UIScrollView.Movement.Vertical;
		uIScrollView.dragEffect = UIScrollView.DragEffect.MomentumAndSpring;
		uIScrollView.restrictWithinPanel = true;
		uIScrollView.disableDragIfFits = true;
		// An invisible backdrop at the list's depth catches drags and the wheel.
		GameObject gameObject4 = new GameObject("Backdrop");
		gameObject4.layer = gameObject.layer;
		gameObject4.transform.SetParent(transform2, worldPositionStays: false);
		UIPanel uIPanel3 = gameObject4.AddComponent<UIPanel>();
		uIPanel3.depth = num5;
		uIPanel3.sortingOrder = sortingOrder;
		UIWidget uIWidget3 = gameObject4.AddComponent<UIWidget>();
		uIWidget3.depth = -100;
		uIWidget3.width = Mathf.Max(2, Mathf.RoundToInt(uIPanel2.baseClipRegion.z));
		uIWidget3.height = Mathf.Max(2, Mathf.RoundToInt(uIPanel2.baseClipRegion.w));
		gameObject4.transform.localPosition = new Vector3(uIPanel2.baseClipRegion.x, uIPanel2.baseClipRegion.y, 0f);
		NGUITools.AddWidgetCollider(gameObject4);
		gameObject4.AddComponent<UIDragScrollView>().scrollView = uIScrollView;
		grid.transform.SetParent(gameObject3.transform, worldPositionStays: true);
		Rows.Clear();
		foreach (PoolEntry item in Pool)
		{
			GameObject gameObject5 = NGUITools.AddChild(grid.gameObject, component.template.gameObject);
			gameObject5.SetActive(value: true);
			ValanceEnchantOptionSlot component2 = gameObject5.GetComponent<ValanceEnchantOptionSlot>();
			if (gameObject5.GetComponent<Collider>() == null)
			{
				NGUITools.AddWidgetCollider(gameObject5);
			}
			OperationType type = item.Type;
			UIEventListener.Get(gameObject5).onClick = delegate
			{
				Pick(type);
			};
			gameObject5.AddComponent<UIDragScrollView>().scrollView = uIScrollView;
			Rows.Add(new KeyValuePair<OperationType, ValanceEnchantOptionSlot>(type, component2));
		}
		grid.Reposition();
		NGUITools.MarkParentAsChanged(gameObject);
		uIScrollView.ResetPosition();
		currentOptionComponent.gameObject.SetActive(value: false);
		newOptionComponent.gameObject.SetActive(value: false);
	}

	private static void ClosePool(bool redraw)
	{
		if (!poolView)
		{
			return;
		}
		poolView = false;
		Rows.Clear();
		if (poolPanel != null)
		{
			poolPanel.SetActive(value: false);
			UnityEngine.Object.Destroy(poolPanel);
		}
		poolPanel = null;
		if (window == null)
		{
			return;
		}
		window.currentOptionComponent.gameObject.SetActive(value: true);
		window.newOptionComponent.gameObject.SetActive(value: true);
		if (!redraw)
		{
			return;
		}
		// Re-activated panels start from their default animation state; redraw
		// them the way the window last showed them.
		ValanceEnchantOptionInfo valanceEnchant = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetValanceEnchant();
		EquipItemInfo sprkTargetItem = window.SprkTargetItem;
		if (ValanceEnchantWindow.EnchantInProgress && valanceEnchant != null && sprkTargetItem != null)
		{
			window.currentOptionComponent.ShowCurrentOptionList(sprkTargetItem);
			window.currentOptionComponent.ShowConfirmButton();
			window.newOptionComponent.ShowNewOptionList(valanceEnchant);
			return;
		}
		refreshing = true;
		try
		{
			window.SprkRefreshPanels();
		}
		finally
		{
			refreshing = false;
		}
	}

	// ---- Enchant flow ------------------------------------------------------

	/// Replaces the Enchant button while targets are set: preview, then confirm.
	public static bool TryStart(ValanceEnchantWindow enchantWindow)
	{
		if (armed != null)
		{
			return false;
		}
		EquipItemInfo item = enchantWindow.SprkTargetItem;
		ItemData core = enchantWindow.SprkSelectedCore;
		if (Targets.Count == 0 || item == null || core == null || item.SlotIndex != targetSlot)
		{
			return false;
		}
		if (busy)
		{
			return true;
		}
		if (poolView)
		{
			ClosePool(redraw: true);
			Paint();
		}
		NShared.ValanceEnchant.Request request = new NShared.ValanceEnchant.Request
		{
			EquipItemSlotIndex = item.SlotIndex,
			ConsumeItemIndex = core.Index
		};
		Asks.Add(request, new Ask
		{
			Targets = TargetText(),
			Preview = true
		});
		busy = true;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ValanceEnchant.Response response)
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
			else if (response.Result != NShared.ValanceEnchant.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (Captured.TryGetValue(response, out var value))
			{
				Confirm(enchantWindow, item, core, value);
			}
		});
		return true;
	}

	private static void Confirm(ValanceEnchantWindow enchantWindow, EquipItemInfo item, ItemData core, Extras odds)
	{
		long price = GameRoot.Instance.ValanceEnchantDataContainer.GetReqGold(core.Code, item.EquipItemData.PartType.ToString(), item.EquipItemData.SubType.ToString());
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(core.Code);
		long gold = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
		double chance = odds.Chance;
		// Cheapest core on average (gold per hit) among those that can hit.
		CoreOdds best = default(CoreOdds);
		foreach (CoreOdds core2 in odds.Cores)
		{
			if (core2.Chance > 0.0 && (best.Chance <= 0.0 || (double)core2.ReqGold / core2.Chance < (double)best.ReqGold / best.Chance))
			{
				best = core2;
			}
		}
		string text = ((best.Chance > 0.0 && best.ItemIndex != core.Index && (chance <= 0.0 || (double)best.ReqGold / best.Chance < (double)price / chance * 0.95)) ? (CoreName(best.ItemIndex) + " is cheaper: " + SprkReforgeUntil.Percent(best.Chance) + " per try, about " + ((long)((double)best.ReqGold / best.Chance)).ToString("N0") + " gold per hit.") : null);
		if (chance <= 0.0)
		{
			MessagePopup.Open(Title, TargetNames() + " cannot all appear with " + CoreName(core.Index) + "." + ((text == null) ? "" : ("\n" + text)));
			return;
		}
		long num = Math.Min(Math.Min((long)MaxTries, itemCount), (price > 0) ? (gold / price) : MaxTries);
		if (num <= 0)
		{
			MessagePopup.Open(Title, "Not enough " + CoreName(core.Index) + " or gold.");
			return;
		}
		int max = (int)num;
		int initial = Mathf.Clamp((int)Math.Ceiling(1.0 / chance), 1, max);
		SprkReforgeUntil.PopupEdit edit = new SprkReforgeUntil.PopupEdit();
		CountMessagePopup popup = CountMessagePopup.Open(Title, string.Empty, "Enchant", 1, max, initial, delegate(int tries)
		{
			edit.Restore();
			Run(enchantWindow, Mathf.Clamp(tries, 1, max));
		}, delegate
		{
			edit.Restore();
		});
		if (popup == null)
		{
			return;
		}
		UILabel message = SprkReforgeUntil.PopupLabel(popup, "Panel/Root/Label_Message");
		UILabel price2 = SprkReforgeUntil.PopupLabel(popup, "Panel/Root/Value (1)/Label_Price");
		Func<int, string> messageText = delegate(int n)
		{
			n = Mathf.Clamp(n, 1, max);
			string text2 = TargetNames() + ": " + SprkReforgeUntil.Percent(chance) + " per try, " + SprkReforgeUntil.Percent(1.0 - Math.Pow(1.0 - chance, n)) + " within " + n + ((n == 1) ? " try" : " tries");
			return text2 + "\n" + (text ?? ("Uses at most " + n + " " + CoreName(core.Index) + " + gold below"));
		};
		edit.Set(SprkReforgeUntil.PopupLabel(popup, "Panel/Root/Title/Label_Title"), Title);
		edit.Set(SprkReforgeUntil.PopupLabel(popup, "Panel/Root/Button_Ok/Label_Action"), "Enchant");
		edit.Set(message, messageText(initial));
		edit.Set(price2, (price * initial).ToString("N0"));
		// The price row shows rubies by default; show the gold icon instead.
		if (popup.PriceIcons != null)
		{
			foreach (CountMessagePopup.PriceIcon priceIcon in popup.PriceIcons)
			{
				if (priceIcon != null && priceIcon.Icon != null)
				{
					edit.SetActive(priceIcon.Icon.gameObject, priceIcon.Type == ShopCostType.Gold);
				}
			}
		}
		Transform title = popup.transform.Find("Panel/Root/Title/Label_Title");
		Transform ok = popup.transform.Find("Panel/Root/Button_Ok");
		edit.MoveDown((message == null) ? null : message.transform, title, ok, 0.055f);
		edit.MoveDown(popup.transform.Find("Panel/Root/Count"), title, ok, 0.1f);
		edit.Mirror = popup.gameObject.AddComponent<SprkReforgeUntil.PopupMirror>();
		edit.Mirror.Popup = popup;
		edit.Mirror.Owner = edit;
		edit.Mirror.Message = message;
		edit.Mirror.Price = price2;
		edit.Mirror.MessageText = messageText;
		edit.Mirror.PriceText = (int n) => (price * Mathf.Clamp(n, 1, max)).ToString("N0");
	}

	/// Runs the window's own Enchant flow (which discards a pending roll first)
	/// with the targets attached to the request it sends.
	private static void Run(ValanceEnchantWindow enchantWindow, int tries)
	{
		if (enchantWindow == null)
		{
			return;
		}
		armed = new Ask
		{
			Targets = TargetText(),
			MaxTries = tries
		};
		try
		{
			enchantWindow.OnClickEnchant();
		}
		finally
		{
			armed = null;
		}
	}

	public static void Arm(NShared.ValanceEnchant.Request request)
	{
		if (armed != null && request != null)
		{
			Asks.Add(request, armed);
			armed = null;
		}
	}

	public static void Done(NShared.ValanceEnchant.Response response)
	{
		if (response == null || !Captured.TryGetValue(response, out var value) || !value.IsRun)
		{
			return;
		}
		string text = TargetNames();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(text).Append(value.Hit ? " found after " : " not found after ").Append(value.Tries)
			.Append((value.Tries == 1) ? " try." : " tries.");
		if (!value.Hit)
		{
			stringBuilder.Append("\nThe last roll is shown.");
		}
		stringBuilder.Append("\nUsed ").Append(value.CoresUsed).Append((value.CoresUsed == 1) ? " core and " : " cores and ")
			.Append(value.GoldSpent.ToString("N0"))
			.Append(" gold.");
		if (value.Hit)
		{
			// The goal is reached; the next Enchant is a normal one again.
			Targets.Clear();
			Paint();
		}
		MessagePopup.Open(Title, stringBuilder.ToString());
	}
}
