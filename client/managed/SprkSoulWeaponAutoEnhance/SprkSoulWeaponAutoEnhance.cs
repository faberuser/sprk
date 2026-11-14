using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;
using Math = System.Math;

/// "Auto Enhance" in the Soul Weapon Ether Enhancement window: one tap repeats
/// Max, Add Ether and Attempt Enhancement until +20 or until the Ether or Gold
/// runs out. The server runs the steps (`auto_enhance` in
/// server/src/api/extensions/soul.rs) with the normal rates, costs and roll:
/// each level is filled to 100% before it is attempted, and Ether is spent from
/// the lowest tier up. The preview below mirrors that loop.
public static class SprkSoulWeaponAutoEnhance
{
	private sealed class Ask
	{
	}

	private sealed class Outcome
	{
		public int StartLevel;

		public int Attempts;

		public long Gold;

		public string Stop;
	}

	private sealed class Plan
	{
		public int From;

		public int To;

		public long Gold;

		public bool Leftover;

		public readonly List<KeyValuePair<string, long>> Used = new List<KeyValuePair<string, long>>();
	}

	private static string Title => SprkText.Get("Auto Enhance");

	private static readonly ConditionalWeakTable<NShared.SoulWeaponEtherInjection.Request, Ask> Asks = new ConditionalWeakTable<NShared.SoulWeaponEtherInjection.Request, Ask>();

	private static readonly ConditionalWeakTable<NShared.SoulWeaponEtherInjection.Response, Outcome> Captured = new ConditionalWeakTable<NShared.SoulWeaponEtherInjection.Response, Outcome>();

	private static bool armed;

	private static bool busy;

	private static float busySince;

	private static SoulWeaponReinforceWindow window;

	private static UIButton button;

	private static UILabel buttonLabel;

	public static void Arm(NShared.SoulWeaponEtherInjection.Request request)
	{
		if (armed && request != null)
		{
			Asks.Add(request, new Ask());
		}
		armed = false;
	}

	public static void Encode(Dictionary<string, object> json, NShared.SoulWeaponEtherInjection.Request request)
	{
		if (json != null && request != null && Asks.TryGetValue(request, out var _))
		{
			json["SprkAutoEnhance"] = "1";
		}
	}

	public static void Capture(IDictionary json, NShared.SoulWeaponEtherInjection.Response response)
	{
		if (json == null || response == null || !json.Contains("SprkAutoAttempts"))
		{
			return;
		}
		Captured.Remove(response);
		Captured.Add(response, new Outcome
		{
			StartLevel = (int)Number(json, "SprkAutoStartLevel"),
			Attempts = (int)Number(json, "SprkAutoAttempts"),
			Gold = Number(json, "SprkAutoGold"),
			Stop = (json["SprkAutoStop"] as string) ?? string.Empty
		});
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

	/// Called at the end of every Ether injection response, successful or not.
	public static void Done(NShared.SoulWeaponEtherInjection.Response response)
	{
		if (response == null || !Captured.TryGetValue(response, out var outcome))
		{
			if (busy)
			{
				busy = false;
				Paint();
			}
			return;
		}
		busy = false;
		SoulWeaponInfo result = response.ResultSoulWeaponInfo;
		StringBuilder text = new StringBuilder();
		if (result != null && result.ReinforceLevel > outcome.StartLevel)
		{
			text.Append(SprkText.Format("Enhanced from +{0} to +{1}.", outcome.StartLevel, result.ReinforceLevel));
		}
		else if (result != null)
		{
			text.Append(SprkText.Format("Ether added toward +{0}.", result.ReinforceLevel + 1));
		}
		if (outcome.Stop == "MaxLevel")
		{
			text.Append("\n").Append(SprkText.Get("Maximum level reached."));
		}
		else if (outcome.Stop == "Gold")
		{
			text.Append("\n").Append(SprkText.Get("Stopped: not enough Gold."));
		}
		else if (outcome.Stop == "Ether")
		{
			text.Append("\n").Append(SprkText.Get("Stopped: not enough Ether."));
		}
		List<KeyValuePair<string, long>> used = new List<KeyValuePair<string, long>>();
		if (response.RemovedItemResultInfos != null)
		{
			foreach (ItemResultInfo removed in response.RemovedItemResultInfos)
			{
				ItemData data = (removed == null) ? null : GameRoot.Instance.ItemDataContainer.GetData(removed.ItemIndex);
				if (data != null && removed.AddCount < 0)
				{
					used.Add(new KeyValuePair<string, long>(data.Code, -(long)removed.AddCount));
				}
			}
		}
		AppendCosts(text, used, outcome.Gold);
		// The game's own result screen re-opens this window after an attempt;
		// doing the same refreshes the soul weapon list's level badges.
		if (window != null && window.IsOpen() && result != null)
		{
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(result.EquipItemSlotIndex);
			if (equipItemInfo != null)
			{
				window.SprkReopen(equipItemInfo);
			}
		}
		Paint();
		MessagePopup.Open(Title, text.ToString());
	}

	/// Called whenever the Ether Enhancement window shows a soul weapon.
	public static void Attach(SoulWeaponReinforceWindow reinforceWindow)
	{
		window = reinforceWindow;
		EnsureButton();
		Paint();
	}

	private static SoulWeaponInfo Current
	{
		get
		{
			if (window == null || window.EquipItemInfo == null)
			{
				return null;
			}
			return window.EquipItemInfo.SoulWeaponInfo;
		}
	}

	private static bool IsMaxLevel(SoulWeaponInfo info)
	{
		return info == null || GameRoot.Instance.SoulWeaponReinforceDataContainer.GetData(info.ReinforceLevel + 1) == null;
	}

	/// A copy of the game's "Add Ether" button, in the same row to the left of
	/// the required-Gold box. It sits beside Add Ether, so it is shown and
	/// hidden with that row (not at max level or when Enhance is ready).
	private static void EnsureButton()
	{
		if (window == null || window.EtherInjectionButton == null)
		{
			return;
		}
		Transform parent = window.EtherInjectionButton.transform.parent;
		if (button == null || button.transform.parent != parent)
		{
			if (button != null)
			{
				UnityEngine.Object.Destroy(button.gameObject);
			}
			GameObject copy = UnityEngine.Object.Instantiate(window.EtherInjectionButton.gameObject, parent, worldPositionStays: true);
			copy.name = "SprkAutoEnhance";
			StripBehaviours(copy);
			button = copy.GetComponent<UIButton>();
			if (button == null)
			{
				button = copy.AddComponent<UIButton>();
			}
			button.onClick.Clear();
			EventDelegate.Add(button.onClick, OnClick);
			buttonLabel = copy.GetComponentInChildren<UILabel>(includeInactive: true);
			if (buttonLabel != null)
			{
				buttonLabel.text = Title;
			}
			NGUITools.MarkParentAsChanged(copy);
		}
		Place();
	}

	/// Left of the Gold box, with the same gap the game leaves between the Gold
	/// box and Add Ether. Measured from the live widgets, so it is skipped while
	/// the row is hidden and keeps the last position.
	private static void Place()
	{
		Transform add = window.EtherInjectionButton.transform;
		Transform priceBox = PriceBox(add);
		if (priceBox == null)
		{
			return;
		}
		Bounds box = NGUIMath.CalculateAbsoluteWidgetBounds(priceBox);
		Bounds addBounds = NGUIMath.CalculateAbsoluteWidgetBounds(add);
		if (box.size.x <= 0f || addBounds.size.x <= 0f)
		{
			return;
		}
		float gap = Mathf.Max(0f, addBounds.min.x - box.max.x);
		// The copy has Add Ether's size and pivot.
		float pivotOffset = add.position.x - addBounds.min.x;
		button.transform.position = new Vector3(box.min.x - gap - addBounds.size.x + pivotOffset, add.position.y, add.position.z);
	}

	/// The Gold box: the closest object holding both the price label and its
	/// coin icon, without reaching up to the row that holds Add Ether.
	private static Transform PriceBox(Transform add)
	{
		RequirePrice price = (window.CountInputField == null) ? null : window.CountInputField.RequirePrice;
		if (price == null || price.ReqPriceLabel == null)
		{
			return null;
		}
		Transform icon = (price.IconProxyRootWidget == null) ? null : price.IconProxyRootWidget.transform;
		Transform box = price.ReqPriceLabel.transform;
		for (Transform node = box.parent; node != null && !add.IsChildOf(node); node = node.parent)
		{
			box = node;
			if (icon == null || icon.IsChildOf(node))
			{
				break;
			}
		}
		return box;
	}

	private static void StripBehaviours(GameObject gameObject)
	{
		MonoBehaviour[] behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour behaviour in behaviours)
		{
			if (!(behaviour is UIWidget) && !(behaviour is UIButtonColor) && !(behaviour is UIButtonScale) && !(behaviour is UIPlaySound))
			{
				UnityEngine.Object.DestroyImmediate(behaviour);
			}
		}
	}

	private static void Paint()
	{
		if (button == null)
		{
			return;
		}
		if (busy && Time.realtimeSinceStartup - busySince > 30f)
		{
			busy = false;
		}
		SoulWeaponInfo info = Current;
		bool show = window != null && info != null && !IsMaxLevel(info);
		button.gameObject.SetActive(show);
		if (show)
		{
			UIHelper.EnableButton(button, !busy);
		}
	}

	private static void OnClick()
	{
		Paint();
		SoulWeaponInfo info = Current;
		if (busy || window == null || info == null || window.IsWaitReinforceResult || IsMaxLevel(info))
		{
			return;
		}
		Plan plan = Simulate(info);
		if (plan.To == plan.From && plan.Used.Count == 0)
		{
			MessagePopup.Open(Title, SprkText.Format("Not enough Ether or Gold to reach +{0}.", plan.From + 1));
			return;
		}
		StringBuilder text = new StringBuilder();
		if (plan.To > plan.From)
		{
			text.Append(SprkText.Format("Enhance +{0} → +{1}.", plan.From, plan.To));
		}
		else
		{
			text.Append(SprkText.Format("Not enough Ether or Gold to reach +{0}.", plan.From + 1));
		}
		if (plan.Leftover)
		{
			text.Append("\n").Append(SprkText.Format("Leftover Ether is added toward +{0} without an attempt.", plan.To + 1));
		}
		text.Append("\n").Append(SprkText.Get("Each level is filled to 100% before it is attempted. Ether is used from the lowest tier first."));
		AppendCosts(text, plan.Used, plan.Gold);
		int slot = info.EquipItemSlotIndex;
		// The game's own Ether warning popup: Confirm, or the close button to cancel.
		MessagePopup.Open(Title, text.ToString(), delegate
		{
			Send(slot);
		}, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
	}

	private static void Send(int slot)
	{
		if (busy)
		{
			return;
		}
		busy = true;
		busySince = Time.realtimeSinceStartup;
		Paint();
		armed = true;
		SingletonAuto<SoulWeaponWebManager>.instance.RequestInjection(slot, new int[0]);
		armed = false;
	}

	private static void AppendCosts(StringBuilder text, List<KeyValuePair<string, long>> used, long gold)
	{
		List<string> names = new List<string>();
		foreach (KeyValuePair<string, long> item in used)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.Key);
			string name = (data == null) ? item.Key : LocalizationManager.GetStringDirect(data.Name);
			names.Add(name + " ×" + item.Value.ToString("N0"));
		}
		if (names.Count > 0)
		{
			text.Append("\n\n").Append(SprkText.Format("Ether: {0}", string.Join(", ", names.ToArray())));
		}
		text.Append("\n").Append(SprkText.Format("Gold: {0}", gold.ToString("N0")));
	}

	/// Mirrors the server loop on the client's view of Ether, Gold and tables.
	private static Plan Simulate(SoulWeaponInfo info)
	{
		GameRoot root = GameRoot.Instance;
		AccountManager account = GlobalAutoBehaviour<AccountManager>.instance;
		long max = root.ConstantDataContainer.GetValueInt("SoulWeaponReinforceMaxRate");
		if (max <= 0)
		{
			max = 100000L;
		}
		List<SoulWeaponEtherData> ethers = new List<SoulWeaponEtherData>();
		IList<SoulWeaponEtherData> all = root.SoulWeaponEtherDataContainer.GetAllData();
		if (all != null)
		{
			foreach (SoulWeaponEtherData ether in all)
			{
				if (ether != null)
				{
					ethers.Add(ether);
				}
			}
		}
		ethers.Sort(delegate(SoulWeaponEtherData a, SoulWeaponEtherData b)
		{
			int order = a.ExpPoints.CompareTo(b.ExpPoints);
			return (order != 0) ? order : ItemIndex(a.ItemCode).CompareTo(ItemIndex(b.ItemCode));
		});
		Dictionary<string, long> owned = new Dictionary<string, long>();
		Dictionary<string, long> used = new Dictionary<string, long>();
		foreach (SoulWeaponEtherData ether in ethers)
		{
			owned[ether.ItemCode] = account.ItemManager.GetItemCount(ether.ItemCode);
		}
		long gold = account.UserManager.Gold;
		int level = info.ReinforceLevel;
		long exp = info.Exp;
		long ratio = info.ReinforceRatio;
		Plan plan = new Plan
		{
			From = level
		};
		while (true)
		{
			SoulWeaponReinforceData data = root.SoulWeaponReinforceDataContainer.GetData(level);
			if (data == null || root.SoulWeaponReinforceDataContainer.GetData(level + 1) == null)
			{
				break;
			}
			bool added = false;
			foreach (SoulWeaponEtherData ether in ethers)
			{
				long expLeft = data.RequiredExp - exp;
				long rateLeft = max - ratio;
				if (expLeft <= 0 && rateLeft <= 0)
				{
					break;
				}
				if (data.UnAvailableItemCode != null && Array.IndexOf(data.UnAvailableItemCode, ether.ItemCode) >= 0)
				{
					continue;
				}
				SoulWeaponEtherRateData rate = root.SoulWeaponEtherRateDataContainer.GetData(new SoulWeaponEtherRateKey(level, ether.ItemCode));
				if (rate == null)
				{
					continue;
				}
				long count = Math.Min(Math.Max(Need(expLeft, ether.ExpPoints), Need(rateLeft, rate.AddedBonusRate)), owned[ether.ItemCode]);
				if (ether.ReqGold > 0)
				{
					count = Math.Min(count, gold / ether.ReqGold);
				}
				if (count <= 0)
				{
					continue;
				}
				gold -= ether.ReqGold * count;
				plan.Gold += ether.ReqGold * count;
				owned[ether.ItemCode] -= count;
				added = true;
				used[ether.ItemCode] = (used.TryGetValue(ether.ItemCode, out var before) ? before : 0) + count;
				exp = Math.Min(exp + ether.ExpPoints * count, data.RequiredExp);
				ratio = Math.Min(ratio + rate.AddedBonusRate * count, max);
			}
			if (exp < data.RequiredExp || ratio < max || gold < data.ReqGold)
			{
				plan.Leftover = added;
				break;
			}
			gold -= data.ReqGold;
			plan.Gold += data.ReqGold;
			level++;
			exp = 0L;
			ratio = 0L;
		}
		plan.To = level;
		foreach (SoulWeaponEtherData ether in ethers)
		{
			if (used.TryGetValue(ether.ItemCode, out var count))
			{
				plan.Used.Add(new KeyValuePair<string, long>(ether.ItemCode, count));
			}
		}
		return plan;
	}

	private static long Need(long left, long per)
	{
		return (left > 0 && per > 0) ? ((left + per - 1) / per) : 0L;
	}

	private static int ItemIndex(string code)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
		return (data == null) ? int.MaxValue : data.Index;
	}
}
