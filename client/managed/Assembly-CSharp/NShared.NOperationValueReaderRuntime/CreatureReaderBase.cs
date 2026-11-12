namespace NShared.NOperationValueReaderRuntime;

public class CreatureReaderBase : IOperationValueReader
{
	public enum CreatureType
	{
		Me,
		Fc,
		Tc
	}

	private CreatureType _creatureType;

	public CreatureReaderBase(CreatureType creatureType)
	{
		_creatureType = creatureType;
	}

	long IOperationValueReader.Read(string[] arguments, OperationInfo operatinInfo)
	{
		BattleCreature battleCreature = SelectCreature(operatinInfo);
		if (battleCreature == null)
		{
			return 0L;
		}
		return GetStat(battleCreature, arguments, operatinInfo);
	}

	private BattleCreature SelectCreature(OperationInfo operatinInfo)
	{
		return _creatureType switch
		{
			CreatureType.Me => operatinInfo.Creature, 
			CreatureType.Fc => operatinInfo.FiredCreature, 
			CreatureType.Tc => operatinInfo.TargetCreature, 
			_ => null, 
		};
	}

	protected virtual long GetStat(BattleCreature creature, string[] arguments, OperationInfo operatinInfo)
	{
		return 0L;
	}
}
