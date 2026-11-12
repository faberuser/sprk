namespace NShared;

public class DamageOrHealOperationTypeAttribute : OperationTypeAttribute
{
	public override bool IsDamageOrHeal => true;
}
