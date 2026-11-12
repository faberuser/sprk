using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ValanceCreatureFilterDataContainer : JitDataContainerDefault<int, ValanceCreatureFilterData>
{
	public const string TABLENAME = "ValanceCreatureFilterTable";

	private JitDictionary<int, List<ValanceCreatureFilterData>> _dicGroup = new JitDictionary<int, List<ValanceCreatureFilterData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ValanceCreatureFilterTable");

	public ValanceCreatureFilterDataContainer()
	{
		base.Packer.RegisterAndBind(_dicGroup);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ValanceCreatureFilterData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicGroup.JsonLoad_AddToListValue(item.GroupIndex, item);
		}
		_dicGroup.JsonLoad_SortListValue((ValanceCreatureFilterData lhs, ValanceCreatureFilterData rhs) => lhs.SortingOrder.CompareTo(rhs.SortingOrder));
	}

	public IList<ValanceCreatureFilterData> GetDataListByGroupIndex(int groupIndex)
	{
		return _dicGroup.GetOrDefault(groupIndex);
	}
}
