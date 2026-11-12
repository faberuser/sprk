using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class CraftItemDataContainer : JitDataContainerDefault<int, CraftItemData>
{
	public const string TABLENAME = "CraftItemTable";

	private JitDictionary<string, List<CraftItemData>> _sortedLists_by_Code_IsOpened = new JitDictionary<string, List<CraftItemData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("CraftItemTable");

	public CraftItemDataContainer()
	{
		base.Packer.RegisterAndBind(_sortedLists_by_Code_IsOpened);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (CraftItemData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.IsOpen && !String_Extension_Method.IsNullOrEmpty(item.ItemCode))
			{
				_sortedLists_by_Code_IsOpened.JsonLoad_AddToListValue(item.ItemCode, item);
			}
		}
		_sortedLists_by_Code_IsOpened.JsonLoad_SortListValue((CraftItemData lhs, CraftItemData rhs) => lhs.CraftIndex.CompareTo(rhs.CraftIndex));
	}

	public IList<CraftItemData> GetCategoryData(string itemCode)
	{
		return _sortedLists_by_Code_IsOpened.GetOrDefault(itemCode);
	}
}
