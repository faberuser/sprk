using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ShopItemSortGroupDataContainer : JitDataContainerDefault<int, ShopItemSortGroupData>
{
	public const string TABLENAME = "ShopItemSortGroupTable";

	private JitDictionary<EShopItemFilterGroupType, List<ShopItemSortGroupData>> _lists_by_GroupType = new JitDictionary<EShopItemFilterGroupType, List<ShopItemSortGroupData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShopItemSortGroupTable");

	public ShopItemSortGroupDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_GroupType);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShopItemSortGroupData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_GroupType.JsonLoad_AddToListValue(item.GroupType, item);
		}
		_lists_by_GroupType.JsonLoad_SortListValue((ShopItemSortGroupData lhs, ShopItemSortGroupData rhs) => lhs.SortingOrder.CompareTo(rhs.SortingOrder));
	}

	public IList<ShopItemSortGroupData> GetDataListByGroupType(EShopItemFilterGroupType filterType)
	{
		return _lists_by_GroupType.GetOrDefault(filterType);
	}
}
