namespace NShared;

[JsonSerializable(Javascript = true)]
public enum MatchArenaType
{
	Normal,
	BanPick,
	WorldBanPick,
	GuildArena,
	Arena3,
	Ordeal,
	Lucky
}
