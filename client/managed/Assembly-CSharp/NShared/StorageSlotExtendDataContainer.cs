using System;
using NShared.NJit;

namespace NShared;

public class StorageSlotExtendDataContainer : JitDataContainerDefault<int, StorageSlotExtendData>
{
	public const string TABLENAME = "StorageSlotExtendTable";

	public override TablePathDefine PathDefine => new TablePathDefine("StorageSlotExtendTable");

	public StorageSlotExtendData GetDataBySlotCount(int slotCount)
	{
		int num = 0;
		foreach (StorageSlotExtendData allDatum in GetAllData())
		{
			if (num < slotCount && slotCount <= allDatum.SlotCount)
			{
				return allDatum;
			}
			num = allDatum.SlotCount;
		}
		return null;
	}

	public int MaxSlotCount()
	{
		int num = 0;
		foreach (StorageSlotExtendData allDatum in GetAllData())
		{
			num = Math.Max(num, allDatum.SlotCount);
		}
		return num;
	}
}
