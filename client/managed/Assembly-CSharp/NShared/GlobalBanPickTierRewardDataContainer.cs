using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GlobalBanPickTierRewardDataContainer : JitDataContainerDefault<MatchRewardDataKey2, MatchBanPickTierRewardData>
{
	public enum GlobalBanpickTier
	{
		Bronze = 1,
		Silver,
		Gold,
		Platinum,
		Diamond,
		Master,
		RoyalMaster,
		Challenge,
		Champion,
		Ranker_3rd,
		Ranker_2rd,
		Ranker_1st
	}

	public const string TABLENAME = "GlobalBanPickTierRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GlobalBanPickTierRewardTable");

	public MatchBanPickTierRewardData GetData(int tierIndex, int minRank)
	{
		return GetData(new MatchRewardDataKey2(tierIndex, minRank));
	}

	public List<MatchBanPickTierRewardData> GetRewardDataListOfTierIndex(int tierIndex)
	{
		List<MatchBanPickTierRewardData> list = new List<MatchBanPickTierRewardData>();
		foreach (MatchBanPickTierRewardData allDatum in GetAllData())
		{
			if (allDatum.TierIndex == tierIndex)
			{
				list.Add(allDatum);
			}
		}
		return list;
	}

	public GlobalBanpickTier ConvertTierIndexToTierEnum(int tierIndex)
	{
		if (tierIndex < 10)
		{
			return (GlobalBanpickTier)tierIndex;
		}
		return (GlobalBanpickTier)(tierIndex / 10);
	}
}
