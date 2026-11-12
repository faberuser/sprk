using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ItemProbabilityDataContainer : JitDataContainerDefault<ProbabilityDataKey2, ItemProbabilityData>
{
	public const string TABLENAME = "ItemProbabilityTable";

	private JitDictionary<int, List<ItemProbabilityData>> _lists_by_ItemIndex = new JitDictionary<int, List<ItemProbabilityData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ItemProbabilityTable");

	public ItemProbabilityDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_ItemIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ItemProbabilityData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_ItemIndex.JsonLoad_AddToListValue(item.ItemIndex, item);
		}
	}

	public ItemProbabilityData GetData(int itemIndex, int subIndex)
	{
		return GetData(new ProbabilityDataKey2(itemIndex, subIndex));
	}

	public IList<ItemProbabilityData> GetProbabilityInfoList(int itemIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_lists_by_ItemIndex.GetOrDefault(itemIndex));
	}
}
