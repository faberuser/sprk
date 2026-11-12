namespace NShared;

public interface IDamageModifier : IBaseDamageModifier, ISnapshotObject
{
	bool Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag);

	DamageModifiedType GetModifiedType();

	DamagePipelinePosition GetPipelinePosition();
}
