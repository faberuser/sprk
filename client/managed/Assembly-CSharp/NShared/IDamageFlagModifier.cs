namespace NShared;

public interface IDamageFlagModifier : IBaseDamageModifier, ISnapshotObject
{
	bool Modify(BattleCreature attacker, ref DamageFlag damageFlag);
}
