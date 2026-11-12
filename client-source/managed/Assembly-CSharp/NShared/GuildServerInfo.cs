namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildServerInfo
{
	public string ServerGroup { get; set; }

	public long GuildId { get; set; }

	public string Name { get; set; }

	public byte Logo { get; set; }

	public byte Back { get; set; }

	public byte Flag { get; set; }

	public string CountryCode { get; set; }

	public byte MemberCount { get; set; }
}
