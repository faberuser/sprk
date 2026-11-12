namespace NShared.NOperationRuntime;

public class ModStatByHpR : ModStatByHpBase
{
	protected override int GetMultedStat()
	{
		return GetModifyValue();
	}
}
