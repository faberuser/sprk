using System.Collections;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ItemGroupDataContainer : JitDataContainerDefault<string, ItemGroupData>
{
	public const string TABLENAME = "ItemGroupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ItemGroupTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, ItemGroupData value)
	{
		int num = 0;
		value.SubDataList = new List<ItemGroupData.SubData>();
		for (int i = 1; i <= 35; i++)
		{
			if (jsonDic.Contains("ItemCode" + i) && JsonMarshalerUsingReflection.Parse(typeof(ItemGroupData.SubData), jsonDic, string.Empty, i.ToString()) is ItemGroupData.SubData subData && ItemGroupData.IsValidItemCode(subData.ItemCode))
			{
				value.SubDataList.Add(subData);
				num += subData.ItemRate;
			}
		}
		if (num == 0)
		{
			num = 1;
		}
		value.TotalRate = num;
	}
}
