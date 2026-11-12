using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KarmaEnemyUnlockSkillData : IDataKeySelector<KarmaEnemyUnlockSkillKey>
{
	[Key(0)]
	public int PunishmentGroupIndex { get; set; }

	[Key(1)]
	public int TriggerLevel { get; set; }

	[Key(2)]
	public int[] UnlockSkills { get; set; }

	KarmaEnemyUnlockSkillKey IDataKeySelector<KarmaEnemyUnlockSkillKey>.Key => new KarmaEnemyUnlockSkillKey(PunishmentGroupIndex, TriggerLevel);
}
