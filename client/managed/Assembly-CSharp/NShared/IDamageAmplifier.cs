namespace NShared;

public interface IDamageAmplifier : IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	bool AccumulateAmplifyDamage(BattleCreature attacker, ref DamageFlag damageFlag, ref int multPower, ref int addPower);
}
