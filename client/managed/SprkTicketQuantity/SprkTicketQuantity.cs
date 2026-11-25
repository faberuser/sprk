using System;
using NGame2.NUI.NWindow;
using NShared;
using UnityEngine;

/// Quantity popup for unique weapon, unique treasure, artifact and soul weapon
/// tickets: raises the native 50-ticket cap to 1000, shows the popup's Max
/// button and mirrors it on the left with a "5★" button that enters the number
/// of tickets needed to reach 5 stars.
public static class SprkTicketQuantity
{
	public const int MaxCount = 1000;

	public const int TargetStar = 5;

	/// Soul weapon stones needed for 5 stars.
	public const int SoulStoneGoal = 7;

	public const string GoalButtonName = "SprkStarGoalButton";

	/// Copies of an equipment item at <paramref name="star"/> needed to merge
	/// into one 5-star item, when each merge combines two copies of one star.
	public static int EquipGoal(int star)
	{
		star = Math.Max(0, star);
		if (star >= TargetStar)
		{
			return 1;
		}
		return 1 << (TargetStar - star);
	}

	public static int EquipGoal(int ticketIndex, ItemDataContainer items)
	{
		WeaponUniqueSelectItemData data = items?.GetData(ticketIndex)?.GetSubData<WeaponUniqueSelectItemData>();
		return EquipGoal(data?.Star ?? 0);
	}

	/// Remembers where the prefab put Max, so other users of the popup keep it.
	private sealed class Layout : MonoBehaviour
	{
		public Vector3 maxPosition;

		public int maxWidth;
	}

	/// The widest widget is the button background; its children follow it.
	private static UIWidget Background(Transform button)
	{
		UIWidget best = null;
		foreach (UIWidget widget in button.GetComponentsInChildren<UIWidget>(includeInactive: true))
		{
			if (best == null || widget.width > best.width)
			{
				best = widget;
			}
		}
		return best;
	}

	private static void Resize(Transform button, int width)
	{
		UIWidget background = Background(button);
		if (background == null || width <= 0 || background.width == width)
		{
			return;
		}
		int shrink = background.width - width;
		foreach (UIWidget widget in button.GetComponentsInChildren<UIWidget>(includeInactive: true))
		{
			if (widget != background && widget.width > width)
			{
				widget.width = Mathf.Max(1, widget.width - shrink);
			}
		}
		background.width = width;
		NGUITools.UpdateWidgetCollider(background.gameObject);
	}

	/// Shows (goal > 0) or hides the 5★ button. Called on every open, because
	/// the popup may be reused for tickets that have no goal.
	public static void Attach(SelectItemMessagePopup popup, int goal)
	{
		if (popup == null || popup.Button_Max == null)
		{
			return;
		}
		Transform max = popup.Button_Max.transform;
		Layout layout = popup.GetComponent<Layout>();
		if (layout == null)
		{
			layout = popup.gameObject.AddComponent<Layout>();
			layout.maxPosition = max.localPosition;
			layout.maxWidth = Background(max)?.width ?? 0;
		}
		Transform existing = max.parent.Find(GoalButtonName);
		if (goal <= 0)
		{
			max.localPosition = layout.maxPosition;
			Resize(max, layout.maxWidth);
			if (existing != null)
			{
				existing.gameObject.SetActive(value: false);
			}
			return;
		}
		GameObject button = (existing != null) ? existing.gameObject : Build(popup);
		button.SetActive(value: true);
		Arrange(popup, button.transform);
		UIButton uIButton = button.GetComponent<UIButton>();
		uIButton.onClick.Clear();
		EventDelegate.Add(uIButton.onClick, delegate
		{
			popup.SetSelectCount(goal);
		});
	}

	/// Bounds of a control in <paramref name="space"/>'s local coordinates.
	private static Bounds Box(Transform space, Transform target)
	{
		Bounds world = NGUIMath.CalculateAbsoluteWidgetBounds(target);
		Bounds local = new Bounds(space.InverseTransformPoint(world.center), Vector3.zero);
		local.Encapsulate(space.InverseTransformPoint(world.min));
		local.Encapsulate(space.InverseTransformPoint(world.max));
		return local;
	}

	/// Row layout like the material ticket popup: [5★] [-] [count] [+] [Max].
	private static void Arrange(SelectItemMessagePopup popup, Transform goal)
	{
		Transform max = popup.Button_Max.transform;
		Transform space = max.parent;
		if (popup.Button_IncCount == null || popup.Button_DecCount == null)
		{
			return;
		}
		Bounds plus = Box(space, popup.Button_IncCount.transform);
		Bounds minus = Box(space, popup.Button_DecCount.transform);
		float gap = 24f;
		if (popup.Input_SelectCount != null)
		{
			Bounds input = Box(space, popup.Input_SelectCount.transform);
			gap = Mathf.Max(gap, plus.min.x - input.max.x);
		}
		// The prefab's Max is far wider than +/-; at full width both buttons
		// would stick out of the popup. Two +/- widths fits like the material popup.
		UIWidget plusBackground = Background(popup.Button_IncCount.transform);
		if (plusBackground != null)
		{
			Resize(max, plusBackground.width * 2);
			Resize(goal, plusBackground.width * 2);
		}
		Bounds own = Box(space, max);
		float half = own.extents.x;
		float offset = max.localPosition.x - own.center.x;
		max.localPosition = new Vector3(plus.max.x + gap + half + offset, plus.center.y + (max.localPosition.y - own.center.y), max.localPosition.z);
		goal.localPosition = new Vector3(minus.min.x - gap - half + offset, max.localPosition.y, max.localPosition.z);
	}

	private static GameObject Build(SelectItemMessagePopup popup)
	{
		Transform max = popup.Button_Max.transform;
		GameObject button = UnityEngine.Object.Instantiate(max.gameObject, max.parent, worldPositionStays: false);
		button.name = GoalButtonName;
		foreach (MonoBehaviour behaviour in button.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
		{
			// A localization component would restore the "Max" caption on enable.
			if (behaviour != null && behaviour.GetType().Name.IndexOf("Locali", StringComparison.Ordinal) >= 0)
			{
				UnityEngine.Object.DestroyImmediate(behaviour);
			}
		}
		foreach (UILabel label in button.GetComponentsInChildren<UILabel>(includeInactive: true))
		{
			label.text = TargetStar + "★";
		}
		return button;
	}
}
