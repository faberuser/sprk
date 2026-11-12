using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class KarmaRewardExponentialDataContainer : JitDataContainerDefault<KarmaRewardExponentialKey, KarmaRewardExponentialData>
{
	public const string TABLENAME = "KarmaRewardExponentialTable";

	private JitDictionary<int, List<KarmaRewardExponentialData>> _rewardDicByGroupIndex = new JitDictionary<int, List<KarmaRewardExponentialData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("KarmaRewardExponentialTable");

	public KarmaRewardExponentialDataContainer()
	{
		base.Packer.RegisterAndBind(_rewardDicByGroupIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (KarmaRewardExponentialData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_rewardDicByGroupIndex.JsonLoad_AddToListValue(item.PunishmentGroupIndex, item);
		}
	}

	public IList<KarmaRewardExponentialData> GetDataWithGroupIndex(int punishGroupIndex)
	{
		return _rewardDicByGroupIndex.GetOrDefault(punishGroupIndex);
	}
}
