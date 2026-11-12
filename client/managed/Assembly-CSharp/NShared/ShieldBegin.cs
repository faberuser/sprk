namespace NShared;

[BattleCallback]
public sealed class ShieldBegin : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public DamageFlag DamageFlag { get; set; }

	public long ShieldMaxHp { get; set; }

	public string StateType { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is ShieldBegin shieldBegin))
		{
			return false;
		}
		if (base.GlobalTimestamp == shieldBegin.GlobalTimestamp && CreatureUid == shieldBegin.CreatureUid && OperationUid == shieldBegin.OperationUid && DamageFlag == shieldBegin.DamageFlag && ShieldMaxHp == shieldBegin.ShieldMaxHp)
		{
			return StateType == shieldBegin.StateType;
		}
		return false;
	}
}
