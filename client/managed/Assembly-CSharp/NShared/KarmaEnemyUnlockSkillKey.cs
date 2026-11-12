using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct KarmaEnemyUnlockSkillKey
{
	[Key(0)]
	public int PunishmentGroupIndex;

	[Key(1)]
	public int TriggerLevel;

	public KarmaEnemyUnlockSkillKey(int punishmentGroupIndex, int triggerLevel)
	{
		PunishmentGroupIndex = punishmentGroupIndex;
		TriggerLevel = triggerLevel;
	}

	public bool Equals(KarmaEnemyUnlockSkillKey other)
	{
		if (PunishmentGroupIndex == other.PunishmentGroupIndex)
		{
			return TriggerLevel == other.TriggerLevel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(PunishmentGroupIndex, TriggerLevel);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({PunishmentGroupIndex}, {TriggerLevel})";
	}
}
