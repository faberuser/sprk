using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ShopItemSortDataContainer : JitDataContainerDefault<int, ShopItemSortData>
{
	public const string TABLENAME = "ShopItemSortTable";

	private JitDictionary<int, List<ShopItemSortData>> _lists_by_GroupIndex = new JitDictionary<int, List<ShopItemSortData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShopItemSortTable");

	public ShopItemSortDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_GroupIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShopItemSortData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_GroupIndex.JsonLoad_AddToListValue(item.GroupIndex, item);
		}
		_lists_by_GroupIndex.JsonLoad_SortListValue((ShopItemSortData lhs, ShopItemSortData rhs) => lhs.SortingOrder.CompareTo(rhs.SortingOrder));
	}

	public IList<ShopItemSortData> GetDataListByGroupIndex(int groupIndex)
	{
		return _lists_by_GroupIndex.GetOrDefault(groupIndex);
	}
}
