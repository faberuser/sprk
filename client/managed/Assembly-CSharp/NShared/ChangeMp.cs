namespace NShared;

[BattleCallback]
public sealed class ChangeMp : BaseBattleCallback
{
	public enum ReasonType
	{
		None,
		Consume,
		MpOnTime,
		MpOnDamage,
		MpOnAttack,
		MpOnKill,
		MpOnWave,
		Skill
	}

	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public ReasonType Reason { get; set; }

	public int Amount { get; set; }

	public long FinalMp { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is ChangeMp changeMp))
		{
			return false;
		}
		if (base.GlobalTimestamp == changeMp.GlobalTimestamp && CreatureUid == changeMp.CreatureUid && OperationUid == changeMp.OperationUid && Reason == changeMp.Reason && Amount == changeMp.Amount)
		{
			return FinalMp == changeMp.FinalMp;
		}
		return false;
	}
}
