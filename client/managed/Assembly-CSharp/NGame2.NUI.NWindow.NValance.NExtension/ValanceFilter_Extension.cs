using NShared;

namespace NGame2.NUI.NWindow.NValance.NExtension;

public static class ValanceFilter_Extension
{
	public static EValanceFilterSelected GetFilterType(this IValanceEquipFilterInfo This)
	{
		if (This == null)
		{
			return EValanceFilterSelected.UnSelect;
		}
		if (This.SetType != EquipItemSetGroupType.None)
		{
			return EValanceFilterSelected.SetType;
		}
		if (This.CreatureTagType != CreatureTagType.None)
		{
			return EValanceFilterSelected.CreatureTagType;
		}
		if (This.EquipPartType != EquipPartType.None)
		{
			return EValanceFilterSelected.EquipPartType;
		}
		if (This.DisplayEquipOptionIndex != 0)
		{
			return EValanceFilterSelected.ExtraEquipOptionIndex;
		}
		return EValanceFilterSelected.UnSelect;
	}

	public static EValanceFilterSelected GetFilterType(this IValanceCreatureFilterInfo This)
	{
		if (This == null)
		{
			return EValanceFilterSelected.UnSelect;
		}
		if (This.AttrType != SkillAttrType.None)
		{
			return EValanceFilterSelected.AttrType;
		}
		if (This.CreatureTagType != CreatureTagType.None)
		{
			return EValanceFilterSelected.CreatureTagType;
		}
		return EValanceFilterSelected.UnSelect;
	}
}
