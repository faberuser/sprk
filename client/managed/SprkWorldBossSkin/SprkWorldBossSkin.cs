using System;
using System.Collections.Generic;
using NShared;

/// <summary>
/// Event forms of the World Bosses. While the server announces the festival
/// (DropBonusEvent EventShop "SprkWorldBossChristmas" or "SprkWorldBossMoonRabbit";
/// LiveRules Festivals christmas-boss and moon-rabbit), the regular World Boss
/// keeps its stats, scoring and rewards but shows the archived event form:
/// Protianus as The Unstoppable Lonely King on the Christmas field, Mountain
/// Fortress as Moon Rabbit LuLu.
///
/// The getters that ask run on battle threads too, so the windows are captured
/// on the main thread when the events change (DropBonusEventManager) and are
/// compared here with the system clock plus the server offset.
/// </summary>
public static class SprkWorldBossSkin
{
	public const string Christmas = "SprkWorldBossChristmas";

	public const string MoonRabbit = "SprkWorldBossMoonRabbit";

	private sealed class Window
	{
		public string Code;

		public DateTime Begin;

		public DateTime End;
	}

	private static volatile Window[] _windows = new Window[0];

	private static long _offsetTicks;

	/// <summary>Main thread: the announced event-form windows and the server clock.</summary>
	public static void Refresh(IEnumerable<DropBonusEvent> events, DateTime serverUtcNow)
	{
		List<Window> windows = new List<Window>();
		if (events != null)
		{
			foreach (DropBonusEvent e in events)
			{
				if (e == null || e.BonusType != EventBonusType.EventShop || (e.Condition != Christmas && e.Condition != MoonRabbit))
				{
					continue;
				}
				if (Vespa_String_Extension_Method2.TryParseNetworkTime2(e.BeginTime, out var begin) && Vespa_String_Extension_Method2.TryParseNetworkTime2(e.EndTime, out var end))
				{
					windows.Add(new Window
					{
						Code = e.Condition,
						Begin = begin,
						End = end
					});
				}
			}
		}
		System.Threading.Interlocked.Exchange(ref _offsetTicks, (serverUtcNow - DateTime.UtcNow).Ticks);
		_windows = windows.ToArray();
	}

	private static bool Active(string code)
	{
		Window[] windows = _windows;
		if (windows.Length == 0)
		{
			return false;
		}
		DateTime now = DateTime.UtcNow.AddTicks(System.Threading.Interlocked.Read(ref _offsetTicks));
		foreach (Window window in windows)
		{
			if (window.Code == code && window.Begin <= now && now < window.End)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Boss model (CampaignWaveData boss VisualInfo).</summary>
	public static string Visual(string visual)
	{
		if (visual == "WorldBoss.Poseidon" && Active(Christmas))
		{
			return "WorldBoss.EventPoseidon";
		}
		if (visual == "WorldBoss.MountainFortress" && Active(MoonRabbit))
		{
			return "WorldBoss.EventMountain";
		}
		return visual;
	}

	/// <summary>
	/// Boss costume (CreatureData.ModelPrefix): the event creatures 8501 and
	/// 8502 are these models with these prefixes.
	/// </summary>
	public static string ModelPrefix(int creature, string prefix)
	{
		if (creature == 5502 && Active(Christmas))
		{
			return "Cos17Christmas";
		}
		if (creature == 5501 && Active(MoonRabbit))
		{
			return "Cos18Chuseok";
		}
		return prefix;
	}

	/// <summary>Battle field (CampaignDungeonData.BattleField).</summary>
	public static string Field(string field)
	{
		if (field == "WorldBoss.PoseidonField1" && Active(Christmas))
		{
			return "WorldBoss.PoseidonField1Christmas";
		}
		return field;
	}

	/// <summary>World Boss screen art (WorldBossData.BossInfoImage).</summary>
	public static string BossInfoImage(string image)
	{
		if (image == "WorldBoss/BossInfo/WorldBoss_Poseidon" && Active(Christmas))
		{
			return "WorldBoss/BossInfo/WorldBoss_EventPoseidon";
		}
		if (image == "WorldBoss/BossInfo/WorldBoss_MountainFortress" && Active(MoonRabbit))
		{
			return "WorldBoss/BossInfo/WorldBoss_Lulu";
		}
		return image;
	}
}
