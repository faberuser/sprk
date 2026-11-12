using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class ShopItemDataContainer : JitDataContainerDefault<ShopAndIndexKey, ShopItemData>
{
	public const string TABLENAME = "ShopItemTable";

	private JitDictionary<ShopAndIndexKey, List<ShopItemData>> _lists_by_GroupShop = new JitDictionary<ShopAndIndexKey, List<ShopItemData>>();

	private JitDictionary<int, List<ShopItemData>> _lists_by_NoGroupShop = new JitDictionary<int, List<ShopItemData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShopItemTable");

	public ShopItemDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_GroupShop);
		base.Packer.RegisterAndBind(_lists_by_NoGroupShop);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShopItemData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.GroupIndex != 0)
			{
				___Shared_Jit_JitDictionary___.JsonLoad_AddToListValue(key: new ShopAndIndexKey(item.ShopIndex, item.GroupIndex), dic: _lists_by_GroupShop, value: item);
				continue;
			}
			_lists_by_NoGroupShop.JsonLoad_AddToListValue(item.ShopIndex, item);
		}
	}

	public ShopItemData GetData(int shopIndex, int index)
	{
		return GetData(new ShopAndIndexKey(shopIndex, index));
	}

	public List<ShopItemData> GetShopItemListByShopItemGroup(int shopIndex, int groupIndex)
	{
		return Internal_GetShopItemListByShopItemGroup(shopIndex, groupIndex)?.ToList();
	}

	private IList<ShopItemData> Internal_GetShopItemListByShopItemGroup(int shopIndex, int groupIndex)
	{
		return ___Shared_Jit_JitDictionary___.GetOrDefault(key: new ShopAndIndexKey(shopIndex, groupIndex), This: _lists_by_GroupShop);
	}

	public List<ShopItemData> GetShopItemList(int ShopIndex)
	{
		return Internal_GetShopItemList(ShopIndex)?.ToList();
	}

	private IList<ShopItemData> Internal_GetShopItemList(int ShopIndex)
	{
		return _lists_by_NoGroupShop.GetOrDefault(ShopIndex);
	}
}
