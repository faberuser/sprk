using NShared.NJit;

namespace NShared;

public class GuildArenaSeasonRewardDataContainer : JitDataContainerDefault<int, GuildArenaSeasonRewardData>
{
	public const string TABLENAME = "GuildArenaSeasonRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildArenaSeasonRewardTable");

	public GuildArenaSeasonRewardData GetGuildArenaSeasonRewardData(int tierIndex, int rank)
	{
		foreach (GuildArenaSeasonRewardData allDatum in GetAllData())
		{
			if (tierIndex == 0)
			{
				if (rank >= allDatum.MinRank && rank <= allDatum.MaxRank)
				{
					return allDatum;
				}
			}
			else if (tierIndex == allDatum.TierIndex)
			{
				return allDatum;
			}
		}
		return null;
	}
}
