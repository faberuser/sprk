using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class WorldBossScoreRewardDataContainer : JitDataContainerDefault<WorldBossScoreRewardDataKey, WorldBossScoreRewardData>
{
	public const string TABLENAME = "WorldBossScoreRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldBossScoreRewardTable");

	public WorldBossScoreRewardData[] GetAllData(int worldBossIndex)
	{
		List<WorldBossScoreRewardData> list = new List<WorldBossScoreRewardData>();
		foreach (WorldBossScoreRewardData allDatum in GetAllData())
		{
			if (allDatum.WorldBossIndex == worldBossIndex)
			{
				list.Add(allDatum);
			}
		}
		return list.ToArray();
	}
}
