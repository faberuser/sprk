namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ChapterType
{
	None,
	City,
	WorldMap,
	Trial,
	DOW,
	Pit,
	Tower,
	GuildRaid,
	WorldBoss,
	Event,
	UnderPrison,
	MemoryFrag,
	OrdealArena
}
