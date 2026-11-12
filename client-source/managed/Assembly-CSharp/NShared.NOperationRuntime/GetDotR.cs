namespace NShared.NOperationRuntime;

public class GetDotR : GetDot
{
	public override long GetDamageAmount()
	{
		long amount = 0L;
		int damageRatio = GetDamageRatio();
		base.OperationInfo.FiredCreature.StatController.GetAttackAmountAndFlagByPower(base.OperationInfo.SkillData, damageRatio, out var _, out amount);
		int addedDamage = GetAddedDamage();
		return amount + addedDamage;
	}

	public override DamageFlag GetDamageFlag()
	{
		DamageFlag addedFlag = GetAddedFlag();
		return DamageFlag.None | addedFlag;
	}

	protected int GetDamageRatio()
	{
		return base.OperationInfo.GetValueInt(0);
	}

	protected int GetAddedDamage()
	{
		return base.OperationInfo.GetValueInt(1);
	}

	protected DamageFlag GetAddedFlag()
	{
		return base.OperationInfo.GetValueFlag<DamageFlag>(2);
	}

	public override int GetHpStealRatio()
	{
		return base.OperationInfo.GetValueInt(3);
	}
}
