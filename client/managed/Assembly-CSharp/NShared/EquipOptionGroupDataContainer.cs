using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EquipOptionGroupDataContainer : JitDataContainerDefault<int, EquipOptionGroupData>
{
	public const string TABLENAME = "EquipOptionGroupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipOptionGroupTable");

	public List<int> GetTotalEquipOptionIndices(EquipItemData equipItemData)
	{
		List<int> list = new List<int>();
		List<int> optionIndices = GetOptionIndices(equipItemData.OptionGroupIndex);
		if (optionIndices.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < optionIndices.Count; i++)
		{
			list.Add(optionIndices[i]);
		}
		if (equipItemData.OptionIndex != null && equipItemData.OptionIndex.Length != 0)
		{
			for (int j = 0; j < equipItemData.OptionIndex.Length; j++)
			{
				list.Add(equipItemData.OptionIndex[j]);
			}
		}
		return list;
	}

	public List<int> GetOptionIndices(int[] optionGroupIndices)
	{
		List<int> list = new List<int>();
		if (optionGroupIndices.Length == 0)
		{
			return list;
		}
		foreach (int key in optionGroupIndices)
		{
			EquipOptionGroupData data = GetData(key);
			if (data == null)
			{
				continue;
			}
			int[] optionIndex = data.OptionIndex;
			if (optionIndex != null && optionIndex.Length != 0)
			{
				for (int j = 0; j < optionIndex.Length; j++)
				{
					list.Add(optionIndex[j]);
				}
			}
		}
		return list;
	}
}
