namespace NShared;

[JsonSerializable(Javascript = true)]
public enum GuildSuppressSessionState
{
	Apply,
	ApplyEnded,
	Battle,
	Ended,
	NotHeld
}
