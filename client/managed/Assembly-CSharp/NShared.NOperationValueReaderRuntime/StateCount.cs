using System.Collections.Generic;

namespace NShared.NOperationValueReaderRuntime;

public class StateCount : CreatureReaderBase, IStateReader
{
	public StateCount(CreatureType creatureType)
		: base(creatureType)
	{
	}

	protected override long GetStat(BattleCreature creature, string[] arguments, OperationInfo operatinInfo)
	{
		string valueString = ArgParser.GetValueString(arguments, 0);
		List<IStateModifier> allStateModifier = creature.StateController.GetAllStateModifier(valueString);
		int num = 0;
		if (allStateModifier != null)
		{
			for (int i = 0; i < allStateModifier.Count; i++)
			{
				IStateModifier stateModifier = allStateModifier[i];
				num += stateModifier.GetStackCount();
			}
		}
		return num;
	}

	string IStateReader.GetState()
	{
		return GetType().Name;
	}
}
