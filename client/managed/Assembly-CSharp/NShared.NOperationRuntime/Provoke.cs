namespace NShared.NOperationRuntime;

public class Provoke : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		BattleCreature firedCreature = base.OperationInfo.FiredCreature;
		if (firedCreature == null)
		{
			return;
		}
		BattleCreature targetCreature = base.OperationInfo.TargetCreature;
		if (targetCreature != null)
		{
			long allRecordSum = targetCreature.ThreatHistoryTracker.GetAllRecordSum();
			int valueInt = base.OperationInfo.GetValueInt(0);
			int valueInt2 = base.OperationInfo.GetValueInt(1);
			long num = allRecordSum * valueInt / 1000 + valueInt2;
			if (num > 0)
			{
				firedCreature.StatController.GainThreat(num, base.OperationInfo.SkillIndex);
			}
		}
	}
}
