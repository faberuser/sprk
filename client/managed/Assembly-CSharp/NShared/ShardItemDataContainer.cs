using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ShardItemDataContainer : JitDataContainerDefault<int, ShardItemData>
{
	public const string TABLENAME = "ShardItemTable";

	private JitDictionary<int, List<ShardItemData>> _lists_by_GroupIndex = new JitDictionary<int, List<ShardItemData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ShardItemTable");

	public ShardItemDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_GroupIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ShardItemData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_GroupIndex.JsonLoad_AddToListValue(item.GroupIndex, item);
		}
		_lists_by_GroupIndex.JsonLoad_SortListValue((ShardItemData lhs, ShardItemData rhs) => lhs.ShowIndex.CompareTo(rhs.ShowIndex));
	}

	public IList<ShardItemData> GetShardDataList(int groupIndex)
	{
		return _lists_by_GroupIndex.GetOrDefault(groupIndex);
	}
}
