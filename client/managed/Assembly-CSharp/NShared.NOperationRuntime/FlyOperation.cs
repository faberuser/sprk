namespace NShared.NOperationRuntime;

public class FlyOperation : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		BattleCreature firedCreature = base.OperationInfo.FiredCreature;
		if (firedCreature != null)
		{
			int valueInt = base.OperationInfo.GetValueInt(0);
			int valueInt2 = base.OperationInfo.GetValueInt(1);
			int valueInt3 = base.OperationInfo.GetValueInt(2, 1);
			firedCreature.SkillController.OperationQueue.FlyOperation(valueInt, valueInt2, valueInt3, base.OperationInfo.TargetCreature, base.OperationInfo.TargetCreatures);
		}
	}
}
