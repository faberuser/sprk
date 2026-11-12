using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class AffixDataContainer : JitDataContainerDefault<int, AffixData>
{
	public const string TABLENAME = "AffixTable";

	private JitDictionary<int, List<AffixData>> _lists_by_GroupIndex = new JitDictionary<int, List<AffixData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("AffixTable");

	public AffixDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_GroupIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (AffixData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_GroupIndex.JsonLoad_AddToListValue(item.GroupIndex, item);
		}
	}

	public IList<AffixData> GetAffixDataListByGroupIndex(int groupIndex)
	{
		return _lists_by_GroupIndex.GetOrDefault(groupIndex);
	}
}
