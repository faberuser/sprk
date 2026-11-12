namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ArchiveSubType
{
	None,
	UniqueWeapon,
	UniqueTreasure,
	Artifact,
	EquipItem,
	NormalItem,
	Rune
}
