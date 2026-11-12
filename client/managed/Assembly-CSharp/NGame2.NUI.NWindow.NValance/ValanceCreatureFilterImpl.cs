using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.NValance;

public class ValanceCreatureFilterImpl : IValanceFilterImpl
{
	public HashSet<SkillAttrType> _hashAttrType = new HashSet<SkillAttrType>();

	public HashSet<CreatureTagType> _hashCreatureTagType = new HashSet<CreatureTagType>();

	public bool IsNoneSelected()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashAttrType))
		{
			return __Extension_Method_ICollection__.IsNullOrEmpty(_hashCreatureTagType);
		}
		return false;
	}

	public void Clear()
	{
		_hashCreatureTagType.Clear();
		_hashAttrType.Clear();
	}

	public bool Filter<T>(T info) where T : IValanceCreatureFilterInfo
	{
		if (info == null)
		{
			return false;
		}
		if (SetAttrTypeContains(info.AttrType) && CreatureTagContains(info.CreatureTagType))
		{
			return true;
		}
		return false;
	}

	private bool SetAttrTypeContains(SkillAttrType atttType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashAttrType))
		{
			return true;
		}
		return _hashAttrType.Contains(atttType);
	}

	private bool CreatureTagContains(CreatureTagType tagType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_hashCreatureTagType))
		{
			return true;
		}
		return _hashCreatureTagType.Contains(tagType);
	}

	public IList<T> GetFilteredItemList<T>(IList<T> list) where T : IValanceCreatureFilterInfo
	{
		if (IsNoneSelected())
		{
			return list;
		}
		List<T> list2 = new List<T>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			T item = list[i];
			if ((__Extension_Method_ICollection__.IsNullOrEmpty(_hashAttrType) || _hashAttrType.Contains(item.AttrType)) && (__Extension_Method_ICollection__.IsNullOrEmpty(_hashCreatureTagType) || _hashCreatureTagType.Contains(item.CreatureTagType)))
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	public EValanceFilterSelected Select(IValanceFilterInfo info)
	{
		if (!(info is IValanceCreatureFilterInfo valanceCreatureFilterInfo))
		{
			return EValanceFilterSelected.UnSelect;
		}
		if (SelectAttrType(valanceCreatureFilterInfo.AttrType))
		{
			return EValanceFilterSelected.AttrType;
		}
		if (SelectCreatureTagType(valanceCreatureFilterInfo.CreatureTagType))
		{
			return EValanceFilterSelected.CreatureTagType;
		}
		return EValanceFilterSelected.UnSelect;
	}

	public bool SelectAttrType(SkillAttrType attrType)
	{
		if (attrType == SkillAttrType.None)
		{
			return false;
		}
		return Select(_hashAttrType, attrType);
	}

	public bool SelectCreatureTagType(CreatureTagType creatureTagType)
	{
		if (creatureTagType == CreatureTagType.None)
		{
			return false;
		}
		return Select(_hashCreatureTagType, creatureTagType);
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
