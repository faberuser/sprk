namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ContentsStatusSubType
{
	None,
	Normal,
	Goblin,
	Arena,
	Guild,
	BlackMarket,
	Royal,
	Skillbook,
	Soulstone,
	Rune,
	Inn,
	Furniture,
	HardMode,
	Notice,
	Calendar,
	Attendance28,
	Guide,
	Stream,
	Google,
	Community,
	PlayRecord,
	EventWeb,
	BanPickSeasonReward,
	PetIncubatorSlotExpansion,
	BuyPetIncubator,
	SetPetIncubator,
	SetEggInPetIncubator,
	UnsetEggInPetIncubator,
	GetEggRewards
}
