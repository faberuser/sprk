using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ValanceFilterGroupDataContainer : JitDataContainerDefault<int, ValanceFilterGroupData>
{
	public const string TABLENAME = "ValanceFilterGroupTable";

	private JitDictionary<EValanceFilterGroupType, List<ValanceFilterGroupData>> _dicGroup = new JitDictionary<EValanceFilterGroupType, List<ValanceFilterGroupData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ValanceFilterGroupTable");

	public ValanceFilterGroupDataContainer()
	{
		base.Packer.RegisterAndBind(_dicGroup);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ValanceFilterGroupData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicGroup.JsonLoad_AddToListValue(item.GroupType, item);
		}
		_dicGroup.JsonLoad_SortListValue((ValanceFilterGroupData lhs, ValanceFilterGroupData rhs) => lhs.SortingOrder.CompareTo(rhs.SortingOrder));
	}

	public IList<ValanceFilterGroupData> GetDataListByGroupType(EValanceFilterGroupType filterType)
	{
		return _dicGroup.GetOrDefault(filterType);
	}
}
