using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.NValance;

public class ValanceEquipFilterImpl : IValanceFilterImpl
{
	public HashSet<EquipItemSetGroupType> _hashSetType = new HashSet<EquipItemSetGroupType>();

	public HashSet<CreatureTagType> _hashCreatureTagTypes = new HashSet<CreatureTagType>();

	public HashSet<EquipPartType> _hashEquipPartType = new HashSet<EquipPartType>();

	public HashSet<EquipItemSubType> _hasEquipItemSubType = new HashSet<EquipItemSubType>();

	public void Clear()
	{
		_hashSetType.Clear();
		_hashCreatureTagTypes.Clear();
		_hashEquipPartType.Clear();
		_hasEquipItemSubType.Clear();
	}

	public bool IsNoneSelected()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashSetType) && __Extension_Method_ICollection__.IsNullOrEmpty(_hashCreatureTagTypes) && __Extension_Method_ICollection__.IsNullOrEmpty(_hashEquipPartType))
		{
			return __Extension_Method_ICollection__.IsNullOrEmpty(_hasEquipItemSubType);
		}
		return false;
	}

	public bool Filter<T>(T info) where T : IValanceEquipFilterInfo
	{
		if (info == null)
		{
			return false;
		}
		if (SetTypeContains(info.SetType) && CreatureTagContains(info) && EquipTypeContains(info))
		{
			return true;
		}
		return false;
	}

	private bool SetTypeContains(EquipItemSetGroupType setType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashSetType))
		{
			return true;
		}
		return _hashSetType.Contains(setType);
	}

	private bool CreatureTagContains(IValanceEquipFilterInfo info)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashCreatureTagTypes))
		{
			return true;
		}
		if (info.IsCreatureTagTypeArray)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info.CreatureTagTypes); i++)
			{
				CreatureTagType item = info.CreatureTagTypes[i];
				if (_hashCreatureTagTypes.Contains(item))
				{
					return true;
				}
			}
		}
		else if (_hashCreatureTagTypes.Contains(info.CreatureTagType))
		{
			return true;
		}
		return false;
	}

	private bool EquipTypeContains(IValanceEquipFilterInfo info)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashEquipPartType) && __Extension_Method_ICollection__.IsNullOrEmpty(_hasEquipItemSubType))
		{
			return true;
		}
		if (info.EquipPartType == EquipPartType.Accessory)
		{
			if (_hasEquipItemSubType.Contains(info.EquipItemSubType))
			{
				return true;
			}
		}
		else if (_hashEquipPartType.Contains(info.EquipPartType))
		{
			return true;
		}
		return false;
	}

	public EValanceFilterSelected Select(IValanceFilterInfo info)
	{
		if (!(info is IValanceEquipFilterInfo valanceEquipFilterInfo))
		{
			return EValanceFilterSelected.UnSelect;
		}
		if (SelectSetType(valanceEquipFilterInfo.SetType))
		{
			return EValanceFilterSelected.SetType;
		}
		if (SelectCreatureTagType(valanceEquipFilterInfo.CreatureTagType))
		{
			return EValanceFilterSelected.CreatureTagType;
		}
		if (SelectEquipPart(valanceEquipFilterInfo.EquipPartType, valanceEquipFilterInfo.EquipItemSubType))
		{
			return EValanceFilterSelected.EquipPartType;
		}
		return EValanceFilterSelected.UnSelect;
	}

	public bool SelectSetType(EquipItemSetGroupType setType)
	{
		if (setType == EquipItemSetGroupType.None)
		{
			return false;
		}
		return Select(_hashSetType, setType);
	}

	public bool SelectCreatureTagType(CreatureTagType creatureTagType)
	{
		if (creatureTagType == CreatureTagType.None)
		{
			return false;
		}
		return Select(_hashCreatureTagTypes, creatureTagType);
	}

	public bool SelectEquipPart(EquipPartType equipPartType, EquipItemSubType equipItemSubType)
	{
		switch (equipPartType)
		{
		case EquipPartType.None:
			return false;
		case EquipPartType.Accessory:
		{
			bool num = SelectEquipItemSubType(equipItemSubType);
			bool flag = _hashEquipPartType.Contains(equipPartType);
			if (num != flag)
			{
				SelectEquipPartType(equipPartType);
			}
			return num;
		}
		default:
			return SelectEquipPartType(equipPartType);
		}
	}

	private bool SelectEquipPartType(EquipPartType equipPartType)
	{
		return Select(_hashEquipPartType, equipPartType);
	}

	private bool SelectEquipItemSubType(EquipItemSubType equipItemSubType)
	{
		return Select(_hasEquipItemSubType, equipItemSubType);
	}

	public bool Select<T>(HashSet<T> hash, T value)
	{
		if (hash.Contains(value))
		{
			hash.Remove(value);
			return false;
		}
		hash.Add(value);
		return true;
	}
}
