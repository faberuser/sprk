namespace NShared.PunishmentRaid;

[JsonSerializable(Javascript = true)]
public enum ResultType
{
	Fail,
	Success,
	InvalidData,
	NotOpenedDungeon,
	UserCampaignInfoNotFound,
	ItemNotExist,
	ExistKarmaDungeon,
	CannotPlayDungeon,
	AlreadyCompleted,
	NotEnoughItem,
	WrongFlaskIndex,
	WrongGroupIndex,
	WrongDungeonType
}
