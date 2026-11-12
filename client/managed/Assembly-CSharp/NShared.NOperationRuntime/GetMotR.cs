namespace NShared.NOperationRuntime;

public class GetMotR : GetMot
{
	public override int GetManaAmount()
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		return base.OperationInfo.Creature.StatController.GetMaxMp() * valueInt / 1000;
	}
}
