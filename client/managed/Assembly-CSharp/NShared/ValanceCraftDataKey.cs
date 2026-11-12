namespace NShared;

public class ValanceCraftDataKey : Tuple<EquipItemSetGroupType, CreatureTagType, EquipPartType, EquipItemSubType>
{
	public static ValanceCraftDataKey DefaultKey = new ValanceCraftDataKey();

	public EquipItemSetGroupType SetType
	{
		get
		{
			return Item1;
		}
		set
		{
			Item1 = value;
		}
	}

	public CreatureTagType CreatureTagType
	{
		get
		{
			return Item2;
		}
		set
		{
			Item2 = value;
		}
	}

	public EquipPartType EquipPartType
	{
		get
		{
			return Item3;
		}
		set
		{
			Item3 = value;
		}
	}

	public EquipItemSubType EquipItemSubType
	{
		get
		{
			return Item4;
		}
		set
		{
			Item4 = value;
		}
	}

	public ValanceCraftDataKey()
		: base(EquipItemSetGroupType.None, CreatureTagType.None, EquipPartType.None, EquipItemSubType.None)
	{
	}

	public ValanceCraftDataKey(EquipItemSetGroupType setType, CreatureTagType creatureTagType, EquipPartType partType, EquipItemSubType subType)
		: base(setType, creatureTagType, partType, subType)
	{
	}

	public bool Equals(ValanceCraftDataKey other)
	{
		if (SetType != other.SetType)
		{
			return false;
		}
		if (CreatureTagType != other.CreatureTagType)
		{
			return false;
		}
		if (EquipPartType != other.EquipPartType)
		{
			return false;
		}
		if (EquipItemSubType != other.EquipItemSubType)
		{
			return false;
		}
		return true;
	}
}
