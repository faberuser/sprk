using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NGame2.NAccount;
using NClientData;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

/// Login-screen fixture for the ticket quantity popup (automation action
/// `ticket-quantity-probe`). Modes: `equip` (unique weapon ticket, 1002 owned),
/// `soul` (soul weapon stone ticket goal of 7), `few` (5 owned: 5★ clamps to 5).
/// The popup is left open for a screenshot.
public static class SprkTicketQuantityProbe
{
	private static SelectItemMessagePopup popup;

	private static bool localized;

	private static void Prepare()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		if (!localized)
		{
			localized = true;
			LocalizationLoader instance = SingletonAuto<LocalizationLoader>.instance;
			instance.SetEnableLoadFromPatch(enableLoad: true);
			instance.ChangeLanguage(SystemLanguage.English);
		}
	}

	/// Partial fixture account (as in SprkTicketSearchProbe) so item icons can
	/// read inventory counts on the Login screen.
	private static void EnsureAccount()
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		PropertyInfo property = instance.GetType().GetProperty("AccountManagers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		object obj = property.GetValue(instance, null) ?? Activator.CreateInstance(property.PropertyType, nonPublic: true);
		foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
		{
			if (field.GetValue(obj) == null && !field.FieldType.IsAbstract && !field.FieldType.IsValueType)
			{
				try
				{
					field.SetValue(obj, Activator.CreateInstance(field.FieldType, nonPublic: true));
				}
				catch (Exception)
				{
				}
			}
		}
		property.SetValue(instance, obj, null);
	}

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Ticket quantity: " + message);
		}
	}

	private static ItemData Ticket(bool soul)
	{
		foreach (ItemData item in GameRoot.Instance.ItemDataContainer.GetAllData())
		{
			if (item == null)
			{
				continue;
			}
			bool isSoul = item.Type == ItemType.SoulStonePackageSelectItem;
			bool isEquip = item.Type == ItemType.WeaponUniqueSelectItem && item.GetSubData<WeaponUniqueSelectItemData>() != null && item.GetSubData<WeaponUniqueSelectItemData>().Star == 0;
			if (soul ? isSoul : isEquip)
			{
				return item;
			}
		}
		throw new Exception("Ticket quantity: no " + (soul ? "soul weapon" : "0-star unique weapon") + " ticket in ItemData");
	}

	private static int Count()
	{
		return int.Parse(popup.Input_SelectCount.value, CultureInfo.InvariantCulture);
	}

	private static void Click(UIButton button)
	{
		EventDelegate.Execute(button.onClick);
	}

	public static string Run(string mode)
	{
		foreach (SelectItemMessagePopup old in UnityEngine.Object.FindObjectsOfType<SelectItemMessagePopup>())
		{
			UnityEngine.Object.DestroyImmediate(old.gameObject);
		}
		popup = null;
		Prepare();
		EnsureAccount();
		bool soul = mode == "soul";
		int owned = (mode == "few") ? 5 : 1002;
		ItemData ticket = Ticket(soul);
		int goal = soul ? SprkTicketQuantity.SoulStoneGoal : SprkTicketQuantity.EquipGoal(ticket.Index, GameRoot.Instance.ItemDataContainer);
		Check(soul || goal == 32, "0-star equipment goal is " + goal + ", expected 32");
		Check(SprkTicketQuantity.EquipGoal(1) == 16 && SprkTicketQuantity.EquipGoal(2) == 8 && SprkTicketQuantity.EquipGoal(5) == 1, "star goals");
		SelectItemMessagePopup.OpenWithStarGoal("Notice", "probe", new List<Tuple<ItemData, int>>
		{
			new Tuple<ItemData, int>(ticket, owned)
		}, delegate
		{
		}, SprkTicketQuantity.MaxCount, goal);
		foreach (SelectItemMessagePopup open in UnityEngine.Object.FindObjectsOfType<SelectItemMessagePopup>())
		{
			if (open.gameObject.activeInHierarchy)
			{
				popup = open;
			}
		}
		Check(popup != null, "popup did not open");
		foreach (UIPanel panel in popup.GetComponentsInChildren<UIPanel>(includeInactive: true))
		{
			panel.alpha = 1f;
		}
		Check(popup.Button_Max != null && popup.Button_Max.gameObject.activeInHierarchy, "Max button hidden");
		Transform goalButton = popup.Button_Max.transform.parent.Find(SprkTicketQuantity.GoalButtonName);
		Check(goalButton != null && goalButton.gameObject.activeInHierarchy, "5-star button missing");
		Bounds maxBox = NGUIMath.CalculateAbsoluteWidgetBounds(popup.Button_Max.transform);
		Bounds starBox = NGUIMath.CalculateAbsoluteWidgetBounds(goalButton);
		Bounds plusBox = NGUIMath.CalculateAbsoluteWidgetBounds(popup.Button_IncCount.transform);
		Bounds minusBox = NGUIMath.CalculateAbsoluteWidgetBounds(popup.Button_DecCount.transform);
		string geometry = string.Format(CultureInfo.InvariantCulture, "minus={0:0.###}..{1:0.###} plus={2:0.###}..{3:0.###} max={4:0.###}..{5:0.###} star={6:0.###}..{7:0.###} y(plus/max/star)={8:0.###}/{9:0.###}/{10:0.###}", minusBox.min.x, minusBox.max.x, plusBox.min.x, plusBox.max.x, maxBox.min.x, maxBox.max.x, starBox.min.x, starBox.max.x, plusBox.center.y, maxBox.center.y, starBox.center.y);
		Check(maxBox.min.x > plusBox.max.x, "Max not right of plus: " + geometry);
		Check(starBox.max.x < minusBox.min.x, "5-star not left of minus: " + geometry);
		Check(Mathf.Abs(maxBox.center.y - plusBox.center.y) < 0.01f && Mathf.Abs(starBox.center.y - plusBox.center.y) < 0.01f, "buttons not on the count row: " + geometry);
		Check(Count() == 1, "initial count " + Count());
		Click(popup.Button_Max);
		int afterMax = Count();
		Check(afterMax == System.Math.Min(owned, SprkTicketQuantity.MaxCount), "Max gave " + afterMax);
		Click(goalButton.GetComponent<UIButton>());
		int afterGoal = Count();
		Check(afterGoal == System.Math.Min(goal, owned), "5-star gave " + afterGoal);
		string label = goalButton.GetComponentInChildren<UILabel>(includeInactive: true)?.text;
		Check(label == "5★", "5-star label is " + label);
		return string.Format(CultureInfo.InvariantCulture, "PASS: ticket={0} owned={1} goal={2} max={3} afterGoal={4} ", ticket.Index, owned, goal, afterMax, afterGoal) + geometry;
	}
}
