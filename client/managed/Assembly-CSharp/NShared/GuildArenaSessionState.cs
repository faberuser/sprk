namespace NShared;

[JsonSerializable(Javascript = true)]
public enum GuildArenaSessionState
{
	None,
	Apply,
	ApplyEnded,
	Battle,
	Ended,
	NotHeld
}
