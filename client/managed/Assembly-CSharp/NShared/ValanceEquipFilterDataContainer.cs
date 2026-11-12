using System.Collections.Generic;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class ValanceEquipFilterDataContainer : JitDataContainerDefault<int, ValanceEquipFilterData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct SortingOrderKey2
	{
		[Key(0)]
		public EquipItemSetGroupType SetType;

		[Key(1)]
		public CreatureTagType TagType;

		[Key(2)]
		public EquipPartType PartType;

		[Key(3)]
		public EquipItemSubType ItemSubType;

		[Key(4)]
		public int ExtraSkillIndex;

		public SortingOrderKey2(IValanceEquipFilterInfo filterInfo)
			: this(filterInfo.SetType, filterInfo.IsCreatureTagTypeArray ? filterInfo.CreatureTagTypes[0] : filterInfo.CreatureTagType, filterInfo.EquipPartType, filterInfo.EquipItemSubType, filterInfo.DisplayEquipOptionIndex)
		{
		}

		public SortingOrderKey2(EquipItemSetGroupType setType = EquipItemSetGroupType.None, CreatureTagType tagType = CreatureTagType.None, EquipPartType partType = EquipPartType.None, EquipItemSubType itemSubType = EquipItemSubType.None, int extraSkillIndex = 0)
		{
			SetType = setType;
			TagType = tagType;
			PartType = partType;
			ItemSubType = itemSubType;
			ExtraSkillIndex = extraSkillIndex;
		}

		public bool Equals(SortingOrderKey2 other)
		{
			if (SetType == other.SetType && TagType == other.TagType && PartType == other.PartType && ItemSubType == other.ItemSubType)
			{
				return ExtraSkillIndex == other.ExtraSkillIndex;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(SetType, TagType, PartType, ItemSubType, ExtraSkillIndex);
		}

		public override string ToString()
		{
			return $"{GetType().Name}({SetType}, {TagType}, {PartType}, {ItemSubType}, {ExtraSkillIndex})";
		}
	}

	public const string TABLENAME = "ValanceEquipFilterTable";

	private JitDictionary<int, List<ValanceEquipFilterData>> _sortedLists_by_GroupIndex = new JitDictionary<int, List<ValanceEquipFilterData>>();

	private JitRef<Dictionary<SortingOrderKey2, int>> _sortingOrdersMap = new JitRef<Dictionary<SortingOrderKey2, int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("ValanceEquipFilterTable");

	public ValanceEquipFilterDataContainer()
	{
		base.Packer.RegisterAndBind(_sortedLists_by_GroupIndex);
		base.Packer.RegisterAndBind(_sortingOrdersMap);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ValanceEquipFilterData item in base.BaseList.JsonLoad_GetOrNew())
		{
			SortingOrderKey2 key = new SortingOrderKey2(item);
			_sortingOrdersMap.JsonLoad_GetOrNew().Add(key, item.SortingOrder);
		}
		foreach (ValanceEquipFilterData item2 in base.BaseList.JsonLoad_GetOrNew())
		{
			_sortedLists_by_GroupIndex.JsonLoad_AddToListValue(item2.GroupIndex, item2);
		}
		_sortedLists_by_GroupIndex.JsonLoad_SortListValue((ValanceEquipFilterData lhs, ValanceEquipFilterData rhs) => lhs.SortingOrder.CompareTo(rhs.SortingOrder));
	}

	public IList<ValanceEquipFilterData> GetDataListByGroupIndex(int groupIndex)
	{
		return _sortedLists_by_GroupIndex.GetOrDefault(groupIndex);
	}

	public int GetSortingOrder(EquipItemSetGroupType setType)
	{
		SortingOrderKey2 key = new SortingOrderKey2(setType);
		if (_sortingOrdersMap.Eval().TryGetValue(key, out var value))
		{
			return value;
		}
		return 0;
	}

	public int GetSortingOrder(CreatureTagType creatureTagType)
	{
		SortingOrderKey2 key = new SortingOrderKey2(EquipItemSetGroupType.None, creatureTagType);
		if (_sortingOrdersMap.Eval().TryGetValue(key, out var value))
		{
			return value;
		}
		return 0;
	}

	public int GetSortingOrder(EquipPartType equipPartType, EquipItemSubType equipItemSubType)
	{
		SortingOrderKey2 key = new SortingOrderKey2(EquipItemSetGroupType.None, CreatureTagType.None, equipPartType, (equipPartType == EquipPartType.Accessory) ? equipItemSubType : EquipItemSubType.None);
		if (_sortingOrdersMap.Eval().TryGetValue(key, out var value))
		{
			return value;
		}
		return 0;
	}
}
