namespace NShared;

public interface IAttackModifier : IBaseDamageModifier, ISnapshotObject
{
	bool Modify(BattleCreature damaged, SkillData firedSkillData, ref long amount, ref DamageFlag damageFlag);

	bool AccumulateAmplifyAttack(BattleCreature damaged, SkillData firedSkillData, ref DamageFlag damageFlag, ref int multPower, ref int addPower);

	DamageModifiedType GetModifiedType();
}
