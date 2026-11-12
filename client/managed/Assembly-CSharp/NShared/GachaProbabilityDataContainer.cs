using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GachaProbabilityDataContainer : JitDataContainerDefault<ProbabilityDataKey2, GachaProbabilityData>
{
	public const string TABLENAME = "GachaProbabilityTable";

	private JitDictionary<int, List<GachaProbabilityData>> _lists_by_GachaIndex = new JitDictionary<int, List<GachaProbabilityData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("GachaProbabilityTable");

	public GachaProbabilityDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_GachaIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<ProbabilityDataKey2, GachaProbabilityData> item in base.BaseDic.JsonLoad_Get())
		{
			_lists_by_GachaIndex.JsonLoad_AddToListValue(item.Value.GachaIndex, item.Value);
		}
	}

	public IList<GachaProbabilityData> GetProbabilityInfoList(int gachaIndex)
	{
		return _lists_by_GachaIndex.GetOrDefault(gachaIndex);
	}
}
