using NVespa;

namespace NShared.NOperationRuntime;

public class ModMaxHpRByStageLevel : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MaxHp };

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

	protected override void OnBegin()
	{
		ModMaxHp.HandleMaxHpChanged(this, delegate
		{
			base.OnBegin();
		});
	}

	protected override void OnStatUpdatedAndMakeDirty()
	{
		ModMaxHp.HandleMaxHpChanged(this, base.MakeCreatureStatDirty);
	}
}
