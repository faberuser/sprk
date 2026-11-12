using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ShopItemGroupDataContainer : JitDataContainerDefault<ShopItemGroupKey, ShopItemGroupData>
{
	public const string TABLENAME = "ShopItemGroupTable";

	private JitDictionary<int, HashSet<int>> _groupSets_by_ShopIndex = new JitDictionary<int, HashSet<int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShopItemGroupTable");

	public ShopItemGroupDataContainer()
	{
		base.Packer.RegisterAndBind(_groupSets_by_ShopIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShopItemGroupData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_groupSets_by_ShopIndex.JsonLoad_AddToHashSetValue(item.ShopIndex, item.GroupIndex);
		}
	}

	public ShopItemGroupData GetData(int ShopIndex, int GroupIndex)
	{
		return GetData(new ShopItemGroupKey(ShopIndex, GroupIndex));
	}

	public List<ShopItemGroupData> GetGroupDataList(int shopIndex)
	{
		HashSet<int> orDefault = _groupSets_by_ShopIndex.GetOrDefault(shopIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(orDefault))
		{
			return null;
		}
		List<ShopItemGroupData> list = new List<ShopItemGroupData>();
		foreach (int item in orDefault)
		{
			list.Add(GetData(shopIndex, item));
		}
		return list;
	}

	public int GetGroupIndexCount(int shopIndex)
	{
		return __Extension_Method_ICollection__.COUNT(_groupSets_by_ShopIndex.GetOrDefault(shopIndex));
	}
}
