namespace NShared;

[JsonSerializable(Javascript = true)]
public enum RaidType
{
	NONE = -1,
	DragonNormal,
	DragonNormalSingle,
	DragonHard,
	DragonHardSingle,
	Normal,
	Guild,
	Hard,
	Challenge,
	Single,
	ChallengeSingle,
	GuildSuppress,
	GuildSuppressSingle,
	FieldRaid,
	FieldRaidSingle,
	TechnoMagic,
	TechnoMagicSingle,
	TechnoEnchant,
	TechnoEnchantSingle,
	PunishmentRaid,
	KarmaRaid
}
