using System;
using System.Linq;
using System.Reflection;
using System.Text;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGame2.NBattleContext;
using NShared;
using UnityEngine;

/// Renders RaidSinglePartySetting from its prefab on the Login screen and lays
/// out the empty slots for each Field Raid the way the native window does.
public static class SprkFieldRaidPartyProbe
{
	private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly int[] FieldRaids = new int[4] { 41, 42, 43, 44 };

	private static void Check(bool value, string reason)
	{
		if (!value)
		{
			throw new Exception("Field raid party: " + reason);
		}
	}

	private static RaidSinglePartySetting Show()
	{
		string shown = (string)Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
			.Invoke(null, new object[1] { "show:RaidSinglePartySetting" });
		GameObject root = GameObject.Find("TicketSearchShow_RaidSinglePartySetting");
		Check(root != null, shown);
		RaidSinglePartySetting window = root.GetComponent<RaidSinglePartySetting>();
		Check(window != null, "window component");
		return window;
	}

	/// Native SetSquardEmptySlot for one raid row, then the geometry of the visible slots.
	private static Layout Apply(RaidSinglePartySetting window, int raidIndex)
	{
		RaidData raid = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, 1);
		Check(raid != null && raid.Type == RaidType.FieldRaidSingle, "raid " + raidIndex + " is a single field raid");
		typeof(RaidPartySetting).GetField("_raidData", Any).SetValue(window, raid);
		typeof(RaidSinglePartySetting).GetMethod("SetSquardEmptySlot", Any).Invoke(window, null);
		Squard squard = (Squard)typeof(BattlePartySetting).GetField("_currentSquard", Any).GetValue(window);
		bool onlyMain = window.OnlyMainSquard.activeSelf;
		Grid main = Measure(squard.GetMainEmptySlotGrid(), squard.GetMainSquardSlotCount());
		Grid sub = Measure(squard.GetSubEmptySlotGrid(), squard.GetSubSquardSlotCount());
		Fill(squard, squard.MainSquard, true, raid.MainPartyCount, 0, ref main);
		Fill(squard, squard.SubSquard, false, raid.SubPartyCount, raid.MainPartyCount, ref sub);
		return new Layout
		{
			Raid = raidIndex,
			Main = raid.MainPartyCount,
			Sub = raid.SubPartyCount,
			OnlyMain = onlyMain,
			MainGrid = main,
			SubGrid = sub,
			Frame = Width(onlyMain ? window.OnlyMainSquardFrame : window.MainSubSquardFrame)
		};
	}

	/// Places real hero cards on the empty slots, as a saved party does when the window opens.
	private static void Fill(Squard squard, SquardPanel panel, bool main, int count, int skip, ref Grid grid)
	{
		if (panel == null || count <= 0)
		{
			return;
		}
		panel.ReleaseHeroPortraitAnim();
		CreatureData[] heroes = (from h in GameRoot.Instance.CreatureDataContainer.GetAllData()
			where h.OpenType == CreatureOpenType.Opened
			select h).Skip(skip).Take(count).ToArray();
		HeroPortraitComponent.OwnerInfo owner = new HeroPortraitComponent.OwnerInfo();
		for (int i = 0; i < heroes.Length; i++)
		{
			CachedHeroInfo info = new CachedHeroInfo(heroes[i]);
			info.Level = 100;
			info.Star = 5;
			info.Transcended = 5;
			Vector3 position = main ? squard.GetMainEmptySlotPositionByIndex(i) : squard.GetSubEmptySlotPositionByIndex(i);
			squard.CreateHeroPortraitAnim(main, info, position, owner, isControllerable: false, delegate
			{
			});
		}
		Transform[] cards = (from a in panel.GetAllHeroPortraitAnim()
			where a != null && a.Component != null
			select a.Component.transform into t
			orderby t.position.x
			select t).ToArray();
		if (cards.Length == 0)
		{
			return;
		}
		Transform space = panel.GetEmptySlotGrid().transform;
		Bounds first = NGUIMath.CalculateRelativeWidgetBounds(space, cards[0]);
		grid.Card = first.size.x;
		if (cards.Length > 1)
		{
			Bounds second = NGUIMath.CalculateRelativeWidgetBounds(space, cards[1]);
			grid.CardGap = second.min.x - first.max.x;
		}
	}

	private static float Width(GameObject go)
	{
		if (go == null)
		{
			return 0f;
		}
		UIWidget widget = go.GetComponent<UIWidget>();
		if (widget != null)
		{
			return widget.width;
		}
		return NGUIMath.CalculateRelativeWidgetBounds(go.transform).size.x;
	}

	private static Grid Measure(UIGrid grid, int count)
	{
		Grid result = new Grid { Count = count };
		if (grid == null || count <= 0)
		{
			return result;
		}
		result.Cell = grid.cellWidth;
		Transform[] slots = (from Transform t in grid.transform
			where t.gameObject.activeSelf
			orderby t.localPosition.x
			select t).ToArray();
		result.Count = slots.Length;
		if (slots.Length > 0)
		{
			result.Slot = NGUIMath.CalculateRelativeWidgetBounds(slots[0]).size.x * slots[0].localScale.x;
			result.Span = slots[slots.Length - 1].localPosition.x - slots[0].localPosition.x + result.Slot;
		}
		return result;
	}

	private struct Grid
	{
		public int Count;

		public float Cell;

		public float Slot;

		public float Span;

		public float Gap => Mathf.Abs(Cell) - Slot;

		public float Card;

		public float CardGap;

		public override string ToString()
		{
			return Count + "x cell" + Cell.ToString("0") + " slot" + Slot.ToString("0") + " gap" + Gap.ToString("0") + " span" + Span.ToString("0") + " card" + Card.ToString("0") + " cardGap" + CardGap.ToString("0");
		}
	}

	private struct Layout
	{
		public int Raid;

		public int Main;

		public int Sub;

		public bool OnlyMain;

		public Grid MainGrid;

		public Grid SubGrid;

		public float Frame;

		public override string ToString()
		{
			return "raid" + Raid + " " + Main + "+" + Sub + (OnlyMain ? " only-main" : " main/sub") + " frame" + Frame.ToString("0") + " main[" + MainGrid + "]" + (Sub > 0 ? " sub[" + SubGrid + "]" : "");
		}
	}

	public static string Run(string mode)
	{
		if (mode == "punishment-spacings")
		{
			string shownText = (string)Assembly.Load("SprkTicketSearch").GetType("SprkTicketSearchProbe").GetMethod("Run")
				.Invoke(null, new object[1] { "show:PunishmentRaidPartySetting" });
			GameObject root = GameObject.Find("TicketSearchShow_PunishmentRaidPartySetting");
			Check(root != null, shownText);
			NGame2.NUI.NWindow.NPunish.PunishmentRaidPartySetting punish = root.GetComponent<NGame2.NUI.NWindow.NPunish.PunishmentRaidPartySetting>();
			return "punishment only-main " + string.Join(" ", punish.OnlyMainPartySlotSpacings.Select((RaidPartySetting.PartySlotSpacing s) => s.MinSlotCount + "-" + s.MaxSlotCount + ":" + s.Spacing).ToArray()) + " main/sub " + string.Join(" ", punish.PartySlotSpacings.Select((RaidPartySetting.PartySlotSpacing s) => s.MinSlotCount + "-" + s.MaxSlotCount + ":" + s.Spacing).ToArray());
		}
		RaidSinglePartySetting window = Show();
		if (mode == "measure")
		{
			StringBuilder text = new StringBuilder("spacings");
			foreach (RaidPartySetting.PartySlotSpacing spacing in window.PartySlotSpacings)
			{
				text.Append(" ").Append(spacing.MinSlotCount).Append("-").Append(spacing.MaxSlotCount).Append(":").Append(spacing.Spacing);
			}
			foreach (BattlePartySetting.SquardUi ui in window.SquardUis)
			{
				text.Append("\nprefab ").Append(ui.Type).Append(" main cell").Append(ui.Squard.MainSquard?.EmptySlotGrid?.cellWidth.ToString("0") ?? "-")
					.Append(" sub cell").Append(ui.Squard.SubSquard?.EmptySlotGrid?.cellWidth.ToString("0") ?? "-");
			}
			foreach (int raid in FieldRaids)
			{
				text.Append("\n").Append(Apply(window, raid));
			}
			return text.ToString();
		}
		if (mode == "test")
		{
			StringBuilder text = new StringBuilder("PASS:");
			foreach (int raid in FieldRaids)
			{
				Layout layout = Apply(window, raid);
				// Archived RaidTable (Sept 2022): Lava 4, every other field raid 4+2.
				Check(layout.Main == 4 && layout.Sub == ((raid == 41) ? 0 : 2), "party size matches archive; " + layout);
				Check(layout.OnlyMain == (layout.Sub == 0), "layout type; " + layout);
				Check(layout.MainGrid.Count == layout.Main && layout.SubGrid.Count == layout.Sub || layout.Sub == 0, "slot counts; " + layout);
				if (layout.OnlyMain)
				{
					Check(Mathf.Abs(layout.MainGrid.Cell) == SprkFieldRaidParty.OnlyMainSpacing, "only-main stride; " + layout);
					Check(layout.MainGrid.Gap > 0f && layout.MainGrid.CardGap >= -1f, "only-main cards must not overlap; " + layout);
					Check(layout.MainGrid.Span < layout.Frame, "only-main slots fit the frame; " + layout);
				}
				else
				{
					Check(Mathf.Abs(layout.MainGrid.Cell) == 180f && Mathf.Abs(layout.SubGrid.Cell) == 180f, "main/sub keeps native stride; " + layout);
				}
				text.Append(" ").Append(layout).Append(";");
			}
			return text.ToString();
		}
		if (mode != null && mode.StartsWith("show:"))
		{
			Layout shownLayout = Apply(window, int.Parse(mode.Substring(5)));
			foreach (UIPanel panel in window.GetComponentsInChildren<UIPanel>(includeInactive: true))
			{
				panel.alpha = 1f;
			}
			return "shown " + shownLayout;
		}
		throw new ArgumentException("unknown mode " + mode);
	}
}
