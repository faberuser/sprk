namespace NShared;

[JsonSerializable]
public class GuildInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => Id;

	public long Id { get; set; }

	public string Name { get; set; }

	public string MasterNick { get; set; }

	public byte Level { get; set; }

	public byte Logo { get; set; }

	public byte Back { get; set; }

	public byte Flag { get; set; }

	public string Introduce { get; set; }

	public int ReqTeamLevel { get; set; }

	public byte JoinWay { get; set; }

	public byte CurMember { get; set; }
}
