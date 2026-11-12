namespace NShared;

public interface IDamageDodger : IBaseDamageModifier, ISnapshotObject
{
	bool Dodge();
}
