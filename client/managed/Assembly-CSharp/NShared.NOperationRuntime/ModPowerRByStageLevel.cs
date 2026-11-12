using System;
using NVespa;

namespace NShared.NOperationRuntime;

public class ModPowerRByStageLevel : SetMultedStat
{
	private const decimal STAT_MULTIPLIER = 1000m;

	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalAttack,
		StatType.MagicalAttack
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
		int ratio = _statHolder.Get() + 1000;
		int exponent = maxStageLevel / valueInt;
		long accRatio = GameRoot.Instance.ExponentialTableK.GetAccRatio(ratio, exponent);
		accRatio -= 1000;
		if (accRatio <= 0)
		{
			return 0;
		}
		if (accRatio > int.MaxValue)
		{
			return int.MaxValue;
		}
		return Convert.ToInt32(accRatio);
	}
}
