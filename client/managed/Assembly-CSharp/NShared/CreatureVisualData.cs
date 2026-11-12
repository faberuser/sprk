using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureVisualData : IDataKeySelector<string>
{
	string IDataKeySelector<string>.Key => Code;

	[Key(0)]
	public string Code { get; set; }

	[Key(1)]
	public bool HideHpBar { get; set; }

	[Key(2)]
	public string OnSpawn { get; set; }

	[Key(3)]
	public string OnBossIntro { get; set; }

	[Key(4)]
	public string OnBattleStart { get; set; }

	[Key(5)]
	public string OnBattleEnd { get; set; }

	[Key(6)]
	public string OnDead { get; set; }

	[Key(7)]
	public string DamageColor { get; set; }

	[Key(8)]
	public bool IgnoreFlip { get; set; }

	[Key(9)]
	public string ModelPrefix { get; set; }

	[Key(10)]
	public string NameCode { get; set; }

	[Key(11)]
	public string NamePrefixCode { get; set; }

	[Key(12)]
	public string DescriptionCode { get; set; }

	[Key(13)]
	public string BossIntroReplaceNameCode { get; set; }

	[Key(14)]
	public string BossIntroReplaceNamePrefixCode { get; set; }

	[Key(15)]
	public bool CanSkipBossIntro { get; set; }
}
