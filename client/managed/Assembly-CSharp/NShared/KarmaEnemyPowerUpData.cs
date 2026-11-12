using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KarmaEnemyPowerUpData : IDataKeySelector<int>
{
	[Key(0)]
	public int SkillIndex { get; set; }

	[Key(1)]
	public int PunishmentGroupIndex { get; set; }

	int IDataKeySelector<int>.Key => SkillIndex;
}
