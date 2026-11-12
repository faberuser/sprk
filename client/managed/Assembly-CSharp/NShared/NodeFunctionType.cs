namespace NShared;

[JsonSerializable(Javascript = true)]
public enum NodeFunctionType
{
	None,
	Battle,
	EntryMenu,
	MoveChapter,
	City,
	Temple,
	Portal,
	Excavation,
	EventTrigger,
	NPC,
	BattleOnce,
	SubStoryEvent,
	SubStoryBattle,
	SubStoryBoss,
	Switching,
	RunBattle,
	MultiBattle,
	HonoraryChamber,
	MoveToNpc,
	MoveSubStory,
	NoEntry,
	ShakmehDungeon,
	OrdealArenaBattle,
	OrdealArenaEvent,
	OrdealArenaStart,
	OrdealArenaEnd,
	PetHouse,
	WebView,
	TreasureHouse,
	PunishmentRaid,
	PunishmentRuneCraft
}
