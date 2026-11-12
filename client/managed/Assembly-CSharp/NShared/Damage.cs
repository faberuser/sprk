namespace NShared;

[BattleCallback]
public sealed class Damage : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public long Amount { get; set; }

	public long FinalHp { get; set; }

	public int FinalPercentage { get; set; }

	public DamageFlag DamageFlag { get; set; }

	public string DamageStateType { get; set; }

	public bool IsDead { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is Damage damage))
		{
			return false;
		}
		if (base.GlobalTimestamp == damage.GlobalTimestamp && CreatureUid == damage.CreatureUid && OperationUid == damage.OperationUid && Amount == damage.Amount && FinalHp == damage.FinalHp && FinalPercentage == damage.FinalPercentage && DamageFlag == damage.DamageFlag && DamageStateType == damage.DamageStateType)
		{
			return IsDead == damage.IsDead;
		}
		return false;
	}
}
