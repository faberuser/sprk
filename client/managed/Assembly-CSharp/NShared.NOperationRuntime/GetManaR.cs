namespace NShared.NOperationRuntime;

public class GetManaR : GetMana
{
	public override void GetInstantManaInfo(out int amount)
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		amount = base.OperationInfo.Creature.StatController.GetMaxMp() * valueInt / 1000;
		amount += valueInt2;
	}
}
