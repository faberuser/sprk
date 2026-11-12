using System;
using System.Collections.Generic;
using NShared;

namespace NGame2;

public static class FilterHelper
{
	private static CreatureTagType[] ArrOrderCreatureTagType = new CreatureTagType[7]
	{
		CreatureTagType.Knight,
		CreatureTagType.Warrior,
		CreatureTagType.Assassin,
		CreatureTagType.Archer,
		CreatureTagType.Mechanic,
		CreatureTagType.Wizard,
		CreatureTagType.Priest
	};

	private static EquipPartType[] ArrOrderEquipPartType = new EquipPartType[7]
	{
		EquipPartType.Weapon,
		EquipPartType.Armor,
		EquipPartType.SecondGear,
		EquipPartType.Accessory,
		EquipPartType.Orb,
		EquipPartType.Artifact,
		EquipPartType.Treasure
	};

	private static List<CreatureTagType> _listOrderCreatureTagType = new List<CreatureTagType>();

	private static List<EquipPartType> _listOrderEquipPartType = new List<EquipPartType>();

	public static List<CreatureTagType> GetListOrderCreatureTagType()
	{
		if (_listOrderCreatureTagType == null)
		{
			_listOrderCreatureTagType = new List<CreatureTagType>();
			_listOrderCreatureTagType.Clear();
		}
		if (_listOrderCreatureTagType.Count <= 0)
		{
			_listOrderCreatureTagType.Clear();
			CreatureTagType[] arrOrderCreatureTagType = ArrOrderCreatureTagType;
			foreach (CreatureTagType item in arrOrderCreatureTagType)
			{
				_listOrderCreatureTagType.Add(item);
			}
			foreach (CreatureTagType value in Enum.GetValues(typeof(CreatureTagType)))
			{
				if (!_listOrderCreatureTagType.Contains(value) && value != CreatureTagType.None && value != CreatureTagType.End)
				{
					_listOrderCreatureTagType.Add(value);
				}
			}
		}
		return _listOrderCreatureTagType;
	}

	public static List<EquipPartType> GetListOrderEquipPartType()
	{
		if (_listOrderEquipPartType == null)
		{
			_listOrderEquipPartType = new List<EquipPartType>();
			_listOrderEquipPartType.Clear();
		}
		if (_listOrderEquipPartType.Count <= 0)
		{
			_listOrderEquipPartType.Clear();
			EquipPartType[] arrOrderEquipPartType = ArrOrderEquipPartType;
			foreach (EquipPartType item in arrOrderEquipPartType)
			{
				_listOrderEquipPartType.Add(item);
			}
			foreach (EquipPartType item2 in EquipPartTypeHelper.EquippableTypes())
			{
				if (!_listOrderEquipPartType.Contains(item2))
				{
					_listOrderEquipPartType.Add(item2);
				}
			}
		}
		return _listOrderEquipPartType;
	}
}
