using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class WorldBossDailyAchievementDataContainer : JitDataContainerDefault<WorldBossDailyAchievemenDataKey, WorldBossDailyAchievementData>
{
	public const string TABLENAME = "WorldBossDailyAchievementTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldBossDailyAchievementTable");

	public WorldBossDailyAchievementData[] GetAllData(int worldBossIndex)
	{
		List<WorldBossDailyAchievementData> list = new List<WorldBossDailyAchievementData>();
		foreach (WorldBossDailyAchievementData allDatum in GetAllData())
		{
			if (allDatum.WorldBossIndex == worldBossIndex)
			{
				list.Add(allDatum);
			}
		}
		list.Sort((WorldBossDailyAchievementData x, WorldBossDailyAchievementData y) => x.Index.CompareTo(y.Index));
		return list.ToArray();
	}
}
