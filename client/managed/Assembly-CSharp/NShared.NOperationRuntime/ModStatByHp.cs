namespace NShared.NOperationRuntime;

public class ModStatByHp : ModStatByHpBase
{
	protected override int GetAddedStat()
	{
		return GetModifyValue();
	}
}
