using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class InventoryExtendDataContainer : JitDataContainerDefault<int, InventoryExtendData>
{
	public const string TABLENAME = "InventoryExtendTable";

	private JitDictionary<InventoryType, List<InventoryExtendData>> _lists_by_InventoryType = new JitDictionary<InventoryType, List<InventoryExtendData>>();

	private JitDictionary<InventoryType, InventoryExtendData> _maxDatas_by_InventoryType = new JitDictionary<InventoryType, InventoryExtendData>();

	public override TablePathDefine PathDefine => new TablePathDefine("InventoryExtendTable");

	public InventoryExtendDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_InventoryType);
		base.Packer.RegisterAndBind(_maxDatas_by_InventoryType);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (InventoryExtendData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_InventoryType.JsonLoad_AddToListValue(item.InventoryType, item);
		}
		foreach (KeyValuePair<InventoryType, List<InventoryExtendData>> item2 in _lists_by_InventoryType.JsonLoad_Get())
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(item2.Value))
			{
				continue;
			}
			InventoryExtendData inventoryExtendData = item2.Value[0];
			for (int i = 1; i < item2.Value.Count; i++)
			{
				InventoryExtendData inventoryExtendData2 = item2.Value[i];
				if (inventoryExtendData.EquipItemExtendCount < inventoryExtendData2.EquipItemExtendCount)
				{
					inventoryExtendData = inventoryExtendData2;
				}
			}
			_maxDatas_by_InventoryType.JsonLoad_AddIfNotExist(item2.Key, inventoryExtendData);
		}
	}

	public IList<InventoryExtendData> GetExtendDataList(InventoryType inventoryType = InventoryType.Inventory)
	{
		return _lists_by_InventoryType.GetOrDefault(inventoryType);
	}

	public int GetExtendMaxCount(InventoryType inventoryType = InventoryType.Inventory)
	{
		if (_maxDatas_by_InventoryType.TryGetValue(inventoryType, out var value))
		{
			return value.EquipItemExtendCount;
		}
		return -1;
	}

	public InventoryExtendData GetNextDataByExtendCount(int count, InventoryType inventoryType = InventoryType.Inventory)
	{
		foreach (InventoryExtendData allDatum in GetAllData())
		{
			if (allDatum.EquipItemExtendCount > count && allDatum.InventoryType == inventoryType)
			{
				return allDatum;
			}
		}
		_maxDatas_by_InventoryType.TryGetValue(inventoryType, out var value);
		return value;
	}
}
