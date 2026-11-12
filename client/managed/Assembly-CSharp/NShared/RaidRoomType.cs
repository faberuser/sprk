namespace NShared;

[JsonSerializable(Javascript = true)]
public enum RaidRoomType
{
	All,
	Help,
	LevelUp,
	Advice,
	Physical,
	Magical
}
