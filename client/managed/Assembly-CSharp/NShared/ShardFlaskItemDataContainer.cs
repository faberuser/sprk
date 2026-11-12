using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class ShardFlaskItemDataContainer : JitDataContainerDefault<int, ShardFlaskItemData>
{
	public const string TABLENAME = "ShardFlaskItemTable";

	private JitDictionary<int, List<ShardFlaskItemData>> _lists_by_GroupIndex = new JitDictionary<int, List<ShardFlaskItemData>>();

	private JitDictionary<int, int> _shardItemIndex_to_shardFlaskIndex = new JitDictionary<int, int>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShardFlaskItemTable");

	public ShardFlaskItemDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_GroupIndex);
		base.Packer.RegisterAndBind(_shardItemIndex_to_shardFlaskIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShardFlaskItemData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_GroupIndex.JsonLoad_AddToListValue(item.GroupIndex, item);
			_shardItemIndex_to_shardFlaskIndex.JsonLoad_AddIfNotExist(item.ShardItemIndex, item.Index);
		}
		_lists_by_GroupIndex.JsonLoad_SortListValue((ShardFlaskItemData lhs, ShardFlaskItemData rhs) => lhs.ShowIndex.CompareTo(rhs.ShowIndex));
	}

	public IList<ShardFlaskItemData> GetShardFlaskDataList(int groupIndex)
	{
		return _lists_by_GroupIndex.GetOrDefault(groupIndex);
	}

	public int GetDefaultFlaskIndex(int groupIndex)
	{
		var list = GetShardFlaskDataList(groupIndex);
		return list == null ? 0 : list.Where(v => v.IsDefault).Select(v => v.Index).FirstOrDefault();
	}

	public bool IsDefaultShardFlask(int shardItemIndex)
	{
		int orDefault = _shardItemIndex_to_shardFlaskIndex.GetOrDefault(shardItemIndex);
		return GetData(orDefault)?.IsDefault ?? false;
	}
}
