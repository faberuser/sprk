namespace NShared.NOperationValueReaderRuntime;

public class StatReader : CreatureReaderBase, IStatReader
{
	private StatType _statType;

	public StatReader(CreatureType creatureType, StatType statType)
		: base(creatureType)
	{
		_statType = statType;
	}

	protected override long GetStat(BattleCreature creature, string[] arguments, OperationInfo operatinInfo)
	{
		return creature.StatController.GetFinalStat(_statType);
	}

	StatType IStatReader.GetStatType()
	{
		return _statType;
	}
}
