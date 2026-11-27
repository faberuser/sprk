using System;
using System.Collections.Generic;
using NShared;

/// <summary>
/// Festival nodes on the Orvel Square. The archived event nodes all share the
/// Square's two spots, so festivals open together would stack. Every revealed
/// festival node (DropBonusEvent EventNode "99,&lt;node&gt;" in its window) from
/// those spots is instead placed on an ellipse around the central fountain,
/// evenly spaced in node order together with the Square's side-quest node,
/// however many are open.
///
/// The windows are captured on the main thread when the events change
/// (DropBonusEventManager), like SprkWorldBossSkin.
/// </summary>
public static class SprkSquareLayout
{
	public const int Chapter = 99;

	/// <summary>The Square's side-quest node ("!"), always on the ring's left.</summary>
	public const int SideQuestNode = 41;

	/// <summary>Fountain centre and ring radii, in world-map units.</summary>
	public const double CenterX = 2225.0;

	public const double CenterY = 2185.0;

	public const double RadiusX = 320.0;

	public const double RadiusY = 180.0;

	private sealed class Window
	{
		public int Node;

		public DateTime Begin;

		public DateTime End;
	}

	private static volatile Window[] _windows = new Window[0];

	/// <summary>
	/// Festival maps of Orvel and the EventShop codes that select them, first
	/// match wins, so Christmas's snowy town replaces the World Tree's once both
	/// are open. Lunar New Year uses Town2_NewYear2022 ("2022" on its banners;
	/// the only New Year map with a copy for every graphics setting). The client
	/// also ships Town2_NewYear2020/2021 and Town_1000Days, which no festival uses.
	/// </summary>
	private static readonly string[][] SeasonMaps = new string[6][]
	{
		new string[2] { "2021_Christmas_Shop", "Town_Christmas" },
		new string[2] { "2021_WorldTree_Shop", "Town2_WorldTree" },
		new string[2] { "2021_Halloween_Shop", "Town_Halloween" },
		new string[2] { "2018_Halloween_Shop", "Town_Halloween" },
		new string[2] { "2021_Orvel_Shop", "Town_OrvelDays" },
		new string[2] { "SprkLunarNewYear", "Town2_NewYear2022" }
	};

	private sealed class ShopWindow
	{
		public string Code;

		public DateTime Begin;

		public DateTime End;
	}

	private static volatile ShopWindow[] _shops = new ShopWindow[0];

	private static long _offsetTicks;

	/// <summary>
	/// Festival nodes on the Square: the archived ones share its two spots,
	/// (2536, 2380) and (2144, 2426); any table position near the fountain counts.
	/// </summary>
	public static bool OnSquare(int x, int y)
	{
		return Math.Abs(x - CenterX) <= 700.0 && Math.Abs(y - CenterY) <= 500.0;
	}

	public static void Refresh(IEnumerable<DropBonusEvent> events, DateTime serverUtcNow)
	{
		List<Window> windows = new List<Window>();
		List<ShopWindow> shops = new List<ShopWindow>();
		if (events != null)
		{
			foreach (DropBonusEvent e in events)
			{
				if (e != null && e.BonusType == EventBonusType.EventShop && Vespa_String_Extension_Method2.TryParseNetworkTime2(e.BeginTime, out var shopBegin) && Vespa_String_Extension_Method2.TryParseNetworkTime2(e.EndTime, out var shopEnd))
				{
					shops.Add(new ShopWindow
					{
						Code = e.Condition,
						Begin = shopBegin,
						End = shopEnd
					});
				}
				if (e == null || e.BonusType != EventBonusType.EventNode || string.IsNullOrEmpty(e.Condition))
				{
					continue;
				}
				string[] parts = e.Condition.Split(',');
				if (parts.Length != 2 || parts[0].Trim() != "99" || !int.TryParse(parts[1].Trim(), out var node))
				{
					continue;
				}
				if (Vespa_String_Extension_Method2.TryParseNetworkTime2(e.BeginTime, out var begin) && Vespa_String_Extension_Method2.TryParseNetworkTime2(e.EndTime, out var end))
				{
					windows.Add(new Window
					{
						Node = node,
						Begin = begin,
						End = end
					});
				}
			}
		}
		System.Threading.Interlocked.Exchange(ref _offsetTicks, (serverUtcNow - DateTime.UtcNow).Ticks);
		_windows = windows.ToArray();
		_shops = shops.ToArray();
	}

	/// <summary>
	/// Orvel's map (CampaignChapterData.Map of chapter 99): its World Tree form
	/// during the World Tree festival.
	/// </summary>
	public static string Map(int chapter, string map)
	{
		if (chapter != Chapter || map != "Town2")
		{
			return map;
		}
		ShopWindow[] shops = _shops;
		DateTime now = DateTime.UtcNow.AddTicks(System.Threading.Interlocked.Read(ref _offsetTicks));
		foreach (string[] season in SeasonMaps)
		{
			foreach (ShopWindow shop in shops)
			{
				if (shop.Code == season[0] && shop.Begin <= now && now < shop.End)
				{
					return season[1];
				}
			}
		}
		return map;
	}

	/// <summary>Festival nodes open now, in node order.</summary>
	public static int[] OpenNodes()
	{
		Window[] windows = _windows;
		DateTime now = DateTime.UtcNow.AddTicks(System.Threading.Interlocked.Read(ref _offsetTicks));
		SortedSet<int> open = new SortedSet<int>();
		foreach (Window window in windows)
		{
			if (window.Begin <= now && now < window.End)
			{
				open.Add(window.Node);
			}
		}
		int[] result = new int[open.Count];
		open.CopyTo(result);
		return result;
	}

	/// <summary>The ring in order: the side-quest node, then the open festival nodes.</summary>
	public static int[] Ring()
	{
		List<int> ring = new List<int> { SideQuestNode };
		foreach (int node in OpenNodes())
		{
			if (node != SideQuestNode)
			{
				ring.Add(node);
			}
		}
		return ring.ToArray();
	}

	public static int X(int chapter, int node, int x, int y)
	{
		return Place(chapter, node, x, y, horizontal: true);
	}

	public static int Y(int chapter, int node, int x, int y)
	{
		return Place(chapter, node, x, y, horizontal: false);
	}

	private static int Place(int chapter, int node, int x, int y, bool horizontal)
	{
		if (chapter != Chapter || !OnSquare(x, y))
		{
			return horizontal ? x : y;
		}
		int[] ring = Ring();
		int index = Array.IndexOf(ring, node);
		if (index < 0)
		{
			return horizontal ? x : y;
		}
		double angle = Angle(index, ring.Length) * Math.PI / 180.0;
		return horizontal ? (int)Math.Round(CenterX + RadiusX * Math.Cos(angle)) : (int)Math.Round(CenterY + RadiusY * Math.Sin(angle));
	}

	/// <summary>
	/// Degrees (0 = right, -90 = top, 180 = left) of slot `index` of `count`:
	/// evenly spaced clockwise from the left, where the side-quest node stands.
	/// </summary>
	public static double Angle(int index, int count)
	{
		return 180.0 + index * 360.0 / count;
	}
}
