namespace NShared;

[BattleCallback]
public sealed class DamageModified : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long OperationUid { get; set; }

	public DamagePipelinePosition PipelinePosition { get; set; }

	public int Mult { get; set; }

	public int Add { get; set; }

	public long BeforeDamage { get; set; }

	public long AfterDamage { get; set; }

	public DamageModifiedType ModifiedType { get; set; }

	public long ModifiedAmount => AfterDamage - BeforeDamage;

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is DamageModified damageModified))
		{
			return false;
		}
		if (base.GlobalTimestamp == damageModified.GlobalTimestamp && CreatureUid == damageModified.CreatureUid && OperationUid == damageModified.OperationUid && BeforeDamage == damageModified.BeforeDamage && AfterDamage == damageModified.AfterDamage && Mult == damageModified.Mult && Add == damageModified.Add && ModifiedType == damageModified.ModifiedType)
		{
			return PipelinePosition == damageModified.PipelinePosition;
		}
		return false;
	}
}
