using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class ChallengeRaidRewardDataContainer : JitDataContainerDefault<int, ChallengeRaidRewardData>
{
	public const string TABLENAME = "ChallengeRaidRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ChallengeRaidRewardTable");

	public List<ChallengeRaidRewardData> GetChallengeRaidRewards(int challengeRaidIndex)
	{
		return (from v in GetAllData()
			where v.ChallengeRaidIndex == challengeRaidIndex
			select v).ToList();
	}
}
