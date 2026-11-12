using System.Collections.Generic;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class HeroPresetStorageSlotDataContainer : JitDataContainerDefault<int, HeroPresetStorageSlotData>
{
	[MessagePackObject(false)]
	public class ContainerInfo
	{
		[Key(0)]
		public int FreeSlotCount { get; set; }

		[Key(1)]
		public int PaidSlotCount { get; set; }
	}

	public const string TABLENAME = "HeroPresetStorageSlotTable";

	private JitRef<ContainerInfo> _containerInfo = new JitRef<ContainerInfo>();

	private JitRef<List<HeroPresetStorageSlotData>> _dataList_of_Paid = new JitRef<List<HeroPresetStorageSlotData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("HeroPresetStorageSlotTable");

	public HeroPresetStorageSlotDataContainer()
	{
		base.Packer.RegisterAndBind(_containerInfo);
		base.Packer.RegisterAndBind(_dataList_of_Paid);
	}

	protected override void OnEndLoadFromJsonData()
	{
		ContainerInfo containerInfo = _containerInfo.JsonLoad_GetOrNew();
		foreach (HeroPresetStorageSlotData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!item.IsPaid)
			{
				containerInfo.FreeSlotCount++;
				continue;
			}
			containerInfo.PaidSlotCount++;
			_dataList_of_Paid.JsonLoad_GetOrNew().Add(item);
		}
		_dataList_of_Paid.JsonLoad_GetOrNew().Sort((HeroPresetStorageSlotData lhs, HeroPresetStorageSlotData rhs) => lhs.Index.CompareTo(rhs.Index));
	}

	public int GetFreeSlotCount()
	{
		return _containerInfo.Eval().FreeSlotCount;
	}

	public int GetPaySlotCount()
	{
		return _containerInfo.Eval().PaidSlotCount;
	}

	public IList<HeroPresetStorageSlotData> GetPaySlotDataList()
	{
		return _dataList_of_Paid.Eval();
	}

	public HeroPresetStorageSlotData GetPrevPurchaseSlotData(HeroPresetStorageSlotData heroPresetStorageSlotData)
	{
		if (_dataList_of_Paid.Eval().Count == 0)
		{
			return null;
		}
		int num = _dataList_of_Paid.Eval().IndexOf(heroPresetStorageSlotData);
		if (num == 0 || num == -1)
		{
			return null;
		}
		return _dataList_of_Paid.Eval()[num - 1];
	}
}
