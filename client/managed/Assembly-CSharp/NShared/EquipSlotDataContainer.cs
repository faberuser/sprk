using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class EquipSlotDataContainer : JitDataContainerDefault<int, EquipSlotData>
{
	public const string TABLENAME = "EquipSlotTable";

	private JitRef<Dictionary<EquipPartType, List<int>>> _indexLists_by_PartType = new JitRef<Dictionary<EquipPartType, List<int>>>();

	public override TablePathDefine PathDefine => new TablePathDefine("EquipSlotTable");

	public EquipSlotDataContainer()
	{
		base.Packer.RegisterAndBind(_indexLists_by_PartType);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EquipSlotData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!_indexLists_by_PartType.JsonLoad_GetOrNew().ContainsKey(item.EquipPartType))
			{
				_indexLists_by_PartType.JsonLoad_GetOrNew()[item.EquipPartType] = new List<int>();
			}
			_indexLists_by_PartType.JsonLoad_GetOrNew()[item.EquipPartType].Add(item.Index);
		}
	}

	public int GetEquipSlotIndexFor(EquipPartType partType)
	{
		if (!_indexLists_by_PartType.Eval().ContainsKey(partType))
		{
			return 0;
		}
		if (_indexLists_by_PartType.Eval()[partType].Count == 0)
		{
			return 0;
		}
		return _indexLists_by_PartType.Eval()[partType][0];
	}

	public int[] GetEquipSlotIndicesFor(EquipPartType partType)
	{
		if (!_indexLists_by_PartType.Eval().ContainsKey(partType))
		{
			return null;
		}
		if (_indexLists_by_PartType.Eval()[partType].Count == 0)
		{
			return null;
		}
		return _indexLists_by_PartType.Eval()[partType].ToArray();
	}

	public EquipPartType GetEquipPartTypeFor(int equipSlotIndex)
	{
		Dictionary<EquipPartType, List<int>>.Enumerator enumerator = _indexLists_by_PartType.Eval().GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.Contains(equipSlotIndex))
			{
				return enumerator.Current.Key;
			}
		}
		return EquipPartType.None;
	}
}
