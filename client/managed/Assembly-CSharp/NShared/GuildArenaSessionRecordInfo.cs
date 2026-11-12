namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaSessionRecordInfo
{
	public int SessionIndex { get; set; }

	public int SeasonIndex { get; set; }

	public string LeftServerGroup { get; set; }

	public long LeftGuildId { get; set; }

	public byte LeftGuildLogo { get; set; }

	public byte LeftGuildBack { get; set; }

	public string LeftGuildName { get; set; }

	public string LeftCountryCode { get; set; }

	public int LeftTierIndex { get; set; }

	public int LeftSessionScore { get; set; }

	public string RightServerGroup { get; set; }

	public long RightGuildId { get; set; }

	public byte RightGuildLogo { get; set; }

	public byte RightGuildBack { get; set; }

	public string RightGuildName { get; set; }

	public string RightCountryCode { get; set; }

	public int RightTierIndex { get; set; }

	public int RightSessionScore { get; set; }

	public int Win { get; set; }

	public string UpdatedTime { get; set; }
}
