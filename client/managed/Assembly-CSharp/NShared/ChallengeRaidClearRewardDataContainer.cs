using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ChallengeRaidClearRewardDataContainer : JitDataContainerDefault<ChallengeRaidClearRewardKey, ChallengeRaidClearRewardData>
{
	public const string TABLENAME = "ChallengeRaidClearRewardTable";

	private JitDictionary<int, List<ChallengeRaidClearRewardData>> _rewardDataByChallengeIndex = new JitDictionary<int, List<ChallengeRaidClearRewardData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ChallengeRaidClearRewardTable");

	public ChallengeRaidClearRewardDataContainer()
	{
		base.Packer.RegisterAndBind(_rewardDataByChallengeIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ChallengeRaidClearRewardData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_rewardDataByChallengeIndex.JsonLoad_AddToListValue(item.ChallengeRaidIndex, item);
		}
	}

	public IList<ChallengeRaidClearRewardData> GetRewardDataByChallengeRaidIndex(int challengeRaidIndex)
	{
		return _rewardDataByChallengeIndex.GetOrDefault(challengeRaidIndex);
	}
}
