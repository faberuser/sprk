namespace NShared;

[BattleCallback]
public sealed class ShieldModified : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public long ShieldMaxHp { get; set; }

	public long ShieldHp { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is ShieldModified shieldModified))
		{
			return false;
		}
		if (base.GlobalTimestamp == shieldModified.GlobalTimestamp && CreatureUid == shieldModified.CreatureUid && OperationUid == shieldModified.OperationUid && ShieldMaxHp == shieldModified.ShieldMaxHp)
		{
			return ShieldHp == shieldModified.ShieldHp;
		}
		return false;
	}
}
