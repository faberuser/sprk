namespace NShared;

[JsonSerializable(Javascript = true)]
public enum ContentsNoticeType
{
	[ContentsNoticeType(RecordsOfHonorContentType = RecordsOfHonorContentType.WorldBoss)]
	WorldBoss,
	[ContentsNoticeType(RecordsOfHonorContentType = RecordsOfHonorContentType.GuildSuppress)]
	GuildSuppress,
	[ContentsNoticeType(RecordsOfHonorContentType = RecordsOfHonorContentType.Match)]
	Match,
	[ContentsNoticeType(RecordsOfHonorContentType = RecordsOfHonorContentType.BanPick)]
	BanPick
}
