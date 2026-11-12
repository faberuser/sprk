using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class MatchRewardDataContainer : JitDataContainerDefault<MatchRewardDataKey2, MatchRewardData>
{
	public const string TABLENAME = "MatchRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MatchRewardTable");

	public MatchRewardData GetData(int tierIndex, int minRank)
	{
		return GetData(new MatchRewardDataKey2(tierIndex, minRank));
	}

	public List<MatchRewardData> GetRewardDataListOfTierIndex(int tierIndex)
	{
		List<MatchRewardData> list = new List<MatchRewardData>();
		foreach (MatchRewardData allDatum in GetAllData())
		{
			if (allDatum.TierIndex == tierIndex)
			{
				list.Add(allDatum);
			}
		}
		return list;
	}
}
