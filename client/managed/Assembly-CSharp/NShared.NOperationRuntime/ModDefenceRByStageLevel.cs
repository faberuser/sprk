using NVespa;

namespace NShared.NOperationRuntime;

public class ModDefenceRByStageLevel : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalDefense,
		StatType.MagicalDefense
	};

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}

	public override int GetMultedStat()
	{
		if (base.OperationInfo == null)
		{
			VespaDebug.LogError("SetMultedStat - OperationInfo is  null.");
			return 0;
		}
		int maxStageLevel = base.OperationInfo.BattleInstance.Snapshot.MaxStageLevel;
		int valueInt = base.OperationInfo.GetValueInt(1, 1);
		return _statHolder.Get() * (maxStageLevel / valueInt);
	}
}
