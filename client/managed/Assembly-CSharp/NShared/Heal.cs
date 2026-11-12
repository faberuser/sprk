namespace NShared;

[BattleCallback]
public sealed class Heal : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public long Amount { get; set; }

	public long FinalHp { get; set; }

	public int FinalPercentage { get; set; }

	public HealFlag HealFlag { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is Heal heal))
		{
			return false;
		}
		if (base.GlobalTimestamp == heal.GlobalTimestamp && CreatureUid == heal.CreatureUid && OperationUid == heal.OperationUid && Amount == heal.Amount && FinalHp == heal.FinalHp && FinalPercentage == heal.FinalPercentage)
		{
			return HealFlag == heal.HealFlag;
		}
		return false;
	}
}
